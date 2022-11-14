
// Orchestrates and manages the execution of ScheduledTasks
// storing them into a datasource.
public class TaskScheduler : ITaskScheduler
{
  private readonly IDataSource dataSource;
  private readonly ILogger logger;

  public TaskScheduler(IDataSource db, ILogger logger)
  {
    this.dataSource = db;
    this.logger = logger;
  }

  public async Task<bool> Schedule(ScheduledTask schTask)
  {
    logger.WriteLine("Registering task: {0}", schTask.Config.CallbackURL);

    if (await this.dataSource.Add(schTask))
    {
      logger.WriteLine("Starting subscription");
      schTask.Schedule();
      return true;
    }


    logger.WriteLine("failed to register subscription to datastore");
    return false;
  }

  public async Task<bool> UnSchedule(ScheduledTask schTask)
  {
    logger.WriteLine("Unegistering task: {0}", schTask.Config.CallbackURL);

    logger.WriteLine("removing from db");
    var task = await this.dataSource.Get(schTask);

    if (await this.dataSource.Remove(schTask))
    {
      logger.WriteLine("Unscheduling subscription");
      task.UnSchedule();
      return true;
    };

    logger.WriteLine("failed to remove task from db");
    return false;

  }

}