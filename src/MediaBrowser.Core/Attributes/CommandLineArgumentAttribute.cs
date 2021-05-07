using System;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// A command line argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineArgumentAttribute : Attribute
    {
        /// <inheritdoc/>
        public CommandLineArgumentAttribute(string longName, string shortName = null)
        {
            LongName = longName;
            ShortName = shortName ?? longName.Substring(0, 1);
        }

        /// <summary>
        /// The description of the argument.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The long name for the argument.
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// The short name.
        /// </summary>
        public string ShortName { get; }
    }
}
