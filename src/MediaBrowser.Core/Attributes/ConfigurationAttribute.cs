using System;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// Indicates the class contains configuration information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationAttribute : Attribute
    {
        /// <summary>
        /// Indicates the class contains configuration information with this name.
        /// </summary>
        public ConfigurationAttribute(string name) => Name = name;

        /// <summary>
        /// The name of the configuration.
        /// </summary>
        public string Name { get; }
    }
}
