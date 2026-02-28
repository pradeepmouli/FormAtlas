using System;
using System.Reflection;

namespace FormAtlas.Tool.Metadata
{
    /// <summary>
    /// Base class for reflection-based DevExpress control metadata extraction.
    /// All access to DevExpress types is via reflection to avoid compile-time dependency.
    /// Implementations must catch all exceptions and degrade gracefully.
    /// </summary>
    public abstract class DevExpressReflectionBase
    {
        /// <summary>
        /// Returns the DevExpress control kind string this adapter handles.
        /// </summary>
        public abstract string Kind { get; }

        /// <summary>
        /// Returns true when this adapter can process the given control.
        /// The check is performed by inspecting the control's type name chain.
        /// </summary>
        public abstract bool CanHandle(Type controlType);

        /// <summary>
        /// Safely reads a property from an object via reflection.
        /// Returns default(T) on any failure without throwing.
        /// </summary>
        protected static T? SafeGet<T>(object obj, string propertyName) where T : class
        {
            try
            {
                var prop = obj?.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance);
                return prop?.GetValue(obj) as T;
            }
            catch { return null; }
        }

        /// <summary>
        /// Safely reads a value-type property from an object via reflection.
        /// Returns default(T) on any failure without throwing.
        /// </summary>
        protected static T SafeGetValue<T>(object obj, string propertyName) where T : struct
        {
            try
            {
                var prop = obj?.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance);
                var val = prop?.GetValue(obj);
                if (val is T t) return t;
                return default;
            }
            catch { return default; }
        }

        /// <summary>
        /// Checks whether the given type's full name or base type chain contains the target class name.
        /// Used to recognize DevExpress types without compile-time reference.
        /// </summary>
        protected static bool TypeChainContains(Type type, string className)
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.FullName?.Contains(className) == true)
                    return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
