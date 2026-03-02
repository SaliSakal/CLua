using System.Diagnostics;

namespace CLua
{
    static class ConsoleHelper
    {
        public static void EnsureConsole(string[] args)
        {
            // Pokud už běží v terminálu, nic nedělej
            if (!Console.IsOutputRedirected )
                return;



            // Pokud není X server, nemá smysl zkoušet otevřít terminál
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
            {
                Console.Error.WriteLine("❌ DISPLAY není nastaven – nelze otevřít terminálové okno.");
                return;
            }


            string? self = Environment.ProcessPath;  //Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(self) || !File.Exists(self))
            {
                Console.Error.WriteLine("❌ Nelze zjistit cestu ke spustitelnému souboru.");
                return;
            }


            // Detekce desktopového prostředí
            string? desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")?.ToLowerInvariant();

            var terminals = new List<string>();

            if (desktop?.Contains("kde") == true) terminals.Add("konsole");
            if (desktop?.Contains("gnome") == true) terminals.Add("gnome-terminal");
            terminals.AddRange(new[] {
                                        "x-terminal-emulator",
                                        "xfce4-terminal",
                                        "xterm"
                                    });

            // Sestavení escapovaných argumentů
            string argumentLine = string.Join(" ", args.Select(a => "\"" + a.Replace("\"", "\\\"") + "\""));


            foreach (var term in terminals.Distinct())
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = term,
                        Arguments = $"-e \"{self} {argumentLine}\"",
                        UseShellExecute = false
                    });
                    Environment.Exit(0);
                }
                catch
                {
                    // Pokračuj na další terminál

                }
            }

            Console.Error.WriteLine("❌ Nepodařilo se spustit žádný terminál.");

            File.AppendAllText("clua.log", $"[{DateTime.Now}] \r\nFailed to launch terminal\n");
            Environment.Exit(1);
        }



    }
}
