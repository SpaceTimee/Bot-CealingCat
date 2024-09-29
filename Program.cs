using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Http = OnaCore.Http;

int TokenIndex = Array.FindIndex(args, arg => arg.Equals("-t", StringComparison.OrdinalIgnoreCase)) + 1;
string? MainBotToken = string.Empty;

if (TokenIndex != 0 && TokenIndex != args.Length)
    MainBotToken = args[TokenIndex];

while (!Regex.IsMatch(MainBotToken!, @"^\d{8,10}:[a-zA-Z0-9_-]{35}$"))
{
    Console.Write("给我 Bot Token 喵: ");
    MainBotToken = Console.ReadLine();
}

HttpClient MainClient = new(new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }) { BaseAddress = new("https://localhost") };
TelegramBotClient MainBot = new(MainBotToken!);

MainBot.OnMessage += OnMessage;
MainBot.OnUpdate += OnUpdate;
MainBot.OnError += OnError;

Console.WriteLine($"@{(await MainBot.GetMeAsync()).Username} 启动!");
Console.ReadLine();

async Task OnMessage(Message msg, UpdateType type)
{
    if (string.IsNullOrEmpty(msg.Text))
    {
        await MainBot.SendTextMessageAsync(msg.Chat, "喵 ?", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));

        return;
    }

    if (msg.Text!.StartsWith("/start"))
        await MainBot.SendTextMessageAsync(msg.Chat, "喵 !", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    else if (msg.Text!.StartsWith("/generate"))
    {
        string generateDomain = msg.Text.Replace("/generate", string.Empty).Replace("@CealingCatBot", string.Empty).Replace("http://", string.Empty).Replace("https://", string.Empty).Trim().TrimEnd('/');

        if (string.IsNullOrWhiteSpace(generateDomain))
        {
            await MainBot.SendTextMessageAsync(msg.Chat, "喵喵喵 ?", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));

            return;
        }

        await MainBot.SendTextMessageAsync(msg.Chat, (await Http.GetAsync<string>($"/api/generate?domain={generateDomain}", MainClient)).Replace('我', '喵'), replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    }
    else if (msg.Text!.StartsWith("/search"))
    {
        string searchDomain = msg.Text.Replace("/search", string.Empty).Replace("@CealingCatBot", string.Empty).Replace("http://", string.Empty).Replace("https://", string.Empty).Trim().TrimEnd('/');

        if (string.IsNullOrWhiteSpace(searchDomain))
        {
            await MainBot.SendTextMessageAsync(msg.Chat, "喵喵喵 ?", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));

            return;
        }

        await MainBot.SendTextMessageAsync(msg.Chat, (await Http.GetAsync<string>($"/api/search?domain={searchDomain}", MainClient)).Replace('哦', '喵'), replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    }
    else if (msg.Text!.StartsWith("/check"))
        await MainBot.SendTextMessageAsync(msg.Chat, await Http.GetAsync<string>("/api/check", MainClient), replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    else if (msg.Text!.StartsWith("/download"))
        await MainBot.SendDocumentAsync(msg.Chat, "https://server.spacetimee.xyz/api/download/Cealing-Host.json", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    else if (msg.Text!.StartsWith("/meow"))
        await MainBot.SendTextMessageAsync(msg.Chat, "喵 ~", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
    else
        await MainBot.SendTextMessageAsync(msg.Chat, "喵 ?", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
}
async Task OnUpdate(Update update)
{
    if (update.CallbackQuery!.Data == "Del")
        await MainBot.DeleteMessageAsync(update.CallbackQuery.Message!.Chat, update.CallbackQuery.Message.MessageId);
    else
        await MainBot.SendTextMessageAsync(update.CallbackQuery.Message!.Chat, "喵 ?", replyMarkup: new InlineKeyboardMarkup().AddButton("×", "Del"));
}
async Task OnError(Exception exception, HandleErrorSource source) => await Task.Run(() => Console.WriteLine(exception.Message));