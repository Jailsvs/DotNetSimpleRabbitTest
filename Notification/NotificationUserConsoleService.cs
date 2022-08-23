namespace SimpleRabbitTest.Notification
{
  public class NotificationUserConsoleService : INotificationService
  {
    public void NotifyUser(int fromId, int toId, string content)
    {
      Console.WriteLine($"From user Id: {fromId} to user Id: {toId} - Content: {content}");
    }
  }
}