// This is the Json object that will get returned whenever there's a bad request
public class BadRequestResponse
{
  public ErrorCode ErrorCode { get; set; }
  public string Error { get; set; }

}