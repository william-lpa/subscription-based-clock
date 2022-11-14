// Interface that orchestrates the execution of tasks
public interface ITaskScheduler
{

  public Task<bool> Schedule(ScheduledTask schTask);
  public Task<bool> UnSchedule(ScheduledTask schTask);
}