# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build (x64 only — RuntimeIdentifier is locked to win-x64)
dotnet build KubeAutomation/KubeAutomation.csproj

# Run
dotnet run --project KubeAutomation/KubeAutomation.csproj

# Run tests (in-app — trigger via the "test" / "тест" console command, or call directly)
# Tests run via TestRunner.RunAll() — no separate test project exists
```

The project requires Windows 10 (10.0.19041+) and the Windows App SDK. It will not build or run on non-Windows platforms.

The game data path is hardcoded in `WinUIWindow.xaml.cs:45`:
```
C:\Users\KOMP_2024\AppData\Roaming\Create_Cog_And_Circut
```
Change `_gamePath` and `_saveDir` there if running on a different machine.

## Architecture

This is a WinUI 3 desktop app (.NET 8, MVVM) that automates KubeJS recipe script generation for Minecraft modpacks (primarily GregTech and Create mod recipes).

### Data flow

1. `App.xaml.cs` creates `WinUIWindow` and calls `MainLogic.StartMainLogicAsync()`.
2. `WinUIWindow.RunItemDemo()` uses `ItemFluidWithIconExtractor` (from the sibling `LogExtractorLibrary` project) to load all Minecraft items/fluids with icons from the game directory.
3. Items are stored in `MainViewModel.Items` (`ObservableCollection<MinecraftItemViewModel>`) and displayed via a filtered `FilteredItems` collection.
4. The user searches for items, then uses the console command panel to trigger recipe creation or extraction.

### Key subsystems

**Recipe configurations** (`GenerationStrategies/RecipeConfigurations/`)  
All recipe types inherit `BaseRecipeConfiguration`, which mandates `RecipeTypeId`, `Validate()`, and `GenerateJsCode()`. Concrete types include:
- `Create/*Config` — Create mod recipes (Mixing, Crushing, Filling, Deploying, etc.)
- `GregTechRecipeConfiguration` — GregTech machine recipes
- `RecipeRemovalConfig` / `RecipeModificationConfig` — removal/modification entries
- `RawCodeBlock` — unrecognized JS preserved verbatim (use `RawNode?.ToString()`, not a `Content` property)
- `CustomRecipeConfig` — arbitrary `event.recipes({...})` calls

**JS parser** (`GenerationStrategies/Extractors/JsRecipeParser.cs`)  
Parses KubeJS recipe files using Esprima (JS AST). Only supports the single-statement format:
```js
ServerEvents.recipes(event => { ... })
```
Each statement inside the lambda is dispatched to a registered `IRecipeExtractor` via `ExtractorRegistry`. Unrecognized nodes become `RawCodeBlock`.

**Extractors** (`GenerationStrategies/Extractors/`)  
`ExtractorRegistry.CreateDefault()` registers all built-in extractors. To add support for a new recipe type: implement `IRecipeExtractor`, register it in `ExtractorRegistry.CreateDefault()`, and create a matching `*Config` class.

**File saving** (`FileSaveStrategies/`)  
`FileSaver` writes new recipe files; `FileSaverToEnd` appends to existing ones.

**Tests** (`Tests/`)  
Custom test framework (no xUnit/NUnit). `TestRunner.RunAll()` runs all cases in `Tests/Cases/`. Add new test classes inheriting `TestBase` and register them in `TestRunner.RunAll()`. Tests are triggered from the in-app console with the `test` command.

**UI threading**  
All background-to-UI updates use `DispatcherQueue.TryEnqueue()`. Console output uses a `_consoleLock` + `DispatcherQueue` pattern. Search uses a 100 ms `DispatcherTimer` debounce.

### External dependency

`LogExtractorLibrary` is a sibling project at `../../LogExtractorLibrary/LogExtractorLibrary/`. It provides `ItemFluidWithIconExtractor`, `RecipesExtractor`, `MinecraftItem`, and `GamePathProvider`. The solution file (`KubeAutomation.sln`) must include both projects for the build to succeed.
