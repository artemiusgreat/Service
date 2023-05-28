using System.Collections.Generic;
using System.Web;

namespace Service.Extensions
{
  public static class DictionaryExtensions
  {
    public static V Get<K, V>(this IDictionary<K, V> input, K index)
    {
      return input.TryGetValue(index, out var value) ? value : default;
    }

    public static string ToQuery<K, V>(this IDictionary<K, V> input)
    {
      var inputs = HttpUtility.ParseQueryString(string.Empty);

      if (input is not null)
      {
        foreach (var item in input)
        {
          inputs[$"{item.Key}"] = $"{item.Value}";
        }
      }

      return $"{inputs}";
    }
  }
}
