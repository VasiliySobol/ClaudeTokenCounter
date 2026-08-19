using System.Text;
using Anthropic;
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
    Console.WriteLine("1 - Enter text to count tokens");
    Console.WriteLine("2 - Exit");

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("> ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter your text:");
            var textContentToCount = Console.ReadLine() ?? string.Empty;

            var parameters = new MessageCountTokensParams
            {
                Model = Model.ClaudeOpus5,
                Messages = [new() { Role = Role.User, Content = textContentToCount }]
            };

            var response = await client.Messages.CountTokens(parameters);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(response);
            break;

        case "2":
            return;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            break;
    }
}