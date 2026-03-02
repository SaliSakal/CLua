using CLua;
using System.Text;
using static Lua.LuaNative;


public static class FileManager
{

    public static Stream OpenGameFile(string relativePath)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory; // Složka aplikace
        string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

        if (!fullPath.StartsWith(basePath))
        {
            throw new UnauthorizedAccessException("❌ Unable to save file outside application folder!");
        }

        string directoryPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }


        return new FileStream(fullPath, FileMode.Create, FileAccess.Write);
    }

    public static int SaveFile(IntPtr L)
    {
        string fileName = ToLuaString(L, 1);
        string data = ToLuaString(L, 2);
        string encodingName = ToLuaString(L, 3).ToLower(); // Převod na malá písmena

        Encoding encoding = encodingName switch
        {
            "ansi" => Encoding.Default, // Systémové ANSI kódování
            "utf8" => new UTF8Encoding(false), // UTF-8 bez BOM
            "utf8bom" => Encoding.UTF8, // UTF-8 s BOM
            "utf16" => Encoding.Unicode, // UTF-16 (LE)
            "utf16le" => Encoding.Unicode, // Explicitně Little Endian
            "utf16be" => Encoding.BigEndianUnicode, // UTF-16 Big Endian
            _ => new UTF8Encoding(false) // Výchozí UTF-8 bez BOM
        };

        try
        {
            using (Stream stream = OpenGameFile(fileName))
            using (StreamWriter writer = new StreamWriter(stream, encoding))
            {
                writer.Write(data);
            }

            //Console.WriteLine($"📄 File '{fileName}' was successfully saved in {encodingName.ToUpper()} encoding.");
            PushLuaBoolean(L, true); // ✅ Úspěch → vrátíme true
            return 1;
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"❌ Error saving file: {ex.Message}");
            PushLuaError(L, "❌ Error saving file: " + ex.Message);
            //PushLuaBoolean(L, false); // ❌ Chyba → vrátíme false
            return 1;
        }
    }

    // Kontrola existence souboru s bezpečnostní kontrolou
    // Je case sensitivní, před voláním použíj Utils.FindFileCaseInsensitive pro nalezení správné cesty
    // use Utils.FindFileCaseInsensitive to get correct path before calling this method for best results, otherwise it will be case sensitive and may not find the file on case sensitive filesystems
    public static bool FileExists(string relativePath)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory; // Složka aplikace
        string fullPath= Path.GetFullPath(Path.Combine(basePath, relativePath));
           
        if (!fullPath.StartsWith(basePath))
        {
            Console.WriteLine($"❌ Unable to check file outside application folder! (" + relativePath + ")");
            return false;
        }
        
        if (!File.Exists(fullPath))
        {

            return false;
        }


        return true;
    }

    public static int FileExists(IntPtr L)

    {
        string path = ToLuaString(L, 1);
        path = Utils.FindFileCaseInsensitive(path,false);

        PushLuaBoolean(L, FileExists(path));
        return 1;
    }

    // use Utils.FindFileCaseInsensitive to get correct path before calling this method for best results, otherwise it will be case sensitive and may not find the file on case sensitive filesystems
    public static string LoadFile(string path, string encodingName = null)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.GetFullPath(Path.Combine(basePath, path));

        if (encodingName != null)
        {
            Encoding? encoding = encodingName switch
            {
                "ansi" => Encoding.Default, // Systémové ANSI kódování
                "utf8" => new UTF8Encoding(false), // UTF-8 bez BOM
                "utf8bom" => Encoding.UTF8, // UTF-8 s BOM
                "utf16" => Encoding.Unicode, // UTF-16 (LE)
                "utf16le" => Encoding.Unicode, // Explicitně Little Endian
                "utf16be" => Encoding.BigEndianUnicode, // UTF-16 Big Endian
                _ => null //new UTF8Encoding(false) // Výchozí UTF-8 s BOM
            };

            if (encoding == null)
            {
                return File.ReadAllText(fullPath); // automatická detekce
            } else { 
                return File.ReadAllText(fullPath, encoding);
            }
        }
        else
            return File.ReadAllText(fullPath); // automatická detekce
    }


    public static int LoadFile(IntPtr L)
    {
        string path = ToLuaString(L, 1);
        string encodingName = null;

        // Volitelný druhý parametr
        if (GetTop(L) >= 2)
        {
            if (!IsLuaNil(L, 2))
                encodingName = ToLuaString(L, 2).ToLower();
        }

        try
        {
            string text = LoadFile(Utils.FindFileCaseInsensitive(path), encodingName);
            PushLuaString(L,text);
            return 1;
        }
        catch (Exception ex)
        {
            //Console.WriteLine("❌ LoadFile error: " + ex.Message);
            PushLuaError(L, " LoadFile error: " + ex.Message);
            //LuaEngine._lua.PushLuaNil();
            return 0;
        }
    }

     public static string EscapeLuaString(string s)
    {
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }



    public static List<string> GetFiles(string relativePath, string? extension = null, bool stripExtension = false)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.GetFullPath(Path.Combine(basePath,relativePath));

        if (!Directory.Exists(fullPath))
            return new List<string>();

        var files = Directory.GetFiles(fullPath);

        if (!string.IsNullOrWhiteSpace(extension) && extension != "*")
        {
            string ext = "." + extension.TrimStart('.');
            files = files
                .Where(f => Path.GetExtension(f).Equals(ext, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        // vracíme jen názvy souborů, ne celé cesty
        return files
                    .Select(f =>
                    {
                        var name = Path.GetFileName(f);
                        return stripExtension ? Path.GetFileNameWithoutExtension(name) : name;
                    })
                    .ToList();
    }

    public static int GetFilesLua(IntPtr L)
    {
        string path = ToLuaString(L, 1);
        string? ext = IsLuaNil(L, 2) ? null : ToLuaString(L, 2);
        try
        {

            bool strip = false;
            if (!IsLuaNil(L, 3))
                strip = ToLuaBoolean(L, 3);


            var files = GetFiles(Utils.FindFileCaseInsensitive(path), ext, strip);

            NewTable(L);

            int i = 1;
            foreach (var file in files)
            {
                PushLuaInteger(L, i++);
                PushLuaString(L, file);
                SetTable(L, -3);
            }

            return 1;
        }
        catch (Exception e)
        {
            //Console.WriteLine("❌ GetFiles error: " + e.Message);
            //LuaEngine._lua.PushLuaNil();
            PushLuaError(L, "GetFiles error:" + path + " " + e.Message);
            return 0;
        }


    }

    public static List<string> GetDirectories(string relativePath)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

        //Console.WriteLine(relativePath);
        if (!Directory.Exists(fullPath))
            return new List<string>();

        return Directory.GetDirectories(fullPath)
            .Select(d => Path.GetFileName(d)!)
            .ToList();

    }

    public static int GetDirsLua(IntPtr L)
    {
        string path = ToLuaString(L, 1);
        try
        {


            var dirs = GetDirectories(Utils.FindFileCaseInsensitive(path));

            NewTable(L);

            int i = 1;
            foreach (var dir in dirs)
            {
                PushLuaInteger(L, i++);
                PushLuaString(L, dir);
                SetTable(L, -3);
            }

            return 1;
        }
        catch (Exception e)
        {
            //Console.WriteLine("❌ GetDirs error: " + e.Message);
            //LuaEngine._lua.PushLuaNil();
            PushLuaError(L, "❌ GetDirs error: " + e.Message);
            return 0;
        }


    }


}
