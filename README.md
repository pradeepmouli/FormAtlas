# FormAtlas

Starter repository for a reusable library targeting .NET Standard 2.0 and .NET Framework 4.7.8.

## Prerequisites

- .NET SDK 9.0+

## Devcontainer (macOS-friendly)

- Reopen the repository in container using `.devcontainer/devcontainer.json`.
- Use `netstandard2.0` for local container/macOS build.

## Build

```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f netstandard2.0
```

Windows release build for .NET Framework 4.7.8:

```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f net478
```

## Pack

```bash
dotnet pack src/FormAtlas.Tool/FormAtlas.Tool.csproj -c Release
```
