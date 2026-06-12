using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Resources;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using static CLua.CLua;

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
                TextAlignment = Alignment.Center,
                Text = asciiLogo
            };

            var label = new Label()
            {
                X = Pos.Center(),
                Y = 7,
                Width = Dim.Fill(),
                TextAlignment = Alignment.Center,
                Text = "CLua - Macro Tool"
            };

            var lblVersion = new Label()
            {
                X = Pos.Center(),
                Y = 9,
                Width = Dim.Fill(),
                TextAlignment = Alignment.Center,
                Text = "Version: " + CLua.CLua_ver
            };

            var lblAuthor = new Label()
            {
                X = Pos.Center(),
                Y = 11,
                Width = Dim.Fill(),
                TextAlignment = Alignment.Center,
                Text = "Author: Petr \"Sali\" Salak"
            };

            var btnOk = new Button
            {
                Title = "OK",
                X = Pos.Center(),
                Y = 13,
                Width = Dim.Auto(),
                Height = Dim.Auto(),
            };
            btnOk.Accepting += (s, e) => App.RequestStop();

            var aboutDialogWindow = new Dialog { Title = "About", Width = 50, Height = 18 };


            aboutDialogWindow.Add(logo, label, lblVersion, lblVersion, lblAuthor, btnOk);

            App.Run(aboutDialogWindow);
        }
    }
}
