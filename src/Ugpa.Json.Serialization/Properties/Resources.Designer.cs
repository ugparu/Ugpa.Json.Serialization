﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ugpa.Json.Serialization.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ugpa.Json.Serialization.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Inheritance hierarchy of type &quot;{0}&quot; already has configuration for json field &quot;{1}&quot; for type &quot;{2}&quot; on property &quot;{3}&quot;..
        /// </summary>
        internal static string Configurator_InheritancePropertyNameConflict {
            get {
                return ResourceManager.GetString("Configurator_InheritancePropertyNameConflict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expression body must be of type &quot;{0}&quot;..
        /// </summary>
        internal static string Configurator_InvalidExpressionBodyType {
            get {
                return ResourceManager.GetString("Configurator_InvalidExpressionBodyType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to For type &quot;{0}&quot; json field &quot;{1}&quot; already configured on property &quot;{2}&quot;..
        /// </summary>
        internal static string Configurator_PropertyNameConflict {
            get {
                return ResourceManager.GetString("Configurator_PropertyNameConflict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expression mut be MemberExpression..
        /// </summary>
        internal static string ReflectionUtils_NotMemberExpression {
            get {
                return ResourceManager.GetString("ReflectionUtils_NotMemberExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parent expression of MemberExpression must be passed ParameterExpression..
        /// </summary>
        internal static string ReflectionUtils_ParameterNotMemberOwner {
            get {
                return ResourceManager.GetString("ReflectionUtils_ParameterNotMemberOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member &quot;{0}&quot; can ot be configured for type &quot;{1}&quot; because it&apos;s declaring type is &quot;{2}&quot;..
        /// </summary>
        internal static string ReflectionUtils_ReflectedTypeNotMemberOwner {
            get {
                return ResourceManager.GetString("ReflectionUtils_ReflectedTypeNotMemberOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to resolve member &quot;{0}&quot; for type &quot;{1}&quot;..
        /// </summary>
        internal static string ReflectionUtils_UnableToResolveMember {
            get {
                return ResourceManager.GetString("ReflectionUtils_UnableToResolveMember", resourceCulture);
            }
        }
    }
}
