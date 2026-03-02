using Terminal.Gui;

namespace CLua
{
    public static class AboutWindow
    {
        public static void Show()
        {

            var asciiLogo = @"
  _____   _                 
 / ____| | |                
 | |     | |    _   _ ____  
 | |     | |   | | | |  _ \ 
 | |____ | |__ | |_| | |_| |
 |______|\____|\_____|_| |_|
";
            var logo = new Label()
            {
                X = Pos.Center(),
                Y = -1,
                Width = Dim.Fill(),
                TextAlignment = TextAlignment.Centered,
                Text = asciiLogo
            };

            var label = new Label()
            {
                X = Pos.Center(),
                Y = 7,
                Width = Dim.Fill(),
                TextAlignment = TextAlignment.Centered,
                Text = "CLua - Macro Tool"
            };

            var lblVersion = new Label()
            {
                X = Pos.Center(),
                Y = 9,
                Width = Dim.Fill(),
                TextAlignment = TextAlignment.Centered,
                Text = "Version: " + CLua.CLua_ver
            };

            var lblAuthor = new Label()
            {
                X = Pos.Center(),
                Y = 11,
                Width = Dim.Fill(),
                TextAlignment = TextAlignment.Centered,
                Text = "Author: Petr \"Sali\" Salak"
            };

            var btnOk = new Button("OK")
            {
                X = Pos.Center(),
                Y = 16,
            };
            btnOk.Clicked += () => Application.RequestStop();

            var aboutDialogWindow = new Dialog("About", 50, 16, btnOk);


            aboutDialogWindow.Add(logo, label, lblVersion, lblVersion, lblAuthor, btnOk);

            Application.Run(aboutDialogWindow);
        }
    }
}
