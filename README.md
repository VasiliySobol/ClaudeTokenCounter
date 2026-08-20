# ClaudeTokenCounter

A small .NET 9 console app that counts input and output tokens in text messages using the Anthropic API.

**Author:** Vasyl Sobol

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- An [Anthropic API key](https://console.anthropic.com/)

## API Key Setup

Choose one of the following methods:

**Option 1 — .NET User Secrets (recommended):**
```powershell
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-api..." --project ClaudeTokenCounter/ClaudeTokenCounter.csproj
```

**Option 2 — Environment variable:**
```powershell
$env:ANTHROPIC_API_KEY = "sk-ant-api..."
```

## Running the App

```powershell
dotnet run --project ClaudeTokenCounter/ClaudeTokenCounter.csproj
```

## Usage

Once started, the app shows an interactive menu:

```
1 - Count input tokens
2 - Count output tokens (sends a real request)
3 - Exit
```

Press `1` to count input tokens without sending a real request. Press `2` to count output tokens — this sends an actual API request and will consume credits. Press `3` to quit.
