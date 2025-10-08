
namespace Petrosik
{
    namespace Http
    {
        using Petrosik.Enums;
        using Petrosik.Utility;
        using System;
        using System.Net.Http;
        using System.Net.Http.Json;
        using System.Threading.Tasks;
        public static class Extensions
        {
            /// <summary>
            /// Simple HTTP request helper method supporting GET, POST, PUT, DELETE.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="_httpClient"></param>
            /// <param name="Uri"></param>
            /// <param name="Method"></param>
            /// <param name="Content"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static async Task<(T? Result, bool Success)> Request<T>(this HttpClient _httpClient, string Uri, HttpMethodType Method = HttpMethodType.GET, object? Content = null)
            {
                HttpResponseMessage response;

                switch (Method)
                {
                    case HttpMethodType.GET:
                        response = await _httpClient.GetAsync(Uri);
                        break;

                    case HttpMethodType.POST:
                        response = await _httpClient.PostAsJsonAsync(Uri, Content);
                        break;

                    case HttpMethodType.PUT:
                        response = await _httpClient.PutAsJsonAsync(Uri, Content);
                        break;

                    case HttpMethodType.DELETE:
                        response = await _httpClient.DeleteAsync(Uri);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported HTTP method: {Method}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    Utility.ConsoleLog($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{error}", Petrosik.Enums.InfoType.Error);
                    return ((T)(object)null, false);
                }

                if (response.Content.Headers.ContentLength == 0)
                    return ((T)(object)null, true);

                return (await response.Content.ReadFromJsonAsync<T>(), true); 
            }
        }
    }
}
