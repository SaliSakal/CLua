# CLua

CLua is a console application for modding the game **Original War**. It extends the game's scripting capabilities using **Lua 5.4**, allowing modders to write and run Lua scripts as part of their mods.

## Requirements

- **Windows:** `Lua54.dll` must be present in the same directory as the executable.
- **Linux:** `lua54.so` must be present in the same directory as the executable.

You can obtain the Lua 5.4 binaries from the [official Lua website](https://www.lua.org/download.html) or build them from source.

## Lua Scripts

The repository also contains the Lua scripts (`.lua` files) that are part of CLua. These scripts define the modding API and helper functions available to mod authors. They are loaded and executed by the application at runtime.

If you want to use or extend the Lua-side functionality, you can find the scripts in the `Lua/` directory.

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
