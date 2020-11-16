using MediaBrowser.Attributes;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Contains information about the version of the Media Browser.
    /// </summary>
    [Configuration("Version")]
    public class VersionInfo
    {
        /// <inheritdoc />
        public VersionInfo()
        {
            var attribute = typeof(VersionInfo).Assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            var versionMatch = Regex.Match(attribute?.Version ?? "1.0.0", @"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)$");

            if (versionMatch.Success)
            {
                Major = int.Parse(versionMatch.Groups["major"].Value);
                Minor = int.Parse(versionMatch.Groups["minor"].Value);
                Build = int.Parse(versionMatch.Groups["build"].Value);
            }
        }

        /// <summary>
        /// Semantic build version.
        /// </summary>
        public int Build { get; }

        /// <summary>
        /// Semantic major version.
        /// </summary>
        public int Major { get; } = 1;

        /// <summary>
        /// Semantic minor version.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// The full semantic version.
        /// </summary>
        public string GetVersion() => $"{Major}.{Minor}.{Build}";
    }
}
