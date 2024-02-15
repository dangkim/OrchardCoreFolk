//using OrchardCore.SongServices.Models;

namespace OrchardCore.SongServices.Models
{
    public interface IRabbitMQProducer
    {
        public bool CreateUserNotify(CreateNotifyModel model);
        public bool GetUserNotify(string userName);
        public bool ConnectedSignalR(string userName);
        public bool ReadUserNotify(string userName, long notifyId);

        public bool JoinToGroup(string conversationId, string userName, string sender);
        public bool GetUserConversation(string userName);       
        public bool CreateConversation(CreateConversationModel model);


        public bool CreateMessage(CreateMessageModel model);
        public bool UserReadMessage(string userName, long conversationId);
    }
}
