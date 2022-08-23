namespace SimpleRabbitTest.Notification
{
  public interface INotificationService
    {
        public void NotifyUser(int fromId, int toId, string content);
    }
}