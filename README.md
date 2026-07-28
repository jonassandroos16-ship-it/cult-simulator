# Cult Simulator

A dark, atmospheric idle/management game built with **Blazor WebAssembly (C#)** that runs entirely in the browser. Found a cult, preach to gather faith, recruit followers, erect sacred constructions, learn rites, and ascend through mystical ranks.

## Tech Stack

- **C# / .NET 9** — Blazor WebAssembly (runs in-browser, no server needed)
- **xUnit** — 53 unit tests covering all game logic
- **GitHub Actions** — CI pipeline: test + build on every push/PR
- **Netlify** — automatic deployment from `main`

## Game Features

- **Preach** — tap the altar to generate Faith
- **Recruit** — convert Faith into Followers (passive income)
- **Build** — 4 construction types with geometric cost scaling
- **Rites** — 4 one-time upgrades that multiply production
- **Ranks** — 6 progression tiers from Novice to Ascended
- **Omens** — 5 random events with narrative choices
- **Save/Load** — progress persists in localStorage

## Local Development

```bash
dotnet restore
dotnet test
dotnet run --project src/CultSimulator
```

## Build for Production

```bash
dotnet publish src/CultSimulator/CultSimulator.csproj -c Release -o release
```

The published output in `release/wwwroot` is a static site deployable to any static host.

## Deployment

Push to `main` triggers:
1. GitHub Actions runs tests + builds the project
2. Netlify auto-deploys from `main` using `netlify.toml`
