using Castle.Components.DictionaryAdapter;
using MediaBrowser.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// Parses a command line argument.
        /// </summary>
        protected virtual void ParseArgument(string name, PropertyInfo property, CommandLineArgumentAttribute attribute, string[] commandArgs)
        {
            if (commandArgs.Length == 0)
            {
                var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttribute != null)
                {
                    property.SetValue(this, defaultValueAttribute.Value);
                    return;
                }
            }

            object parseValue(Type type, string value)
            {
                if (type == typeof(string))
                {
                    return value;
                }

                var underlyingType = Nullable.GetUnderlyingType(type);

                if (!underlyingType.IsPrimitive && !underlyingType.IsValueType)
                {
                    throw new InvalidProgramException("Data type not supported.");
                }

                if (string.IsNullOrEmpty(value) && underlyingType != type)
                {
                    return null;
                }

                if (underlyingType.IsEnum)
                {
                    return string.IsNullOrEmpty(value) ? Activator.CreateInstance(underlyingType) : Enum.Parse(underlyingType, value, true);
                }

                if (underlyingType == typeof(TimeSpan))
                {
                    return string.IsNullOrEmpty(value) ? TimeSpan.Zero : TimeSpan.Parse(value);
                }

                if (underlyingType == typeof(DateTime))
                {
                    return string.IsNullOrEmpty(value) ? DateTime.Now : DateTime.Parse(value);
                }

                return string.IsNullOrEmpty(value) ? Activator.CreateInstance(underlyingType) : Convert.ChangeType(value, underlyingType);
            }

            Func<string[], object> getConstructor(Type type)
            {
                if (type.IsPrimitive || type.IsValueType || type == typeof(string))
                {
                    return values => values.Select(it => parseValue(type, it)).Single();
                }

                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != type && (underlyingType.IsPrimitive || underlyingType.IsValueType || underlyingType == typeof(string)))
                {
                    return values => values.Select(it => parseValue(type, it)).SingleOrDefault();
                }

                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    underlyingType = Nullable.GetUnderlyingType(elementType) ?? elementType;
                    if (underlyingType.IsPrimitive || underlyingType.IsValueType || underlyingType == typeof(string))
                    {
                        return values =>
                        {
                            var array = Array.CreateInstance(elementType, values.Length);

                            for (var index = 0; index < values.Length; index++)
                            {
                                array.SetValue(parseValue(elementType, values[index]), index);
                            }

                            return array;
                        };
                    }
                }

                if (type.IsGenericType)
                {
                    var genericArguments = type.GetGenericArguments();
                    if (genericArguments.Length == 1)
                    {
                        var elementType = genericArguments[0];
                        underlyingType = Nullable.GetUnderlyingType(elementType) ?? elementType;
                        if (underlyingType.IsPrimitive || underlyingType.IsValueType || underlyingType == typeof(string))
                        {
                            var contrArgumentTypes = new[]
                            {
                                elementType.MakeArrayType(),
                                typeof(IEnumerable),
                                typeof(IEnumerable<>).MakeGenericType(elementType)
                            };

                            var constructor = type.GetConstructors().FirstOrDefault(ctor =>
                            {
                                var @params = ctor.GetParameters();
                                return @params.Length == 1 && contrArgumentTypes.Any(it => @params[0].ParameterType.IsAssignableFrom(it));
                            });

                            return values =>
                            {
                                var array = Array.CreateInstance(elementType, values.Length);

                                for (var index = 0; index < values.Length; index++)
                                {
                                    array.SetValue(parseValue(elementType, values[index]), index);
                                }

                                return constructor.Invoke(new object[] { array });
                            };
                        }
                    }
                }

                throw new InvalidProgramException("Data type not supported.");
            }

            property.SetValue(this, getConstructor(property.PropertyType)(commandArgs));
        }

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

                ParseArgument(name, property, attribute, commandArgs);   

                index += commandArgs.Length;
            }

            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, new ValidationContext(this), results, true))
            {
                throw new ArgumentException("The follow arguments are missing or are invalid:" + Environment.NewLine +
                    string.Join(Environment.NewLine, results.Select(it => it.ErrorMessage)));
            }
        }
    }
}
