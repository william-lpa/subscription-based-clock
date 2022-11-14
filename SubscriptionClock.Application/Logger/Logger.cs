// My very rudimentar implementation of logging.
// In production systems, one can probably add
// Log4net (equivalent to log4j) or another 3rd
// party lib.
public class Logger : ILogger
{
  private readonly string logId;
  private readonly Guid uuid;

  public String DateEntry => DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff tt");

  public Logger()
  {
    uuid = Guid.NewGuid();
  }
  public void WriteLine(string message)
  {
    System.Console.WriteLine("{0}, ID {1}: {2}", DateEntry, uuid, message);
  }

  public void WriteLine(string format, params object[] args)
  {
    var message = $"{DateEntry}, ID {uuid}: {format}";
    System.Console.WriteLine(message, args);
  }
}