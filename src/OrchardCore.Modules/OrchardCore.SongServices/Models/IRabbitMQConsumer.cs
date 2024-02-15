
namespace OrchardCore.SongServices.Models
{    
    public interface IConversationConsumer
    {
        public void GetUserConversation();        
    }

    public interface ICreateConversationConsumer
    {        
        public void CreateConversation();
    }

    public interface IReadMessageConsumer
    {
        public void UserReadMessage();
    }
    public interface ICreateMessageConsumer
    {
        public void CreateMessage();
    }

    public interface ICreateNotifyConsumer
    {
        public void CreateUserNotify();
    }
    public interface IUserNotifyConsumer
    {
        public void GetUserNotify();
    }
    public interface IReadUserNotifyConsumer
    {
        public void ReadUserNotify();
    }
    public interface IJoinToGroupConsumer
    {
        public void JoinToGroup();
    }
    public interface IConnectedSignalRConsumer
    {
        public void GetConnectedSignalRUser();
    }

}
