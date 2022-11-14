using Xunit.Abstractions;

// My very rudimentar implementation of logging during tests.
public class TestLogger : ILogger
{
  private readonly ITestOutputHelper output;
  public TestLogger(ITestOutputHelper output) => this.output = output;

  public void WriteLine(string message) => output.WriteLine(message);

  public void WriteLine(string format, params object[] args) => output.WriteLine(format, args);

}