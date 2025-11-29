namespace Petrosik
{
    namespace Utility
    {
        using Enums;
        using System;
        using System.Collections.Generic;
        using System.Drawing;
        using System.IO;
        using System.IO.Compression;
        using System.Numerics;
        using System.Text;
        using System.Text.Json;
        using System.Text.RegularExpressions;

        /// <summary>
        /// Main Utility Class
        /// </summary>
        public static class Utility
        {
            /// <summary>
            /// Splits text based on spaces or by `"`
            /// </summary>
            /// <param name="message">Original text to split</param>
            /// <param name="tempparam">Remaining text that couldn't fit</param>
            /// <param name="paramlenght">How many words to split for</param>
            /// <param name="prefix">Prefix to remove</param>
            /// <returns>
            /// Input:  `Hi, world. "This is a example"`  
            /// Output: [`Hi,`,`world.`,`"This is a example"`]
            /// </returns>
            [Obsolete("Use SmartSplit instead")]
            public static string?[] SplitText(string message, out string? tempparam, int paramlenght = 8, char prefix = '$')
            {
                int[] lenght = new int[] { 0, 1 };
                string mainmessage = message;
                string?[] param = new string[paramlenght];
                for (int i = 0; i < paramlenght; i++)
                {
                    param[i] = null;
                }

                lenght[0] = mainmessage.IndexOf(' ');
                if (lenght[0] <= 0)
                {
                    lenght[0] = message.Length;
                }
                if (message[0] == prefix)
                {
                    mainmessage = mainmessage.Substring(1, --lenght[0]);
                }
                else
                {
                    mainmessage = mainmessage.Substring(0, lenght[0]);
                    lenght[0]--;
                }
                param[0] = mainmessage;
                tempparam = null;
                if (++lenght[0] != message.Length)
                {
                    tempparam = message.Substring(lenght[0]);
                    while ((tempparam != null && tempparam != "") && lenght[1] <= param.GetLength(0) - 1)
                    {
                        if (tempparam.StartsWith(" \""))
                        {
                            lenght[0] = tempparam.IndexOf('"', 2);
                            if (lenght[0] < 0)
                            {
                                lenght[0] = tempparam.Length;
                            }
                            param[lenght[1]] = tempparam.Substring(2, lenght[0] - 2);
                            tempparam = tempparam.Substring(lenght[0] + 1);
                            lenght[1]++;
                        }
                        else if (tempparam.StartsWith(' '))
                        {
                            lenght[0] = tempparam.IndexOf(" ", 1);
                            if (lenght[0] < 0)
                            {
                                lenght[0] = tempparam.Length;
                            }
                            param[lenght[1]] = tempparam.Substring(1, lenght[0] - 1);
                            tempparam = tempparam.Substring(lenght[0]);
                            lenght[1]++;
                        }
                    }
                }
                return param;
            }
            /// <summary>
            /// Splits <paramref name="input"/> by the <paramref name="separator"/>
            /// <para>Takes into account brackets ( (, {, [ ) and " and will not split text inside them</para>
            /// </summary>
            /// <param name="input"></param>
            /// <param name="separator"></param>
            /// <returns></returns>
            public static List<string> SmartSplit(string input, char separator = ',')
            {
                var result = new List<string>();
                var current = new StringBuilder();
                int parens = 0, brackets = 0, braces = 0;
                bool inQuotes = false;

                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];

                    // Handle quote toggle (assuming only double quotes used)
                    if (c == '"' && (i == 0 || input[i - 1] != '\\'))
                    {
                        inQuotes = !inQuotes;
                    }

                    if (!inQuotes)
                    {
                        switch (c)
                        {
                            case '(': parens++; break;
                            case ')': parens--; break;
                            case '[': brackets++; break;
                            case ']': brackets--; break;
                            case '{': braces++; break;
                            case '}': braces--; break;
                        }
                    }

                    if (c == separator && parens == 0 && brackets == 0 && braces == 0 && !inQuotes)
                    {
                        result.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }

                if (current.Length > 0)
                {
                    result.Add(current.ToString().Trim());
                }

                return result;
            }
            /// <summary>
            /// Splits string into a list of strings by spaces untill it reached message limit, overridechunksize makes it ignore spaces
            /// </summary>
            /// <param name="str">Original Text</param>
            /// <param name="messagelimit">How long should the split message be max (with overhead)</param>
            /// <param name="overhead">How much space should be at the end of the split part</param>
            /// <param name="overridechunksize">Set lenght how long should the messages be (ignores overhead)</param>
            /// <returns></returns>
            public static List<string> FitMessageLimit(string str, int messagelimit, int overhead = 650, int overridechunksize = 0)
            {
                List<string> result = new();
                string substr = "";
                //separating by set size
                if (overridechunksize != 0)
                {
                    while (str.Length > 0)
                    {
                        if (str.Length > overridechunksize)
                        {
                            result.Add(str.Substring(0, overridechunksize));
                            str = str.Substring(overridechunksize);
                            continue;
                        }
                        result.Add(str);
                        break;
                    }
                    return result;
                }
                //separating by spaces?
                int lenght = 0;
                while (str.Length + overhead > messagelimit)
                {
                    int backup = 0;
                    while ((substr.Length + lenght) < messagelimit && backup < 1000)
                    {
                        if (str.Contains(' '))
                        {
                            if (substr == "")
                            {
                                substr += str.Substring(0, str.IndexOf(' '));
                                lenght = overhead - 20;
                            }
                            else
                            {
                                substr += str.Substring(0, str.IndexOf(' '));

                            }

                            str = str.Substring(str.IndexOf(' ') + 1);
                        }
                        backup++;
                    }
                    result.Add(substr);
                    substr = "";
                }
                result.Add(str);
                return result;
            }
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
            public static List<T?> MatchAndFillNull<T>(int count)
            {
                List<T?> list = new();
                for (int i = 0; i < count; i++)
                {
                    //yes very bad, shut
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    list.Add((T)(object)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                return list;
            }
            /// <summary>
            /// rotates vector(normalized direction) by set amount of degres
            /// </summary>
            /// <param name="v">Original vector</param>
            /// <param name="delta">Degres</param>
            /// <returns></returns>
            public static PointF Rotate(PointF v, float delta)
            {
                delta = (float)(delta * (180 / Math.PI));
                return new PointF((float)(v.X * Math.Cos(delta) - v.Y * Math.Sin(delta)), (float)(v.X * Math.Sin(delta) + v.Y * Math.Cos(delta)));
            }
            /// <summary>
            /// Compresses data using GZip
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public static byte[] Compress(byte[] data)
            {
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    using (GZipStream compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        compressionStream.Write(data, 0, data.Length);
                    }
                    return compressedStream.ToArray();
                }
            }
            /// <summary>
            /// Decompresses data using GZip
            /// </summary>
            /// <param name="compressedData"></param>
            /// <returns></returns>
            public static byte[] Decompress(byte[] compressedData)
            {
                using (MemoryStream compressedStream = new MemoryStream(compressedData))
                {
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }
                        return decompressedStream.ToArray();
                    }
                }
            }
            /// <summary>
            /// Serializes obj using JsonSerializer and compresses it before returning byte array
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static byte[] Serialize<T>(T obj)
            {
                string jsonString = JsonSerializer.Serialize(obj);
                return Compress(Encoding.UTF8.GetBytes(jsonString));
            }
            /// <summary>
            /// Decompresses byte array and deserializes using JsonSerializer before returning the type
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T? Deserialize<T>(byte[] compressedData)
            {
                byte[] decompressedData = Decompress(compressedData);
                string jsonString = Encoding.UTF8.GetString(decompressedData);
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            /// <summary>
            /// Returns random direction
            /// </summary>
            /// <returns></returns>
            public static PointF GetRandomDirection()
            {
                System.Random r = new();
                return new PointF((float)(r.NextDouble() - 0.5f), (float)(r.NextDouble() - 0.5f)).Normalize();
            }
            /// <summary>
            /// Returns a random "chance" from 100 to 0 in the range based on semi exponencial curve.
            /// <para>Mapped 100 = min , 0 = max</para> 
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="rngNum">The randomly generated number</param>
            /// <param name="lambda">How straight the curve is(bigger number straigther it is)</param>
            /// <returns></returns>
            public static double NextExponential(int min, int max, out double rngNum, double lambda = 2f)
            {
                Random r = new();
                if (max > 10)
                {
                    lambda = lambda * (max / 10);
                }
                rngNum = r.Next(min, max - 1) + r.NextDouble();
                var y = 100 * Math.Pow(2, -(rngNum / lambda));
                y = Math.Clamp(y, 0, 100);
                return y;
            }
            /// <summary>
            /// Rolls and returns true or false if the rolled number is smaller than the chance
            /// </summary>
            /// <param name="chance">clamped 0 to 100</param>
            /// <returns></returns>
            public static bool ChanceRoll(double chance)
            {
                chance = Math.Clamp(chance, 0, 100);
                Random r = new();
                var roll = r.Next(0, 100) + r.NextDouble();
                if (roll < chance)
                {
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Rolls and returns true or false if the rolled number is smaller than the maxChance and bigger than minChance
            /// </summary>
            /// <param name="maxChance">clamped 0 to 100</param>
            /// <param name="minChance">clamped 0 to 100</param>
            /// <returns></returns>
            public static bool ChanceRoll(double minChance, double maxChance)
            {
                maxChance = Math.Clamp(maxChance, 0, 100);
                minChance = Math.Clamp(minChance, 0, 100);
                Random r = new();
                var roll = r.Next(0, 100) + r.NextDouble();
                if (minChance > roll && roll < maxChance)
                {
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Formatted log method, ie:
            /// <para>Input: ConsoleLog("Hello World!", InfoType.Info)</para>
            /// <para>Console: [HH:mm:ss] [i]Hello World!</para>
            /// </summary>
            /// <param name="text">Message</param>
            /// <param name="infotype">Severity</param>
            public static void ConsoleLog(string text, InfoType infotype = InfoType.Info)
            {
                var inf = "[?]";
                switch (infotype)
                {
                    case InfoType.Info:
                        inf = "[i]";
                        break;
                    case InfoType.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        inf = "[W]";
                        break;
                    case InfoType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        inf = "[E]";
                        break;
                    case InfoType.Important:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        inf = "[I]";
                        break;
                    default:
                        inf = "[?]";
                        break;
                }
                var message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {inf}{text}";
                Console.WriteLine(message);
                Console.ResetColor();
            }
            /// <summary>
            /// Formatted log method, simplified version for quicker use
            /// </summary>
            /// <param name="obj"></param>
            public static void ConsoleLog(object obj)
            {
                ConsoleLog($"{obj}", InfoType.Info);
            }
            /// <summary>
            /// Formatted log method based on exception with additional message
            /// </summary>
            /// <param name="e"></param>
            /// <param name="AdittionalMessage">Additional message that will be shown</param>
            public static void ConsoleLog(Exception e, string? AdittionalMessage = null)
            {
                ConsoleLog($"{e.Message} {AdittionalMessage} | {e}", InfoType.Error);
            }
            /// <summary>
            /// should blend colors additevely
            /// </summary>
            /// <param name="basec">Original color</param>
            /// <param name="additivec">Color that will be added</param>
            /// <returns></returns>
            public static Color BlendColors(Color basec, Color additivec)
            {
                if (additivec.A == 255) return additivec;
                if (basec.A == 255) return additivec;
                if (additivec.A == 0f) return basec;

                var c = Color.FromArgb(basec.A + additivec.A, (basec.R + additivec.R) / 2, (basec.G + additivec.G) / 2, (basec.B + additivec.B) / 2);
                return c;
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static uint SetBit(uint value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1u << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1u << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static int SetBit(int value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1 << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1 << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static long SetBit(long value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1L << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1L << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static byte SetBit(byte value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return (byte)(value | (1 << bitPosition));
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return (byte)(value & ~(1 << bitPosition));
                }
            }
            /// <summary>
            /// Removes any characters that are invalid in file name with '_'
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string SanitizeFileName(string str)
            {
                string invalidChars = new string(Path.GetInvalidFileNameChars());
                string invalidRegStr = string.Format("[{0}]", Regex.Escape(invalidChars));
                string sanitized = Regex.Replace(str, invalidRegStr, "_");
                return sanitized;
            }

            /// <summary>
            /// Calculates the Levenshtein distance between two strings.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static int Levenshtein(string a, string b)
            {
                int n = a.Length;
                int m = b.Length;
                var d = new int[n + 1, m + 1];

                for (int i = 0; i <= n; i++) d[i, 0] = i;
                for (int j = 0; j <= m; j++) d[0, j] = j;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost
                        );
                    }
                }
                return d[n, m];
            }
            /// <summary>
            /// Copies all files and subdirectories from the source directory to the destination directory.
            /// </summary>
            /// <remarks>This method performs a recursive copy of all files and subdirectories from
            /// the source to the destination. If <paramref name="overwrite"/> is <see langword="false"/>, existing
            /// files in the destination with the same name as files in the source will not be overwritten.</remarks>
            /// <param name="sourceDir">The path of the directory to copy files and subdirectories from. Cannot be null or empty.</param>
            /// <param name="destinationDir">The path of the directory to copy files and subdirectories to. If the directory does not exist, it will
            /// be created.</param>
            /// <param name="overwrite">A boolean value indicating whether to overwrite existing files in the destination directory. Defaults to
            /// <see langword="true"/>.</param>
            public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
            {
                destinationDir = SanitizeFileName(destinationDir);
                sourceDir = SanitizeFileName(sourceDir);
                // Create destination directory if it doesn’t exist
                Directory.CreateDirectory(destinationDir);

                // Copy all files
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite);
                }

                // Copy all subdirectories recursively
                foreach (var dir in Directory.GetDirectories(sourceDir))
                {
                    var destSubDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                    CopyDirectory(dir, destSubDir, overwrite);
                }
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
            /// <param name="v">Original Point</param>
            /// <param name="delta">Degres</param>
            /// <returns></returns>
            public static PointF Rotate(this PointF v, float delta)
            {
                delta = (float)(delta * (180 / Math.PI));
                return new PointF((float)(v.X * Math.Cos(delta) - v.Y * Math.Sin(delta)), (float)(v.X * Math.Sin(delta) + v.Y * Math.Cos(delta)));
            }
            /// <summary>
            /// Returns direction from v1 to v2 normalized 
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <returns></returns>
            public static PointF Direction(this PointF v1, PointF v2)
            {
                return new PointF(v2.X - v1.X,v2.Y - v1.Y).Normalize();
            }
            /// <summary>
            /// Merges Arrays into one
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns></returns>
            public static byte[] Merge(this byte[][] bytes)
            {
                var size = 0;
                foreach (var b in bytes)
                {
                    size += b.Length;
                }
                var result = new byte[size];

                int offset = 0;
                foreach (var chunk in bytes)
                {
                    foreach (var b in chunk)
                    {
                        result[offset++] = b;
                    }
                }

                return result;
            }
            /// <summary>
            /// Splits array into multiple arrays by provided chunk size
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="chunkSize"></param>
            /// <returns></returns>
            public static byte[][] Split(this byte[] bytes, int chunkSize)
            {
                var result = new byte[(int)Math.Ceiling((float)bytes.Length / chunkSize)][];

                int index = 0;
                for (int i = 0; i < bytes.Length; i += chunkSize, index++)
                {
                    int remainingLength = bytes.Length - i;
                    int currentChunkSize = Math.Min(chunkSize, remainingLength);

                    byte[] chunk = new byte[currentChunkSize];
                    Array.Copy(bytes, i, chunk, 0, currentChunkSize);
                    result[index] = chunk;
                }
                return result;
            }
            /// <summary>
            /// Scales value between 0 and 1
            /// </summary>
            /// <param name="value"></param>
            /// <param name="min_value"></param>
            /// <param name="max_value"></param>
            /// <returns></returns>
            public static float Normalize(this float value, float min_value, float max_value)
            {
                return (value - min_value) / (max_value - min_value);
            }
            /// <summary>
            /// Normalizes point between 0 and 1
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public static PointF Normalize(this PointF point)
            {
                float magnitude = (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
                if (magnitude > 0)
                {
                    float normalizedX = point.X / magnitude;
                    float normalizedY = point.Y / magnitude;

                    return new PointF(normalizedX, normalizedY);
                }
                else
                {
                    return point;
                }
            }
            /// <summary>
            /// Returns abs vector value
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static PointF Abs(this PointF v)
            {
                return new(Math.Abs(v.X), Math.Abs(v.Y));
            }
            /// <summary>
            /// Rounds the numbers to the set of digits
            /// </summary>
            /// <param name="v"></param>
            /// <param name="digits"></param>
            /// <returns></returns>
            public static PointF Round(this PointF v, int digits)
            {
                return new PointF((float)Math.Round(v.X, digits), (float)Math.Round(v.Y, digits));
            }
            /// <summary>
            /// Floors the point
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static PointF Floor(this PointF v)
            {
                return new PointF((float)Math.Floor(v.X), (float)Math.Floor(v.Y));
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static PointF Ceiling(this PointF v)
            {
                return new PointF((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y));
            }
            /// <summary>
            /// Gets sum of the x and y
            /// </summary>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float Sum(this PointF v)
            {
                return v.X + v.Y;
            }
            /// <summary>
            /// Creates string similar to debbuger display if the number is too big
            /// </summary>
            /// <param name="val"></param>
            /// <param name="digits">How many digits to show</param>
            /// <param name="cutoff">Number bigger than this is gonna get converted to the shorter version</param>
            /// <returns></returns>
            public static string ToShortString(this BigInteger val, int digits = 8, int cutoff = 99999999)
            {
                return (val > cutoff ? $"{val.ToString($"E{digits}")}" : $"{val}");
            }
            /// <summary>
            /// Returns distance from v1 to v2
            /// </summary>
            /// <param name="v1"></param> 
            /// <param name="v2"></param>
            /// <returns></returns>
            public static float Distance(this PointF v1, PointF v2)
            {
                float num = v1.X - v2.X;
                float num2 = v1.Y - v2.Y;
                return (float)Math.Sqrt(num * num + num2 * num2);
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static uint SetBit(this uint value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1u << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1u << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static int SetBit(this int value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1 << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1 << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static long SetBit(this long value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return value | (1L << bitPosition);
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return value & ~(1L << bitPosition);
                }
            }
            /// <summary>
            /// Sets the <paramref name="bitPosition"/> to <paramref name="bitValue"/> in the <paramref name="value"/>
            /// </summary>
            /// <param name="value"></param>
            /// <param name="bitPosition"></param>
            /// <param name="bitValue"></param>
            /// <returns></returns>
            public static byte SetBit(this byte value, int bitPosition, bool bitValue)
            {
                if (bitValue)
                {
                    // Set the bit at bitPosition to 1
                    return (byte)(value | (1 << bitPosition));
                }
                else
                {
                    // Clear the bit at bitPosition (set it to 0)
                    return (byte)(value & ~(1 << bitPosition));
                }
            }
            /// <summary>
            /// Removes any characters that are invalid in file name with '_'
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string SanitizeFileName(this string str)
            {
                string invalidChars = new string(Path.GetInvalidFileNameChars());
                string invalidRegStr = string.Format("[{0}]", Regex.Escape(invalidChars));
                string sanitized = Regex.Replace(str, invalidRegStr, "_");
                return sanitized;
            }
        }
    }
}
