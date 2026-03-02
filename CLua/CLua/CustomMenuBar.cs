using Terminal.Gui;

namespace CLua
{
    public class MainTopLevel : Toplevel
    {
        public CustomMenuBar Menu;

        public override bool ProcessKey(KeyEvent key)
        {
            // F10 otevře menu
            if (key.Key == Key.F10 || key.Key == Key.Esc)
            {
                Menu.Activate(); // vlastní metoda
                Menu.SetFocus();
                BringSubviewToFront(Menu);
                return true;
            }

            return base.ProcessKey(key);
        }
    }
    public class MenuEntry
    {
        public string Title;
        public char? HotKey;          // např. 'F' pro File
        public bool Enabled = true;
        public Action? Action;
        public List<MenuEntry>? Children; // 0 = žádné submenu

        public MenuEntry? Parent;

        public bool IsSubmenu => Children != null && Children.Count > 0;

        public MenuEntry(string title, Action? action = null)
        {
            Title = title;
            Action = action;
        }

        public void AddChild(MenuEntry child)
        {
            if (Children == null)
                Children = new List<MenuEntry>();
            child.Parent = this;
            Children.Add(child);
        }

        public void Remove()
        {
            Parent?.Children?.Remove(this);
            Parent = null;
        }
    }

    public class CustomMenuBar : View
    {
        public List<MenuEntry> Menus = new();

        int selectedMenu = 0;
        bool menuActive = false;
        View previousFocusedView;

        int selectedSub = 0;
        bool submenuOpen = false;
        //int lastMenu = -1;

        List<int> menuX = new();
        List<int> menuWidth = new();



        public CustomMenuBar()
        {
            Height = 1;
            Width = Dim.Fill();
            CanFocus = true;
        }
        /*
        class SubMenuView : View
        {
            public MenuEntry ParentMenu;
            public int SelectedIndex;

            public override void Redraw(Rect bounds)
            {
                // tady kreslíš box + položky
            }
        }
        */

        public override void Redraw(Rect bounds)
        {
            Driver.SetAttribute(Colors.Menu.Normal);
            Move(0, 0);
            Driver.AddStr(new string(' ', bounds.Width));

            menuX.Clear();
            menuWidth.Clear();

            int x = 1;
            Move(0, 0);
            Driver.AddStr("ESC:  ");
            x = x + "ESC:  ".Length;
            Move(x, 0);
            for (int i = 0; i < Menus.Count; i++)
            {
                var item = Menus[i];

                Terminal.Gui.Attribute attr;


                if (!item.Enabled)
                {
                    attr = Colors.Menu.Disabled;
                }
                else if (menuActive && i == selectedMenu)
                {
                    // TADY je zvýraznění položky
                    attr = Colors.Menu.Focus;
                }
                else
                {
                    attr = Colors.Menu.Normal;
                }


                Driver.SetAttribute(attr);
                Move(x, 0);
                Driver.AddStr($" {item.Title} ");

                menuX.Add(x);
                menuWidth.Add(Menus[i].Title.Length + 2);


                if (i == selectedMenu && submenuOpen)
                {
                    var menu = Menus[selectedMenu];
                    int y = 1;

                    var items = menu.Children;
                    int width = items.Max(i => i.Title.Length) + 2; // mezery
                    int height = items.Count;

                    int boxWidth = width + 2;
                    int boxHeight = height + 2;

                    Driver.SetAttribute(Colors.Menu.Normal);

                    Move(x, y);
                    Driver.AddRune('┌');
                    Driver.AddStr(new string('─', boxWidth - 2));
                    Driver.AddRune('┐');

                    for (int j = 1; j < boxHeight - 1 ; j++)
                    {
                        Move(x, y + j);
                        Driver.AddRune('│');
                        Driver.AddStr(new string(' ', boxWidth - 2));
                        Driver.AddRune('│');
                    }

                    Move(x, y + boxHeight - 1);
                    Driver.AddRune('└');
                    Driver.AddStr(new string('─', boxWidth - 2));
                    Driver.AddRune('┘');

                    int k = 0;

                    foreach (var sItem in items)
                    {
                        Terminal.Gui.Attribute attrSub;

                        if (!sItem.Enabled)
                        {
                            attrSub = Colors.Menu.Disabled;
                        }
                        else if (sItem == items[selectedSub])
                        {
                            // TADY je zvýraznění pod-položky
                            attrSub = Colors.Menu.Focus;
                        }
                        else
                        {
                            attrSub = Colors.Menu.Normal;
                        }
                        Driver.SetAttribute(attrSub);


                        Move(x + 1, y + 1 + k++);
                        Driver.AddStr(sItem.Title.PadRight(width));

                    }
                }

                x += item.Title.Length + 2;

            }


        }


        public void Activate()
        {
            if (menuActive)
            {
                menuActive = false;

                if (previousFocusedView?.CanFocus == true)
                {
                    previousFocusedView.SetFocus();
                    previousFocusedView.SetNeedsDisplay();
                }
                else
                {
                    Application.Top.SetFocus();
                    Application.Top.SetNeedsDisplay();
                }

                
            }
            else
            {
                previousFocusedView = Application.Top.Focused;

                menuActive = true;

                selectedMenu = Menus.FindIndex(m => m.Enabled);
                if (selectedMenu < 0)
                    selectedMenu = 0; // fallback

                if (Menus[selectedMenu].IsSubmenu)
                {
                    submenuOpen = true;
                    Application.GrabMouse(this);
                }
                else { Application.GrabMouse(this); }

                    CLua.menu.SetFocus();
                SetNeedsDisplay();
                SetChildNeedsDisplay();
                
            }
            
            //BringSubviewToFront();
            

        }

        void OnMenuSelectionChanged()
        {
            var menu = Menus[selectedMenu];

            if (menu.IsSubmenu)
            {
                submenuOpen = true;
                selectedSub = 0;
                //Application.Top.MouseGrabView = this;
                Application.GrabMouse(this);
            }
            else
            {
                submenuOpen = false;
                Application.GrabMouse(this);
            }
        }


        public int FindNextEnabled(int start, int direction, List<MenuEntry> menu)
        {
            int count = menu.Count;
            int i = start;

            for (int step = 0; step < count; step++)
            {
                i = (i + direction + count) % count;

                if (menu[i].Enabled)
                    return i;
            }

            // žádná enabled položka
            return start;
        }

        public override bool ProcessKey(KeyEvent key)
        {

            if (key.Key == Key.F10 || key.Key == Key.Esc)
            {
                menuActive = false;
                submenuOpen = false;
                if (previousFocusedView?.CanFocus == true)
                    previousFocusedView.SetFocus();
                else
                    Application.Top.SetFocus();
                return true;
            }

            // když menu NENÍ aktivní, pust klávesy dál
            if (!menuActive)
                return false;



            // šipky
            if (key.Key == Key.CursorRight)
            {
                //selectedMenu = (selectedMenu + 1) % Menus.Count;
                selectedMenu = FindNextEnabled(selectedMenu, +1, Menus);
                OnMenuSelectionChanged();
                SetNeedsDisplay();
                return true;
            }

            if (key.Key == Key.CursorLeft)
            {
                //selectedMenu = (selectedMenu - 1 + Menus.Count) % Menus.Count;
                selectedMenu = FindNextEnabled(selectedMenu, -1, Menus);
                OnMenuSelectionChanged();
                SetNeedsDisplay();
                return true;
            }

            if(key.Key == Key.CursorUp)
            {
                {
                    //selectedMenu = (selectedMenu + 1) % Menus.Count;
                    selectedSub = FindNextEnabled(selectedSub, -1, Menus[selectedMenu].Children);

                    SetNeedsDisplay();
                    return true;
                }

            }

            if (key.Key == Key.CursorDown)
            {
                {
                    //selectedMenu = (selectedMenu + 1) % Menus.Count;
                    selectedSub = FindNextEnabled(selectedSub, +1, Menus[selectedMenu].Children);

                    SetNeedsDisplay();
                    return true;
                }

            }

            // Enter
            if (key.Key == Key.Enter)
            {
                if (submenuOpen)
                {
                    var item = Menus[selectedMenu].Children[selectedSub];
                    item.Action?.Invoke();
                }
                else
                {
                    Menus[selectedMenu].Action?.Invoke();
                }
                menuActive = false;
                submenuOpen = false;
                if (previousFocusedView?.CanFocus == true)
                    previousFocusedView.SetFocus();
                else
                    Application.Top.SetFocus();
                return true;
            }

            return true; // cokoliv jiného menu SEŽERE
        }

        public override bool MouseEvent(MouseEvent me)
        {
            if (me.Flags.HasFlag(MouseFlags.Button1Clicked))
            {
                // klik do hlavní lišty
                if (me.Y == 0)
                {
                    for (int i = 0; i < Menus.Count; i++)
                    {
                        if (me.X >= menuX[i] && me.X < menuX[i] + menuWidth[i])
                        {
                            selectedMenu = i;
                            OnMenuSelectionChanged();

                            if (!Menus[i].IsSubmenu)
                                Menus[i].Action?.Invoke();
                            else
                                submenuOpen = true;

                            menuActive = true;
                            CLua.menu.SetFocus();
                            CLua.top.BringSubviewToFront(CLua.menu);
                            SetNeedsDisplay();
                            return true;
                        }
                    }
                }

                // klik do submenu
                if (submenuOpen)
                {
                    int subY = me.Y - 2;
                    var children = Menus[selectedMenu].Children;
                    
                    if (children != null && subY >= 0 && subY < children.Count)
                    {
                        selectedSub = subY;
                        var item = children[subY];

                        if (item.Enabled)
                            item.Action?.Invoke();

                        SetNeedsDisplay();
                        return true;
                    }
                }


                if (me.Flags.HasFlag(MouseFlags.ReportMousePosition) && me.Y == 0)
                {
                    for (int i = 0; i < Menus.Count; i++)
                    {
                        if (me.X >= menuX[i] && me.X < menuX[i] + menuWidth[i])
                        {
                            if (selectedMenu != i)
                            {
                                selectedMenu = i;
                                OnMenuSelectionChanged();
                                SetNeedsDisplay();
                            }
                            return true;
                        }
                    }
                }

                if (me.Flags.HasFlag(MouseFlags.Button1Clicked))
                {
                    if (me.Y > 0 && !submenuOpen)
                        menuActive = false;

                }


            }

            return false;
        }

    }
}
