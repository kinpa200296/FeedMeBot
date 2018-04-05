using System.Threading.Tasks;

namespace FeedMeBot.Logic
{
    public interface IMessageAnalyzer
    {
        Task<string> GetResponse(string message, Order currentOrder);
    }
}
