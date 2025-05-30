﻿using com.cyberinternauts.csharp.CmdStarter.Lib.Interfaces;
using System.CommandLine;
using static com.cyberinternauts.csharp.CmdStarter.Lib.Reflection.Helper;

namespace com.cyberinternauts.csharp.CmdStarter.Lib
{
    /// <summary>
    /// Manager for all global options classes found
    /// </summary>
    public sealed class GlobalOptionsManager
    {
        private IEnumerable<Type>? foundTypes;
        private readonly Dictionary<Type, object> globalOptions = new();
        private readonly Starter starter;

        internal GlobalOptionsManager(Starter starter) 
        {
            this.starter = starter;
            // FindTypes() is now called on-demand rather than in constructor
            // to allow setting AssemblyLoadErrorHandler preferences first
        }

        /// <summary>
        /// List of all global options types
        /// </summary>
        public List<Type> GlobalOptionsTypes { get; private set; } = new();

        /// <summary>
        /// Retrieve a global options container of a specific type
        /// </summary>
        /// <typeparam name="GlobalOptionsType">Global options container type to look for</typeparam>
        /// <returns>An <see cref="IGlobalOptionsContainer"/> or null if nothing found</returns>
        public GlobalOptionsType? GetGlobalOptions<GlobalOptionsType>() where GlobalOptionsType : class, IGlobalOptionsContainer
        {
            var type = typeof(GlobalOptionsType);

            return globalOptions.TryGetValue(type, out object? value) ? (GlobalOptionsType)value : null;
        }

        /// <summary>
        /// Add/change a global options container object based on its type
        /// </summary>
        /// <typeparam name="GlobalOptionsType">Global options container type to use as key</typeparam>
        /// <param name="globalOption"></param>
        internal void SetGlobalOptions<GlobalOptionsType>(GlobalOptionsType globalOption) where GlobalOptionsType : class, IGlobalOptionsContainer
        {
            var type = typeof(GlobalOptionsType);

            if (globalOptions.ContainsKey(type)) 
            {
                globalOptions[type] = globalOption;
            }
            else
            {
                globalOptions.Add(type, globalOption);
            }
        }

        internal void FilterTypes()
        {
            FindTypes(); // Ensure types are loaded on-demand
            
            GlobalOptionsTypes.Clear();

            var filteredTypes = FilterTypesByNamespaces(foundTypes!, starter.Namespaces.ToList()); // foundTypes can't be null as initialized by FindTypes()
            filteredTypes = FilterTypesByClasses(filteredTypes, starter.Classes.ToList());

            GlobalOptionsTypes.AddRange(filteredTypes);
        }

        internal void LoadOptions(Command receptacle)
        {
            foreach(var type in GlobalOptionsTypes)
            {
                Reflection.Loader.LoadOptions(type, receptacle);
            }
        }

        private void FindTypes()
        {
            if (foundTypes != null) return;

            var assemblies = starter.AssemblyLoadErrorHandler.GetAssemblies();
            var types = assemblies
                .SelectMany(a => starter.AssemblyLoadErrorHandler.GetTypesFromAssembly(a))
                .Where(t => t.IsClass && t.IsAssignableTo(typeof(IGlobalOptionsContainer)));

            foundTypes = new List<Type>(types);
        }
    }
}
