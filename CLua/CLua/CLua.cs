using System.Text;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Resources;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ThemeManager = Terminal.Gui.Configuration.ThemeManager;
using Attribute = Terminal.Gui.Drawing.Attribute;
using Color = Terminal.Gui.Drawing.Color;



namespace CLua
{ 
    public partial class CLua
    {
        public static IApplication App { get; private set; } = null!;
        static public string CLua_ver = "1.3.0:50";
        static public LuaManager luaGUI = new LuaManager();
        static TextView logView;
        static Queue<string> logQueue = new Queue<string>();
        static TextWriter originalConsoleOut; // Uložíme původní Console.Out

        static public MenuBar menu = new MenuBar();
        static public List<MenuBarItem> items = new List<MenuBarItem>();
        static public Window top = new Window();


        static public Dictionary<string, MenuBarItem> menuReferences = new Dictionary<string, MenuBarItem>();
        static public Dictionary<string, MenuItem> menuItemReferences = new Dictionary<string, MenuItem>();
        static public Dictionary<MenuItem, Action> menuItemActions = new Dictionary<MenuItem, Action>();

        public static void StoreMenuReferences(MenuBar menuBar)
        {
            if (menuReferences.Count > 0) return; // Už bylo uloženo

            foreach (var menu in items)
            {
                menuReferences[menu.Title.ToString()] = menu;

                var subItems = menu.PopoverMenu?.Root?.GetMenuItemsOfAllSubMenus();
                if (subItems != null)
                {
                    foreach (var item in subItems)
                    {
                        var title = item.Title?.ToString();
                        if (title != null)
                            menuItemReferences[title] = item;
                    }
                }
            }
        }


        public static List<string> GetLanguageFiles()
        {
            string locPath = Utils.FindFileCaseInsensitive("Lua/Loc"); //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lua/Loc/");

            if (!Directory.Exists(locPath))
            {
                //Console.WriteLine("⚠️ Složka s jazyky neexistuje!");
                Console.WriteLine("⚠️ Language folder does not exist!");
                return new List<string>();
            }

            return Directory.GetFiles(locPath, "*.lua")
                            .Select(file => Path.GetFileNameWithoutExtension(file)) // Jen název souboru
                            .ToList();
        }

        public static void SetLanguage(string lang)
        {
            Console.WriteLine($"🌍 Change language to: {lang}");

            ConfigManager.SaveSetting("Language", lang); // ✅ Uložíme jazyk do configu

            GuiLuaBridge.ResetSGUI(IntPtr.Zero);
        }

        public static MenuBarItem CreateLanguageMenu()
        {
            var languageItems = new List<MenuItem>();

            foreach (var lang in GetLanguageFiles())
            {
                languageItems.Add(new MenuItem(lang, "", () => SetLanguage(lang)));
            }

            string currentLang = ConfigManager.LoadSetting("Language", "English");

            return new MenuBarItem("_Language", languageItems.ToArray());

        }

        [Obsolete]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(System.AppContext.BaseDirectory);

            if (OperatingSystem.IsLinux())
                ConsoleHelper.EnsureConsole(args);
            
            SetupCrashHandlers();

            Console.OutputEncoding = Encoding.UTF8; // Správné kódování
            Console.InputEncoding = Encoding.UTF8;  // správné kodování vstupu
            
            Console.WriteLine("ℹ️ Initializing...");


            App = Application.Create();
            App.Init();
            // Používáme jednoduché ASCII znaky pro checkboxy — Terminal.Gui v2 jinak používá Unicode PUA znaky které terminat barví vlastními barvami
            // Used simple ASCII characters for checkboxes — Terminal.Gui v2 otherwise uses Unicode PUA characters that the terminal colors with its own colors
            Glyphs.CheckStateChecked   = new Rune('✓');
            Glyphs.CheckStateUnChecked = new Rune('▭');
            //Glyphs.CheckStateNone      = new Rune('⬜');

            // Hlavní menu

            //    starý systém
            items.Add(new MenuBarItem("_Program", new MenuItem[]
                {
                    //new MenuItem("_Spustit Lua skript", "", RunLuaScript),
                    new MenuItem("_Reset CLua", "", () => GuiLuaBridge.ResetSGUI(IntPtr.Zero) ), //,() => false  ),
                    new MenuItem("_Exit", "", () => App.RequestStop())
                }));

            items.Add(new MenuBarItem("_File", new MenuItem[] { } ));

            items.Add(CreateLanguageMenu());
            items.Add(new MenuBarItem("_Help",  new MenuItem[]
               {
                    //new MenuItem("_Spustit Lua skript", "", RunLuaScript),
                    new MenuItem("_About", "", () => AboutWindow.Show() ), //,() => false  ),
                }));


            menu.Menus = items.ToArray();


            top.Add(menu);

            StoreMenuReferences(menu);

            // Hlavní barevné schéma RAOS

            //Colors.ColorSchemes

            var baseScheme = new Scheme
            {
                Normal = new Attribute(Color.Green, Color.Black), 
                Focus = new Attribute(Color.BrightGreen, Color.Green), 
                HotNormal = new Attribute(Color.BrightGreen, Color.Black), 
                HotFocus = new Attribute(Color.Black, Color.BrightGreen),
                Highlight = new Attribute(Color.BrightGreen, Color.Black, TextStyle.Italic),
            };

            SchemeManager.AddScheme("Base", baseScheme); // Fokus na zvýrazněném textu

            SchemeManager.AddScheme("Menu", new Scheme
            {
                Normal = new Attribute(Color.Black, Color.Green),
                HotNormal = new Attribute(Color.BrightGreen, Color.Black),
                HotFocus = new Attribute(Color.Black, Color.Green),
                Focus = new Attribute(Color.Black, Color.BrightGreen),
                Disabled = new Attribute(Color.DarkGray, Color.Green),
            });
            menu.SchemeName = "Menu";


            SchemeManager.AddScheme("Error", new Scheme
            {
                Normal = new Attribute(Color.BrightRed, Color.Black),
            }); // Červený text pro chyby



            SchemeManager.AddScheme("Buttons", baseScheme);

            SchemeManager.AddScheme("Label", baseScheme);

            // Další systémové prvky
            // Other system elements
            SchemeManager.AddScheme("Dialog", new Scheme
            {
                Normal = new Attribute(Color.Green, Color.Black),
                Focus = new Attribute(Color.Black, Color.BrightGreen),
                HotNormal = new Attribute(Color.BrightGreen, Color.Black),
                HotFocus = new Attribute(Color.Black, Color.Green),
                Disabled = new Attribute(Color.Green, Color.Black),
                Highlight = new Attribute(Color.BrightGreen, Color.Black, TextStyle.Italic),
                
            });

            SchemeManager.AddScheme("ProgressBar", new Scheme()
            {
                Normal = new Attribute(Color.Green, Color.Black), 
                HotNormal = new Attribute(Color.BrightGreen, Color.Black), 
                Focus = new Attribute(Color.Black, Color.Green), 
                HotFocus = new Attribute(Color.Black, Color.BrightGreen) 
            });

            SchemeManager.AddScheme("Checkbox", new Scheme
            {
                Normal = new Attribute(Color.Green, Color.Black),
                Focus = new Attribute(Color.BrightGreen, Color.Black),
                HotNormal = new Attribute(Color.Green, Color.Black),
                HotFocus = new Attribute(Color.BrightGreen, Color.Black),
                Disabled = new Attribute(Color.DarkGray, Color.Black),
                Highlight = new Attribute(Color.BrightGreen, Color.Black, TextStyle.Italic),
            });

            SchemeManager.AddScheme("logView", new Scheme
            {
                Normal = new Attribute(Color.Green, Color.Black),
                Focus = new Attribute(Color.Green, Color.Black),
                HotNormal = new Attribute(Color.BrightGreen, Color.Black), 
                HotFocus = new Attribute(Color.Black, Color.Green), 
                Disabled = new Attribute(Color.Green, Color.Black),

            });

            SchemeManager.AddScheme("TextField", new Scheme
            {
                Normal = new Attribute(Color.Green, Color.Black),
                Focus = new Attribute(Color.Green, Color.Black),
                HotNormal = new Attribute(Color.BrightGreen, Color.Black), 
                HotFocus = new Attribute(Color.Black, Color.Green), 
                Disabled = new Attribute(Color.Green, Color.Black),

            });

            logView = new TextView
            {
                X = 0,
                Y = 27,
                //Y = Pos.Bottom(top) - 7,
                Width = Dim.Fill(),
                //Height = 8, // Pevná výška na 3 řádky
                Height = Dim.Fill(),
                ReadOnly = true,
                WordWrap = true,   // Zakázat zalamování, způsobuje pád při error se stacktracem // Disable word wrap, causes crash on error with stacktrace
                //CanFocus = false,

                Multiline = true,

                SchemeName = "logView",
                ViewportSettings = ViewportSettingsFlags.HasVerticalScrollBar
            };


            top.Add(logView);


            // Uložíme původní výstup konzole
            originalConsoleOut = Console.Out;

            // Přesměrujeme Console.WriteLine() do GUI logu
            Console.SetOut(new GuiLogWriter());

            top.SchemeName = "Base";       

            StartWatchdog();

            luaGUI.SandboxLua();

            RegisterLuaGUIFunctions(luaGUI);
            RegisterLuaGUIConstants(luaGUI);


            luaGUI.RegisterGlobalsForENV(
                // Utils
                "switch", "safeCall",
                // Base Config
                "VERSION", "LANG",
                "SaveFile", "LoadFile", "FileExists",
                "reset"
            );

            bool FirstRun = true;

            App.Iteration += (s, e) =>
            {
                if (FirstRun)
                {
                    FirstRun = false;
                    luaGUI.Init("CLua", "");
                }
                Beat();
                luaGUI.RunExecTick();
            };

            try
            {
                App.Run(top);


                // vypíše LOG z fronty do původní konzole při ukončení aplikace
                originalConsoleOut.WriteLine(string.Join("\n", logQueue));
            }
            catch (Exception ex)
            {
                // Vypíše LOG a CRASH do původní konzole
                originalConsoleOut.WriteLine(string.Join("\n", logQueue));
                originalConsoleOut.WriteLine(ex.ToString());
            }
            finally
            {
                // VŽDYCKY se provede - obnov konzoli
                Console.SetOut(originalConsoleOut);
            }


        }




        class GuiLogWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void WriteLine(string message)
            {

                // Zapíšeme zprávu i do původní konzole
                //originalConsoleOut.WriteLine(message);
                
                message = message.Replace("❌", "X ")
                                         .Replace("✅", "✓ ")
                                         .Replace("⚠️", "! ")
                                         .Replace("📂", "▣ ")
                                         .Replace("📌", "→ ")
                                         .Replace("📄", "✎ ")
                                         .Replace("🌙", "☾ ");
                

                if (logQueue.Count >= 200)
                        logQueue.Dequeue();

                // Odstraň problematické řídicí znaky (kromě tabulátoru a nového řádku)
                //message = new string(message.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());


                logQueue.Enqueue(message);


                logView.Text = string.Join("\n", logQueue.Reverse());


                logView.SetNeedsDraw();


                


            }
        }


        public static class Log
        {

            public static void WriteLine(string message)
            {
                CLua.App.Invoke(() =>
                {

                    logQueue.Enqueue(message);
                });
            }
        }
    }
}
