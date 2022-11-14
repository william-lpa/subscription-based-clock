using Xunit.Abstractions;

namespace SubscriptionClock.UnitTests.Database
{
  public class InMemoryDataSourceTest
  {
    private readonly ITestOutputHelper output;

    public InMemoryDataSourceTest(ITestOutputHelper output)
    {
      this.output = output;
    }

    [Fact]
    public async Task Fails_to_add_same_task_twice()
    {
      //arrange
      var store = new InMemoryDataSource();
      var cfg = new SubscriptionConfig("a-url", 100);
      var task = new ScheduledTask(cfg, null, new TestLogger(output));

      //act
      var insert = store.Add(task);
      var insert2 = store.Add(task);

      await Task.WhenAll(insert, insert2);

      //assert
      Assert.True(insert.Result);
      Assert.False(insert2.Result);
      Assert.Equal(task, await store.Get(task));
    }

    [Fact]
    public async Task Successfully_removes_added_data()
    {
      //arrange
      var store = new InMemoryDataSource();
      var cfg = new SubscriptionConfig("a-url", 100);
      var task = new ScheduledTask(cfg, null, new TestLogger(output));

      //act & assert
      Assert.True(await store.Add(task));
      Assert.True(await store.Remove(task));
      //data is not there anymore
      Assert.Null(await store.Get(task));
    }

    [Fact]
    public async Task Returns_false_if_removing_not_added_data()
    {
      //arrange
      var store = new InMemoryDataSource();
      var cfg = new SubscriptionConfig("a-url", 100);
      var task = new ScheduledTask(cfg, null, new TestLogger(output));

      //act & assert
      //nothing was removed
      Assert.False(await store.Remove(task));
    }

    [Fact]
    public async Task Returns_false_if_updating_not_added_data()
    {
      //arrange
      var store = new InMemoryDataSource();
      var cfg = new SubscriptionConfig("a-url", 100);
      var task = new ScheduledTask(cfg, null, new TestLogger(output));

      //act & assert
      Assert.False(await store.Update(task));
      Assert.Null(await store.Get(task));
    }

    [Fact]
    public async Task Successfully_updates_added_data()
    {
      //arrange
      var store = new InMemoryDataSource();
      var cfg = new SubscriptionConfig("a-url", 100);
      var task = new ScheduledTask(cfg, null, new TestLogger(output));

      //act
      await store.Add(task);

      //act & assert
      Assert.True(await store.Update(new ScheduledTask(new SubscriptionConfig("a-url", 123), null, new TestLogger(output))));
      Assert.Equal(123, (await store.Get(task)).Config.IntervalMs);
    }

  }

}
