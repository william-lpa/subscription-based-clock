public interface ILogger
{
  //
  // Summary:
  //     Adds a line of text to the output.
  //
  // Parameters:
  //   message:
  //     The message
  void WriteLine(string message);
  //
  // Summary:
  //     Formats a line of text and adds it to the output.
  //
  // Parameters:
  //   format:
  //     The message format
  //
  //   args:
  //     The format arguments
  void WriteLine(string format, params object[] args);
}