using System;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// Indicates the priority to run the init class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InitAttribute : Attribute
    {
        /// <summary>
        /// The run priority when multiple init classes can be ran at a time.
        /// Default value is <see cref="int.MaxValue"/>.
        /// </summary>
        public int Priority { get; set; } = int.MaxValue;
    }
}
