// The contract between the DTO sent by the client
// and the domain model handled by our services
// to interact with a scheduled task.
public class SubscriptionConfig
{
  public string CallbackURL { get; private set; }
  public int IntervalMs { get; private set; }

  public SubscriptionConfig(string callbackURL, int intervalMs)
  {
    CallbackURL = callbackURL;
    IntervalMs = intervalMs;
  }

  public SubscriptionConfig(string callbackURL) => CallbackURL = callbackURL;

  // Converting the client DTO to its type
  // Here the concept of IntervalUnit is not relevant
  // since the scheduled task only takes milliseconds
  // into account.
  public SubscriptionConfig(SubscriptionConfigDto subscriptionDto)
  {
    CallbackURL = subscriptionDto.CallbackURL;
    IntervalMs = subscriptionDto.Interval * 1000;

    if (subscriptionDto.IntervalUnit == IntervalUnit.Minutes) { IntervalMs *= 60; }
    if (subscriptionDto.IntervalUnit == IntervalUnit.Hours) { IntervalMs *= 60 * 60; }

  }
}