using Schedule.Runners;
using ServiceScheduler.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceScheduler
{
  public class Service
  {
    /// <summary>
    /// Max execution time
    /// </summary>
    public virtual TimeSpan Timeout { get; set; }

    /// <summary>
    /// HTTP client instance
    /// </summary>
    public virtual HttpClient Client { get; set; }

    /// <summary>
    /// Scheduler
    /// </summary>
    public virtual BackgroundRunner Scheduler { get; set; }

    /// <summary>
    /// Serialization options
    /// </summary>
    public virtual JsonSerializerOptions Options { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Service()
    {
      Client = new HttpClient();
      Timeout = TimeSpan.FromSeconds(15);
      Scheduler = new BackgroundRunner(Environment.ProcessorCount);
      Options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
        {
          new Converters.DateConverter(),
          new Converters.DoubleConverter()
        },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
          Modifiers =
          {
            contract => contract.Properties.ToList().ForEach(property =>
            {
              var name = Regex.Replace(property.Name,"(.)([A-Z])","$1_$2");

              if (string.Equals(name, property.Name) is false)
              {
                var o = contract.CreateJsonPropertyInfo(property.PropertyType, name);

                o.Set = property.Set;
                o.AttributeProvider = property.AttributeProvider;

                contract.Properties.Add(o);
              }
            })
          }
        }
      };
    }

    /// <summary>
    /// Stream HTTP content
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public virtual async Task<Stream> Stream(
      HttpRequestMessage message,
      JsonSerializerOptions options = null,
      CancellationTokenSource cts = null)
    {
      using (var client = new HttpClient())
      {
        return await client.GetStreamAsync(message.RequestUri, (cts ?? new CancellationTokenSource(Timeout)).Token);
      }
    }

    /// <summary>
    /// Generic query sender
    /// </summary>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public virtual async Task<T> Send<T>(
      HttpRequestMessage message,
      JsonSerializerOptions options = null,
      CancellationTokenSource cts = null)
    {
      try
      {
        cts ??= new CancellationTokenSource(Timeout);

        var response = await Scheduler.Send(Client.SendAsync(message, cts.Token)).Task;
        var content = await Scheduler.Send(response.Content.ReadAsStreamAsync(cts.Token)).Task;

        return await JsonSerializer.DeserializeAsync<T>(content, Options);
      }
      catch (Exception e)
      {
        InstanceService<LogService>.Instance.Log.Error(e.Message);
      }

      return default;
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose()
    {
      Client?.Dispose();
      Scheduler?.Dispose();
    }
  }
}
