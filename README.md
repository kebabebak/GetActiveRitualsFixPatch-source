# GetActiveRitualsFixPatch — build kit

Ready-to-use patch is here: https://github.com/kebabebak/HSK-Get-Active-Rituals-Fix-Patch

Files to compile `GetActiveRitualsFixPatch.dll` for RimWorld HSK 1.5.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (builds `net48`)
- Harmony and RimWorld refs from NuGet (`Lib.Harmony`, `Krafs.Rimworld.Ref`)

## Build

```powershell
dotnet build GetActiveRitualsFixPatch.csproj -c Release
```

Output: `out\GetActiveRitualsFixPatch.dll`
