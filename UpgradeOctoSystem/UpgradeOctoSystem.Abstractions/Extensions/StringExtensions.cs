using Newtonsoft.Json;
using System;
using System.Linq;

namespace UpgradeOctoSystem.Abstractions.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Try the deserialize json.
        /// </summary>
        /// <param name="str">The str.</param>
        /// <param name="response">The response.</param>
        /// <param name="error">The error.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <typeparam name="T"></typeparam>
        public static bool TryDeserializeJson<T>(this string str, out T response, JsonSerializerSettings settings = null)
            where T : new()
        {
            response = default;
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            try
            {
                response = JsonConvert.DeserializeObject<T>(
                    str,
                    settings
                        ?? new JsonSerializerSettings()
                        {
                            MissingMemberHandling = MissingMemberHandling.Error,
                        }
                ); ;
                return true;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ToSnakeCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}