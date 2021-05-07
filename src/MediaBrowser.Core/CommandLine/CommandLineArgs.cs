using MediaBrowser.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MediaBrowser.CommandLine
{
    /// <summary>
    /// The base class for command line arguments.
    /// </summary>
    public abstract class CommandLineArgs
    {
        /// <inheritdoc/>
        protected CommandLineArgs() => Properties = GetType()
            .GetProperties()
            .Where(it => it.CanRead && it.CanWrite)
            .Select(it => new
            {
                attribute = it.GetCustomAttribute<CommandLineArgumentAttribute>(),
                property = it
            })
            .Where(it => it.attribute != null)
            .Select(it => (it.attribute, it.property))
            .ToArray();

        /// <summary>
        /// The command properties.
        /// </summary>
        public (CommandLineArgumentAttribute attribute, PropertyInfo property)[] Properties { get; }

        /// <summary>
        /// Pares the command line arguments.
        /// </summary>
        public virtual void Parse(string[] args)
        {
            for (var index = 1; index < args.Length; index++)
            {
                var arg = args[index].Trim();

                if (!arg.StartsWith("-"))
                {
                    throw new ArgumentException($"Invalid command line argument: {arg}");
                }

                var useLongName = arg.StartsWith("--");

                var name = arg.Substring(useLongName ? 2 : 1);

                CommandLineArgumentAttribute attribute;
                PropertyInfo property;
                try
                {
                    (attribute, property) = Properties.SingleOrDefault(it => string.Equals(name, useLongName ? it.attribute.LongName : it.attribute.ShortName, StringComparison.OrdinalIgnoreCase));

                    if (attribute == null)
                    {
                        throw new ArgumentException($"Command argument not found: {arg}");
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException($"Multiple matches for: {arg}");
                }

                var commandArgs = args.Skip(index + 1).TakeWhile(it => !it.StartsWith("-")).ToArray();

                if (commandArgs.Length == 0)
                {
                    var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                    if (defaultValueAttribute != null)
                    {
                        property.SetValue(this, defaultValueAttribute.Value);
                    }
                    else if (property.PropertyType.IsValueType)
                    {
                        property.SetValue(this, Activator.CreateInstance(property.PropertyType));
                    }
                }
                else if (commandArgs.Length == 1)
                {
                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(this, Enum.Parse(property.PropertyType, commandArgs[0], true));
                    }
                    else
                    {
                        property.SetValue(this, Convert.ChangeType(commandArgs[0], property.PropertyType));
                    }
                }

                index += commandArgs.Length;
            }
        }
    }
}
