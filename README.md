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

Press `1` to count input tokens (opens a submenu). Press `2` to count output tokens — this sends an actual API request and will consume credits. Press `3` to quit.

### Count input tokens

```
1 - Enter text directly
2 - Read few texts from a file
3 - Return back
```

- `1` — type a text and get its input token count (uses the token counting endpoint, no message is generated).
- `2` — read texts from a file, one text per line (empty lines are skipped). The app lists the `*.txt` files in the default directory (`data` under the current working directory if it exists, otherwise the working directory itself); enter a file number, a full path, or press Enter to use the default. For every line the app prints its token count, then:
  - detects the language of the texts (e.g. `English`, `Chinese (Simplified)`, or `Mixed` if the texts are in different languages). This makes one additional real API request, which consumes a small amount of credits;
  - prints the token count for each text and the average.
- `3` — return to the main menu.

### Sample data

The [ClaudeTokenCounter/data](ClaudeTokenCounter/data) folder contains ready-made prompt files (`Prompts - <LANG>.txt`) in 18 languages — English, Chinese (Simplified and Traditional), Japanese, Korean, Arabic, Hebrew, Hindi, Urdu, Bengali, Thai, Vietnamese, Indonesian, Persian, Georgian, Armenian, Azerbaijani and Kazakh — useful for comparing how many tokens the same content takes in different languages. The files are copied to the build output (`bin/<Config>/net9.0/data`) on every build. The app looks for `data` in the *current working directory*, so to have the files listed automatically, run it from the `ClaudeTokenCounter` project folder or from the build output folder (e.g. `bin/Debug/net9.0`); otherwise enter the path to a file manually.
