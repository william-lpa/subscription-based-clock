using System.Collections.Concurrent;

// In memory cache to store added tasks
public class InMemoryDataSource : IDataSource
{
  private IDictionary<string, ScheduledTask> memCache;

  public InMemoryDataSource()
  {
    // ConcurrentDictionary is a data structure from c#, similar to
    // a Hashmap which is able to read/write data in a multi-threading
    // environment.
    this.memCache = new ConcurrentDictionary<string, ScheduledTask>();
  }

  public Task<bool> Add(ScheduledTask schTask)
  {
    return Task.FromResult(this.memCache.TryAdd(schTask.Config.CallbackURL, schTask));
  }

  public Task<ScheduledTask> Get(ScheduledTask schTask)
  {
    if (!this.memCache.ContainsKey(schTask.Config.CallbackURL)) { return Task.FromResult<ScheduledTask>(null); };

    return Task.FromResult(this.memCache[schTask.Config.CallbackURL]);
  }

  public Task<bool> Remove(ScheduledTask schTask)
  {
    return Task.FromResult(this.memCache.Remove(schTask.Config.CallbackURL));
  }

  public Task<bool> Update(ScheduledTask schTask)
  {
    if (this.memCache.ContainsKey(schTask.Config.CallbackURL))
    {
      this.memCache[schTask.Config.CallbackURL] = schTask;
      return Task.FromResult(true);
    }

    return Task.FromResult(false);
  }
}