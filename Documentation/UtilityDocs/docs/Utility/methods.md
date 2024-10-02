# Methods


### string?[] SplitText(...)
Splits text based on spaces or by `'"'`  

>**Method:**  
>string?[] SplitText(string message, out string? tempparam, int paramlenght = 8, char prefix = '$')  

`message` - Original text to split  
`tempparam` - Remaining text that couldn't fit  
`paramlenght` (default: 8) - How many words to split for  
`prefix` (default: $) - Prefix to remove  

```csharp
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
``` 

<!-- /// <summary>
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
        list.Add((T)(object)null);
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
public static void ConsoleLog(string text, InfoType infotype)
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
/// 
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
} -->