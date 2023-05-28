using Serilog;

namespace Service.Services
{
  public class LogService
  {
    /// <summary>
    /// Logger instance
    /// </summary>
    public virtual ILogger Log => Serilog.Log.Logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public LogService() => Serilog.Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .Enrich.FromLogContext()
      .WriteTo.Debug()
      .CreateLogger();
  }
}
