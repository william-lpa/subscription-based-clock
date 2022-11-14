using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

// My very rudimentar implementation of logging during tests.
// it writes to a file.
public class FileTestLogger : ILogger
{
  private static object object_lock = new object();
  const string myfile = @"test-log.txt";

  public void WriteLine(string message)
  {
    // mutual exclusion for handling parallel tests
    lock (object_lock)
    {
      using (var sw = File.AppendText(myfile))
      {
        sw.WriteLine(message);
      }
    }

  }

  public void WriteLine(string format, params object[] args)
  {
    // mutual exclusion for handling parallel tests
    lock (object_lock)
    {
      using (var sw = File.AppendText(myfile))
      {
        sw.WriteLine(format, args);
      }
    }
  }
}


public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    // Is be called after the `ConfigureServices` from the Startup
    // which allows you to overwrite the DI with mocked instances
    builder.ConfigureTestServices(services =>
    {
      services.AddScoped<ILogger, FileTestLogger>();
    });
  }
}