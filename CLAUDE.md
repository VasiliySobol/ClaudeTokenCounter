# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build
dotnet build ClaudeTokenCounter/ClaudeTokenCounter.csproj

# Run
dotnet run --project ClaudeTokenCounter/ClaudeTokenCounter.csproj
```

## API Key Setup

The app requires an Anthropic API key provided via one of:

```powershell
# Preferred — stored in .NET user secrets (not committed to git)
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-api..." --project ClaudeTokenCounter/ClaudeTokenCounter.csproj

# Alternative — environment variable
$env:ANTHROPIC_API_KEY = "sk-ant-api..."
```

User secrets take precedence over the environment variable.

## Architecture

Single-file console app (`ClaudeTokenCounter/Program.cs`) using top-level statements and local functions. No classes. Sample prompt files live in `ClaudeTokenCounter/data/` (`Prompts - <LANG>.txt`, one text per line; ENG, CHS, CHT, JPN, KOR, ARA, HEB, HIN, URD, BEN, THA, VIE, IND, PER, GEO, ARM, AZE, KAZ). The `.csproj` copies `data\**\*.txt` to the output directory (`CopyToOutputDirectory="PreserveNewest"`), so the files also end up in `bin/<Config>/net9.0/data/`.

- Reads the API key from .NET user secrets (`Microsoft.Extensions.Configuration.UserSecrets`) or `ANTHROPIC_API_KEY` env var
- Creates one `AnthropicClient` instance for the lifetime of the process
- Main menu loop: **1** opens the input-tokens submenu, **2** calls `client.Messages.Create()` (`MaxTokens = 8096`) and prints `Usage.OutputTokens`, **3** exits
- Input-tokens submenu (`RunCountInputTokensMenu`): **1** counts tokens for a single typed text, **2** reads texts from a file, **3** returns to the main menu
- File option: the default directory is `./data` under the *current working directory* if it exists, otherwise the working directory itself (the lookup is not relative to the exe, so the copied `bin/.../data` is only found when the app is started from the output folder). `*.txt` files there are listed as a numbered menu; the user enters a number, a full path, or empty input (= default directory). Each non-empty line is counted separately via `CountInputTokens` (`client.Messages.CountTokens()`), then the average is printed (rounded away from zero)
- Language detection (`DetectLanguage`): after the file is processed, one `Messages.Create()` call with a forced tool call (`report_language`, `ToolChoiceTool`, thinking disabled, `MaxTokens = 200`) returns the language name, `Mixed` if the texts differ, or `Chinese (Simplified)` / `Chinese (Traditional)` for Chinese. The prompt differs for one text vs. several
- All calls use `Model.ClaudeOpus5`
- `AnthropicBadRequestException` is caught in the main loop and the API error message is extracted from the response body by `GetApiErrorMessage`
- Console output is colour-coded (cyan menus, green results, red errors, blue key-source info)

## Key Dependencies

| Package | Purpose |
|---|---|
| `Anthropic` v12.40.0 | Official Anthropic .NET SDK — provides `AnthropicClient`, `MessageCountTokensParams`, `Model` |
| `Microsoft.Extensions.Configuration.UserSecrets` | Reads API key from .NET user secrets store |
