
using System.Text.Json;

// This project is mainly for HTTPNotification.
// Other protocols could be implemented using
// the same interface  
public class HttpNotifier : INotifier
{
  private readonly HttpClient httpClient;
  private readonly ILogger logger;

  public HttpNotifier(HttpClient httpClient, ILogger logger)
  {
    this.httpClient = httpClient;
    this.logger = logger;
  }

  // HTTPs are unreliable. Exponential backoff retries
  // could be easily implemented here if supported by
  // the client
  public async Task Notify(string address, DateTime date)
  {
    logger.WriteLine("It's {0}, time to call {1}", date, address);

    string json = JsonSerializer.Serialize(new { notificationTime = date });

    using var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    using var response = await httpClient.PostAsync(address, httpContent);
    var resBody = await response.Content.ReadAsStringAsync();
    logger.WriteLine("response received was {0}, content: {1}", response.StatusCode, resBody);
  }
}