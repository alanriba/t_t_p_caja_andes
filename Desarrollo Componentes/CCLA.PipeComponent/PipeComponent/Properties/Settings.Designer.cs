﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PipeComponent.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.7.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("./Log")]
        public string RutaLog {
            get {
                return ((string)(this["RutaLog"]));
            }
            set {
                this["RutaLog"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("componentPipe")]
        public string NombrePipe {
            get {
                return ((string)(this["NombrePipe"]));
            }
            set {
                this["NombrePipe"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SHA1")]
        public string validation {
            get {
                return ((string)(this["validation"]));
            }
            set {
                this["validation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AES")]
        public string decryption {
            get {
                return ((string)(this["decryption"]));
            }
            set {
                this["decryption"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C21F090935F6E49C2C797F69DDEED8402ABD2EE0B667A8B44EA7DD4374267A75D7AD972A119482D15" +
            "A4127461DB1DC347C1A63AE5F1CCFAACFF1B72A7F0A281B")]
        public string validationKey {
            get {
                return ((string)(this["validationKey"]));
            }
            set {
                this["validationKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ABAA84D7EC4BB56D75D217CECFFB9628809BDB8BF91CFCD64568A145BE59719F")]
        public string decryptionKey {
            get {
                return ((string)(this["decryptionKey"]));
            }
            set {
                this["decryptionKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("NII ExD NP-K205")]
        public string Impresora {
            get {
                return ((string)(this["Impresora"]));
            }
            set {
                this["Impresora"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int PuertoComLectorBarra {
            get {
                return ((int)(this["PuertoComLectorBarra"]));
            }
            set {
                this["PuertoComLectorBarra"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HID Global OMNIKEY 5022 Smart Card Reader 0")]
        public string NombreLectorHid {
            get {
                return ((string)(this["NombreLectorHid"]));
            }
            set {
                this["NombreLectorHid"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Wondershare PDFelement")]
        public string ImpresoraLaser {
            get {
                return ((string)(this["ImpresoraLaser"]));
            }
            set {
                this["ImpresoraLaser"] = value;
            }
        }
    }
}
