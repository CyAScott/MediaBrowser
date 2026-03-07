using Microsoft.Extensions.Configuration;
using Shouldly;

namespace MediaBrowser.Media.Import;

public class NfoTests
{
    public NfoTests()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "MediaBrowserTests");
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }
        Directory.CreateDirectory(tempDirectory);

        _mediaDirectory = Path.Combine(tempDirectory, "media");
        Directory.CreateDirectory(_mediaDirectory);

        var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"media:importExtensions:mp4:order", "1"},
            {"media:importExtensions:mp4:mime", "video/mp4"},
            {"media:importExtensions:mp4:ext", "mp4"},
            {"media:mediaDirectory", _mediaDirectory}

        });
        _nfo = new(new(config));
    }
    readonly Nfo _nfo;
    readonly string _mediaDirectory;

    [Test]
    public void ReadValidXml()
    {
        const string xml = "<root>\n" +
                           "<id>123e4567-e89b-12d3-a456-426614174000</id>\n" +
                           "<md5>1234567890abcdef1234567890abcdef</md5>\n" +
                           "<mime>video/mp4</mime>\n" +
                           "<ffprobe>{\"streams\":[],\"format\":{}}</ffprobe>\n" +
                           "<title>Test Title</title>\n" +
                           "<originaltitle>Original Title</originaltitle>\n" +
                           "<description>Desc</description>\n" +
                           "<rating>7.5</rating>\n" +
                           "<userStarRating>5</userStarRating>\n" +
                           "<cast><name>Actor1</name></cast>\n" +
                           "<directors><name>Director1</name></directors>\n" +
                           "<genres><name>Genre1</name></genres>\n" +
                           "<producers><name>Producer1</name></producers>\n" +
                           "<writers><name>Writer1</name></writers>\n" +
                           "<thumbnail>1.0</thumbnail>\n" +
                           "<height>1080</height>\n" +
                           "<width>1920</width>\n" +
                           "<ctime>123456789</ctime>\n" +
                           "<mtimeMs>987654321</mtimeMs>\n" +
                           "</root>";

        // Create the file to simulate file existence
        var filePath = Path.Combine(_mediaDirectory, "1234567890abcdef1234567890abcdef.mp4");
        File.WriteAllText(filePath, "dummy");

        var media = _nfo.Read(xml);
        media.ShouldNotBeNull();
        // Additional asserts for fields can be added here
        File.Delete(filePath);
    }

    [Test]
    public void ReadInvalidMediaId()
    {
        const string xml = "<root>\n" +
                           "<id>invalid-guid</id>\n" +
                           "<md5>1234567890abcdef1234567890abcdef</md5>\n" +
                           "<mime>video/mp4</mime>\n" +
                           "<ffprobe>{\"streams\":[],\"format\":{}}</ffprobe>\n" +
                           "</root>";
        Should.Throw<ParseNfoException>(() => _nfo.Read(xml));
    }

    [Test]
    public void ReadInvalidHash()
    {
        const string xml = "<root>\n" +
                           "<id>123e4567-e89b-12d3-a456-426614174000</id>\n" +
                           "<md5>short</md5>\n" +
                           "<mime>video/mp4</mime>\n" +
                           "<ffprobe>{\"streams\":[],\"format\":{}}</ffprobe>\n" +
                           "</root>";
        Should.Throw<ParseNfoException>(() => _nfo.Read(xml));
    }

    [Test]
    public void ReadInvalidMime()
    {
        const string xml = "<root>\n" +
                           "<id>123e4567-e89b-12d3-a456-426614174000</id>\n" +
                           "<md5>1234567890abcdef1234567890abcdef</md5>\n" +
                           "<mime>bad-mime</mime>\n" +
                           "<ffprobe>{\"streams\":[],\"format\":{}}</ffprobe>\n" +
                           "</root>";
        Should.Throw<ParseNfoException>(() => _nfo.Read(xml));
    }

    [Test]
    public void ReadFileNotFound()
    {
        const string xml = "<root>\n" +
                           "<id>123e4567-e89b-12d3-a456-426614174000</id>\n" +
                           "<md5>1234567890abcdef1234567890abcdef</md5>\n" +
                           "<mime>video/mp4</mime>\n" +
                           "<ffprobe>{\"streams\":[],\"format\":{}}</ffprobe>\n" +
                           "</root>";
        Should.Throw<ParseNfoException>(() => _nfo.Read(xml));
    }

    [Test]
    public void ReadInvalidFfprobe()
    {
        const string xml = "<root>\n" +
                           "<id>123e4567-e89b-12d3-a456-426614174000</id>\n" +
                           "<md5>1234567890abcdef1234567890abcdef</md5>\n" +
                           "<mime>video/mp4</mime>\n" +
                           "<ffprobe>not-json</ffprobe>\n" +
                           "</root>";
        // Create the file to simulate file existence
        var filePath = Path.Combine(_mediaDirectory, "1234567890abcdef1234567890abcdef.mp4");
        File.WriteAllText(filePath, "dummy");

        Should.Throw<ParseNfoException>(() => _nfo.Read(xml));
        File.Delete(filePath);
    }
}