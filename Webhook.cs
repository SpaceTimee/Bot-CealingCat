using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot_CealingCat;

public class Webhook(TelegramBotClient bot, HttpClient client, ILogger<Webhook> logger)
{
    private static readonly InlineKeyboardMarkup DeleteButton = new InlineKeyboardMarkup().AddButton("×", "Del");

    [Function("Webhook")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bot")] HttpRequest request)
    {
        Update? update = await request.ReadFromJsonAsync<Update>(JsonBotAPI.Options);

        if (update == null)
            return OnError(null, new Exception("Update is null"));

        if (update.Type == UpdateType.Message)
        {
            if (update.Message == null)
                return OnError(null, new Exception("Message is null"));

            try { await OnMessage(update.Message); }
            catch (Exception ex) { return OnError(update.Message.Chat, ex); }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            if (update.CallbackQuery == null || update.CallbackQuery.Message == null)
                return OnError(null, new Exception("Callback is null"));

            try { await OnUpdate(update.CallbackQuery); }
            catch (Exception ex) { return OnError(update.CallbackQuery.Message.Chat, ex); }
        }

        return new OkResult();
    }

    private async Task OnMessage(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            await bot.SendMessage(message.Chat, "喵 ?", replyMarkup: DeleteButton);
            return;
        }

        if (message.Text.StartsWith("/start"))
            await bot.SendMessage(message.Chat, "喵 !", replyMarkup: DeleteButton);
        else if (message.Text.StartsWith("/generate"))
        {
            string generateDomain = message.Text.Replace("/generate", string.Empty).Replace("@CealingCatBot", string.Empty).Trim();

            if (string.IsNullOrEmpty(generateDomain))
            {
                await bot.SendMessage(message.Chat, "喵喵喵 ?", replyMarkup: DeleteButton);
                return;
            }

            await bot.SendMessage(message.Chat, (await client.GetStringAsync($"api/generate?domain={Uri.EscapeDataString(generateDomain)}")).Replace('我', '喵'), replyMarkup: DeleteButton);
        }
        else if (message.Text.StartsWith("/search"))
        {
            string searchDomain = message.Text.Replace("/search", string.Empty).Replace("@CealingCatBot", string.Empty).Trim();

            if (string.IsNullOrEmpty(searchDomain))
            {
                await bot.SendMessage(message.Chat, "喵喵喵 ?", replyMarkup: DeleteButton);
                return;
            }

            await bot.SendMessage(message.Chat, (await client.GetStringAsync($"api/search?domain={Uri.EscapeDataString(searchDomain)}")).Replace('哦', '喵'), replyMarkup: DeleteButton);
        }
        else if (message.Text.StartsWith("/check"))
            await bot.SendMessage(message.Chat, await client.GetStringAsync("api/check"), replyMarkup: DeleteButton);
        else if (message.Text.StartsWith("/download"))
            await bot.SendDocument(message.Chat, $"{Environment.GetEnvironmentVariable("BACKEND_API_URL")}/files/Cealing-Host.json", replyMarkup: DeleteButton);
        else if (message.Text.StartsWith("/meow"))
            await bot.SendMessage(message.Chat, "喵 ~", replyMarkup: DeleteButton);
        else
            await bot.SendMessage(message.Chat, "喵 ?", replyMarkup: DeleteButton);
    }

    private async Task OnUpdate(CallbackQuery callback)
    {
        if (callback.Message == null)
            return;

        if (callback.Data == "Del")
            await bot.DeleteMessage(callback.Message.Chat, callback.Message.MessageId);
        else
            await bot.SendMessage(callback.Message.Chat, "喵 ?", replyMarkup: DeleteButton);

        await bot.AnswerCallbackQuery(callback.Id);
    }

    private StatusCodeResult OnError(Chat? chat, Exception exception)
    {
        if (chat != null)
            bot.SendMessage(chat, "失败喵 ×");

        logger.LogError(exception, "失败喵");

        return new StatusCodeResult(500);
    }
}
