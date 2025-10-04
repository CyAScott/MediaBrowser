namespace MediaBrowser.Media;

public class MediaConfig(IConfiguration configuration)
{
    public string CastDirectory { get; } = configuration["media:castDirectory"]!;
    public string DirectorsDirectory { get; } = configuration["media:directorsDirectory"]!;
    public string GenresDirectory { get; } = configuration["media:genresDirectory"]!;
    public string MediaDirectory { get; } = configuration["media:mediaDirectory"]!;
    public string ProducersDirectory { get; } = configuration["media:producersDirectory"]!;
    public string WritersDirectory { get; } = configuration["media:writersDirectory"]!;
}