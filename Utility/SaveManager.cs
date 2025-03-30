namespace Petrosik
{
    namespace Utility
    {
        using Newtonsoft.Json;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;

        public class SaveManager
        {
            public string SavesPath = null;
            public Version CurrentSaveVersion = new Version(0, 0, 0, 0);
            public SaveManager(string SavesFolderPath)
            {
                SavesPath = SavesFolderPath;
                if (!Directory.Exists(SavesPath))
                {
                    Directory.CreateDirectory(SavesPath);
                }
            }
            public SaveManager(string SavesFolderName, Environment.SpecialFolder SpecialFolder)
            {
                SavesPath = Path.Combine(Environment.GetFolderPath(SpecialFolder), SavesFolderName);
                if (!Directory.Exists(SavesPath))
                {
                    Directory.CreateDirectory(SavesPath);
                }
            }
            /// <summary>
            /// Saves the <paramref name="Save"/> by the <paramref name="Name"/> in the save folder
            /// <para>In case it get passed the wrapper Save&lt;T> class as the <paramref name="Save"/> it strips it and saves only the SaveData</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Name"></param>
            /// <param name="Save"></param>
            public async void Save<T>(string Name, T Save) where T : notnull
            {
                SaveGeneral(Name, Save, true);
            }
            /// <summary>
            /// If <paramref name="UpdateWrapper"/> is false it save the <paramref name="Save"/> as is, otherwise it tries to updates wrapper values (LastUpdated, Version, etc.) if the file already exists
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Name"></param>
            /// <param name="Save"></param>
            /// <param name="UpdateWrapper"></param>
            public async void Save<T>(string Name, Save<T> Save, bool UpdateWrapper = false) where T : notnull
            {
                SaveGeneral(Name, Save, false, UpdateWrapper);
            }
            /// <summary>
            /// Works same as it's single counter part
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Saves"></param>
            public async void Save<T>(IEnumerable<KeyValuePair<string, T>> Saves) where T : notnull
            {
                foreach (var save in Saves)
                {
                    SaveGeneral(save.Key, save.Value, true);
                }
            }
            /// <summary>
            /// Works same as it's single counter part
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Saves"></param>
            public async void Save<T>(IEnumerable<(string Name, Save<T> Save, bool UpdateWrapper)> Saves) where T : notnull
            {
                foreach (var save in Saves)
                {
                    SaveGeneral(save.Name, save.Save, false, save.UpdateWrapper);
                }
            }
            /// <summary>
            /// Tries to load the save by the <paramref name="Name"/> if it exists
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Name"></param>
            /// <param name="InfoOnly">Specifies if to load only the Save&lt;<typeparamref name="T"/>> wrapper information</param>
            /// <returns></returns>
            public Save<T>? Load<T>(string Name, bool InfoOnly) where T : notnull
            {
                return LoadGeneral<T>(Name, InfoOnly);
            }
            /// <summary>
            /// Tries to load all saves in the saves folder
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="InfoOnly">Specifies if to load only the Save&lt;<typeparamref name="T"/>> wrapper information</param>
            /// <returns></returns>
            public IEnumerable<Save<T>?> Load<T>(bool InfoOnly) where T : notnull
            {
                var saves = new List<Save<T>?>();
                var savefiles = Directory.GetFiles(SavesPath);
                foreach (var save in savefiles)
                {
                    saves.Add(LoadGeneral<T>(save, InfoOnly));
                }
                return saves;
            }
            private void SaveGeneral(string Name, object SaveData, bool Strip, bool update = true)
            {
                if (Name == null || Name == "" || SaveData == null) return;
                if (Strip && SaveData.GetType().IsGenericType && SaveData.GetType().GetGenericTypeDefinition() == typeof(Save<>))
                {
                    var saveDataProperty = SaveData.GetType().GetField("SaveData");

                    if (saveDataProperty != null)
                    {
                        SaveData = saveDataProperty.GetValue(SaveData); // Get the SaveData property value
                    }
                }
                var path = Path.Combine(SavesPath, Name);
                if (File.Exists(path))
                {
                    if (Strip)
                    {
                        var old = LoadInfo<object>(Name);
                        old.SaveData = SaveData;
                        if (update)
                        {
                            old.LastUpdated = DateTime.UtcNow;
                            old.Version = CurrentSaveVersion;
                            old.Name = Name;
                        }
                        File.WriteAllText(path, JsonConvert.SerializeObject(old));
                    }
                    else
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(SaveData));
                    }
                }
                else
                {
                    using (var wr = File.CreateText(path))
                    {
                        if (Strip)
                        {
                            wr.Write(JsonConvert.SerializeObject(new Save<object>(SaveData) { Name = Name, Version = CurrentSaveVersion }));
                        }
                        else
                        {
                            wr.Write(JsonConvert.SerializeObject(SaveData));
                        }
                        wr.Flush();
                    }
                }
            }
            private Save<T>? LoadGeneral<T>(string Name, bool InfoOnly)
            {
                var path = Path.Combine(SavesPath, Name);
                if (Name == null || Name == "" || !File.Exists(path)) return null;
                if (InfoOnly)
                {
                    return LoadInfo<T>(Name);
                }
                else
                {
                    return JsonConvert.DeserializeObject<Save<T>>(File.ReadAllText(path));
                }
            }
            /// <summary>
            /// Loads only Additional Info, if the file exists
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            public Save<T>? LoadInfo<T>(string Name)
            {
                var path = Path.Combine(SavesPath, Name);
                if (string.IsNullOrEmpty(Name) || !File.Exists(path)) return null;
                using (var s = new StreamReader(path))
                {
                    char[] buffer = new char[64 + 1024 + 150];
                    int read = s.Read(buffer, 0, buffer.Length);
                    var savetext = new string(buffer, 0, read);
                    var savedatastr = $",\"{nameof(Petrosik.Utility.Save<T>.SaveData)}\":";
                    if (savetext.Contains(savedatastr))
                    {
                        savetext = savetext.Substring(0, savetext.LastIndexOf(savedatastr));
                        savetext += '}';
                        return JsonConvert.DeserializeObject<Save<T>>(savetext);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            /// <summary>
            /// Tries to deletes the save file in the save folder if it exists
            /// </summary>
            /// <param name="Name"></param>
            public async void Delete(string Name)
            {
                var path = Path.Combine(SavesPath, Name);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        /// <summary>
        /// Wrapper for save data with additional information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Save<T>
        {
            [JsonIgnore]
            public string Name
            {
                get => _Name;
                set
                {
                    if (value != null)
                    {
                        var invalid = Path.GetInvalidFileNameChars();
                        // Remove invalid filename characters
                        string sanitizedValue = string.Concat(value.Where(c => !invalid.Contains(c)));

                        _Name = sanitizedValue.Length > 64 ? sanitizedValue.Substring(0, 64) : sanitizedValue;
                    }
                    else
                    {
                        _Name = "";
                    }
                }
            }
            [JsonProperty] 
            private string _Name = "";

            [JsonIgnore] 
            public string Description { get => _Description; set { _Description = _Description = value?.Length > 1024 ? value.Substring(0, 1024) : value ?? ""; } }
            [JsonProperty] 
            private string _Description = "";
            public DateTime Created = DateTime.UtcNow;
            public DateTime LastUpdated = DateTime.UtcNow;
            public Version Version = new Version(0, 0, 0, 0);
            public T SaveData;
            public Save(T SaveData)
            {
                this.SaveData = SaveData;
            }
        }
    }
}
