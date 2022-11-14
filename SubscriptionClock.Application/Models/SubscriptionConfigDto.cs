using System.Text.Json;

// The request body sent by the client when.
// trying to create/update a scheduled task.
public class SubscriptionConfigDto
{
  public string CallbackURL { get; set; }
  public int Interval { get; set; }
  public IntervalUnit IntervalUnit { get; set; }

  public void Deconstruct(out string callbackUrl, out int interval, out IntervalUnit intervalUnit)
  {
    callbackUrl = CallbackURL;
    interval = Interval;
    intervalUnit = IntervalUnit;
  }

  private bool ValidateIntervalUnit() => Enum.IsDefined(typeof(IntervalUnit), IntervalUnit);

  // SubscriptionConfigDto is responsible for validating its own domain
  public bool IsValid()
  {
    return !string.IsNullOrEmpty(CallbackURL) && Interval > 0 && ValidateIntervalUnit();
  }

  public override string ToString()
  {
    return JsonSerializer.Serialize(this);
  }

}