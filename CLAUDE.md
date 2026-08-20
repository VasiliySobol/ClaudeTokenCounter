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

Single-file console app (`ClaudeTokenCounter/Program.cs`) using top-level statements. No classes or additional files.

- Reads the API key from .NET user secrets (`Microsoft.Extensions.Configuration.UserSecrets`) or `ANTHROPIC_API_KEY` env var
- Creates one `AnthropicClient` instance for the lifetime of the process
- Runs a menu loop: **1** calls `client.Messages.CountTokens()` with the user-supplied text against `Model.ClaudeOpus5`, **2** calls `client.Messages.Create()` and reads `Usage.OutputTokens` from the response, **3** exits
- Token count response is printed directly from the API result object

## Key Dependencies

| Package | Purpose |
|---|---|
| `Anthropic` v12.40.0 | Official Anthropic .NET SDK — provides `AnthropicClient`, `MessageCountTokensParams`, `Model` |
| `Microsoft.Extensions.Configuration.UserSecrets` | Reads API key from .NET user secrets store |
