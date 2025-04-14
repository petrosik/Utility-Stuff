namespace Petrosik
{
    namespace VersioningTool
    {
        using Petrosik.Enums;
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reflection;
        using System.Reflection.Emit;
        using Utility = Utility.Utility;

        public class VersioningTool<TIn, TOut> : IEnumerable<VersioningAction> where TIn : notnull where TOut : notnull
        {
            private Dictionary<Version, VersioningAction> VersioningActions = new();
            public IReadOnlyDictionary<Version, VersioningAction> Actions
            {
                get
                {
                    return VersioningActions.OrderBy(x => x.Key).ToDictionary();
                }
            }
            internal VersioningTool()
            {
                if (!typeof(TIn).IsClass || (typeof(TIn).IsValueType && !typeof(TIn).IsEnum))
                {
                    throw new Exception($"TIn({typeof(TIn)}) or TOut({typeof(TOut)}) are not class or struct, which are only supported types");
                }
            }
            /// <summary>
            /// Adds Action
            /// </summary>
            /// <param name="version"></param>
            /// <param name="action"></param>
            /// <exception cref="Exception"></exception>
            internal void Add(Version version, VersioningAction action)
            {
                if (!VersioningActions.ContainsKey(version))
                {
                    VersioningActions.Add(version, action);
                }
                else
                {
                    throw new Exception($"You already have action for this version({version}), either remove it or change the input version.");
                }
            }
            /// <summary>
            /// Removes Action
            /// </summary>
            /// <param name="version"></param>
            /// <exception cref="Exception"></exception>
            public void Remove(Version version)
            {
                if (!VersioningActions.ContainsKey(version))
                {
                    throw new Exception($"{version} does not exists");
                }
                VersioningActions.Remove(version);
            }
            /// <summary>
            /// Modifies Action specified by <paramref name="version"/>
            /// </summary>
            /// <param name="version"></param>
            /// <param name="action"></param>
            public void Modify(Version version, VersioningAction action)
            {
                if (VersioningActions.ContainsKey(version))
                {
                    VersioningActions[version] = action;
                }
                else
                {
                    throw new Exception($"{version} does not exists");
                }
            }
            /// <summary>
            /// Insesrts <paramref name="action"/> into the list. If the <paramref name="version"/> is already inside it shifts it and all after that conflicts
            /// </summary>
            /// <param name="version"></param>
            /// <param name="action"></param>
            public void Insert(Version version, VersioningAction action)
            {
                if (!VersioningActions.ContainsKey(version))
                {
                    Add(version, action);
                }
                else
                {
                    var tocheck = Actions.OrderBy(x => x.Key)
                        .Where(x => x.Key >= version).ToList();
                    var checke = tocheck.TakeWhile((x, index) =>
                    {
                        if (index + 1 < tocheck.Count())
                        {
                            VersioningCheck(x.Key, tocheck.ElementAt(index + 1).Key, out var b);
                            if (b)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }).Reverse().ToList();
                    //VersioningCheck()
                    try
                    {
                        foreach (var item in checke)
                        {
                            int[] verint = new int[4] { item.Key.Major, item.Key.Minor, item.Key.Build, item.Key.Revision };
                            verint[VersioningCheck(item.Key, tocheck.ElementAt(checke.Count()).Key, out _)]++;
                            Shift(item.Key, new Version(verint[0], verint[1], verint[2], verint[3]));
                        }
                        Add(version, action);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Failed to insert | {e}");
                    }
                }
            }
            /// <summary>
            /// Moves the action on <paramref name="oldVersion"/> to the <paramref name="newVersion"/>
            /// </summary>
            /// <param name="oldVersion"></param>
            /// <param name="newVersion"></param>
            /// <exception cref="Exception"></exception>
            public void Shift(Version oldVersion, Version newVersion)
            {
                if (oldVersion == newVersion)
                {
                    throw new Exception($"old version ({oldVersion}) is the same as  new version ({newVersion})");
                }
                if (!VersioningActions.ContainsKey(oldVersion))
                {
                    throw new Exception($"{oldVersion} does not exists");
                }
                else if (VersioningActions.ContainsKey(newVersion))
                {
                    throw new Exception($"You already have action for this version({newVersion}), either remove it or change the input version.");
                }
                else
                {
                    var action = VersioningActions[oldVersion];
                    Remove(oldVersion);
                    Add(newVersion, action);
                }
            }
            /// <summary>
            /// Removes all Actions
            /// </summary>
            public void Clear()
            {
                VersioningActions = new();
            }
            /// <summary>
            /// Applies the updates to the <paramref name="obj"/> that are > than <paramref name="current"/> and &lt;= than <paramref name="target"/> and tries to convert it to the output type
            /// <para>Make sure TOut type has empty constructor (no parameters) otherwise use the SimpleUpdate</para>
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="current"></param>
            /// <param name="target"></param>
            public TOut CompleteUpdate(TIn obj, Version current, Version target)
            {
                if (typeof(TOut) != typeof(object) && typeof(TOut).GetConstructor(Type.EmptyTypes) != null)
                {
                    var tmp = CopyValues(Activator.CreateInstance(typeof(TOut))!, SimpleUpdate(obj, current, target));
                    return (TOut)tmp;
                }
                else if (typeof(TOut).GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new Exception($"The output type is {typeof(TOut).Name} and does not have empty constructor. Use SimpleUpdate instead");
                }
                else if (typeof(TOut) == typeof(object))
                {
                    throw new Exception($"The output type is {typeof(TOut).Name} which cannot be converted to. Use SimpleUpdate instead");
                }
                else
                {
                    throw new Exception("Something went worng");
                }
            }
            /// <summary>
            /// Applies the updates to the <paramref name="obj"/> that are > than <paramref name="current"/> and &lt;= than <paramref name="target"/>
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="current"></param>
            /// <param name="target"></param>
            public object SimpleUpdate(object obj, Version current, Version target)
            {
                var acts = Actions.Where(x => x.Key > current && x.Key <= target && x.Value.Type != VersioningActionType.None)
                    .Select(x => new KeyValuePair<Version, VersioningAction>(x.Key, x.Value))
                    .OrderBy(x => x.Key);

                foreach (var act in acts)
                {
                    try
                    {
                        obj = ApplyAction(obj, act.Value);
                    }
                    catch (Exception e)
                    {
                        Utility.ConsoleLog(e, $"Action {act}");
                    }
                }
                return obj;
            }
            private object ApplyAction(object obj, VersioningAction action)
            {
                var oi = obj.GetType();
                if (action.Type == VersioningActionType.ModifyValue)
                {
                    var aa = GetEither(action);
                    var property = oi.GetProperty(aa.name, aa.type);
                    var field = oi.GetField(aa.name);
                    if (property == null && field == null)
                    {
                        return obj;
                    }

                    if (field != null)
                    {
                        field.SetValue(obj, action.Value != null ? Convert.ChangeType(action.Value, field.FieldType) : action.ValueConverter.Invoke(field.GetValue(obj)));
                    }
                    else if (property != null)
                    {
                        property.SetValue(obj, action.Value != null ? Convert.ChangeType(action.Value, property.PropertyType) : action.ValueConverter.Invoke(property.GetValue(obj)));
                    }
                    return obj;
                }

                AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

                TypeBuilder? typeBuilder = null;
                if (action.Type == VersioningActionType.ModifyProperty || action.Type == VersioningActionType.RemoveProperty)
                {
                    typeBuilder = moduleBuilder.DefineType($"{oi.Name}Modif", oi.Attributes, null);
                }
                else
                {
                    typeBuilder = moduleBuilder.DefineType($"{oi.Name}Modif", oi.Attributes, oi);
                }

                //construct method
                //MethodBuilder methodBuilder = typeBuilder.DefineMethod("DynamicMethod", MethodAttributes.Public | MethodAttributes.Static, typeof(void), null);

                //ILGenerator ilGenerator = methodBuilder.GetILGenerator();
                //ilGenerator.Emit(OpCodes.Ldstr, "Dynamic class");
                //ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                //ilGenerator.Emit(OpCodes.Ret);

                switch (action.Type)
                {
                    case VersioningActionType.ModifyValue:
                        //done above, its useless to create custom assembly for this
                        break;
                    case VersioningActionType.AddProperty:
                        if (action.IsField)
                        {
                            typeBuilder.DefineField(action.OriginName, action.OriginType, action.FAttributes);
                        }
                        else
                        {
                            typeBuilder.DefineProperty(action.OriginName, action.PAttributes, action.OriginType, null);
                        }
                        break;
                    case VersioningActionType.RemoveProperty:
                        var aa = GetEither(action);
                        typeBuilder = CopyMembers(typeBuilder, oi, new() { (aa.type, aa.name) });
                        break;
                    case VersioningActionType.ModifyProperty:
                        typeBuilder = CopyMembers(typeBuilder, oi, new() { (action.OriginType, action.OriginName) });
                        if (action.IsField)
                        {
                            typeBuilder.DefineField(action.TargetName, action.TargetType, action.FAttributes);
                        }
                        else
                        {
                            typeBuilder.DefineProperty(action.TargetName, action.PAttributes, action.TargetType, null);
                        }
                        break;
                    case VersioningActionType.AddMethod:
                    case VersioningActionType.RemoveMethod:
                    case VersioningActionType.ModifyMethod:
                        Utility.ConsoleLog($"Action {action.Type} is not implemented", InfoType.Warn);
                        break;
                    default:
                        Utility.ConsoleLog($"Action {action.Type} is not processable", InfoType.Error);
                        break;
                }

                if (oi.GetConstructor(Type.EmptyTypes) == null)
                {
                    ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                    ILGenerator ctorIlGenerator = constructorBuilder.GetILGenerator();
                    ctorIlGenerator.Emit(OpCodes.Ret);
                }

                //creating type and filling it
                Type dynamicType = typeBuilder.CreateType();
                var obj1 = Activator.CreateInstance(dynamicType);

                obj1 = CopyValues(obj1, obj);
                if (action.Type == VersioningActionType.AddProperty)
                {
                    if (action.IsField)
                    {
                        dynamicType.GetField(action.OriginName)?.SetValue(obj1, action.Value);
                    }
                    else
                    {
                        dynamicType.GetProperty(action.OriginName)?.SetValue(obj1, action.Value);
                    }
                }
                else if (action.Type == VersioningActionType.ModifyProperty)
                {
                    //might need check for same type and copy it
                    if (action.IsField)
                    {
                        dynamicType.GetField(action.TargetName)?.SetValue(obj1, action.ValueConverter != null ? action.ValueConverter(oi.GetField(action.OriginName).GetValue(obj)) : null);
                    }
                    else
                    {
                        dynamicType.GetProperty(action.TargetName)?.SetValue(obj1, action.ValueConverter != null ? action.ValueConverter(oi.GetProperty(action.OriginName).GetValue(obj)) : null);
                    }
                }
                return obj1;
            }
            private TypeBuilder CopyMembers(TypeBuilder builder, Type basetype, List<(Type? type, string? name)> blacklist)
            {
                foreach (var field in basetype.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (!blacklist.Any(x => x.name == field.Name && x.type == field.FieldType))
                    {
                        builder.DefineField(field.Name, field.FieldType, field.Attributes);
                    }
                }
                foreach (var property in basetype.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (!blacklist.Any(x => x.name == property.Name && x.type == property.PropertyType))
                    {
                        builder.DefineProperty(property.Name, property.Attributes, property.PropertyType, property.GetIndexParameters().Select(x => x.ParameterType).ToArray());
                    }
                }
                return builder;
            }
            private object CopyValues(object obj, object oldobj)
            {
                var btype = oldobj.GetType();
                var ntype = obj.GetType();
                foreach (var field in btype.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (ntype.GetField(field.Name) != null)
                        ntype.GetField(field.Name)?.SetValue(obj, btype?.GetField(field.Name)?.GetValue(oldobj));
                }
                foreach (var property in btype!.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (ntype.GetProperty(property.Name) != null)
                        ntype.GetProperty(property.Name)?.SetValue(obj, btype?.GetProperty(property.Name)?.GetValue(oldobj));
                }
                return obj;
            }
            private (string? name, Type? type) GetEither(VersioningAction action)
            {
                string? outn = null;
                Type? outtype = null;
                if (action.OriginName != string.Empty && action.OriginName != null && (action.TargetName == string.Empty || action.TargetName == null))
                {
                    outn = action.OriginName;
                }
                else if ((action.OriginName == string.Empty || action.OriginName == null) && action.TargetName != string.Empty && action.TargetName != null)
                {
                    outn = action.TargetName;
                }

                if (action.OriginType != null && action.TargetType == null)
                {
                    outtype = action.OriginType;
                }
                else if (action.OriginType == null && action.TargetType != null)
                {
                    outtype = action.TargetType;
                }

                if (outn == null)
                {
                    outn = action.OriginName;
                }
                if (outtype == null)
                {
                    outtype = action.OriginType;
                }
                return (outn, outtype);
            }
            /// <summary>
            /// Tries to generate actions that will change the current object into the target. Additionally you can specify what version they will start and tries to fit into the <paramref name="maxv"></paramref> version
            /// </summary>
            /// <warning>Deletes all current actions!</warning>
            /// <note>I give up on trying to also get difference between values. mby in the future(prob not)</note>
            /// <param name="current"></param>
            /// <param name="target"></param>
            /// <param name="currentv"></param>
            /// <param name="maxv"></param>
            public void AutoGenerateVersions(object current, object target, Version? currentv = null, Version? maxv = null)
            {
                if (current.Equals(target) || current.Equals(null) || target.Equals(null))
                {
                    Utility.ConsoleLog($"The objects are the same or one of them is null. {current}=>{target}", InfoType.Warn);
                    return;
                }
                if (Actions.Count > 0)
                {
                    Clear();
                }

                //version stuff
                Version version;
                Version maxversion;
                if (currentv != null)
                {
                    version = currentv;
                }
                else
                {
                    version = new(0, 0, 0, 1);
                }
                if (maxv != null)
                {
                    maxversion = maxv;
                }
                else
                {
                    //if you have bigger build numbers than this idk what r u doing
                    maxversion = new(99999, 99999, 99999, 99999);
                }
                int versionpos = VersioningCheck(version, maxversion, out _);
                int[] verint = new int[4] { version.Major, version.Minor, version.Build, version.Revision };

                //actuall stuff
                var tartype = target.GetType();
                var currtype = current.GetType();
                foreach (var field in tartype.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    versionpos = VersioningCheck(version, maxversion, out _);
                    var cf = currtype.GetField(field.Name);
                    if (cf != null && cf.Attributes == field.Attributes && cf.FieldType == field.FieldType)
                    {
                        continue;
                    }
                    if (cf == null)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(VersioningActionType.AddProperty, field.Name, field.FieldType, null) { FAttributes = field.Attributes, IsField = true });
                    }
                    else if (cf.Attributes != field.Attributes)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(cf.Name, cf.FieldType, field.Name, field.FieldType, obj => obj) { FAttributes = field.Attributes, IsField = true });
                    }
                    else if (cf.FieldType != field.FieldType)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(cf.Name, cf.FieldType, field.Name, field.FieldType, null) { FAttributes = field.Attributes, IsField = true });
                    }
                    verint[versionpos]++;
                }
                foreach (var property in tartype.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    versionpos = VersioningCheck(version, maxversion, out _);
                    var cf = currtype.GetProperty(property.Name);
                    if (cf != null && cf.Attributes == property.Attributes && cf.PropertyType == property.PropertyType)
                    {
                        continue;
                    }
                    if (cf == null)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(VersioningActionType.AddProperty, property.Name, property.PropertyType, null) { PAttributes = property.Attributes, IsField = false });
                    }
                    else if (cf.Attributes != property.Attributes)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(cf.Name, cf.PropertyType, property.Name, property.PropertyType, obj => obj) { PAttributes = property.Attributes, IsField = false });
                    }
                    else if (cf.PropertyType != property.PropertyType)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(cf.Name, cf.PropertyType, property.Name, property.PropertyType, null) { PAttributes = property.Attributes, IsField = false });
                    }
                    verint[versionpos]++;
                }

                //removing old types
                foreach (var field in currtype.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    versionpos = VersioningCheck(version, maxversion, out _);
                    var cf = tartype.GetField(field.Name);
                    if (cf == null)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(VersioningActionType.RemoveProperty, field.Name, field.FieldType));
                        verint[versionpos]++;
                    }
                }
                foreach (var property in currtype.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    versionpos = VersioningCheck(version, maxversion, out _);
                    var cf = tartype.GetField(property.Name);
                    if (cf == null)
                    {
                        Add(new Version(verint[0], verint[1], verint[2], verint[3]), new(VersioningActionType.RemoveProperty, property.Name, property.PropertyType));
                        verint[versionpos]++;
                    }
                }
            }
            private int VersioningCheck(Version version, Version maxversion, out bool overflow)
            {
                int versionpos = 3;
                overflow = false;
                if (version.Revision >= maxversion.Revision)
                {
                    versionpos--;
                }
                else if (version.Build >= maxversion.Build && versionpos == 2)
                {
                    versionpos--;
                }
                else if (version.Minor >= maxversion.Minor && versionpos == 1)
                {
                    versionpos--;
                }
                else if (version.Major >= maxversion.Major && versionpos == 0)
                {
                    versionpos--;
                }
                if (versionpos < 0)
                {
                    versionpos = 3;
                    overflow = true;
                }
                return versionpos;
            }
            public IEnumerator<VersioningAction> GetEnumerator()
            {
                return Actions.Select(x => x.Value).GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            public VersioningAction this[Version key]
            {
                get
                {
                    if (Actions.ContainsKey(key))
                    {
                        return Actions[key];
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Version '{key}' not found.");
                    }
                }
            }
            public IEnumerable<KeyValuePair<Version, VersioningAction>> this[VersioningActionType key]
            {
                get
                {
                    if (Actions.Where(x => x.Value.Type == key).Count() > 0)
                    {
                        return Actions.Where(x => x.Value.Type == key);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Type '{key}' not found.");
                    }
                }
            }
            public override string ToString()
            {
                return $"In: {typeof(TIn).Name}, Out: {typeof(TOut).Name} | Actions {Actions.Count}x";
            }
            public override int GetHashCode()
            {
                int actionhash = ToString().GetHashCode();
                foreach (var action in Actions)
                {
                    actionhash = HashCode.Combine(actionhash, action.Key.GetHashCode());
                    actionhash = HashCode.Combine(actionhash, action.Value.GetHashCode());
                }
                return actionhash;
            }
        }
        /// <param></param>
        /// <note>OriginName and OriginType are pairs you need to fill out both. Same with target.</note>
        /// <methods></methods>
        /// <tip>
        /// Make sure to use correct constructor if you don't know what you are doing.
        /// There are multiple preset constructors that you can use, and for the most part it's all you need.
        /// </tip>
        public class VersioningAction
        {
            public VersioningActionType Type { get; set; } = VersioningActionType.None;
            public string? OriginName { get; set; } = null;
            public Type? OriginType { get; set; } = null;
            public string? TargetName { get; set; } = null;
            public Type? TargetType { get; set; } = null;
            public object? Value { get; set; } = null;
            /// <summary>
            /// this can be value converter or value modifer
            /// </summary>
            public Func<object, object>? ValueConverter { get; set; } = null;
            /// <summary>
            /// only applies for field/property changes
            /// <para>true = field, false = property</para>
            /// </summary>
            public bool IsField { get; set; } = true;
            public FieldAttributes FAttributes { get; set; } = FieldAttributes.Public;
            public PropertyAttributes PAttributes { get; set; } = PropertyAttributes.None;

            /// <summary>
            /// Default, make sure to fill in stuff or use one of the preset constructors
            /// </summary>
            internal VersioningAction()
            {
            }
            /// <summary>
            /// Preset for ModifyValue
            /// </summary>
            /// <param name="Type"></param>
            /// <param name="Name"></param>
            /// <param name="Value"></param>
            internal VersioningAction(string Name, Type Type, object Value)
            {
                this.Type = VersioningActionType.ModifyValue;
                this.OriginName = Name;
                this.OriginType = Type;
                this.Value = Value;
            }
            /// <summary>
            /// Preset for ModifyValue
            /// <para>if the value that you are modifing is class or anything that has items inside it you can use the <paramref name="ValueModifier"/> to access the values and modify them. it is given the value inside the member matching the <paramref name="Name"/> and <paramref name="Type"/></para>
            /// </summary>
            /// <param name="Type"></param>
            /// <param name="Name"></param>
            /// <param name="ValueModifier"></param>
            internal VersioningAction(string Name, Type Type, Func<object, object> ValueModifier)
            {
                this.Type = VersioningActionType.ModifyValue;
                this.OriginName = Name;
                this.OriginType = Type;
                this.ValueConverter = ValueModifier;
            }
            /// <summary>
            /// Preset for AddProperty, RemoveProperty
            /// </summary>
            /// <param name="Type"></param>
            /// <param name="Name"></param>
            /// <param name="PType"></param>
            /// <param name="value">Additonal value to be set to the new property</param>
            internal VersioningAction(VersioningActionType Type, string Name, Type PType, object? value = null)
            {
                this.Type = Type;
                this.OriginName = Name;
                this.OriginType = PType;
                Value = value;
            }
            /// <summary>
            /// Preset for ModifyProperty
            /// <para>If you want to keep the original value make sure to fill the <paramref name="valueConverter"/></para>
            /// </summary>
            /// <param name="OriginName"></param>
            /// <param name="OriginType"></param>
            /// <param name="TargetName"></param>
            /// <param name="TargetType"></param>
            /// <param name="valueConverter">Converts the orginal (<paramref name="OriginType"/>)value to the new one (<paramref name="TargetType"/>)</param>
            internal VersioningAction(string OriginName, Type OriginType, string TargetName, Type TargetType, Func<object, object>? valueConverter = null)
            {
                this.Type = VersioningActionType.ModifyProperty;
                this.OriginName = OriginName;
                this.OriginType = OriginType;
                this.TargetName = TargetName;
                this.TargetType = TargetType;
                ValueConverter = valueConverter;
            }
            public override string ToString()
            {
                if (TargetType == null && TargetName == null && Value == null && ValueConverter == null)
                {
                    return $"{Type} | {OriginType} {OriginName}";
                }
                else
                {
                    return $"{Type} | {OriginType} {OriginName} => {(TargetType == null ? "" : $"{TargetType} ")}{(TargetName == null ? "" : $"{TargetName} ")}({Value}) {{{ValueConverter}}}";
                }
            }
            public override int GetHashCode()
            {
                var hashcode = ToString().GetHashCode();
                if (ValueConverter != null)
                    hashcode = HashCode.Combine(hashcode, ValueConverter.GetHashCode());
                hashcode = HashCode.Combine(hashcode, IsField.GetHashCode());
                hashcode = HashCode.Combine(hashcode, FAttributes.GetHashCode());
                hashcode = HashCode.Combine(hashcode, PAttributes.GetHashCode());
                return hashcode.GetHashCode();
            }
        }
    }
}
