
public static class Utilities
{

  // Valid boundaries are any time between 5 seconds and 4 hours
  public static bool IsFrequencyOutOfBounds(int interval, IntervalUnit intervalUnit)
  {
    switch (intervalUnit)
    {
      case IntervalUnit.Seconds:
        // between 5 seconds and 4 hours
        return interval < 5 || interval > 14400;
      case IntervalUnit.Minutes:
        // between 1m and 240m
        return interval < 1 || interval > 240;
      case IntervalUnit.Hours:
        // between 1h and 4h
        return interval < 1 || interval > 4;
      default:
        return false;
    }
  }

  // check if given string is a valid URI object
  public static bool ValidURI(string uriName)
  {
    return Uri.TryCreate(uriName, UriKind.Absolute, out var uri)
    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
  }

  // Utility for the validation utilised in the controler to check whether the request sent
  // by the client is valid
  public class RequestValidation
  {
    public bool Valid { get; set; }
    public IResult Outcome { get; set; }
  }

  public static RequestValidation ValidateRequest(SubscriptionConfigDto? newSubs, ILogger logger)
  {
    var requestValidation = new RequestValidation()
    {
      Outcome = null,
      Valid = true,
    };

    logger.WriteLine("Validating request");

    if (newSubs is null)
    {
      return new RequestValidation
      {
        Outcome = Results.BadRequest(new BadRequestResponse { ErrorCode = ErrorCode.NoBody, Error = "No body was provided" })
      };
    };

    var (callbackUrl, interval, intervalUnit) = newSubs;

    if (!newSubs.IsValid())
    {
      return new RequestValidation
      {
        Outcome = Results.BadRequest(new BadRequestResponse { ErrorCode = ErrorCode.InvalidRequestBody, Error = "Invalid body format" })
      };
    }

    logger.WriteLine("Validating frequency");

    if (Utilities.IsFrequencyOutOfBounds(interval, intervalUnit))
    {
      return new RequestValidation
      {
        Outcome = Results.BadRequest(new BadRequestResponse { ErrorCode = ErrorCode.InvalidFrequency, Error = "Frequency should be between 5 seconds and 4 hours" })
      };
    }

    logger.WriteLine("Validating callbackurl format");

    if (!Utilities.ValidURI(callbackUrl))
    {
      return new RequestValidation
      {
        Outcome = Results.BadRequest(new BadRequestResponse { ErrorCode = ErrorCode.InvalidURLFormat, Error = "Provided URL is not in a valid format" })
      };
    }

    return new RequestValidation { Valid = true };
  }
}