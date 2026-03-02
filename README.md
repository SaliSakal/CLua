# CLua

CLua is a console application originally created for modding the game **Original War**. It uses **Lua 5.4** scripts to process data tables and generate game files for use in mods.

> **Note:** The Original War game module — including the export/import tables and the corresponding Lua scripts — is **not part of this repository**, as it depends on proprietary data. It is included in the compiled releases available for download on the [CLua website](https://salisakal.cz/clua).

## Requirements

- **Windows:** `Lua54.dll` must be present in the same directory as the executable.
- **Linux:** `lua54.so` must be present in the same directory as the executable.

You can obtain the Lua 5.4 binaries from the [official Lua website](https://www.lua.org/download.html) or build them from source.

## Lua Scripts

The repository also contains the Lua scripts (`.lua` files) that form the CLua API. The core files — `CLua.lua`, `Constants.lua`, and the `utils/` folder — are **required** for the application to function. They are loaded and executed by the application at runtime and must be present alongside the executable.

## Building from Source

The project is written in **C#**. To compile it yourself:

1. Clone this repository.
2. Open the solution in **Visual Studio** (or use the `dotnet` CLI).
3. Build the project in Release mode:
   ```
   dotnet build -c Release
   ```
4. Place `Lua54.dll` (Windows) or `lua54.so` (Linux) next to the compiled executable.

## Documentation

Full documentation for the CLua API, available functions, and usage examples is available at:

**https://salisakal.cz/clua**

## License

This project is licensed under the **GNU General Public License v3.0**.
See the [LICENSE](LICENSE) file for details.
