using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot_CealingCat_Server;

public class Webhook(TelegramBotClient bot, HttpClient client, ILogger<Webhook> logger)
{
    private static readonly InlineKeyboardMarkup DeleteButton = new InlineKeyboardMarkup().AddButton("×", "Del");

    [Function("Webhook")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bot")] HttpRequest request)
    {
        Update? update = await request.ReadFromJsonAsync<Update>(JsonBotAPI.Options);

        if (update == null)
        {
            OnError(new Exception("Update is null"));
            return new OkResult();
        }

        try
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message != null)
                    await OnMessage(update.Message);
                else
                    OnError(new Exception("Message is null"));
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery?.Message != null)
                    await OnUpdate(update.CallbackQuery);
                else
                    OnError(new Exception("Callback is null"));
            }
        }
        catch (Exception ex) { OnError(ex); }

        return new OkResult();
    }

    private async Task OnMessage(Message message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            if (message.Type != MessageType.PinnedMessage)
                await bot.SendMessage(message.Chat, "喵 ?", replyMarkup: DeleteButton);

            return;
        }

        string[] parts = message.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string command = parts[0].Replace("@CealingCatBot", string.Empty);
        string param = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        switch (command)
        {
            case "/start":
                await bot.SendMessage(message.Chat, "喵 !", replyMarkup: DeleteButton);
                break;

            case "/generate":
                if (string.IsNullOrEmpty(param))
                {
                    await bot.SendMessage(message.Chat, "喵喵喵 ?", replyMarkup: DeleteButton);
                    return;
                }

                try
                {
                    await bot.SendMessage(message.Chat, await client.GetStringAsync($"api/host/generate?domain={Uri.EscapeDataString(param)}"), replyMarkup: DeleteButton);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
                {
                    await bot.SendMessage(message.Chat, "生成失败喵 ×", replyMarkup: DeleteButton);
                }
                break;

            case "/search":
                if (string.IsNullOrEmpty(param))
                {
                    await bot.SendMessage(message.Chat, "喵喵喵 ?", replyMarkup: DeleteButton);
                    return;
                }

                try
                {
                    await bot.SendMessage(message.Chat, await client.GetStringAsync($"api/host/search?domain={Uri.EscapeDataString(param)}"), replyMarkup: DeleteButton);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await bot.SendMessage(message.Chat, "没有找着喵 ×", replyMarkup: DeleteButton);
                }
                break;

            case "/check":
                await bot.SendMessage(message.Chat, await client.GetStringAsync("api/host/check"), replyMarkup: DeleteButton);
                break;

            case "/download":
                await bot.SendDocument(message.Chat, $"{Environment.GetEnvironmentVariable("API_URL")}/files/host", replyMarkup: DeleteButton);
                break;

            case "/meow":
                await bot.SendMessage(message.Chat, "喵 ~", replyMarkup: DeleteButton);
                break;

            default:
                await bot.SendMessage(message.Chat, "喵 ?", replyMarkup: DeleteButton);
                break;
        }
    }

    private async Task OnUpdate(CallbackQuery callback)
    {
        if (callback.Data == "Del")
            await bot.DeleteMessage(callback.Message!.Chat, callback.Message.MessageId);
        else
            await bot.SendMessage(callback.Message!.Chat, "喵 ?", replyMarkup: DeleteButton);

        await bot.AnswerCallbackQuery(callback.Id);
    }

    private void OnError(Exception exception) => logger.LogError(exception, "错误喵 ×");
}