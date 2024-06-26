namespace Petrosik
{
    namespace Sql
    {
        using Dapper;
        using Petrosik.Enums;
        using System;
        using System.Collections.Generic;
        using System.Data;
        using System.Data.SQLite;
        using System.IO;
        using System.Linq;
        using Utility = Utility.Utility;
        public class SqlManager
        {
            private string filePath = "";

            /// <summary>
            /// Creates new SqlManager and sets path to .db file
            /// </summary>
            /// <param name="path"></param>
            /// <param name="ignoreCheck">Ignore if the .db file doesn't exist</param>
            internal SqlManager(string path, bool ignoreCheck = false)
            {
                SetPath(path, ignoreCheck);
            }

            private string LoadConnectionString()
            {
                return $"Data Source={filePath};Version=3;";
            }
            /// <summary>
            /// Sets path to the .db file
            /// </summary>
            /// <param name="path"></param>
            /// <param name="ignoreCheck">Ignore if the .db file doesn't exist</param>
            public void SetPath(string path, bool ignoreCheck = false)
            {
                if (File.Exists(path) || ignoreCheck)
                {
                    filePath = path;
                }
                else
                {
                    throw new Exception("File does not exists");
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Loads entire table
            /// <para>Needs the table and the output type to have Id property!!!!</para>
            /// </summary>
            /// <typeparam name="TKey"></typeparam>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="tableName"></param>
            /// <param name="dataConverter"></param>
            /// <returns></returns>
            public Dictionary<TKey, TValue>? LoadAll<TKey, TValue>(string tableName, Func<object, TValue> dataConverter)
            {
                try
                {
                    using (IDbConnection connection = new SQLiteConnection(LoadConnectionString()))
                    {
                        var queryResult = connection.Query<object>($"select * from {tableName}", new DynamicParameters());
                        Dictionary<TKey, TValue> dataDictionary = new Dictionary<TKey, TValue>();
                        for (int i = 0; i < queryResult.Count(); i++)
                        {
                            var convertedData = dataConverter(queryResult.ElementAt(i));
                            var id = (TKey)convertedData.GetType().GetProperty("Id").GetValue(convertedData);
                            dataDictionary.Add(id, convertedData);
                        }
                        Utility.ConsoleLog($"SqlManager.LoadAll {tableName} successfull", InfoType.Info);
                        return dataDictionary;
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.LoadAll {tableName} failed");
                    return null;
                }
            }
            /// <summary>
            /// Loads single entry from table
            /// <para>Needs the table and the output type to have Id property!!!!</para>
            /// </summary>
            /// <typeparam name="TId"></typeparam>
            /// <typeparam name="TData"></typeparam>
            /// <param name="id"></param>
            /// <param name="tableName"></param>
            /// <param name="entityConverter"></param>
            /// <returns></returns>
            public TData Load<TId, TData>(TId id, string tableName, Func<object, TData> entityConverter)
            {
                try
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        var output = cnn.QuerySingleOrDefault<object>($"select * from {tableName} where ID = @Id", new { Id = id });
                        if (output != null)
                        {
                            TData entity = entityConverter(output);
                            Utility.ConsoleLog($"SqlManager.Load {tableName} success", InfoType.Info);
                            return entity;
                        }
                        else
                        {
                            Utility.ConsoleLog($"SqlManager.Load {tableName}, {id} doesn't exist", InfoType.Warn);
                            return (TData?)(object)null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.Load {tableName} failed");
                    return (TData?)(object)null;
                }
            }
            /// <summary>
            /// Saves single entry to the database
            /// <para>Uses insert as the db command</para>
            /// </summary>
            /// <typeparam name="TData"></typeparam>
            /// <param name="data"></param>
            /// <param name="tableName"></param>
            public void Save<TData>(TData data, string tableName)
            {
                try
                {
                    var properties = typeof(TData).GetProperties();
                    string propertyNames = string.Join(", ", properties.Select(p => p.Name));
                    string parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute($"insert into {tableName} ({propertyNames}) values ({parameterNames})", data);
                        Utility.ConsoleLog($"SqlManager.Save {tableName} success", InfoType.Info);
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.Save {tableName} failed");
                }
            }
            /// <summary>
            /// Saves list of entries to database
            /// <para>Uses insert as the db command</para>
            /// </summary>
            /// <typeparam name="TData"></typeparam>
            /// <param name="data"></param>
            /// <param name="tableName"></param>
            public void SaveAll<TData>(IEnumerable<TData> data, string tableName)
            {
                try
                {
                    var properties = typeof(TData).GetProperties();
                    string propertyNames = string.Join(", ", properties.Select(p => p.Name));
                    string parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        for (int i = 0; i < data.Count(); i++)
                        {
                            cnn.Execute($"insert into {tableName} ({propertyNames}) values ({parameterNames})", data.ElementAt(i));
                        }
                        Utility.ConsoleLog($"SqlManager.SaveAll {tableName} succes", InfoType.Info);
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.SaveAll {tableName} failed");
                }
            }
            /// <summary>
            /// Updates entry in database
            /// <para>Uses update as the db command</para>
            /// </summary>
            /// <typeparam name="TData"></typeparam>
            /// <param name="user"></param>
            /// <param name="tableName"></param>
            public void Update<TData>(TData user, string tableName)
            {
                try
                {
                    var properties = typeof(TData).GetProperties();
                    string setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute($"update {tableName} set {setClause} where Id = @Id", user);
                    }
                    Utility.ConsoleLog($"SqlManager.Update {tableName} succes", InfoType.Info);
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.Update {tableName} failed");
                }
            }
            /// <summary>
            /// Updates entry in database
            /// <para>Uses insert or replace as the db command</para>
            /// </summary>
            /// <typeparam name="TData"></typeparam>
            /// <param name="data"></param>
            /// <param name="tableName"></param>
            public void UpdateOrUpsert<TData>(TData data, string tableName)
            {
                try
                {
                    var properties = typeof(TData).GetProperties();
                    string propertyNames = string.Join(", ", properties.Select(p => p.Name));
                    string parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute($"insert or replace into {tableName} ({propertyNames}) values ({parameterNames})", data);
                    }
                    Utility.ConsoleLog($"SqlManager.UpdateOrInsert {tableName} succes", InfoType.Info);
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.UpdateOrInsert {tableName} failed");
                }
            }
            /// <summary>
            /// Updates multiple entries in database
            /// <para>Uses insert or replace as the db command</para>
            /// </summary>
            /// <typeparam name="TData"></typeparam>
            /// <param name="data"></param>
            /// <param name="tableName"></param>
            public void UpdateOrUpsertList<TData>(IEnumerable<TData> data, string tableName)
            {
                try
                {
                    var properties = typeof(TData).GetProperties();
                    string propertyNames = string.Join(", ", properties.Select(p => p.Name));
                    string parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        for (int i = 0; i < data.Count(); i++)
                        {
                            cnn.Execute($"insert or replace into {tableName} ({propertyNames}) values ({parameterNames})", data.ElementAt(i));
                        }
                    }
                    Utility.ConsoleLog($"SqlManager.UpdateOrInsertList {tableName} succes", InfoType.Info);
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.UpdateOrInsertList {tableName} failed");
                }
            }
            /// <summary>
            /// Removes entry from database
            /// <para>Uses delete as the db command</para>
            /// </summary>
            /// <param name="Id"></param>
            /// <param name="tableName"></param>
            public void Remove<Tkey>(Tkey Id, string tableName)
            {
                try
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute($"delete from {tableName} where Id={Id}");
                        Utility.ConsoleLog($"SqlManager.Remove {tableName} succes", InfoType.Info);
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.Remove {tableName} failed");
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Basic SqlManager,while it should be functional, it is mainly an EXAMPLE and it probably shoulnd NOT be used!!!
            /// </summary>
            /// <typeparam name="Tkey">Key in the db</typeparam>
            /// <typeparam name="TValue">Data</typeparam>
            /// <typeparam name="TDBValue">DB friendly data</typeparam>
            /// <param name="opt"></param>
            /// <param name="tableName"></param>
            /// <param name="Id"></param>
            /// <param name="data"></param>
            /// <param name="datalist"></param>
            /// <param name="converter">converts from db object to <typeparamref name="TValue"/></param>
            /// <param name="converterToDB">converts from <typeparamref name="TValue"/> to db object</param>
            /// <returns></returns>
            public IEnumerable<TValue>? SqlMainManager<Tkey, TValue, TDBValue>(SQLOptions opt, string tableName, Tkey? Id = default, TValue? data = default, IEnumerable<TValue> datalist = null, Func<object, TValue>? converter = null, Func<TValue, TDBValue>? converterToDB = null)
            {
                switch (opt)
                {
                    case SQLOptions.Save:
                        Save<TValue>(data, tableName);
                        break;
                    case SQLOptions.Load:
                        data = Load(Id, tableName, converter);
                        List<TValue> result = new List<TValue>();
                        if (data != null)
                        {
                            result.Add(data);
                        }
                        return result;
                    case SQLOptions.SaveAll:
                        if (datalist != null)
                        {
                            var r1 = datalist.Select(x => converterToDB.Invoke(x));
                            SaveAll(r1, tableName);
                        }
                        break;
                    case SQLOptions.LoadAll:
                        var rr = LoadAll<Tkey, TValue>(tableName, converter);
                        return rr.Select(x => x.Value);
                    case SQLOptions.Delete:
                        if (Id != null)
                        {
                            Remove(Id, tableName);
                        }
                        break;
                    case SQLOptions.Update:
                        Update(data, tableName);
                        break;
                    case SQLOptions.Sync:
                        var r = datalist.Select(x => converterToDB.Invoke(x));
                        UpdateOrUpsertList(r, tableName);
                        break;
                    default:
                        break;
                }
                return null;
            }
        }
    }
}
