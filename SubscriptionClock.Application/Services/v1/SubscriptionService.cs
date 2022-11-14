// Service that serves business logic requested
// by the controller on the client's behalf
public class HttpSubscriptionService : IHttpSubscriptionService
{
  // orchestrates the scheduled tasks in the running service
  private readonly ITaskScheduler scheduler;
  private readonly ILogger logger;

  // utilises a source of notification. For this project it
  // implements a HTTPNotification using DI. Other protocols could
  // be implemented using the same interface  
  private readonly INotifier notifier;

  public HttpSubscriptionService(ITaskScheduler scheduler, ILogger logger, INotifier notifier)
  {
    this.scheduler = scheduler;
    this.logger = logger;
    this.notifier = notifier;
  }

  public async Task<RequestOutcome> SubscribeTaskAsync(SubscriptionConfigDto newSubscriptionDto)
  {
    logger.WriteLine("starting subscription service");

    var subscriptionCfg = new SubscriptionConfig(newSubscriptionDto);
    if (await scheduler.Schedule(new ScheduledTask(subscriptionCfg, this.notifier, this.logger)))
    {
      logger.WriteLine("Task scheduled. Next execution is at {0}",
                        DateTime.Now.AddMilliseconds(subscriptionCfg.IntervalMs)
                        .ToString("dd/MM/yyyy HH:mm:ss"));

      return RequestOutcome.Valid;
    }

    logger.WriteLine("Task not scheduled. Callback {0} already exists already registered", newSubscriptionDto.CallbackURL);
    return RequestOutcome.Conflict;
  }

  public async Task<RequestOutcome> UnScribeTaskAsync(DeleteSubscriptionConfigDto dto)
  {

    logger.WriteLine("starting unsubscription service");

    var subscriptionCfg = new SubscriptionConfig(dto.CallbackURL);
    var task = new ScheduledTask(subscriptionCfg, this.notifier, this.logger);

    if (await scheduler.UnSchedule(task))
    {
      logger.WriteLine("Task unscheduled");
      return RequestOutcome.Valid;
    }

    logger.WriteLine("Task was not unscheduled. Callback {0} was never registered", dto.CallbackURL);
    return RequestOutcome.NonExisting;
  }

  public async Task<RequestOutcome> UpdateTaskAsync(SubscriptionConfigDto dto)
  {
    logger.WriteLine("starting updating service");

    var outcome = RequestOutcome.Valid;

    outcome = await this.UnScribeTaskAsync(new DeleteSubscriptionConfigDto { CallbackURL = dto.CallbackURL });

    if (outcome == RequestOutcome.Valid)
    {
      outcome = await this.SubscribeTaskAsync(dto);
    }

    return outcome;
  }
}