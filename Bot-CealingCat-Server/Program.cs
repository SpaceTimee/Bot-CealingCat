using Microsoft.AspNetCore.Http.Json;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using Telegram.Bot;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.ConfigureTelegramBot<JsonOptions>(options => options.SerializerOptions);
builder.Services.AddSingleton(new TelegramBotClient(Environment.GetEnvironmentVariable("BOT_TOKEN")!));
builder.Services.AddSingleton(new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator })
{
    BaseAddress = new(Environment.GetEnvironmentVariable("API_URL") ?? "http://localhost"),
    DefaultRequestHeaders = { { "User-Agent", Environment.GetEnvironmentVariable("USER_AGENT") } }
});

builder.Build().Run();