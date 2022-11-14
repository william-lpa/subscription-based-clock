// Interface for the cache, one could probably use a different data source to permanently persist data
public interface IDataSource
{
  public Task<bool> Add(ScheduledTask schTask);
  public Task<bool> Remove(ScheduledTask schTask);
  public Task<ScheduledTask> Get(ScheduledTask schTask);
  public Task<bool> Update(ScheduledTask schTask);

}