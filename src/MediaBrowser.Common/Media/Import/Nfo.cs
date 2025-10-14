using System.Xml;
using HandlebarsDotNet;

namespace MediaBrowser.Media.Import;

public class Nfo(MediaConfig mediaConfig)
{
    static Nfo()
    {
        using var stream = typeof(Nfo).Assembly.GetManifestResourceStream("MediaBrowser.Media.Import.Nfo.xml");
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        
        var template = reader.ReadToEnd();

        _template = Handlebars.Compile(template);
    }
    static readonly HandlebarsTemplate<object, string> _template;

    public async Task Save(MediaEntity mediaEntity, string fileLocation)
    {
        var contents = _template(mediaEntity);

        if (File.Exists(fileLocation))
        {
            File.Delete(fileLocation);
        }
        
        await File.WriteAllTextAsync(fileLocation, contents);
    }

    public MediaEntity Read(string rawXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(rawXml);

        if (!Guid.TryParse(xmlDoc.SelectSingleNode("//id")?.InnerText, out var mediaId))
        {
            throw new ParseNfoException(1, "Invalid or missing media ID");
        }
        
        var hash = xmlDoc.SelectSingleNode("//md5")?.InnerText;
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 32)
        {
            throw new ParseNfoException(2, "Invalid or missing hash");
        }

        var mime = xmlDoc.SelectSingleNode("//mime")?.InnerText;

        if (!mediaConfig.TryToGetExtensionFromMime(mime, out var extension))
        {
            throw new ParseNfoException(3, "Invalid or missing mime type");
        }
        
        var fileInfo = new FileInfo(Path.Combine(mediaConfig.MediaDirectory, $"{hash}.{extension}"));
        if (!fileInfo.Exists)
        {
            throw new ParseNfoException(4, "File not found");
        }

        FfprobeResponse ffprobeResponse;
        try
        {
            var json = Guard.Against.NullOrEmpty(xmlDoc.SelectSingleNode("//ffprobe")?.InnerText);
            
            ffprobeResponse = Guard.Against.Null(JsonSerializer.Deserialize<FfprobeResponse>(json));
        }
        catch (Exception error)
        {
            throw new ParseNfoException(5, "Error getting reading ffprobe info.", error);
        }
        
        var request = new ImportMediaRequest
        {
            Title = xmlDoc.SelectSingleNode("//title")?.InnerText ?? string.Empty,
            OriginalTitle = xmlDoc.SelectSingleNode("//originaltitle")?.InnerText  ?? string.Empty,
            Description = xmlDoc.SelectSingleNode("//description")?.InnerText  ?? string.Empty,
            Published = xmlDoc.SelectSingleNode("//published")?.InnerText  ?? string.Empty,
            Rating = double.TryParse(xmlDoc.SelectSingleNode("//rating")?.InnerText ?? string.Empty, out var rating)
                ? rating
                : null,
            UserStarRating = int.TryParse(xmlDoc.SelectSingleNode("//userStarRating")?.InnerText, out var userStarRating)
                ? userStarRating
                : null,
            Cast = xmlDoc.SelectNodes("//cast/name")?.Cast<XmlNode>()
                .Select(x => x.InnerText).Distinct().ToList() ?? [],
            Directors = xmlDoc.SelectNodes("//directors/name")?.Cast<XmlNode>()
                .Select(x => x.InnerText).Distinct().ToList() ?? [],
            Genres = xmlDoc.SelectNodes("//genres/name")?.Cast<XmlNode>()
                .Select(x => x.InnerText).Distinct().ToList() ?? [],
            Producers = xmlDoc.SelectNodes("//producers/name")?.Cast<XmlNode>()
                .Select(x => x.InnerText).Distinct().ToList() ?? [],
            Writers = xmlDoc.SelectNodes("//writers/name")?.Cast<XmlNode>()
                .Select(x => x.InnerText).Distinct().ToList() ?? [],
            Thumbnail = 0 //not needed when importing from nfo
        };
        
        var media = MediaEntity.Create(
            mediaId: mediaId,
            fileInfo: fileInfo,
            ffprobe: ffprobeResponse,
            request: request,
            hash: hash, mime: mime!,
            cast: request.Cast,
            directors: request.Directors,
            genres: request.Genres,
            producers: request.Producers,
            writers: request.Writers,
            height: int.TryParse(xmlDoc.SelectSingleNode("//height")?.InnerText ?? string.Empty, out var height) ? height : null,
            width: int.TryParse(xmlDoc.SelectSingleNode("//width")?.InnerText ?? string.Empty, out var width) ? width : null,
            ctimeMs: long.TryParse(xmlDoc.SelectSingleNode("//ctime")?.InnerText ?? string.Empty, out var ctimeMs) ? ctimeMs : null,
            mtimeMs: long.TryParse(xmlDoc.SelectSingleNode("//mtimeMs")?.InnerText ?? string.Empty, out var mtimeMs) ? mtimeMs : null);

        return media;
    }
}

public class ParseNfoException : Exception
{
    public ParseNfoException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public ParseNfoException(int errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
    
    public int ErrorCode { get; }
}