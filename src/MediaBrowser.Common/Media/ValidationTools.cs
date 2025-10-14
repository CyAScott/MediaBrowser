using System.Text.RegularExpressions;

namespace MediaBrowser.Media;

public static class ValidationTools
{
    public static bool IsNameValid(string name) => Regex.IsMatch(name,
        @"^([a-z\d]+ )*[a-z\d]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase) && name.Length <= 50;
}