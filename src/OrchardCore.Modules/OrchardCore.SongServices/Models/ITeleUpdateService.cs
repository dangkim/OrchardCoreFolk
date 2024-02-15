using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SongServices.Core.Models
{
    public interface ITeleUpdateService
    {
        Task EchoAsync(Update update);

        Task SendMessageAsync(string message, long chatId);
    }
}
