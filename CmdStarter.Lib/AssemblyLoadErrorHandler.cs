using System.Reflection;

namespace com.cyberinternauts.csharp.CmdStarter.Lib
{
    /// <summary>
    /// Handles errors related to assembly loading and type resolution
    /// </summary>
    public class AssemblyLoadErrorHandler
    {
        /// <summary>
        /// Specifies how to handle assembly load errors
        /// </summary>
        public enum ErrorHandlingMode
        {
            /// <summary>
            /// Silently ignore errors and continue
            /// </summary>
            Silent,
            
            /// <summary>
            /// Raise events but don't throw exceptions
            /// </summary>
            RaiseEvent,
            
            /// <summary>
            /// Throw exceptions immediately
            /// </summary>
            Throw
        }
        
        /// <summary>
        /// Current error handling mode
        /// </summary>
        public ErrorHandlingMode Mode { get; set; } = ErrorHandlingMode.Silent;
        
        /// <summary>
        /// Event raised when an assembly fails to load
        /// </summary>
        public event EventHandler<AssemblyLoadErrorEventArgs>? AssemblyLoadError;
        
        /// <summary>
        /// Event raised when getting types from an assembly fails
        /// </summary>
        public event EventHandler<TypeLoadErrorEventArgs>? TypeLoadError;
        
        /// <summary>
        /// Safely get assemblies, handling any exceptions according to the current Mode
        /// </summary>
        public IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            
            try
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            }
            catch (Exception ex)
            {
                HandleAssemblyLoadError(null, ex);
            }
            
            return assemblies;
        }
        
        /// <summary>
        /// Safely get types from an assembly, handling any exceptions according to the current Mode
        /// </summary>
        public IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            if (assembly == null) return Enumerable.Empty<Type>();
            
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                HandleTypeLoadError(assembly, ex);
                return ex.Types?.Where(t => t != null) ?? Enumerable.Empty<Type>();
            }
            catch (Exception ex)
            {
                HandleTypeLoadError(assembly, ex);
                return Enumerable.Empty<Type>();
            }
        }
        
        private void HandleAssemblyLoadError(Assembly? assembly, Exception ex)
        {
            var args = new AssemblyLoadErrorEventArgs(assembly, ex);
            
            switch (Mode)
            {
                case ErrorHandlingMode.RaiseEvent:
                    AssemblyLoadError?.Invoke(this, args);
                    break;
                case ErrorHandlingMode.Throw:
                    throw ex;
                case ErrorHandlingMode.Silent:
                default:
                    // Do nothing
                    break;
            }
        }
        
        private void HandleTypeLoadError(Assembly assembly, Exception ex)
        {
            var args = new TypeLoadErrorEventArgs(assembly, ex);
            
            switch (Mode)
            {
                case ErrorHandlingMode.RaiseEvent:
                    TypeLoadError?.Invoke(this, args);
                    break;
                case ErrorHandlingMode.Throw:
                    throw ex;
                case ErrorHandlingMode.Silent:
                default:
                    // Do nothing
                    break;
            }
        }
    }
    
    /// <summary>
    /// Event arguments for assembly load errors
    /// </summary>
    public class AssemblyLoadErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The assembly that failed to load, if available
        /// </summary>
        public Assembly? Assembly { get; }
        
        /// <summary>
        /// The exception that occurred
        /// </summary>
        public Exception Exception { get; }
        
        public AssemblyLoadErrorEventArgs(Assembly? assembly, Exception exception)
        {
            Assembly = assembly;
            Exception = exception;
        }
    }
    
    /// <summary>
    /// Event arguments for type load errors
    /// </summary>
    public class TypeLoadErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The assembly from which types were being loaded
        /// </summary>
        public Assembly Assembly { get; }
        
        /// <summary>
        /// The exception that occurred
        /// </summary>
        public Exception Exception { get; }
        
        public TypeLoadErrorEventArgs(Assembly assembly, Exception exception)
        {
            Assembly = assembly;
            Exception = exception;
        }
    }
}