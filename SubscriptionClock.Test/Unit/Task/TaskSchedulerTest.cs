using Xunit.Abstractions;

namespace SubscriptionClock.UnitTests.TaskSchedulerTest
{

  // A notifier that acknowledges if
  // it received the expected notification
  class TestNotifier : INotifier
  {
    public string ExpectedAddress { get; set; }
    public int DelayMs { get; set; }

    public int Runs { get; private set; }

    public async Task Notify(string address, DateTime date)
    {
      Assert.Equal(ExpectedAddress, address);
      Assert.NotNull(date);
      Runs++;
      await Task.Delay(DelayMs);
    }
  }

  public class TaskSchedulerTest
  {
    private readonly ITestOutputHelper output;

    public TaskSchedulerTest(ITestOutputHelper output)
    {
      this.output = output;
    }


    [Fact]
    public async Task Succesfully_invokes_a_sheduled_task()
    {
      //arrange
      const string expectedAddress = "http://my-address.com";

      var db = new InMemoryDataSource();
      var log = new TestLogger(this.output);

      var taskScheduler = new TaskScheduler(db, log);
      var taskConfig = new SubscriptionConfig(expectedAddress, 5);
      var testNofiter = new TestNotifier
      {
        ExpectedAddress = "http://my-address.com",
        DelayMs = 0
      };

      var task = new ScheduledTask(taskConfig, testNofiter, log);

      //act & assert
      Assert.True(await taskScheduler.Schedule(task));
      Assert.Equal(task, await db.Get(task));
      await Task.Delay(60);
      Assert.False(task.Running);
      // it ran every 5ms
      Assert.True(testNofiter.Runs > 1);
    }

    [Fact]
    public async Task It_skips_notification_if_event_is_still_in_progress()
    {
      //arrange
      const string expectedAddress = "http://my-address.com";
      var db = new InMemoryDataSource();
      var log = new TestLogger(this.output);

      var taskScheduler = new TaskScheduler(db, log);
      var taskConfig = new SubscriptionConfig(expectedAddress, 5);
      var testNofiter = new TestNotifier
      {
        ExpectedAddress = "http://my-address.com",
        DelayMs = 100
      };

      var task = new ScheduledTask(taskConfig, testNofiter, log);

      //act & assert
      Assert.True(await taskScheduler.Schedule(task));
      Assert.Equal(task, await db.Get(task));

      await Task.Delay(60);
      Assert.Equal(1, testNofiter.Runs);
      // Notifier is still hanging
      Assert.True(task.Running);

      await Task.Delay(100);
      // 160ms has passed, it should have resumed and ran it for the 2nd time.
      Assert.Equal(2, testNofiter.Runs);
    }

    [Fact]
    public async Task Succesfully_stops_invoking_a_sheduled_task_when_unsubscribed()
    {
      // arrange
      const string expectedAddress = "http://my-address.com";
      var db = new InMemoryDataSource();
      var log = new TestLogger(this.output);

      var taskScheduler = new TaskScheduler(db, log);
      var taskConfig = new SubscriptionConfig(expectedAddress, 5);
      var testNofiter = new TestNotifier
      {
        ExpectedAddress = "http://my-address.com",
        DelayMs = 0
      };
      var task = new ScheduledTask(taskConfig, testNofiter, log);

      //act & assert
      Assert.True(await taskScheduler.Schedule(task));
      Assert.Equal(task, await db.Get(task));
      await Task.Delay(60);
      Assert.True(await taskScheduler.UnSchedule(task));
      var runs = testNofiter.Runs;

      await Task.Delay(60);
      Assert.False(task.Running);
      // since it has been unsubscribed, no more runs should have happened
      Assert.Equal(runs, testNofiter.Runs);
    }
  }
}