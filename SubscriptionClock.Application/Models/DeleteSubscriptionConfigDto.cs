using System.Text.Json;

// The request body sent by the client when.
// trying to remove an exisiting scheduled task
public class DeleteSubscriptionConfigDto
{
  public string CallbackURL { get; set; }

  public override string ToString()
  {
    return JsonSerializer.Serialize(this);
  }

}