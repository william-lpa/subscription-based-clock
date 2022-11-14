// Interface for source of notification. For this project it
// implements a HTTPNotification using DI. Other protocols could
// be implemented using the same interface  
public interface INotifier
{
  public Task Notify(string address, DateTime date);

}