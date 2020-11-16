using System;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// Indicates how an installer will be ran.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InstallerAttribute : Attribute
    {
        /// <summary>
        /// The group name that will group together multiple installers.
        /// Default value is null and no grouping will be selected.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The install priority when multiple installers can be ran at a time.
        /// Default value is <see cref="int.MaxValue"/>.
        /// </summary>
        public int Priority { get; set; } = int.MaxValue;
    }
}
