namespace timesheets.Properties {


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default {
            get {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection RecentJobIds {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["RecentJobIds"]));
            }
            set {
                this["RecentJobIds"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://localhost:7053/")]
        public string ApiBaseUrl {
            get {
                return ((string)(this["ApiBaseUrl"]));
            }
            set {
                this["ApiBaseUrl"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("YOUR_SECURE_KEY")]
        public string ApiKey {
            get {
                return ((string)(this["ApiKey"]));
            }
            set {
                this["ApiKey"] = value;
            }
        }
    }
}
