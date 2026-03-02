namespace CLua
{
    public partial class CLua
    {

        static void SetupCrashHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    File.WriteAllText("crash.log",
                        (e.ExceptionObject as Exception)?.ToString()
                        ?? "Unknown fatal error");
                }
                catch { }
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                try
                {
                    File.AppendAllText("crash.log", e.Exception.ToString());
                }
                catch { }

                e.SetObserved();
            };
        }

        static long lastHeartbeat;

        static void Beat()
        {
            Interlocked.Exchange(ref lastHeartbeat, Environment.TickCount64);
        }

        static void StartWatchdog()
        {
            Beat();

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(2000);

                    long hb = Interlocked.Read(ref lastHeartbeat);
                    if (Environment.TickCount64 - hb > 50000)
                    {
                        File.WriteAllText("crash.log", "Application hang detected");
                        Environment.FailFast("Application hang detected");
                    }
                }
            })
            { IsBackground = true }.Start();
        }
    }
}