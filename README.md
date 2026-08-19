# ClaudeTokenCounter

A small .NET 9 console app that counts input tokens for text messages using the Anthropic API.

**Author:** Vasyl Sobol

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- An [Anthropic API key](https://console.anthropic.com/)

## API Key Setup

Choose one of the following methods:

**Option 1 — .NET User Secrets (recommended, not committed to git):**
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
1 - Enter text to count tokens
2 - Exit
```

Press `1`, paste or type your text, and hit Enter — the token count will be displayed. Press `2` to quit.
