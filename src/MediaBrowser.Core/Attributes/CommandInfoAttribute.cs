using System;
using static System.Console;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// Meta information about a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandInfoAttribute : Attribute
    {
        /// <inheritdoc/>
        public CommandInfoAttribute(string description) => Description = description;

        /// <summary>
        /// A friendly description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Prints the command information.
        /// </summary>
        public void PrintInfo(Type service, string name)
        {
            Write($"{name}");
            if (!string.IsNullOrEmpty(Description))
            {
                WriteLine($" - {Description}");
            }
            else
            {
                WriteLine();
            }
        }
    }
}
