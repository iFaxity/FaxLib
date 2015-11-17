using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System;
using Newtonsoft.Json;

namespace FaxLib {
    [System.Diagnostics.DebuggerStepThrough]
    public class Settings {
        #region Properties & Fields
        Dictionary<string, Setting> _collection;

        /// <summary>
        /// Path to settings file
        /// </summary>
        string Path { get; set; }
        /// <summary>
        /// Saves the file everytime a setting changes
        /// </summary>
        bool AutoSave { get; set; }
        #endregion

        /// <summary>
        /// Constructs a new setting. Used for saving & loading settings in JSON
        /// </summary>
        /// <param name="path">Settings file path</param>
        public Settings(string path = "settings.json", bool autoSave = false) {
            Path = path;
            AutoSave = autoSave;
            // Load file if it exists
            Load(path);
        }

        private Setting this[string key] {
            get {
                return _collection[key];
            }
            set {
                if(_collection.ContainsKey(key))
                    _collection[key] = value;
                else
                    _collection.Add(key, value);
                // If autosave is enabled then save
                if(AutoSave)
                    Save();
            }
        }

        #region Public methods
        /// <summary>
        /// Checks if settings has key
        /// </summary>
        /// <param name="key">Key of setting</param>
        public bool HasKey(string key) {
            return _collection.ContainsKey(key);
        }
        /// <summary>
        /// Loads JSON from either a file or from text
        /// </summary>
        /// <param name="str">File path or text</param>
        public bool Load(string json) {
            // Read JSON from string
            if(File.Exists(json))
                json = File.ReadAllText(json);
            // Read JSON from file
            if(json.StartsWith("{") || json.StartsWith("[")) {
                try {
                    _collection = JsonConvert.DeserializeObject<Dictionary<string, Setting>>(json);
                    return true;
                }
                catch { }
            }
            _collection = new Dictionary<string, Setting>();
            return false;
        }
        /// <summary>
        /// Saves the file
        /// </summary>
        public void Save() {
            File.WriteAllText(Path, JsonConvert.SerializeObject(_collection, Formatting.Indented));
        }
        /// <summary>
        /// Resets all settings to their default values
        /// </summary>
        public void Reset() {
            foreach(var pair in _collection)
                pair.Value.Reset();
        }
        /// <summary>
        /// Resets one setting to it's default value
        /// </summary>
        /// <param name="key">Key of setting</param>
        public void Reset(string key) {
            if(HasKey(key))
                this[key].Reset();
        }

        /// <summary>
        /// Sets a value of a setting. If it does not exists it creates a new Setting
        /// </summary>
        /// <param name="key">Key of setting</param>
        /// <param name="value">New value of setting</param>
        public bool SetValue(string key, object value) {
            if(HasKey(key)) {
                var obj = this[key];
                if(obj.Value.GetType().IsAssignableFrom(value.GetType())) {
                    obj.Value = value;
                    this[key] = obj;
                    return true;
                }
            }
            else {
                this[key] = new Setting() { Default = value, Value = value };
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the value of a setting
        /// </summary>
        /// <param name="key">Key of setting</param>
        public object GetValue(string key) {
            return GetValue<object>(key);
        }
        /// <summary>
        /// Gets the value of a setting with matching type
        /// </summary>
        /// <typeparam name="T">Generic type to cast</typeparam>
        /// <param name="key">Key of setting</param>
        public T GetValue<T>(string key) {
            if(HasKey(key)) {
                try {
                    return (T)Convert.ChangeType(this[key].Value, typeof(T));
                }
                catch { }
            }
            return default(T);
        }
        /// <summary>
        /// Gets the default value of a setting
        /// </summary>
        /// <param name="key">Key of setting</param>
        public object GetDefault(string key) {
            return GetDefault<object>(key);
        }
        /// <summary>
        /// Gets the default value of a setting
        /// </summary>
        /// <typeparam name="T">Generic type to cast</typeparam>
        /// <param name="key">Key of setting</param>
        public T GetDefault<T>(string key) {
            if(HasKey(key)) {
                try {
                    return (T)Convert.ChangeType(this[key].Default, typeof(T));
                }
                catch { }
            }
            return default(T);
        }

        /// <summary>
        /// Removes a setting with the defined key
        /// </summary>
        /// <param name="key">Setting key</param>
        public bool Remove(string key) {
            return _collection.Remove(key);
        }
        #endregion
    }

    [System.Diagnostics.DebuggerStepThrough]
    public struct Setting {
        /// <summary>
        /// Value of this setting
        /// </summary>
        [JsonProperty(PropertyName = "value", Required = Required.AllowNull)]
        public object Value { get; internal set; }

        /// <summary>
        /// Default value of this setting
        /// </summary>
        [JsonProperty(PropertyName = "default", Required = Required.AllowNull)]
        public object Default { get; internal set; }

        /// <summary>
        /// Gets the Default type of this object
        /// </summary>
        [JsonIgnore]
        public Type Type { get { return Default.GetType(); } }

        /// <summary>
        /// Resets this value to default
        /// </summary>
        public void Reset() { Value = Default; }
    }
}