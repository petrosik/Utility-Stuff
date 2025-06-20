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
            /// Saves the <paramref name="Save"/> by the <paramref name="Name"/> in the save folder
            /// <para>In case it get passed the wrapper Save&lt;T> class as the <paramref name="Save"/> it strips it and saves only the SaveData</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="Name"></param>
            /// <param name="Save"></param>
            public async void Save<T, TPreview>(string Name, T Save, TPreview CustomPreview) where T : notnull
            {
                SaveGeneral(Name, Save, CustomPreview, true);
            }
            /// <summary>
            /// If <paramref name="UpdateWrapper"/> is false it save the <paramref name="Save"/> as is, otherwise it tries to updates wrapper values (LastUpdated, Version, etc.) if the file already exists
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="Name"></param>
            /// <param name="Save"></param>
            /// <param name="UpdateWrapper"></param>
            public async void Save<T, TPreview>(string Name, Save<T, TPreview> Save, bool UpdateWrapper = false) where T : notnull
            {
                SaveGeneral(Name, Save, false, UpdateWrapper);
            }
            /// <summary>
            /// Works same as it's single counter part
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="Saves"></param>
            public async void Save<T, TPreview>(IEnumerable<(string Name, T Save, TPreview CustomPreview)> Saves) where T : notnull
            {
                foreach (var save in Saves)
                {
                    SaveGeneral(save.Name, save.Save, save.CustomPreview, true);
                }
            }
            /// <summary>
            /// Works same as it's single counter part
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="Saves"></param>
            public async void Save<T, TPreview>(IEnumerable<(string Name, Save<T, TPreview> Save, bool UpdateWrapper)> Saves) where T : notnull
            {
                foreach (var save in Saves)
                {
                    SaveGeneral(save.Name, save.Save, null, false, save.UpdateWrapper);
                }
            }
            /// <summary>
            /// Tries to load the save by the <paramref name="Name"/> if it exists
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="Name"></param>
            /// <param name="InfoOnly">Specifies if to load only the Save&lt;<typeparamref name="T"/>> wrapper information</param>
            /// <returns></returns>
            public Save<T, TPreview>? Load<T, TPreview>(string Name, bool InfoOnly) where T : notnull
            {
                return LoadGeneral<T, TPreview>(Name, InfoOnly);
            }
            /// <summary>
            /// Tries to load all saves in the saves folder
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TPreview"></typeparam>
            /// <param name="InfoOnly">Specifies if to load only the Save&lt;<typeparamref name="T"/>,<typeparamref name="TPreview"/>> wrapper information</param>
            /// <returns>
            /// Only valid parsable saves regards to supplied <typeparamref name="T"/> and <typeparamref name="TPreview"/>
            /// </returns>
            public IEnumerable<Save<T, TPreview>?> Load<T, TPreview>(bool InfoOnly) where T : notnull
            {
                var saves = new List<Save<T, TPreview>?>();
                var savefiles = Directory.GetFiles(SavesPath).Select(Path.GetFileName);
                foreach (var save in savefiles)
                {
                    saves.Add(LoadGeneral<T, TPreview>(save, InfoOnly));
                }
                return saves.Where(x => x != null);
            }
            /// <summary>
            /// Tries to load all saves in the saves folder
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="InfoOnly">Specifies if to load only the Save&lt;<typeparamref name="T"/>> wrapper information</param>
            /// <returns>
            /// Only valid parsable saves regards to supplied <typeparamref name="T"/>
            /// </returns>
            public IEnumerable<Save<T>?> Load<T>(bool InfoOnly) where T : notnull
            {
                var saves = new List<Save<T>?>();
                var savefiles = Directory.GetFiles(SavesPath).Select(Path.GetFileName);
                foreach (var save in savefiles)
                {
                    saves.Add(LoadGeneral<T>(save, InfoOnly));
                }
                return saves.Where(x => x != null);
            }
            private void SaveGeneral(string Name, object SaveData, object CustomPreview, bool Strip, bool update = true)
            {
                Name = Name.SanitizeFileName();
                if (string.IsNullOrEmpty(Name) || SaveData == null) return;
                if (Strip && SaveData.GetType().IsGenericType && SaveData.GetType().GetGenericTypeDefinition() == typeof(Save<,>))
                {
                    var saveDataProperty = SaveData.GetType().GetField("SaveData");

                    if (saveDataProperty != null)
                    {
                        SaveData = saveDataProperty.GetValue(SaveData); // Get the SaveData property value
                    }

                    var savepreviewProperty = SaveData.GetType().GetField("CustomPreview");
                    if (savepreviewProperty != null)
                    {
                        if (CustomPreview == null)
                        {
                            CustomPreview = savepreviewProperty.GetValue(SaveData);
                        }
                    }
                }
                var path = Path.Combine(SavesPath, Name);
                if (File.Exists(path))
                {
                    if (Strip)
                    {
                        var old = LoadInfo<object, object>(Name);
                        old.SaveData = SaveData;
                        old.CustomPreview = CustomPreview;
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
                            wr.Write(JsonConvert.SerializeObject(new Save<object, object>(SaveData) { Name = Name, Version = CurrentSaveVersion, CustomPreview = CustomPreview }));
                        }
                        else
                        {
                            wr.Write(JsonConvert.SerializeObject(SaveData));
                        }
                        wr.Flush();
                    }
                }
            }
            private void SaveGeneral(string Name, object SaveData, bool Strip, bool update = true)
            {
                Name = Name.SanitizeFileName();
                if (string.IsNullOrEmpty(Name) || SaveData == null) return;
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
            private Save<T, TPreview>? LoadGeneral<T, TPreview>(string Name, bool InfoOnly)
            {
                var path = Path.Combine(SavesPath, Name.SanitizeFileName());
                if (string.IsNullOrEmpty(Name) || !File.Exists(path)) return null;
                if (InfoOnly)
                {
                    return LoadInfo<T, TPreview>(Name);
                }
                else
                {
                    return JsonConvert.DeserializeObject<Save<T, TPreview>>(File.ReadAllText(path));
                }
            }
            private Save<T>? LoadGeneral<T>(string Name, bool InfoOnly)
            {
                var path = Path.Combine(SavesPath, Name.SanitizeFileName());
                if (string.IsNullOrEmpty(Name) || !File.Exists(path)) return null;
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
            public Save<T, TPreview>? LoadInfo<T, TPreview>(string Name)
            {
                var path = Path.Combine(SavesPath, Name.SanitizeFileName());
                if (string.IsNullOrEmpty(Name) || !File.Exists(path)) return null;
                int len = 0;
                using (var s = new StreamReader(path))
                {
                    char[] buffer = new char[64 + 1024 + 160 + 10];
                    int read = s.Read(buffer, 0, buffer.Length);
                    var savetext = new string(buffer, 0, read);
                    var savedatastr = $"\"{nameof(Petrosik.Utility.Save<T, TPreview>.CustomPreviewSize)}\":";
                    if (!savetext.Contains(savedatastr))
                    {
                        return null;
                    }
                    var dd = savetext.Split(savedatastr)[1];
                    if (!int.TryParse(dd.Substring(0, dd.IndexOf(',')), out len))
                    {
                        return null;
                    }
                }
                using (var s = new StreamReader(path))
                {
                    char[] buffer = new char[64 + 1024 + 160 + 10 + len];
                    int read = s.Read(buffer, 0, buffer.Length);
                    var savetext = new string(buffer, 0, read);


                    var savedatastr = $",\"{nameof(Petrosik.Utility.Save<T, TPreview>.SaveData)}\":";
                    if (savetext.Contains(savedatastr))
                    {
                        savetext = savetext.Substring(0, savetext.LastIndexOf(savedatastr));
                        savetext += '}';
                        return JsonConvert.DeserializeObject<Save<T, TPreview>>(savetext);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            /// <summary>
            /// Loads only Additional Info, if the file exists
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            public Save<T>? LoadInfo<T>(string Name)
            {
                var path = Path.Combine(SavesPath, Name.SanitizeFileName());
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
                var path = Path.Combine(SavesPath, Name.SanitizeFileName());
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
        /// <typeparam name="TPreview"></typeparam>
        public class Save<T, TPreview>
        {
            /// <summary>
            /// Limited to 64 characters
            /// </summary>
            [JsonIgnore]
            public string Name
            {
                get => _Name;
                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        string sanitizedValue = value.SanitizeFileName();

                        _Name = sanitizedValue.Length > 64 ? sanitizedValue.Substring(0, 64) : sanitizedValue;
                    }
                    else
                    {
                        _Name = "save1";
                    }
                }
            }
            [JsonProperty(Order = -10)]
            private string _Name = "save1";

            /// <summary>
            /// Limited to 1024 characters
            /// </summary>
            [JsonIgnore]
            public string Description { get => _Description; set { _Description = _Description = value?.Length > 1024 ? value.Substring(0, 1024) : value ?? ""; } }
            [JsonProperty(Order = -9)]
            private string _Description = "";
            [JsonProperty(Order = -8)]
            public DateTime Created = DateTime.UtcNow;
            [JsonProperty(Order = -7)]
            public DateTime LastUpdated = DateTime.UtcNow;
            [JsonProperty(Order = -6)]
            public Version Version = new Version(0, 0, 0, 0);
            [JsonProperty(Order = -5)]
            public int CustomPreviewSize { get { return JsonConvert.SerializeObject(CustomPreview).Length; } }
            public TPreview CustomPreview = (TPreview)(object)null!;
            public T SaveData;
            public Save(T SaveData, TPreview CustomPreview)
            {
                this.SaveData = SaveData;
                this.CustomPreview = CustomPreview;
            }
            public Save(T SaveData)
            {
                this.SaveData = SaveData;
            }
            public Save()
            {
                SaveData = (T)(object)null!;
            }
            public override string ToString()
            {
                return $"{_Name} V:{Version} - {_Description} | C: {Created} LU: {LastUpdated} CPS: {CustomPreviewSize}";
            }
        }
        /// <summary>
        /// Wrapper for save data with additional information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Save<T>
        {
            /// <summary>
            /// Limited to 64 characters
            /// </summary>
            [JsonIgnore]
            public string Name
            {
                get => _Name;
                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        string sanitizedValue = value.SanitizeFileName();

                        _Name = sanitizedValue.Length > 64 ? sanitizedValue.Substring(0, 64) : sanitizedValue;
                    }
                    else
                    {
                        _Name = "save1";
                    }
                }
            }
            [JsonProperty(Order = -10)]
            private string _Name = "save1";

            /// <summary>
            /// Limited to 1024 characters
            /// </summary>
            [JsonIgnore]
            public string Description { get => _Description; set { _Description = _Description = value?.Length > 1024 ? value.Substring(0, 1024) : value ?? ""; } }
            [JsonProperty(Order = -9)]
            private string _Description = "";
            [JsonProperty(Order = -8)]
            public DateTime Created = DateTime.UtcNow;
            [JsonProperty(Order = -7)]
            public DateTime LastUpdated = DateTime.UtcNow;
            [JsonProperty(Order = -6)]
            public Version Version = new Version(0, 0, 0, 0);
            public T SaveData;
            public Save(T SaveData)
            {
                this.SaveData = SaveData;
            }
            public Save()
            {
                SaveData = (T)(object)null!;
            }
            public override string ToString()
            {
                return $"{_Name} V:{Version} - {_Description} | C: {Created} LU: {LastUpdated}";
            }
        }
    }
}
