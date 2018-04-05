using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FeedMeBot.Logic;

namespace FeedMeBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private Order _currentOrder;


        public RootDialog()
        {
            _currentOrder = new Order();
        }


        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (string.IsNullOrWhiteSpace(activity.Text))
            {
                return;
            }

            IMessageAnalyzer analyzer = new LuisMessageAnalyzer();
            var response = await analyzer.GetResponse(activity.Text, _currentOrder);

            await context.PostAsync(response);

            context.Wait(MessageReceivedAsync);
        }
    }
}