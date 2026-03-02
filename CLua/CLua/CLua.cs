//#define NEW_MENU

using Terminal.Gui;
using System.Text;

namespace CLua
{ 
    public partial class CLua
    {
        static public string CLua_ver = "1.1.3:45";
        static public LuaManager luaGUI = new LuaManager();
        static TextView logView;
        static Queue<string> logQueue = new Queue<string>();
        static TextWriter originalConsoleOut; // Uložíme původní Console.Out

#if NEW_MENU
        static public CustomMenuBar menu = new CustomMenuBar();
        static public MainTopLevel top = new MainTopLevel();

        static public Dictionary<string, MenuEntry> menuReferences = new Dictionary<string, MenuEntry>();
        static public Dictionary<string, MenuEntry> menuItemReferences = new Dictionary<string, MenuEntry>();

        public static void StoreMenuReferences(CustomMenuBar menuBar)
        {
            if (menuReferences.Count > 0) return; // Už bylo uloženo

            foreach (var menu in menuBar.Menus)
            {
                menuReferences[menu.Title.ToString()] = menu;

                if (menu.Children == null)
                    continue;
                foreach (var item in menu.Children)
                {
                    menuItemReferences[item.Title.ToString()] = item;
                }
            }
        }

#else
        static public MenuBar menu = new MenuBar();
        static public List<MenuBarItem> items = new List<MenuBarItem>();
        static public Toplevel top = new Toplevel();


        static public Dictionary<string, MenuBarItem> menuReferences = new Dictionary<string, MenuBarItem>();
        static public Dictionary<string, MenuItem> menuItemReferences = new Dictionary<string, MenuItem>();
        static public Dictionary<MenuItem, Action> menuItemActions = new Dictionary<MenuItem, Action>();

        public static void StoreMenuReferences(MenuBar menuBar)
        {
            if (menuReferences.Count > 0) return; // Už bylo uloženo

            foreach (var menu in menuBar.Menus)
            {
                menuReferences[menu.Title.ToString()] = menu;

                foreach (var item in menu.Children)
                {
                    menuItemReferences[item.Title.ToString()] = item;
                    menuItemActions[item] = item.Action;
                }

                //menuItemActions[menu] = menu.Action;
            }
        }

#endif


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

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(System.AppContext.BaseDirectory);

            if (OperatingSystem.IsLinux())
                ConsoleHelper.EnsureConsole(args);
            
            SetupCrashHandlers();

            Console.OutputEncoding = Encoding.UTF8; // Správné kódování
            Console.InputEncoding = Encoding.UTF8;  // správné kodování vstupu
            
            Console.WriteLine("ℹ️ Initializing...");


            Application.Init();

            // Hlavní menu

#if NEW_MENU

            var program = new MenuEntry("_Program");

            program.AddChild(new MenuEntry("_Reset CLua")
            {
                Enabled = true,
                Action = () => LuaEngine.ResetSGUI(IntPtr.Zero)
            });
            program.AddChild(new MenuEntry("_Exit")
            {
                Enabled = true,
                Action = () => Application.RequestStop()
            });

            menu.Menus.Add(program);
            menu.Menus.Add(new MenuEntry("_File"));

            var languageMenu = new MenuEntry("_Language");

            foreach (var lang in GetLanguageFiles())
            {
                languageMenu.AddChild(new MenuEntry(lang)
                {
                    Enabled = true,
                    Action = () => SetLanguage(lang)
                });
            }
            menu.Menus.Add(languageMenu);
            menu.Menus.Add(new MenuEntry("_About")
            {
                Enabled = true,
                Action = () => AboutWindow.Show()
            });
            StoreMenuReferences(menu);

            top.Add(menu);
            top.Menu = menu;

#else

            //    starý systém
            items.Add(new MenuBarItem("_Program", new MenuItem[]
                {
                    //new MenuItem("_Spustit Lua skript", "", RunLuaScript),
                    new MenuItem("_Reset CLua", "", () => GuiLuaBridge.ResetSGUI(IntPtr.Zero) ), //,() => false  ),
                    new MenuItem("_Exit", "", () => Application.RequestStop())
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
#endif




            logView = new TextView
            {
                X = 0,
                Y = 27,
                //Y = Pos.Bottom(top) - 7,
                Width = Dim.Fill(),
                //Height = 8, // Pevná výška na 3 řádky
                Height = Dim.Fill(),
                ReadOnly = true,
                WordWrap = true,
                //CanFocus = false,

                Multiline = true,

                ColorScheme = new ColorScheme
                {
                    //Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Black)

                    Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
                    Focus = new Terminal.Gui.Attribute(Color.Green, Color.Black),
                    HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black), // Zvýraznění žluté
                    HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green), // Oranžovo-žluté zvýraznění v menu
                    Disabled = new Terminal.Gui.Attribute(Color.Green, Color.Black),
                }
            };

            /*
            var button = new Button("🔄 Přidat zprávu do logu")
            {
                X = Pos.Center(),
                Y = Pos.Center()
            };
            button.Clicked += () => Console.WriteLine($"[{DateTime.Now:T}] ✖ Náhodná zpráva!");
            */
            // win.Add(button);
            //top.Add(win);
            top.Add(logView);


            // Uložíme původní výstup konzole
            originalConsoleOut = Console.Out;

            // Přesměrujeme Console.WriteLine() do GUI logu
            Console.SetOut(new GuiLogWriter());


            // Hlavní barevné schéma RAOS





            //Colors.ColorSchemes


            Colors.Base.Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black); // Zelený text na černém pozadí
            Colors.Base.Focus = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Green);  // Černý text na zeleném pozadí (fokus)
            Colors.Base.HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black); // Žlutý zvýrazněný text
            Colors.Base.HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen); // Fokus na zvýrazněném textu

            // Další systémové prvky
            Colors.Dialog.Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black); // Dialogová okna (zelený text)
            Colors.Dialog.Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen); // Aktivní prvek v menu
            Colors.Dialog.HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black);
            Colors.Dialog.HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green);
            Colors.Dialog.Disabled = new Terminal.Gui.Attribute(Color.Green, Color.Black);
            //Colors.Dialog. = new Terminal.Gui.Attribute(Color.Green, Color.Black); // Dialogová okna (zelený text)
            Colors.Menu.Normal = new Terminal.Gui.Attribute(Color.Black, Color.Green);  // Zelené menu
            Colors.Menu.HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black); // Zvýraznění žluté
            Colors.Menu.HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green); // Oranžovo-žluté zvýraznění v menu
            Colors.Menu.Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen); // Aktivní prvek v menu
            Colors.Menu.Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Green);

            Colors.Error.Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black); // Červený text pro chyby

            
            Colors.ColorSchemes.TryAdd("ProgressBar", new ColorScheme()
            {
                Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black), // Zelená výplň, černé pozadí
                HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black), // Jasná zelená při zvýraznění
                Focus = new Terminal.Gui.Attribute(Color.Black, Color.Green), // Invertované při zaměření
                HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen) // Zvýrazněné při zaměření
            });

            top.ColorScheme = new ColorScheme()
            {
                Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
                Focus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
                HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
                HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen)
            };

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

            Application.MainLoop.AddIdle(() =>
            {
                if (FirstRun)
                    {
                    FirstRun = false;
                    luaGUI.Init("CLua", "");
                }
                Beat();

                luaGUI.RunExecTick();
                return true;
            });

            try
            {
                Application.Run(top);


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

                    logQueue.Enqueue(message);

                    logView.Text = string.Join("\n", logQueue.Reverse());


                    Application.Refresh();


            }
        }


        public static class Log
        {

            public static void WriteLine(string message)
            {
                Application.MainLoop.Invoke(() =>
                {

                    logQueue.Enqueue(message);

                    // Otočíme pořadí, aby nové zprávy byly nahoře
                    logView.Text = string.Join("\n", logQueue.Reverse());
                    
                });
            }

        }

    }

}


