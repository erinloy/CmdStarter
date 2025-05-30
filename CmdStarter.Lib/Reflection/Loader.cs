﻿using com.cyberinternauts.csharp.CmdStarter.Lib.Attributes;
using com.cyberinternauts.csharp.CmdStarter.Lib.Extensions;
using com.cyberinternauts.csharp.CmdStarter.Lib.Interfaces;
using System.CommandLine;
using System.CommandLine.Completions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using static com.cyberinternauts.csharp.CmdStarter.Lib.Reflection.Helper;

namespace com.cyberinternauts.csharp.CmdStarter.Lib.Reflection
{
    internal static class Loader
    {

        internal static void LoadOptions(Type from, Command receptacle)
        {
            var properties = GetProperties(from);

            foreach (var property in properties)
            {
                var isList = property.PropertyType.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));

                var optionType = typeof(Option<>).MakeGenericType(property.PropertyType);
                var constructor = optionType.GetConstructor(new Type[] { typeof(string), typeof(string) });
                var optionName = StarterCommand.OPTION_PREFIX + property.Name.PascalToKebabCase();
                var option = (Option)constructor!.Invoke(new object[] { optionName, string.Empty });
                option.IsRequired = Attribute.IsDefined(property, typeof(RequiredAttribute));
                option.IsHidden = Attribute.IsDefined(property, typeof(HiddenAttribute));
                option.AllowMultipleArgumentsPerToken = isList;
                LoadDescription(property, option);
                LoadAliases(property, option);
                LoadAutoCompletes(property, (completion) => option.AddCompletions(completion));

                if (typeof(IGlobalOptionsContainer).IsAssignableFrom(from))
                {
                    receptacle.AddGlobalOption(option);
                }
                else
                {
                    receptacle.AddOption(option);
                }
            }
        }

        internal static void LoadArguments(MethodInfo method, Command receptacle)
        {
            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.Name == null) continue; // Skipping param without name

                var argumentType = typeof(Argument<>).MakeGenericType(parameter.ParameterType);
                var constructor = argumentType.GetConstructor(Type.EmptyTypes);
                var argument = (Argument)constructor!.Invoke(null);
                argument.Name = parameter.Name;
                argument.IsHidden = Attribute.IsDefined(parameter, typeof(HiddenAttribute));
                if (parameter.DefaultValue is not System.DBNull)
                {
                    argument.SetDefaultValue(parameter.DefaultValue);
                }
                LoadDescription(parameter, argument);
                LoadAutoCompletes(parameter, (completion) => argument.AddCompletions(completion));
                receptacle.Add(argument);
            }
        }

        internal static void LoadDescription(ICustomAttributeProvider provider, Symbol receptacle)
        {
            try
            {
                var descriptions = provider.GetCustomAttributes(false)
                    .Where(a => a is DescriptionAttribute)
                    .Select(a => ((DescriptionAttribute)a).Description);
                var description = descriptions?.Aggregate(
                        new StringBuilder(),
                        (current, next) => current.Append(current.Length == 0 ? "" : StarterCommand.DESCRIPTION_JOINER).Append(next)
                    ).ToString() ?? string.Empty;

                receptacle.Description = description;
            }
            catch (Exception)
            {
                // If there's an error loading descriptions, set an empty description
                receptacle.Description = string.Empty;
            }
        }

        internal static void LoadAliases(ICustomAttributeProvider provider, IdentifierSymbol receptacle)
        {
            try
            {
                var aliases = provider.GetCustomAttributes(false)
                    .Where(a => a is AliasAttribute)
                    .Cast<AliasAttribute>()
                    .SelectMany(a => a.Aliases);

                foreach (var alias in aliases)
                {
                    receptacle.AddAlias(alias);
                }
            }
            catch (Exception)
            {
                // If there's an error loading aliases, continue without adding any
            }
        }

        internal static void LoadAutoCompletes(ICustomAttributeProvider provider, Action<CompletionDelegate> action)
        {
            try
            {
                var completionDelegates = GetAutoCompleteAttributes(provider)
                    .Select(attribute => attribute.Context);

                foreach (var completion in completionDelegates)
                {
                    action(completion);
                }
            }
            catch (Exception)
            {
                // If there's an error loading auto-completes, continue without adding any
            }
        }

        private static IEnumerable<AutoCompleteAttribute> GetAutoCompleteAttributes(ICustomAttributeProvider provider)
        {
            return provider.GetCustomAttributes(false)
                .Where(attribute => attribute is AutoCompleteAttribute)
                .Cast<AutoCompleteAttribute>();
        }
    }
}
