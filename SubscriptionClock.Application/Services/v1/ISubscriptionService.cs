public enum RequestOutcome
{
  Valid, Conflict, NonExisting
}

// Service interface that serves business logic requested
// by the controller on the client's behalf
public interface IHttpSubscriptionService
{
  public Task<RequestOutcome> SubscribeTaskAsync(SubscriptionConfigDto dto);
  public Task<RequestOutcome> UnScribeTaskAsync(DeleteSubscriptionConfigDto dto);
  public Task<RequestOutcome> UpdateTaskAsync(SubscriptionConfigDto dto);
}