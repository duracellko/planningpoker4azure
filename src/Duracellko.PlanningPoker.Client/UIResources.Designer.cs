﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duracellko.PlanningPoker.Client {
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
    internal class UIResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UIResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Duracellko.PlanningPoker.Client.UIResources", typeof(UIResources).Assembly);
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
        ///   Looks up a localized string similar to 0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89.
        /// </summary>
        internal static string EstimationDeck_Fibonacci {
            get {
                return ResourceManager.GetString("EstimationDeck_Fibonacci", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1, 2, 3, 4, 5, 6, 7, 8, 9, 10.
        /// </summary>
        internal static string EstimationDeck_Rating {
            get {
                return ResourceManager.GetString("EstimationDeck_Rating", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Rock, Paper, Scissors, Lizard, Spock.
        /// </summary>
        internal static string EstimationDeck_RockPaperScissorsLizardSpock {
            get {
                return ResourceManager.GetString("EstimationDeck_RockPaperScissorsLizardSpock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100.
        /// </summary>
        internal static string EstimationDeck_Standard {
            get {
                return ResourceManager.GetString("EstimationDeck_Standard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to T-shirt: XS, S, M, L, XL.
        /// </summary>
        internal static string EstimationDeck_Tshirt {
            get {
                return ResourceManager.GetString("EstimationDeck_Tshirt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reconnect.
        /// </summary>
        internal static string JoinTeam_ReconnectButton {
            get {
                return ResourceManager.GetString("JoinTeam_ReconnectButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you want to reconnect?.
        /// </summary>
        internal static string JoinTeam_ReconnectMessage {
            get {
                return ResourceManager.GetString("JoinTeam_ReconnectMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reconnect.
        /// </summary>
        internal static string JoinTeam_ReconnectTitle {
            get {
                return ResourceManager.GetString("JoinTeam_ReconnectTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        internal static string MessagePanel_Error {
            get {
                return ResourceManager.GetString("MessagePanel_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection failed. Please, check your internet connection..
        /// </summary>
        internal static string PlanningPokerService_ConnectionError {
            get {
                return ResourceManager.GetString("PlanningPokerService_ConnectionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Service is temporarily not available. Please, try again later..
        /// </summary>
        internal static string PlanningPokerService_UnexpectedError {
            get {
                return ResourceManager.GetString("PlanningPokerService_UnexpectedError", resourceCulture);
            }
        }
    }
}
