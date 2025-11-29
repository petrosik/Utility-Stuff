namespace Petrosik
{
    namespace UnityUtility
    {
        using System;
        using System.Collections.Generic;
        using UnityEngine;
        /// <summary>
        /// Contains all Unity specific utility stuff
        /// </summary>
        public static class Utility
        {
            /// <summary>
            /// Returns list of all options in the enum
            /// </summary>
            /// <typeparam name="T">Enum type</typeparam>
            /// <returns></returns>
            public static List<T> GetEnumTypes<T>() where T : Enum
            {
                List<T> list = new List<T>();
                var n = Enum.GetNames(typeof(T));
                foreach (var enu in n)
                {
                    list.Add((T)Enum.Parse(typeof(T), enu));
                }
                return list;
            }
            /// <summary>
            /// Creates a 2D perlin
            /// </summary>
            /// <param name="xsize"></param>
            /// <param name="ysize"></param>
            /// <param name="scale">bigger scale number more detail</param>
            /// <param name="convertToPercent">Should convert the values to %</param>
            /// <returns></returns>
            public static float[,] Perlin2D(int xsize, int ysize, float scale = 6, bool convertToPercent = false)
            {
                var noise = new float[xsize, ysize];
                var cnv = convertToPercent ? 100 : 1;
                for (float y = 0; y < ysize; y++)
                {
                    for (float x = 0; x < xsize; x++)
                    {
                        float xCoord = x / xsize * scale;
                        float yCoord = y / ysize * scale;
                        noise[(int)x, (int)y] = Mathf.PerlinNoise(xCoord, yCoord) * cnv;
                    }
                }
                return noise;
            }
            /// <summary>
            /// Returns random direction without using unity's random
            /// </summary>
            /// <returns></returns>
            public static Vector2 GetRandomDirection()
            {
                System.Random r = new();
                return new Vector2((float)(r.NextDouble() - 0.5f), (float)(r.NextDouble() - 0.5f)).normalized;
            }
            /// <summary>
            /// Returns distance while ignoring y values
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static float DistanceIY(Vector3 a, Vector3 b)
            {
                return Vector3.Distance(new Vector3(a.x, b.y, a.z), b);
            }
            /// <summary>
            /// Should blend colors additevely
            /// </summary>
            /// <param name="basec">Original color</param>
            /// <param name="additivec">Color that will be added</param>
            /// <returns></returns>
            public static Color BlendColors(Color basec, Color additivec)
            {
                if (additivec.a == 1f) return additivec;
                if (basec.a == 0f) return additivec;
                if (additivec.a == 0f) return basec;

                var c = new Color((basec.r + additivec.r) / 2f, (basec.g + additivec.g) / 2f, (basec.b + additivec.b) / 2f, basec.a + additivec.a);
                return c;
            }
        }
        /// <summary>
        /// All basic Modifications/Extensions
        /// </summary>
        public static class Extensions
        {
            /// <summary>
            /// Rotates and returns vector by set amount of degres
            /// </summary>
            /// <param name="v">Original Vector</param>
            /// <param name="delta">Degres</param>
            /// <returns></returns>
            public static Vector2 Rotate(this Vector2 v, float delta)
            {
                delta = delta * Mathf.Deg2Rad;
                return new Vector2(v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta), v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta));
            }
            /// <summary>
            /// Returns direction from v1 to v2 normalized 
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <returns></returns>
            public static Vector2 Direction(this Vector2 v1, Vector2 v2)
            {
                return (v2 - v1).normalized;
            }
            /// <summary>
            /// Returns direction from v1 to v2 normalized 
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <returns></returns>
            public static Vector3 Direction(this Vector3 v1, Vector3 v2)
            {
                return (v2 - v1).normalized;
            }
            public static void DestroyChildren(this GameObject obj, List<string>? namelist = null, bool whitelist = false)
            {
                foreach (Transform child in obj.transform)
                {
                    if (namelist == null || (whitelist && namelist.Exists(l => l == child.name)) || (!whitelist && !namelist.Exists(l => l == child.name)))
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
            }
            /// <summary>
            /// Tries to clean the list from all null GameObjects
            /// </summary>
            /// <param name="l"></param>
            public static void CleanList(this List<GameObject> l)
            {
                if (l.Exists(a => a == null || a.IsUnityNull()))
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        if (l[i] == null || l[i].IsUnityNull())
                        {
                            l.RemoveAt(i);
                        }
                    }
                }
            }
            /// <summary>
            /// Check if obj is unity obj and is not null
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static bool IsUnityNull(this object obj)
            {
                return obj == null || ((obj is UnityEngine.Object) && ((UnityEngine.Object)obj) == null);
            }
            /// <summary>
            /// Returns distance while ignoring y values
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static float DistanceIY(this Vector3 a, Vector3 b)
            {
                return Vector3.Distance(new Vector3(a.x, b.y, a.z), b);
            }
            /// <summary>
            /// Returns new Vector3(v.x, 0f, v.y)
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector3 Convert(this Vector2 v)
            {
                return new(v.x, 0f, v.y);
            }
            /// <summary>
            /// Returns abs vector value (same as for Vector2) 
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector3 Abs(this Vector3 v)
            {
                return new(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
            }
            /// <summary>
            /// Rounding to the specific number of digits
            /// </summary>
            /// <param name="v"></param>
            /// <param name="digits"></param>
            /// <returns></returns>
            public static Vector3 Round(this Vector3 v, int digits)
            {
                return new Vector3((float)Math.Round(v.x, digits), (float)Math.Round(v.y, digits), (float)Math.Round(v.z, digits));
            }
            public static Vector3 Floor(this Vector3 v)
            {
                return new Vector3((float)Math.Floor(v.x), (float)Math.Floor(v.y), (float)Math.Floor(v.z));
            }
            public static Vector3 Ceiling(this Vector3 v)
            {
                return new Vector3((float)Math.Ceiling(v.x), (float)Math.Ceiling(v.y), (float)Math.Ceiling(v.z));
            }
            /// <summary>
            /// Gets sum of the x, y, z
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static int Sum(this Vector3Int v)
            {
                return v.x + v.y + v.z;
            }
            /// <summary>
            /// Returns abs vector value
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector2 Abs(this Vector2 v)
            {
                return new(Math.Abs(v.x), Math.Abs(v.y));
            }
            /// <summary>
            /// Rounding to the specific number of digits
            /// </summary>
            /// <param name="v"></param>
            /// <param name="digits"></param>
            /// <returns></returns>
            public static Vector2 Round(this Vector2 v, int digits)
            {
                return new Vector2((float)Math.Round(v.x, digits), (float)Math.Round(v.y, digits));
            }
            public static Vector2 Floor(this Vector2 v)
            {
                return new Vector2((float)Math.Floor(v.x), (float)Math.Floor(v.y));
            }
            public static Vector2 Ceiling(this Vector2 v)
            {
                return new Vector2((float)Math.Ceiling(v.x), (float)Math.Ceiling(v.y));
            }
            /// <summary>
            /// Gets sum of the x and y
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static int Sum(this Vector2Int v)
            {
                return v.x + v.y;
            }
            public static float Sum(this Vector3 v)
            {
                return v.x + v.y + v.z;
            }
            public static float Sum(this Vector2 v)
            {
                return v.x + v.y;
            }
            /// <summary>
            /// Takes all values from <paramref name="v"/> sum's them up and divides them by 3
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float Average(this Vector3 v)
            {
                return v.Sum() / 3f;
            }
            /// <summary>
            /// Takes all values from <paramref name="v"/> sum's them up and divides them by 2
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float Average(this Vector2 v)
            {
                return v.Sum() / 2f;
            }
            /// <summary>
            /// Takes all values from <paramref name="v"/> sum's them up and divides them by 3
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float Average(this Vector3Int v)
            {
                return v.Sum() / 3f;
            }
            /// <summary>
            /// Takes all values from <paramref name="v"/> sum's them up and divides them by 2
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float Average(this Vector2Int v)
            {
                return v.Sum() / 2f;
            }
            /// <summary>
            /// Clamps all axis to between min and max, same as Mathf.Clamp()
            /// </summary>
            /// <param name="v"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <returns></returns>
            public static Vector3 Clamp(this Vector3 v, float min, float max)
            {
                return new Vector3(
                           Mathf.Clamp(v.x, min, max),
                           Mathf.Clamp(v.y, min, max),
                           Mathf.Clamp(v.z, min, max));
            }
            /// <summary>
            /// Clamps all axis to between min and max, same as Mathf.Clamp()
            /// </summary>
            /// <param name="v"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <returns></returns>
            public static Vector2 Clamp(this Vector2 v, float min, float max)
            {
                return new Vector2(
                           Mathf.Clamp(v.x, min, max),
                           Mathf.Clamp(v.y, min, max));
            }
            /// <summary>
            /// Aligns vector <paramref name="v"/> to be either 1,0,-1 on either axis if the axis is &gt;= 0.5f
            /// <para>Multiple Axis can have values (ie [1,-1,0]) </para>
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector3 AlignToAxis(this Vector3 v)
            {
                return new Vector3(
                           Mathf.Abs(v.x) >= 0.5f ? Mathf.Sign(v.x) : 0f,
                           Mathf.Abs(v.y) >= 0.5f ? Mathf.Sign(v.y) : 0f,
                           Mathf.Abs(v.z) >= 0.5f ? Mathf.Sign(v.z) : 0f);
            }
            /// <summary>
            /// Aligns vector <paramref name="v"/> to be either 1,0,-1 on either axis if the axis is &gt;= 0.5f
            /// <para>Multiple Axis can have values (ie [1,-1]) </para>
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector2 AlignToAxis(this Vector2 v)
            {
                return new Vector2(
                           Mathf.Abs(v.x) >= 0.5f ? Mathf.Sign(v.x) : 0f,
                           Mathf.Abs(v.y) >= 0.5f ? Mathf.Sign(v.y) : 0f);
            }
            /// <summary>
            /// Aligns vector <paramref name="v"/> to be either 1,0,-1 on either axis if the axis is &gt;= 0.5f
            /// <para>Only one Axis can have values (ie [1,0,0] but never [1,-1,0]) </para>
            /// <para>Direction is preffered in order: x, y, z</para>
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector3 AlignToSingleAxis(this Vector3 v)
            {
                var x = Mathf.Abs(v.x) >= 0.5f ? Mathf.Sign(v.x) : 0f;
                var y = x == 0 ? Mathf.Abs(v.y) >= 0.5f ? Mathf.Sign(v.y) : 0f : 0f;
                var z = y == 0 && x == 0 ? Mathf.Abs(v.z) >= 0.5f ? Mathf.Sign(v.z) : 0f : 0f;
                return new Vector3(x, y, z);
            }
            /// <summary>
            /// Aligns vector <paramref name="v"/> to be either 1,0,-1 on either axis if the axis is &gt;= 0.5f
            /// <para>Only one Axis can have values (ie [1,0] but never [1,-1]) </para>
            /// <para>Direction is preffered in order: x, y</para>
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static Vector2 AlignToSingleAxis(this Vector2 v)
            {
                var x = Mathf.Abs(v.x) >= 0.5f ? Mathf.Sign(v.x) : 0f;
                var y = x == 0 ? Mathf.Abs(v.y) >= 0.5f ? Mathf.Sign(v.y) : 0f : 0f;
                return new Vector2(x, y);
            }
        }
        namespace ColorConvert
        {
            public static class Extensions
            {
                /// <summary>
                /// Converts System.Drawing color to UnityEngine color
                /// </summary>
                /// <param name="c"></param>
                /// <returns></returns>
                public static Color Convert(this System.Drawing.Color c)
                {
                    return new Color(
                      c.R / 255f,
                      c.G / 255f,
                      c.B / 255f,
                      c.A / 255f);

                }
                /// <summary>
                /// Converts UnityEngine color to System.Drawing color
                /// </summary>
                /// <param name="c"></param>
                /// <returns></returns>
                public static System.Drawing.Color Convert(this Color c)
                {
                    return System.Drawing.Color.FromArgb(
                         (int)(c.a * 255),
                         (int)(c.r * 255),
                         (int)(c.g * 255),
                         (int)(c.b * 255));
                }
            }
        }
    }
}
