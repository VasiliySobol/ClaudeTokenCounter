using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = Encoding.UTF8;
Console.ForegroundColor = ConsoleColor.White;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKeyFromSecrets = config["Anthropic:ApiKey"];
var apiKeyFromEnv = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
var apiKey = apiKeyFromSecrets ?? apiKeyFromEnv;

if (apiKey is null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(
        """
        API key not found. Set it using one of these commands:
        dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-api..."
        $env:ANTHROPIC_API_KEY = "sk-ant-api..."
        """);

    Console.ForegroundColor = ConsoleColor.White;
    return;
}

var source = apiKeyFromSecrets is not null ? "user secrets" : "environment variable";

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine($"Anthropic API key found successfully via {source}.");

AnthropicClient client = new(new Anthropic.Core.ClientOptions { ApiKey = apiKey });

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("1 - Count input tokens");
    Console.WriteLine("2 - Count output tokens (sends a real request)");
    Console.WriteLine("3 - Exit");

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("> ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await RunCountInputTokensMenu();
                break;

            case "2":
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Enter your text:");
                var textContentToSend = Console.ReadLine() ?? string.Empty;

                var messageParams = new MessageCreateParams
                {
                    Model = Model.ClaudeOpus5,
                    MaxTokens = 8096,
                    Messages = [new() { Role = Role.User, Content = textContentToSend }]
                };

                var messageResponse = await client.Messages.Create(messageParams);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Output tokens: {messageResponse.Usage.OutputTokens}");
                break;

            case "3":
                return;

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                break;
        }
    }
    catch (AnthropicBadRequestException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(GetApiErrorMessage(ex));
    }
}

async Task RunCountInputTokensMenu()
{
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("1 - Enter text directly");
        Console.WriteLine("2 - Enter few texts from a file");
        Console.WriteLine("3 - Return back");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("> ");
        var subChoice = Console.ReadLine();

        switch (subChoice)
        {
            case "1":
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Enter your text:");
                var text = Console.ReadLine() ?? string.Empty;
                await CountInputTokens(text);
                break;

            case "2":
                Console.ForegroundColor = ConsoleColor.White;
                var currentDirectory = Directory.GetCurrentDirectory();
                var dataDirectory = Path.Combine(currentDirectory, "data");
                var defaultPath = Directory.Exists(dataDirectory) ? dataDirectory : currentDirectory;
                Console.WriteLine($"Enter path to the file [{defaultPath}]:");

                var txtFiles = Directory.Exists(defaultPath)
                    ? Directory.GetFiles(defaultPath, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray()
                    : [];

                if (txtFiles.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    for (var i = 0; i < txtFiles.Length; i++)
                    {
                        Console.WriteLine($"{i + 1} - {Path.GetFileName(txtFiles[i])}");
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                }

                var pathInput = Console.ReadLine();
                string path;
                if (int.TryParse(pathInput, out var fileChoice) && fileChoice >= 1 && fileChoice <= txtFiles.Length)
                {
                    path = txtFiles[fileChoice - 1];
                }
                else
                {
                    path = string.IsNullOrWhiteSpace(pathInput) ? defaultPath : pathInput;
                }

                if (!File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File not found: {path}");
                    break;
                }

                var lines = await File.ReadAllLinesAsync(path);
                var texts = new List<string>();
                var tokenCounts = new List<long>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Text: {line}");
                    texts.Add(line);
                    tokenCounts.Add(await CountInputTokens(line));
                    Console.WriteLine();
                }

                if (tokenCounts.Count > 0)
                {
                    var language = await DetectLanguage(texts);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Texts language is {language}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    for (var i = 0; i < tokenCounts.Count; i++)
                    {
                        Console.WriteLine($"Input tokens for Text {i + 1}: {tokenCounts[i]}");
                    }

                    Console.WriteLine();
                    var average = (int)Math.Round(tokenCounts.Average(), MidpointRounding.AwayFromZero);
                    Console.WriteLine($"Average input tokens: {average}");
                }

                break;

            case "3":
                return;

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                break;
        }
    }

    async Task<long> CountInputTokens(string text)
    {
        var countParams = new MessageCountTokensParams
        {
            Model = Model.ClaudeOpus5,
            Messages = [new() { Role = Role.User, Content = text }]
        };

        var countResponse = await client.Messages.CountTokens(countParams);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Input tokens: {countResponse.InputTokens}");
        return countResponse.InputTokens;
    }

    async Task<string> DetectLanguage(IReadOnlyList<string> texts)
    {
        var prompt = new StringBuilder();
        if (texts.Count == 1)
        {
            prompt.AppendLine("Identify the language of the following text.");
            prompt.AppendLine("Respond with only the English name of that language (for example: English, Russian, Spanish) and nothing else.");
            prompt.AppendLine("Do not include any explanation, punctuation, or extra words.");
            prompt.AppendLine();
            prompt.AppendLine(texts[0]);
        }
        else
        {
            prompt.AppendLine("Identify the language of each of the following numbered texts.");
            prompt.AppendLine("If all texts are written in the same language, respond with only the English name of that language (for example: English, Russian, Spanish) and nothing else.");
            prompt.AppendLine("If the texts are written in different languages, respond with only the single word: Mixed");
            prompt.AppendLine("Do not include any explanation, punctuation, or extra words.");
            prompt.AppendLine();

            for (var i = 0; i < texts.Count; i++)
            {
                prompt.AppendLine($"Text {i + 1}: {texts[i]}");
            }
        }

        var reportLanguageTool = new Tool
        {
            Name = "report_language",
            Description = "Reports the detected language of the given text(s).",
            InputSchema = new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["language"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "The English name of the language (for example: English, Russian, Spanish), or the single word Mixed if the texts are in different languages."
                    })
                },
                Required = ["language"]
            }
        };

        var languageParams = new MessageCreateParams
        {
            Model = Model.ClaudeOpus5,
            MaxTokens = 200,
            Thinking = new ThinkingConfigDisabled(),
            Tools = [reportLanguageTool],
            ToolChoice = new ToolChoiceTool("report_language"),
            Messages = [new() { Role = Role.User, Content = prompt.ToString() }]
        };

        var languageResponse = await client.Messages.Create(languageParams);
        foreach (var block in languageResponse.Content)
        {
            if (block.TryPickToolUse(out var toolUse) && toolUse.Input.TryGetValue("language", out var languageValue))
            {
                return languageValue.GetString() ?? "Unknown";
            }
        }

        return "Unknown";
    }
}

static string GetApiErrorMessage(AnthropicApiException ex)
{
    try
    {
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(ex.ResponseBody);
        return errorResponse?.Error.Message ?? ex.Message;
    }
    catch (JsonException)
    {
        return ex.Message;
    }
}