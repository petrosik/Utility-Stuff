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
        using System.Linq.Expressions;
        using System.Reflection;
        using System.Text.RegularExpressions;
        using Utility;
        public class SqlManager
        {
            private string filePath = "";

            /// <summary>
            /// Creates new SqlManager and sets path to .db file
            /// </summary>
            /// <param name="path"></param>
            /// <param name="ignoreCheck">Ignore if the .db file doesn't exist at creation</param>
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
            /// Tries to execute the Command with the params
            /// </summary>
            /// <param name="Command"></param>
            /// <param name="param"></param>
            /// <returns></returns>
            public int? ExecuteCommand(string Command, object? param)
            {
                try
                {
                    using (IDbConnection connection = new SQLiteConnection(LoadConnectionString()))
                    {
                        return connection.Execute(Command, param);
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.ExecuteCommand failed");
                    return null;
                }
            }
            /// <summary>
            /// Tries to execute the QuarryCommand with the params
            /// </summary>
            /// <typeparam name="TKey"></typeparam>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="Command"></param>
            /// <param name="param"></param>
            /// <returns></returns>
            public IEnumerable<object> ExecuteQuarry<TKey, TValue>(string Command, object? param)
            {
                try
                {
                    using (IDbConnection connection = new SQLiteConnection(LoadConnectionString()))
                    {
                        return connection.Query<object>(Command, param);
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog(ex, $"SqlManager.ExecuteQuarry failed");
                    return null;
                }
            }
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

        public static class SqlUtility
        {
            /// <summary>
            /// Parses a string representation of order criteria and returns a list of key selector expressions and sort
            /// directions.
            /// </summary>
            /// <remarks>The method uses regular expressions to parse the input string and generate
            /// expressions for ordering. The key selector expressions are constructed using the specified type
            /// parameters <typeparamref name="T"/> and <typeparamref name="TKey"/>.</remarks>
            /// <typeparam name="T">The type of the objects to be ordered.</typeparam>
            /// <typeparam name="TKey">The type of the key used for ordering.</typeparam>
            /// <param name="orderBy">A string containing property names and sort directions, formatted as "Property:asc" or "Property:desc".
            /// Multiple criteria can be separated by spaces. For example, "Name:asc CreatedAt:desc".</param>
            /// <returns>A list of tuples, each containing a key selector expression and a boolean indicating descending order. Also skips missing properties in the <typeparamref name="T"/> if after there are no valid order properties retuns <see langword="null"/>.
            /// Returns <see langword="null"/> if <paramref name="orderBy"/> is <see langword="null"/> or whitespace.</returns>
            public static List<(Expression<Func<T, TKey>> KeySelector, bool Descending)>? GetOrder<T, TKey>(string? orderBy) where T : class
            {
                if (string.IsNullOrWhiteSpace(orderBy))
                    return null;

                // Matches: "Property":asc|"Property":desc
                // Example: "Name":asc "CreatedAt":desc
                var matches = Regex.Matches(orderBy, @"""((?:\\.|[^""""])*)"":([aA]sc|[dD]esc)");
                var orders = matches
                    .Cast<Match>()
                    .Where(m => typeof(T).GetProperty(m.Groups[1].Value, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null)
                    .Select(m =>
                    {
                        var param = Expression.Parameter(typeof(T), "x");

                        var propInfo = typeof(T).GetProperty(m.Groups[1].Value, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;

                        var property = Expression.Property(param, propInfo);

                        Expression body = typeof(TKey) == property.Type
                            ? property
                            : Expression.Convert(property, typeof(TKey));

                        var lambda = Expression.Lambda<Func<T, TKey>>(body, param);

                        return (KeySelector: lambda, Descending: string.Equals(m.Groups[2].Value, "desc", StringComparison.OrdinalIgnoreCase));
                    })
                    .ToList();
                if (orders.Count == 0)
                    return null;
                return orders;
            }
        }

        public static class Extensions
        {
            /// <summary>
            /// Filters the elements of an <see cref="IQueryable{T}"/> based on a search string with specific property
            /// conditions.
            /// </summary>
            /// <remarks>The search string supports the following operators: <list type="bullet">
            /// <item><description><c>+</c>: Optional inclusion of the property value.</description></item>
            /// <item><description><c>*</c>: Required inclusion of the property value.</description></item>
            /// <item><description><c>-</c>: Exclusion of the property value.</description></item> </list> The method
            /// constructs a predicate based on these conditions and applies it to the query.</remarks>
            /// <typeparam name="T">The type of the elements in the source queryable.</typeparam>
            /// <param name="query">The source queryable to filter.</param>
            /// <param name="search">A search string containing property conditions in the format "property:[+*-]?&quot;value&quot;". The
            /// search string can include multiple conditions separated by spaces.</param>
            /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the input sequence that satisfy the search
            /// conditions. If the search string is null or whitespace, the original query is returned.</returns>
            public static IQueryable<T> Search<T>(this IQueryable<T> query, string? search) where T : class
            {
                if (string.IsNullOrWhiteSpace(search))
                    return query;

                // Matches: property:[+*-]?"value"
                // Example: Name:+"gold" _tags.name:*"sharp"
                // name:+"gold" _tags.name:-"sharp" _components.name.name:-"mightydumbbell"
                var matches = Regex.Matches(search, @"([\w\.]+):([+\-\*]?)""((?:\\.|[^""])*)""");

                var filters = matches.Cast<Match>()
                     .Select(m => new
                     {
                         PropertyPath = m.Groups[1].Value,
                         Operator = string.IsNullOrEmpty(m.Groups[2].Value) ? "+" : m.Groups[2].Value,
                         Value = m.Groups[3].Value
                     })
                     .Where(x => !string.IsNullOrEmpty(x.Value))
                     //.GroupBy(f => f.PropertyPath)
                     //.Select(g => g.Last())
                     .ToList();

                var parameter = Expression.Parameter(typeof(T), "x");
                Expression? mustInclude = null; // for "*" (required)
                Expression? shouldInclude = null; // for "+" (optional)
                Expression? mustExclude = null; // for "-" (excluded)

                foreach (var filter in filters)
                {
                    var path = filter.PropertyPath.Split('.');
                    var op = filter.Operator;
                    var value = filter.Value;

                    var expr = BuildPropertyAccess(parameter, typeof(T), path, 0, value);
                    if (expr == null) continue;

                    if (op == "-" && expr != null)
                    {
                        expr = Expression.Not(expr);
                        mustExclude = mustExclude == null ? expr : Expression.AndAlso(mustExclude, expr);
                    }
                    else if (op == "*")
                    {
                        mustInclude = mustInclude == null ? expr : Expression.AndAlso(mustInclude, expr);
                    }
                    else // default or "+"
                    {
                        shouldInclude = shouldInclude == null ? expr : Expression.OrElse(shouldInclude, expr);
                    }
                }

                // Combine priority: required (*) AND (optional (+)) AND (not (-))
                Expression? predicate = mustInclude;

                if (shouldInclude != null)
                    predicate = predicate == null ? shouldInclude : Expression.AndAlso(predicate, shouldInclude);

                if (mustExclude != null)
                    predicate = predicate == null ? mustExclude : Expression.AndAlso(predicate, mustExclude);

                if (predicate == null)
                    return query;

                var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
                return query.Where(lambda);
            }

            private static Expression? BuildPropertyAccess(Expression instance, Type type, string[] path, int index, string filterValue)
            {
                if (index >= path.Length)
                    return null;

                var property = type.GetProperty(path[index],
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    return null;

                var member = Expression.Property(instance, property);
                var memberType = property.PropertyType;

                bool isEnumerable =
                     memberType != typeof(string) &&
                     (typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType) ||
                     (memberType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(memberType.GetGenericTypeDefinition())));

                if (isEnumerable)
                {
                    var elementType = memberType.IsGenericType
                        ? memberType.GetGenericArguments()[0]
                        : typeof(object);

                    var elementParam = Expression.Parameter(elementType, "e");

                    Expression? elementBody;
                    if (index + 1 < path.Length)
                        elementBody = BuildPropertyAccess(elementParam, elementType, path, index + 1, filterValue);
                    else
                        elementBody = BuildContainsExpression(elementParam, elementType, filterValue);

                    if (elementBody == null)
                        return null;

                    var anyLambda = Expression.Lambda(elementBody, elementParam);
                    var anyMethod = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(elementType);

                    return Expression.Call(anyMethod, member, anyLambda);
                }
                else
                {
                    if (index == path.Length - 1)
                        return BuildContainsExpression(member, memberType, filterValue);

                    return BuildPropertyAccess(member, memberType, path, index + 1, filterValue);
                }
            }

            private static Expression? BuildContainsExpression(Expression member, Type memberType, string filterValue)
            {
                if (memberType == typeof(string))
                {
                    // Case-insensitive contains
                    var notNull = Expression.NotEqual(member, Expression.Constant(null));
                    var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                    var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
                    var memberToLower = Expression.Call(member, toLower);
                    var constant = Expression.Constant(filterValue.Replace("\\\"", "\"").ToLower());
                    var contains = Expression.Call(memberToLower, containsMethod, constant);
                    return Expression.AndAlso(notNull, contains);
                }

                if (memberType.IsValueType)
                {
                    var toString = memberType.GetMethod(nameof(object.ToString), Type.EmptyTypes)!;
                    var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                    var toStringCall = Expression.Call(member, toString);
                    var memberToLower = Expression.Call(toStringCall, toLower);
                    var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
                    var constant = Expression.Constant(filterValue.Replace("\\\"", "\"").ToLower());
                    return Expression.Call(memberToLower, containsMethod, constant);
                }

                return null;
            }

            public static IQueryable<T> GetPageContentRaw<T, TKey>(this IQueryable<T> query, int skip, int take, Expression<Func<T, TKey>>? orderBy = null, bool Reverse = false) where T : class
            {
                if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative.");
                if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");

                if (orderBy != null)
                {
                    if (Reverse)
                    {
                        return query.OrderByDescending(orderBy).Skip(skip).Take(take);
                    }
                    else
                    {
                        return query.OrderBy(orderBy).Skip(skip).Take(take);
                    }
                }

                return query.Skip(skip).Take(take);
            }
            /// <summary>
            /// Gets a 'page' of elements from the query
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TKey"></typeparam>
            /// <param name="query"></param>
            /// <param name="page">Index of the page</param>
            /// <param name="orderBy"></param>
            /// <param name="Reverse">Reverses the querry</param>
            /// <param name="pageSize">Size of the page</param>
            /// <returns></returns>
            public static IQueryable<T> GetPageContent<T, TKey>(this IQueryable<T> query, int page, Expression<Func<T, TKey>>? orderBy = null, bool Reverse = false, int? pageSize = null) where T : class
            {
                if (page < 0) page = 0;
                if (pageSize == null || pageSize <= 0)
                {
                    pageSize = 100;
                }

                int skip = page * (int)pageSize;
                return query.GetPageContentRaw(skip, (int)pageSize, orderBy, Reverse);
            }
            public static IQueryable<T> GetPageContentRaw<T>(this IQueryable<T> query, int skip, int take) where T : class
            {
                if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative.");
                if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");

                return query.Skip(skip).Take(take);
            }
            /// <summary>
            /// Gets a 'page' of elements from the query without any ordering
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="query"></param>
            /// <param name="page">Index of the page</param>
            /// <param name="pageSize">Size of the page</param>
            /// <returns></returns>
            public static IQueryable<T> GetPageContent<T>(this IQueryable<T> query, int page, int? pageSize = null) where T : class
            {
                if (page < 0) page = 0;
                if (pageSize == null || pageSize <= 0)
                {
                    pageSize = 100;
                }

                int skip = page * (int)pageSize;
                return query.GetPageContentRaw(skip, (int)pageSize);
            }

            public static IQueryable<T> GetPageContentRaw<T, TKey>(this IQueryable<T> query, int skip, int take, List<(Expression<Func<T, TKey>> KeySelector, bool Descending)>? orderBys = null) where T : class
            {
                if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative.");
                if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");

                if (orderBys != null && orderBys.Any())
                {
                    IOrderedQueryable<T> orderedQuery = null!;

                    for (int i = 0; i < orderBys.Count; i++)
                    {
                        var (keySelector, descending) = orderBys[i];

                        if (i == 0)
                        {
                            orderedQuery = descending
                                ? query.OrderByDescending(keySelector)
                                : query.OrderBy(keySelector);
                        }
                        else
                        {
                            orderedQuery = descending
                                ? orderedQuery.ThenByDescending(keySelector)
                                : orderedQuery.ThenBy(keySelector);
                        }
                    }

                    return orderedQuery.Skip(skip).Take(take);
                }

                return query.Skip(skip).Take(take);
            }
            /// <summary>
            /// Gets a 'page' of elements from the query with multiple selectors
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TKey"></typeparam>
            /// <param name="query"></param>
            /// <param name="page">Index of the page</param>
            /// <param name="orderBys"></param>
            /// <param name="pageSize">Size of the page</param>
            /// <returns></returns>
            public static IQueryable<T> GetPageContent<T, TKey>(this IQueryable<T> query, int page, List<(Expression<Func<T, TKey>> KeySelector, bool Descending)>? orderBys = null, int? pageSize = null) where T : class
            {
                if (page < 0) page = 0;
                if (pageSize == null || pageSize <= 0)
                {
                    pageSize = 100;
                }

                int skip = page * (int)pageSize;
                return query.GetPageContentRaw(skip, (int)pageSize, orderBys);
            }
        }
    }
}
