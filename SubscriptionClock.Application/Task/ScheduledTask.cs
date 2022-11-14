using Timer = System.Timers.Timer;

// I've used the decorator pattern here to wrap a Timer class from C#
// It's a lightweitgh timer which is designed to work in a multi-threading
// environment.
public class ScheduledTask
{
  public Timer Timer { get; private set; }
  public ILogger Logger { get; private set; }
  public INotifier Notifier { get; private set; }
  public bool Running { get; private set; }
  public SubscriptionConfig Config { get; private set; }

  private bool Scheduled => Timer?.Enabled ?? false;

  private void Pause() => Timer.Stop();

  public ScheduledTask(SubscriptionConfig config, INotifier notifier, ILogger logger)
  {
    Logger = logger;
    Notifier = notifier;
    Config = config;
  }

  public void Schedule()
  {
    Timer = new(interval: Config.IntervalMs);
    Timer.Elapsed += async (sender, e) => await TaskRun(sender, e);
    Timer.Start();
  }

  public void UnSchedule()
  {
    if (Scheduled)
    {
      Timer.Stop();
      Timer.Dispose();
    }

  }

  private async Task TaskRun(object? sender, System.Timers.ElapsedEventArgs e)
  {
    // throttling strategy:
    // Task is still running from previous execution.
    // It means the request is taking more than the set interval to resolve
    // Unschedule future executions until current one finishes.
    if (Running) { Pause(); return; }

    Running = true;
    // this can take longer than the set frequency. Throttling may be necessary
    await Notifier.Notify(Config.CallbackURL, e.SignalTime);
    Running = false;

    // if it has been automatically unscheduled due to the above-mentioned
    // reasons. Subscribe again.
    if (!Scheduled) { Schedule(); }
  }

}