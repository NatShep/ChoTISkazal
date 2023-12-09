using Serilog;
using Serilog.Events;
using TelegramSink;

namespace SayWhat.Bll;

public static class TelegramLogger {
    public static ILogger CreateLogger(string apiKey, string chatId) {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        return Log.Logger = new LoggerConfiguration()
            .WriteTo.TeleSink(
                telegramApiKey: apiKey,
                telegramChatId: chatId,
                minimumLevel: LogEventLevel.Information)
            .CreateLogger();
    }
}