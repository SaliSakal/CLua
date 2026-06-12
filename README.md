# CLua 1.3

CLua is a console application originally created for modding the game **Original War**. It uses **Lua 5.5** scripts to process data tables and generate game files for use in mods.

> **Note:** The Original War game module — including the export/import tables and the corresponding Lua scripts — is **not part of this repository**, as it depends on proprietary data. It is included in the compiled releases available for download on the [CLua website](https://salisakal.cz/clua-clua).

## Built with

- [Terminal.GUI](https://github.com/gui-cs/Terminal.Gui) — console UI framework
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — Excel (.xlsx) read/write
- [Lua 5.5](https://www.lua.org/) — embedded scripting

Terminal.GUI and ClosedXML are NuGet packages and are restored automatically during build.

## Requirements

- **Windows:** `lua55.dll` must be placed in the repository root (next to the `CLua/` & `Content/` folders). The build process copies it automatically to the output directory.
- **Linux:** `lua55.so` must be placed in the repository root (next to the `CLua/` & `Content/` folder). The build process copies it automatically to the output directory.

> ⚠️ **Note:** On case-sensitive file systems, CLua behaves as if they were case-insensitive.

You can obtain the Lua 5.5 binaries from the [official Lua website](https://www.lua.org/download.html) or build them from source.

## Lua Scripts

The repository also contains the Lua scripts (`.lua` files) that form the CLua API. The core files — `CLua.lua`, `constants.lua`, and the `utils/` folder — are **required** for the application to function. They are loaded and executed by the application at runtime.

Any additional `.lua` file placed directly in the root of the `Lua/` folder is loaded automatically after the essential files.

## Building from Source

The project is written in **C#**. To compile it yourself:

1. Clone this repository.
2. Place `lua55.dll` (Windows) or `lua55.so` (Linux) in the repository root.
3. Open the solution in **Visual Studio** (or use the `dotnet` CLI).
4. Build the project in Release mode:
   ```
   dotnet build -c Release
   ```

## Documentation

Full documentation for the CLua API, available functions, and usage examples is available at:

**https://salisakal.cz/clua-clua?jazyk=en**

## License

This project is licensed under the **GNU General Public License v3.0**.
See the [LICENSE](LICENSE) file for details.
