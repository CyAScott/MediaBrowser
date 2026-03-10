namespace MediaBrowser.Media;

public static partial class ValidationTools
{
    public static bool IsNameValid(string name) => ValidNameRegex().IsMatch(name) && name.Length <= 50;
    [GeneratedRegex(@"^([a-z\d]+ )*[a-z\d]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    static private partial Regex ValidNameRegex();
}