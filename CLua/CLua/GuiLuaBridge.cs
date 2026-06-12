using System.Text;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Resources;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using static CLua.CLua;
using static GuiLuaBridge;
using static Lua.LuaNative;
using Attribute = Terminal.Gui.Drawing.Attribute;
using Color = Terminal.Gui.Drawing.Color;
using ThemeManager = Terminal.Gui.Configuration.ThemeManager;



public static class GuiLuaBridge
{
    private static int nextId = 1; // Unikátní ID pro prvky
    private static Dictionary<int, View> elements = new Dictionary<int, View>();

    public static MyWindowView? ActiveWindow;

    public class MyWindowView : FrameView
    {
    }

    public static int CreateWindow(IntPtr L)
    {
        string title = ToLuaString(L, 1);
        bool visible = ToLuaBoolean(L, 2);


        var window = new MyWindowView()
        {
            Title = title,
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 26,//Dim.Fill() - 9, //26
            Visible = visible,
            CanFocus = true,

        };

        int id = nextId++;
        elements[id] = window; // Uložíme okno podle ID

        //Application.Top.Add(window);

        top.Add(window);
              

        window.SetNeedsDraw();

        PushLuaNumber(L, id); // Vrátíme ID do Lua
        return 1; // Počet návratových hodnot pro Lua
    }

    public static int CreateFrame(IntPtr L)
    {
        int windowId = ToLuaInteger(L, 1);
        string title = ToLuaString(L, 2);
        int x = ToLuaInteger(L, 3);
        int y = ToLuaInteger(L, 4);
        int width = ToLuaInteger(L, 5);  // Šířka
        int height = ToLuaInteger(L, 6); // Výška


        if (!elements.ContainsKey(windowId) || !(elements[windowId] is View window))
        {
            Console.WriteLine($"⚠️ Element ID {windowId} doesn't exist!");
            return 0;
        }

        var frame = new FrameView()
        {
            Title = title,
            X = x, // Pos.X(window) + x,
            Y = y, // Pos.Y(window) + y,
            Width = width > 0 ? width : Dim.Fill(),
            Height = height > 0 ? height : Dim.Fill(),
            CanFocus=true



        };

        int id = nextId++;
        elements[id] = frame; // Uložíme okno podle ID

        //Application.Top.Add(window);


        window.Add(frame);

        if (elements[windowId] is MyScrollView pScroll)
            ScrollSizeUpdate(pScroll, frame.Frame.Right, frame.Frame.Bottom);

        frame.SetNeedsDraw();

        PushLuaInteger(L, id); // Vrátíme ID do Lua
        return 1; // Počet návratových hodnot pro Lua
    }

    public class MyScrollView : View
    {
        private int _lastViewportWidth;
        private int _lastViewportHeight;

        protected override void OnSubViewsLaidOut(LayoutEventArgs args)
        {
            base.OnSubViewsLaidOut(args);

            // Přepočítej content size podle skutečných Frame hodnot dětí (po layoutu)
            int maxX = 0, maxY = 0;
            foreach (var child in SubViews)
            {
                maxX = Math.Max(maxX, child.Frame.Right);
                maxY = Math.Max(maxY, child.Frame.Bottom);
            }
            if (maxX > 0) SetContentWidth(maxX);
            if (maxY > 0) SetContentHeight(maxY);

            // Reset scroll pozice když se změní velikost viewportu (port z v1)
            if (Viewport.Width != _lastViewportWidth || Viewport.Height != _lastViewportHeight)
            {
                _lastViewportWidth  = Viewport.Width;
                _lastViewportHeight = Viewport.Height;
                Viewport = Viewport with { X = 0, Y = 0 };
                SetNeedsDraw();
            }
        }
    }

    public static int CreateScrollbox(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);
        int x = ToLuaInteger(L, 2);
        int y = ToLuaInteger(L, 3);
        int width = ToLuaInteger(L, 4);  // Šířka
        int height = ToLuaInteger(L, 5); // Výška


        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            Console.WriteLine($"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }

        var scroll = new MyScrollView()
        {
            X = x, // Pos.X(window) + x,
            Y = y, // Pos.Y(window) + y,
            Width = width > 0 ? width : Dim.Fill(),
            Height = height > 0 ? height : Dim.Fill(),
            CanFocus = true,
            ViewportSettings = ViewportSettingsFlags.HasScrollBars

        };

        int id = nextId++;
        elements[id] = scroll; // Uložíme podle ID

        //Application.Top.Add(window);

        
        parent.Add(scroll);

        scroll.SetNeedsDraw();

        if (elements[parentId] is MyScrollView pScroll)
            ScrollSizeUpdate(pScroll, scroll.Frame.Right, scroll.Frame.Bottom);

        PushLuaInteger(L, id); // Vrátíme ID do Lua
        return 1; // Počet návratových hodnot pro Lua
    }

    public static void ScrollSizeUpdate(View scroll, int x, int y)
    {
        int maxX = 0;
        int maxY = 0;

        foreach (var child in scroll.SubViews.ToList())
        {
            maxX = Math.Max(maxX, child.Frame.Right);
            maxY = Math.Max(maxY, child.Frame.Bottom);
        }

        // Guard: nenastavovat 0 když Frame ještě nemá platné hodnoty (před layoutem).
        // OnSubViewsLaidOut v MyScrollView to stejně přepočítá po prvním layoutu.
        if (maxX > 0) scroll.SetContentWidth(maxX);
        if (maxY > 0) scroll.SetContentHeight(maxY);
        scroll.SetNeedsDraw();
    }

    private static Dictionary<Button, EventHandler<CommandEventArgs>> buttonCallbacks = new Dictionary<Button, EventHandler<CommandEventArgs>>();
    private static Dictionary<Button, int> buttonCallbackRefs = new Dictionary<Button, int>();
    private static Dictionary<Button, object[]> buttonCallbackArgs = new();

    public static int CreateButton(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);
        string text = ToLuaString(L, 2);
        int x = ToLuaInteger(L, 3);
        int y = ToLuaInteger(L, 4);
        //string luaCallback = _lua.ToLuaString(L, 5);
        int width = ToLuaInteger(L, 6);  // Šířka
        int height = ToLuaInteger(L, 7); // Výška

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            Console.WriteLine($"⚠️ ElementID {parentId} doesn't exist!");
            return 0;
        }

        var button = new Button()
        {
            Title = text,
            X = x,
            Y = y,
            Width = width > 0 ? Dim.Absolute(width) : Dim.Auto(),
            Height = Dim.Auto(),
            SchemeName = "Button",
        };

        int callbackRef = -1;
        string luaCallbackString = null;

        Action newHandler; // = () => _lua.RunSlicedString(luaCallback);

        if (IsLuaFunction(L, 5))
        {
            callbackRef = LuaLRef(L, 5);

            // ⭐ Argumenty od indexu 4+
            int numArgs = GetTop(L) - 7;
            object[] args = new object[numArgs];

            for (int i = 0; i < numArgs; i++)
            {
                args[i] = LuaValueToObject(L, 8 + i);
            }

            newHandler = () => luaGUI.CallLuaFunction(callbackRef, true, args);
            buttonCallbackRefs[button] = callbackRef;
            buttonCallbackArgs[button] = args;
        }
        else
        {
            luaCallbackString = ToLuaString(L, 5);
            newHandler = () => luaGUI.RunSlicedString(luaCallbackString);
        }

        EventHandler<CommandEventArgs> wrappedHandler = (s, e) => newHandler();
        button.Accepting += wrappedHandler;
        
        buttonCallbacks[button] = wrappedHandler;

        int id = nextId++;
        elements[id] = button;

        parent.Add(button);
        button.SetNeedsDraw();

        if ((elements[parentId] is MyScrollView scroll))
            ScrollSizeUpdate(scroll, button.Frame.Right, button.Frame.Bottom);

        PushLuaInteger(L, id); // Vrátíme ID tlačítka do Lua
        return 1;
    }

    public static int SetActiveWindow(IntPtr L)
    {
        int windowId = ToLuaInteger(L, 1); // ID okna

        if (!elements.ContainsKey(windowId) || !(elements[windowId] is MyWindowView activeWindow))
        {
            // Console.WriteLine($"⚠️ (SetActiveWindow) Window ID {windowId} doesn't exist!");
            PushLuaError(L, $"Window ID {windowId} doesn't exist!");
            return 1;
        }

        foreach (var elem in elements.Values)
        {
            if (elem is FrameView window)
            {
                if (window.SuperView == App.TopRunnableView)
                    window.Visible = (window == activeWindow); // Jen jedno okno je viditelné
            }
        }

        // Nastavíme aktivní okno

        ActiveWindow = activeWindow;
        activeWindow.SetFocus();


        activeWindow.SetNeedsDraw();
        App.LayoutAndDraw();
        return 0;
    }

  
    public static int CreateLabel(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);
        string text = ToLuaString(L, 2);
        int x = ToLuaInteger(L, 3);
        int y = ToLuaInteger(L, 4);
        int width = ToLuaInteger(L, 5);  // Šířka
        int height = ToLuaInteger(L, 6); // Výška

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            Console.WriteLine($"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }

        var label = new Label()
        {
            // HotKeySpecifier musí být před Text, jinak se _ parsuje jako hotkey při přiřazení textu
            // HotKeySpecifier must be before Text, otherwise _ is parsed as hotkey when assigning text
            HotKeySpecifier = new Rune('\xffff'),
            Text = text,
            X = x,
            Y = y,
            Width = width > 0 ? Dim.Absolute(width) : Dim.Auto(),
            Height = height > 0 ? Dim.Absolute(height) : Dim.Auto(),
            SchemeName = "Label"
        };

        int id = nextId++;
        elements[id] = label; // Uložíme label podle ID

        parent.Add(label);
        label.SetNeedsDraw();

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, label.Frame.Right, label.Frame.Bottom);

        PushLuaInteger(L, id); // Vrátíme ID do Lua
        return 1;
    }




    public static int CreateProgressBar(IntPtr L)
    {

        int parentId = ToLuaInteger(L, 1);
        int x = ToLuaInteger(L, 2);
        int y = ToLuaInteger(L, 3);
        int width = ToLuaInteger(L, 4);



        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            Console.WriteLine($"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }

        var progressBar = new ProgressBar()
        {
            X = x,
            Y = y,
            Width = width > 0 ? width : Dim.Fill() - x, // vezme WIDTH, pokud je 0, nastaví autoamtické roztahování s okrajem jako pozice
            Fraction = 0.0f, // Výchozí hodnota
            SchemeName = "ProgressBar",
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage

        };


        int id = nextId++;
        elements[id] = progressBar;
        parent.Add(progressBar);

        progressBar.SetNeedsDraw();

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, progressBar.Frame.Right, progressBar.Frame.Bottom);


        PushLuaInteger(L, id);
        return 1;
    }

    public static int CreateCheckbox(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1); // ID rodičovského okna
        string text = ToLuaString(L, 2);  // Text checkboxu (název jazyka)
        int x = ToLuaInteger(L, 3);       // X pozice
        int y = ToLuaInteger(L, 4);       // Y pozice
       

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            Log.WriteLine($"⚠️ Window ID {parentId} dos't exist!");
            return 0;
        }

        var checkbox = new CheckBox()
        {
            Text = text,
            X = x,
            Y = y,
            Value = CheckState.Checked,
            SchemeName = "Checkbox"
        };
        int id = nextId++;
        elements[id] = checkbox;

        parent.Add(checkbox);

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, checkbox.Frame.Right, checkbox.Frame.Bottom);

        checkbox.SetNeedsDraw();

        PushLuaInteger(L, id); // Vrátíme ID do Lua
        return 1;
    }


    public static int CreateRadio(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);     // ID rodičovského okna // Parent window ID
        string itemsStr = ToLuaString(L, 2);   // Položky oddělené čárkou, např. "Volba 1,Volba 2,Volba 3" // Items separated by commas, e.g. "Option 1,Option 2,Option 3"
        int x = ToLuaInteger(L, 3);            // X pozice // X position
        int y = ToLuaInteger(L, 4);            // Y pozice // Y position
        string orientation = ToLuaString(L, 5); // "horizontal" nebo "vertical" // "horizontal" or "vertical"

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            PushLuaError(L, $"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }
        // Rozdělení stringu na pole položek a převedeme na ustring[]
        // Split string into array of items and convert to ustring[]
        string[] itemsArray = itemsStr.Split(';');
        string[] items = new string[itemsArray.Length];
        for (int i = 0; i < itemsArray.Length; i++)
        {
            items[i] = itemsArray[i].Trim();
        }

        var radioGroup = new OptionSelector
        {
            X = x,
            Y = y,
            Labels = items,
            Value = 0,
            Orientation = orientation == "horizontal" ? Orientation.Horizontal : Orientation.Vertical,
            SchemeName = "Checkbox"
        };

        int id = nextId++;
        elements[id] = radioGroup; // Uložíní radioGroup do seznamu prvků // Save radioGroup to elements list
        
        parent.Add(radioGroup);

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, radioGroup.Frame.Right, radioGroup.Frame.Bottom);

        radioGroup.SetNeedsDraw();

        PushLuaInteger(L, id); // Vrácení ID do Lua // Return ID to Lua
        return 1;
    }

    public static int GetRadioSelected(IntPtr L) // RadioGroup ID 
    {
        int radioId = ToLuaInteger(L, 1);

        if (!elements.ContainsKey(radioId) || !(elements[radioId] is OptionSelector radio))
        {
            PushLuaError(L, $"RadioGroup ID {radioId} doesn't exist!");
            return 0;
        }

        PushLuaInteger(L, (radio.Value ?? 0) + 1);  // Index položky (0-based) proto ++1 // Item index (0-based) hence +1
        return 1;
    }

    public static int SetRadioSelected(IntPtr L)
    {
        int radioId = ToLuaInteger(L, 1); // RadioGroup ID 
        int selectedIndex = ToLuaInteger(L, 2) -1; // Index položky (0-based) proto -1 // Item index (0-based) hence -1
        if (selectedIndex < 0) selectedIndex = 0; // Ochrana proti zápornému indexu // Protection against negative index 
        if (!elements.ContainsKey(radioId) || !(elements[radioId] is OptionSelector radio))
        {

            PushLuaError(L, $"RadioGroup ID {radioId} doesn't exist!");
            return 0;
        }
        // Kontrola, zda je index v platném rozsahu
        // Check if index is within valid range
        if (selectedIndex >= 0 && selectedIndex < radio.Labels.Count)
        {
            radio.Value = (int)selectedIndex;
            radio.SetNeedsDraw();
        }
        else
        {
            PushLuaError(L, $"⚠️ Invalid index {selectedIndex} for RadioGroup ID {radioId}");
        }

        return 0;
    }

    public static int IsCheckboxChecked(IntPtr L)
    {
        int checkboxId = ToLuaInteger(L, 1); // ID checkboxu // Checkbox ID

        if (!elements.ContainsKey(checkboxId) || !(elements[checkboxId] is CheckBox checkbox))
        {
            PushLuaError(L, $"⚠️ Checkbox with ID {checkboxId} doesn't exist!");
            PushLuaBoolean(L,false);
            return 1;
        }

        PushLuaBoolean(L, checkbox.Value == CheckState.Checked);
        return 1;
    }

    // Přidej k ostatním slovníkům
    private static Dictionary<View, int> textFieldCallbackRefs = new Dictionary<View, int>();
    private static Dictionary<View, EventHandler<Key>> textFieldKeyUpCallbacks = new Dictionary<View, EventHandler<Key>>();
    private static Dictionary<View, object[]> textFieldCallbackArgs = new();
    private static Dictionary<View, DateTime> lastKeyUpTime = new Dictionary<View, DateTime>();

    public static int CreateTextField(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);
        string text = ToLuaString(L, 2);
        int x = ToLuaInteger(L, 3);
        int y = ToLuaInteger(L, 4);
        int width = ToLuaInteger(L, 5);
        bool secret = ToLuaBoolean(L, 6);

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            PushLuaError(L, $"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }

        var textField = new TextField
        {
            Text = text,
            X = x,
            Y = y,
            Width = width,
            Secret = secret,
            SchemeName = "TextField"
        };

        int id = nextId++;
        elements[id] = textField;

        parent.Add(textField);

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, textField.Frame.Right, textField.Frame.Bottom);

        textField.SetNeedsDraw();

        PushLuaInteger(L, id);
        return 1;
    }

    public static int CreateTextView(IntPtr L)
    {
        int parentId = ToLuaInteger(L, 1);
        string text = ToLuaString(L, 2);
        int x = ToLuaInteger(L, 3);
        int y = ToLuaInteger(L, 4);
        int width = ToLuaInteger(L, 5);
        int height = ToLuaInteger(L, 6);
        bool readOnly = ToLuaBoolean(L, 7);  // Volitelné

        if (!elements.ContainsKey(parentId) || !(elements[parentId] is View parent))
        {
            PushLuaError(L, $"⚠️ Window ID {parentId} doesn't exist!");
            return 0;
        }

        var textView = new TextView()
        {
            X = x,
            Y = y,
            Width = width > 0 ? width : Dim.Fill(),
            Height = height > 0 ? height : Dim.Fill(),
            Text = text,
            ReadOnly = readOnly,
            SchemeName = "TextField",
            Multiline = true,
            WordWrap = true,
            ViewportSettings = ViewportSettingsFlags.HasVerticalScrollBar
        };

        int id = nextId++;
        elements[id] = textView;

        parent.Add(textView);

        if (elements[parentId] is MyScrollView scroll)
            ScrollSizeUpdate(scroll, textView.Frame.Right, textView.Frame.Bottom);

        textView.SetNeedsDraw();

        PushLuaInteger(L, id);
        return 1;
    }

    public static int SetProperty(IntPtr L)
    {
        int elementId = ToLuaInteger(L,1);
        string property = ToLuaString(L, 2);
        //Console.WriteLine($"LuaState = {L}");

        if (!elements.ContainsKey(elementId))
        {
            Console.WriteLine($"⚠️ Element ID {elementId} doesn't exist!");
            return 0;
        }

        var element = elements[elementId];

        switch (property)
        {
            case "Text":
                if (element is FrameView window)
                    window.Title = ToLuaString(L, 3);
                else
                    if (element is Button btn_t) btn_t.Title = ToLuaString(L, 3); else element.Text = ToLuaString(L, 3);
                element.SetNeedsLayout();
                element.SetNeedsDraw();
                break;
            case "X":
                element.X = ToLuaInteger(L, 3);
                break;
            case "Y":
                element.Y = ToLuaInteger(L, 3);
                break;

            case "Width":
                element.Width = ToLuaInteger(L, 3) > 0 ? ToLuaInteger(L, 3) : Dim.Fill();
                break;
            case "Height":
                element.Height = ToLuaInteger(L, 3) > 0 ? ToLuaInteger(L, 3) : Dim.Fill();
                break;

            case "XY":
                element.X = ToLuaInteger(L, 3);
                element.Y = ToLuaInteger(L, 4);
                break;
            case "WH":
                element.Width = ToLuaInteger(L, 3) > 0 ? ToLuaInteger(L, 3) : Dim.Fill();
                element.Height = ToLuaInteger(L, 4) > 0 ? ToLuaInteger(L, 4) : Dim.Fill();
                break;
            case "XYWH":
                element.X = ToLuaInteger(L, 3);
                element.Y = ToLuaInteger(L, 4);
                element.Width = ToLuaInteger(L, 5) > 0 ? ToLuaInteger(L, 5) : Dim.Fill();
                element.Height = ToLuaInteger(L, 6) > 0 ? ToLuaInteger(L, 6) : Dim.Fill();
                break;

            case "Progress":
                if (element is ProgressBar progressBar)
                    progressBar.Fraction = Math.Clamp((float)ToLuaNumber(L, 3), 0f, 1f);
                break;

            case "Visible":
                if (element is View view)
                {
                    view.Visible = ToLuaBoolean(L, 3);

                }

                break;
            case "Checked":
                if (element is CheckBox cb)
                    cb.Value = ToLuaBoolean(L, 3) ? CheckState.Checked : CheckState.UnChecked;
                break;
            case "ReadOnly":
                if (element is TextView textView)
                {
                    textView.ReadOnly = ToLuaBoolean(L, 3);
                    textView.SetNeedsDraw();
                }
                break;
            case "Clicked":
                if (element is Button bt)
                {

                    // Pokud už má tlačítko callback, odstraníme ho
                    // Remove existing callback if present
                    if (buttonCallbacks.ContainsKey(bt))
                    {
                        bt.Accepting -= buttonCallbacks[bt];
                        buttonCallbacks.Remove(bt);

                        if (buttonCallbackRefs.TryGetValue(bt, out int oldRef))
                        {
                            LuaLUnref(L, oldRef);
                            buttonCallbackRefs.Remove(bt);
                        }
                    }

                    // Vytvoříme nový event handler
                    // Create new event handler
                    Action newHandler; 

                    if (IsLuaFunction(L, 3))
                    {
                        // ✅ FUNKCE s argumenty
                        // ✅ FUNCTION with arguments
                        int funcRef = LuaLRef(L,3); // Pop funkci // Pop the function

                        // Sesbírej všechny další argumenty (index 4+)
                        // Collect all additional arguments (index 4+)
                        int numArgs = GetTop(L) - 3; // Co je za funkcí // What's after the function
                        object[] args = new object[numArgs];

                        for (int i = 0; i < numArgs; i++)
                        {
                            args[i] = LuaValueToObject(L, 4 + i);
                        }

                        newHandler = () => luaGUI.CallLuaFunction(funcRef, true, args);
                        buttonCallbackRefs[bt] = funcRef;
                        buttonCallbackArgs[bt] = args;
                    }
                    else
                    {
                        // ✅ STRING (žádné extra argumenty)
                        // ✅ STRING (no extra arguments)
                        string luaCallback = ToLuaString(L, 3);
                        newHandler = () => luaGUI.RunSlicedString(luaCallback);
                    }


                    // Uložíme ho do slovníku, aby šel později odstranit
                    // Save it in the dictionary for later removal
                    EventHandler<CommandEventArgs> wrappedHandler2 = (s, e) => newHandler();
                    buttonCallbacks[bt] = wrappedHandler2;

                    // Připojíme nový handler
                    // Attach the new handler
                    bt.Accepting += wrappedHandler2;
                }
                break;
            case "OnUpKey":
                if (element is TextField || element is TextView)
                {
                    View textControl = (View)element;

                    // Odstraň starý callback
                    if (textFieldKeyUpCallbacks.ContainsKey(textControl))
                    {
                        textControl.KeyUp -= textFieldKeyUpCallbacks[textControl];
                        textFieldKeyUpCallbacks.Remove(textControl);

                        if (textFieldCallbackRefs.TryGetValue(textControl, out int oldRef))
                        {
                            LuaLUnref(L, oldRef);
                            textFieldCallbackRefs.Remove(textControl);
                        }

                        if (textFieldCallbackArgs.ContainsKey(textControl))
                        {
                            textFieldCallbackArgs.Remove(textControl);
                        }
                    }

                    EventHandler<Key> newHandler;

                    if (IsLuaFunction(L, 3))
                    {
                        // ✅ FUNKCE s argumenty
                        int funcRef = LuaLRef(L, 3);

                        // Sesbírej všechny další argumenty (index 4+)
                        int numArgs = GetTop(L) - 3;
                        object[] args = new object[numArgs];

                        for (int i = 0; i < numArgs; i++)
                        {
                            args[i] = LuaValueToObject(L, 4 + i);
                        }


                        newHandler = (sender, key) =>
                        {

                            // Deduplikace - ignoruj pokud byl event před méně než 50ms
                            if (lastKeyUpTime.TryGetValue(textControl, out DateTime lastTime))
                            {
                                if ((DateTime.Now - lastTime).TotalMilliseconds < 1)
                                    return;
                            }
                            lastKeyUpTime[textControl] = DateTime.Now;

                            int keyCode = (int)key.KeyCode;
                            char keyChar = key.AsRune.Value <= 0xFFFF ? (char)key.AsRune.Value : '\0';

                            // Zpracuj argumenty - nahraď placeholdery
                            object[] processedArgs = new object[args.Length];
                            for (int i = 0; i < args.Length; i++)
                            {
                                if (args[i] is string strArg)
                                {
                                    // Pokud je to ČISTĚ placeholder, vrať číslo
                                    if (strArg == "%k")
                                    {
                                        processedArgs[i] = keyCode;
                                    }
                                    else if (strArg == "%id")
                                    {
                                        processedArgs[i] = elementId;
                                    }
                                    else if (strArg == "%c")
                                    {
                                        processedArgs[i] = keyChar.ToString();
                                    }
                                    else
                                    {
                                        // Obsahuje text + placeholdery, vrať string
                                        processedArgs[i] = strArg
                                            .Replace("%k", keyCode.ToString())
                                            .Replace("%c", keyChar.ToString())
                                            .Replace("%id", elementId.ToString());
                                    }
                                }
                                else
                                {
                                    processedArgs[i] = args[i];
                                }
                            }

                            luaGUI.CallLuaFunction(funcRef, true, processedArgs);
                        };

                        textFieldCallbackRefs[textControl] = funcRef;
                        textFieldCallbackArgs[textControl] = args;
                    }
                    else
                    {
                        // ✅ STRING - nahraď placeholdery
                        string luaCallback = ToLuaString(L, 3);

                        newHandler = (sender, key) =>
                        {
                            int keyCode = (int)key.KeyCode;
                            char keyChar = key.AsRune.Value <= 0xFFFF ? (char)key.AsRune.Value : ' ';
                            
                            string processedCallback = luaCallback

                                .Replace("%k", keyCode.ToString())
                                .Replace("%cs", $"\"{keyChar}\"")
                                .Replace("%c", keyChar.ToString())
                                .Replace("%id", elementId.ToString());

                            luaGUI.RunSlicedString(processedCallback);
                        };
                    }

                    textFieldKeyUpCallbacks[textControl] = newHandler;
                    textControl.KeyUp += newHandler;
                }
                break;
            case "Color":
            {
                int argCount = GetTop(L) - 2;
                element.SetScheme(BuildColorScheme(L, 3, argCount));
                element.SetNeedsDraw();
                break;
            }
            case "ColorNormal":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                element.SetScheme( new Scheme
                {
                    Normal    = new Attribute(new Color(fr,fg_c,fb), new Color(br,bg_c,bb)),
                    Focus     = ex.Focus,
                    HotNormal = ex.HotNormal,
                    HotFocus  = ex.HotFocus,
                    Disabled  = ex.Disabled,
                    Highlight = ex.Highlight,
                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorFocus":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                element.SetScheme( new Scheme
                {
                    Normal    = ex.Normal,
                    Focus     = new Attribute(new Color(fr, fg_c, fb), new Color(br, bg_c, bb)),
                    HotNormal = ex.HotNormal,
                    HotFocus  = ex.HotFocus,
                    Disabled  = ex.Disabled,
                    Highlight = ex.Highlight,

                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorHotNormal":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                element.SetScheme( new Scheme
                {
                    Normal    = ex.Normal,
                    Focus     = ex.Focus,
                    HotNormal = new Attribute(new Color(fr,fg_c,fb), new Color(br,bg_c,bb)),
                    HotFocus  = ex.HotFocus,
                    Disabled  = ex.Disabled,
                    Highlight = ex.Highlight,
                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorHotFocus":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                element.SetScheme( new Scheme
                {
                    Normal    = ex.Normal,
                    Focus     = ex.Focus,
                    HotNormal = ex.HotNormal,
                    HotFocus  = new Attribute(new Color(fr,fg_c,fb), new Color(br,bg_c,bb)),
                    Disabled  = ex.Disabled,
                    Highlight = ex.Highlight,
                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorHot":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                var attr = new Attribute(new Color(fr,fg_c,fb), new Color(br,bg_c,bb));
                element.SetScheme( new Scheme
                {
                    Normal    = ex.Normal,
                    Focus     = ex.Focus,
                    HotNormal = attr,
                    HotFocus  = attr,
                    Disabled  = ex.Disabled,
                    Highlight = ex.Highlight,
                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorDisabled":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L,3), fg_c = ToLuaInteger(L,4), fb = ToLuaInteger(L,5);
                int br = ToLuaInteger(L,6), bg_c = ToLuaInteger(L,7), bb = ToLuaInteger(L,8);
                element.SetScheme( new Scheme
                {
                    Normal    = ex.Normal,
                    Focus     = ex.Focus,
                    HotNormal = ex.HotNormal,
                    HotFocus  = ex.HotFocus,
                    Disabled  = new Attribute(new Color(fr,fg_c,fb), new Color(br,bg_c,bb)),
                    Highlight = ex.Highlight,
                });
                element.SetNeedsDraw();
                break;
            }
            case "ColorHighlight":
            {
                var ex = element.GetScheme() ?? new Scheme();
                int fr = ToLuaInteger(L, 3), fg_c = ToLuaInteger(L, 4), fb = ToLuaInteger(L, 5);
                int br = ToLuaInteger(L, 6), bg_c = ToLuaInteger(L, 7), bb = ToLuaInteger(L, 8);

                element.SetScheme(new Scheme
                {
                    Normal = ex.Normal,
                    Focus = ex.Focus,
                    HotNormal = ex.HotNormal,
                    HotFocus = ex.HotFocus,
                    Disabled = ex.Disabled,
                    Highlight = new Attribute(new Color(fr, fg_c, fb), new Color(br, bg_c, bb), TextStyle.Italic),
                });
                element.SetNeedsDraw();
                break;
            }
            case "Scheme":
            {
                string schemeName = ToLuaString(L, 3);
                    var cs = SchemeManager.GetScheme(schemeName);
                if (cs != null)
                {
                    element.SchemeName = schemeName;
                    element.SetNeedsDraw();
                }
                else
                    Console.WriteLine($"⚠️ Unknown scheme: {schemeName}");
                break;
            }
            default:
                Console.WriteLine($"⚠️ Unknown property: {property}");
                break;
        }
        return 0;
    }

    public static int GetProperty(IntPtr L)
    {
        int elementId = ToLuaInteger(L, 1);
        string property = ToLuaString(L, 2);

        if (!elements.ContainsKey(elementId))
        {
            Console.WriteLine($"⚠️ Element ID {elementId} doesn't exist!");
            return 0;
        }

        var element = elements[elementId];

        switch (property)
        {
            case "Text":
                if (element is FrameView window)
                {
                    PushLuaString(L, window.Title.ToString());
                    return 1;

                }
                else if (element is View)
                {
                    PushLuaString(L, element.Text.ToString());
                    return 1;
                }

                break;

            case "Checked":
                if (element is CheckBox cb)
                {
                    PushLuaBoolean(L, cb.Value == CheckState.Checked);
                    return 1;
                }
                break;

            case "Selected":
                if (element is OptionSelector radio)
                {
                    PushLuaInteger(L, radio.Value ?? 0);
                    return 1;
                }
                break;

            case "ReadOnly":
                if (element is TextView textView)
                {
                    PushLuaBoolean(L, textView.ReadOnly);
                    return 1;
                }
                break;


        }

        return 0;
    }

    static public List<MenuItem> fileMenuItems = new List<MenuItem>();
    public static int AddMenuItem(IntPtr L)
    {
        string menuName = ToLuaString(L, 1); // Název položky
        string luaCallback = ToLuaString(L, 2); // Funkce v Lua

        if (!menuReferences.TryGetValue("_File", out MenuBarItem? projectsMenu))

        {
            Console.WriteLine("⚠️ Menu '_File' doesn't exist!");
            return 0;
        }
        var newItem = new MenuItem(menuName, "", () => luaGUI.RunSlicedString(luaCallback, true));

        fileMenuItems.Add(newItem);
        menuItemReferences[menuName] = newItem;

        var idx = items.IndexOf(projectsMenu);
        if (idx >= 0)
        {
            var updatedMenu = new MenuBarItem("_File", fileMenuItems.ToArray());
            items[idx] = updatedMenu;
            menuReferences["_File"] = updatedMenu;
            menu.Menus = items.ToArray();
        }

        App.TopRunnableView.SetNeedsDraw();

        return 0;
    }

    public static int UpdateMenuItem(IntPtr L)
    {
        int argCount = GetTop(L); // Počet argumentů

        if (argCount == 2)
        {
            string originalName = ToLuaString(L, 1);
            string newName = ToLuaString(L, 2);

            if (newName == "") { return 0; }
            UpdateMenuTitle(originalName, newName);

        }
        else if (argCount == 3)
        {
            string menuOriginal = ToLuaString(L, 1);
            string itemOriginal = ToLuaString(L, 2);
            string newItemName = ToLuaString(L, 3);
            if (newItemName == "") { return 0; }
            UpdateMenuItem(menuOriginal, itemOriginal, newItemName);
        }
        else
        {
            Console.WriteLine("⚠️ Invalid number of arguments for UpdateMenuItem!");
        }

        return 0;
    }

    public static void UpdateMenuTitle(string originalTitle, string newTitle)
    {
        if (menuReferences.TryGetValue(originalTitle, out MenuBarItem? menu))
        {
            menu.Title = newTitle;
            App.TopRunnableView.SetNeedsDraw(); // Obnova GUI
        }
    }


    public static void UpdateMenuItem(string menuOriginal, string itemOriginal, string newItemName)
    {
        if (menuItemReferences.TryGetValue(itemOriginal, out MenuItem item))
        {
            item.Title = newItemName;
            App.TopRunnableView.SetNeedsDraw(); // Obnova GUI
        }

    }
    /*
    public static void ClearProjectMenu()
    {
        if (!menuReferences.TryGetValue("_File", out MenuBarItem? projectsMenu))
        {
            Console.WriteLine("⚠️ Menu '_File' doesn't exist!");
            return;
        }

        // Vyčistíme položky
        projectsMenu.Children = Array.Empty<MenuItem>();

        // Odstraníme je i z referencí
        foreach (var key in menuItemReferences.Keys.Where(k => menuItemReferences[k].Parent == projectsMenu).ToList())
        {
            menuItemReferences.Remove(key);
        }


    }*/

    public static void ClearProjectMenu()
    {
        if (!menuReferences.TryGetValue("_File", out MenuBarItem? projectsMenu))
        {
            Console.WriteLine("⚠️ Menu '_File' doesn't exist!");
            return;
        }

        // Zjistíme aktuální položky z PopoverMenu a odstraníme je z referencí
        var currentItems = projectsMenu.PopoverMenu?.Root
            ?.GetMenuItemsOfAllSubMenus()
            ?.Select(i => i.Title?.ToString())
            ?.Where(t => t != null)
            ?.ToList() ?? new List<string>();

        foreach (var title in currentItems)
            menuItemReferences.Remove(title!);

        fileMenuItems.Clear();

        // Nahradíme "_File" prázdným MenuBarItem
        var idx = items.FindIndex(m => m.Title?.ToString() == "_File");
        if (idx >= 0)
        {
            var emptyMenu = new MenuBarItem("_File", Array.Empty<MenuItem>());
            items[idx] = emptyMenu;
            menuReferences["_File"] = emptyMenu;
            menu.Menus = items.ToArray();
        }
    }

    public static int RemoveMenuItem(IntPtr L)
    {
        string itemName = ToLuaString(L, 1); // Název položky
        RemoveMenuItem(itemName);
        return 0;
    }


    public static void RemoveMenuItem(string itemName)
    {
        if (!menuReferences.TryGetValue("_File", out MenuBarItem? projectsMenu))
        {
            Console.WriteLine("⚠️ Menu '_File' doesn't exist!");
            return;
        }

        // Hledáme položku v menu
        var menuItems = projectsMenu.PopoverMenu?.Root?.GetMenuItemsOfAllSubMenus().ToList();
        var itemToRemove = menuItems.FirstOrDefault(item => item.Title.ToString() == itemName);

        if (itemToRemove == null)
        {
            Console.WriteLine($"⚠️ Menu item '{itemName}' doesn't exist!");
            return;
        }

        // Odstraníme ji z menu
        menuItems.Remove(itemToRemove);
        //projectsMenu.Children = menuItems.ToArray();

        // Odstraníme ji i z referencí
        menuItemReferences.Remove(itemName);

        // Obnovíme GUI
        App.TopRunnableView.SetNeedsDraw();
    }



    public static int DisableMenu(IntPtr L)
    {
        bool disable = ToLuaBoolean(L, 1);
        DisableMenu(disable);
        return 0;
    }

    public static void DisableMenu(bool disable)
    {
        SetMenuEnabled("_File", !disable);
        SetMenuEnabled("_Language", !disable);

        //SetMenuItemEnabled("_Reset CLua", !disable);

        App.TopRunnableView.SetNeedsDraw();

    }

    public static void SetMenuEnabled(string menuTitle, bool enabled)
    {
        if (menuReferences.TryGetValue(menuTitle, out MenuBarItem? menu))
        {

            menu.Enabled = enabled;

        }
        else
        {
            Console.WriteLine($"⚠️ Menu '{menuTitle}' doesn't exist!");
            return;

        }
        App.TopRunnableView.SetNeedsDraw();
    }


    // Hlavní funkce pro zničení elementu
    // Main function to destroy an element
    public static int DestroyElement(IntPtr L)
    {
        int elementId = ToLuaInteger(L, 1);

        if (!elements.ContainsKey(elementId))
        {
            Console.WriteLine($"⚠️ Element ID {elementId} doesn't exist!");
            return 0;
        }

        var elem = elements[elementId];

        // Rekurzivně vyčisti všechny potomky
        // Recursively clean up all children
        CleanupElementAndChildren(L, elem);

        // Odeber z UI
        // Pokud je to okno, odeber ho z top-level, jinak z jeho SuperView
        // If it's a window, remove it from top-level, otherwise from its SuperView
        if (elem is MyWindowView)
        {
            top.Remove(elem);
        }
        else
        {
            elem.SuperView?.Remove(elem);
        }

        elements.Remove(elementId);
        //Console.WriteLine($"✅ Element ID {elementId} was destroyed.");

        top.SetNeedsDraw();

        return 0;
    }

    // Pomocná funkce pro rekurzivní cleanup
    // Helper function for recursive cleanup
    private static void CleanupElementAndChildren(IntPtr L, View element)
    {
        // Nejdřív vyčisti všechny děti
        // First, clean up all children
        if (element.SubViews != null)
        {
            foreach (var child in element.SubViews.ToList())
            {
                CleanupElementAndChildren(L, child);

                // Najdi ID dítěte a odstraň ze slovníku
                var childId = elements.FirstOrDefault(x => x.Value == child).Key;
                if (childId != 0) // 0 = default(int), znamená nenalezeno
                {
                    elements.Remove(childId);
                //    Console.WriteLine($"  ↳ Destroyed child element ID {childId}");
                }
            }
        }

        // Pak vyčisti tento element
        // Next, clean up this element
        CleanupElementCallbacks(L, element);
    }

    // Vyčištění callbacků pro jeden element
    // Cleanup callbacks for a single element
    private static void CleanupElementCallbacks(IntPtr L, View element)
    {
        // TextField/TextView KeyUp callbacky
        if (textFieldKeyUpCallbacks.ContainsKey(element))
        {
            element.KeyUp -= textFieldKeyUpCallbacks[element];
            textFieldKeyUpCallbacks.Remove(element);
        }

        if (textFieldCallbackRefs.ContainsKey(element))
        {
            LuaLUnref(L, textFieldCallbackRefs[element]);
            textFieldCallbackRefs.Remove(element);
        }

        if (textFieldCallbackArgs.ContainsKey(element))
        {
            textFieldCallbackArgs.Remove(element);
        }

        if (lastKeyUpTime.ContainsKey(element))
        {
            lastKeyUpTime.Remove(element);
        }

        // Button callbacky
        if (element is Button btn)
        {
            if (buttonCallbacks.ContainsKey(btn))
            {
                btn.Accepting -= buttonCallbacks[btn];
                buttonCallbacks.Remove(btn);
            }

            if (buttonCallbackRefs.TryGetValue(btn, out int funcRef))
            {
                LuaLUnref(L, funcRef);
                buttonCallbackRefs.Remove(btn);
            }

            if (buttonCallbackArgs.ContainsKey(btn))
            {
                buttonCallbackArgs.Remove(btn);
            }
        }
    }

    public static int DestroyAllElements(IntPtr L)
    {
        Console.WriteLine("🔄 Reset GUI: Removing all elements...");

        foreach (var elem in elements.Values.ToList()) // Projdeme všechny prvky
        {
            if (elem is FrameView window)
            {
                foreach (var child in window.SubViews.ToList())
                {
                    window.Remove(child);
                }
                top.Remove(window);
            }
            else
            {
                elem.SuperView?.Remove(elem);
            }
        }

        ClearProjectMenu();

        // ⭐ Sanity check - vyčisti všechno co zbylo
        foreach (var funcRef in buttonCallbackRefs.Values.ToList())
        {
            LuaLUnref(luaGUI.MainState, funcRef);
        }
        foreach (var LuaArg in buttonCallbackArgs.Values.ToList())
        {
            foreach (var LuaRef in LuaArg)
                if (LuaRef is LuaReference luaRef)
                    LuaLUnref(luaGUI.MainState, luaRef.Ref);
        }
        
        buttonCallbacks.Clear();
        buttonCallbackRefs.Clear();
        buttonCallbackArgs.Clear();

        textFieldCallbackArgs.Clear();
        textFieldCallbackRefs.Clear();
        textFieldCallbackRefs.Clear();
        textFieldKeyUpCallbacks.Clear();

        elements.Clear(); // Vyčistíme seznam prvků
        top.SetNeedsDraw(); // Aktualizace GUI
        App.LayoutAndDraw();
        Console.WriteLine("✅ GUI has been reset.");

        return 0;

    }

    public static void PrintCallbackStats()
    {
        Console.WriteLine($"📊 Active callbacks: {buttonCallbacks.Count}");
        Console.WriteLine($"📊 Registry refs: {buttonCallbackRefs.Count}");
    }


    // ── Pomocná funkce pro sestavení ColorScheme z Lua argumentů ──────────────
    private static Scheme BuildColorScheme(IntPtr L, int startIndex, int argCount)
    {
        Color ParseFg(int offset) => new Color(
            ToLuaInteger(L, startIndex + offset),
            ToLuaInteger(L, startIndex + offset + 1),
            ToLuaInteger(L, startIndex + offset + 2));

        Color ParseBg(int offset) => new Color(
            ToLuaInteger(L, startIndex + offset + 3),
            ToLuaInteger(L, startIndex + offset + 4),
            ToLuaInteger(L, startIndex + offset + 5));

        if (argCount >= 36)
        {
            // Téměř Plná kontrola: Normal, Focus, HotNormal, HotFocus, Disabled, Highlight
            return new Scheme
            {
                Normal = new Attribute(ParseFg(0), ParseBg(0)),
                Focus = new Attribute(ParseFg(6), ParseBg(6)),
                HotNormal = new Attribute(ParseFg(12), ParseBg(12)),
                HotFocus = new Attribute(ParseFg(18), ParseBg(18)),
                Disabled = new Attribute(ParseFg(24), ParseBg(24)),
                Highlight = new Attribute(ParseFg(30), ParseBg(30), TextStyle.Italic),
            };

        }
        else if (argCount >= 30)
        {
            // Téměř Plná kontrola: Normal, Focus, HotNormal, HotFocus, Disabled
            return new Scheme
            {
                Normal = new Attribute(ParseFg(0), ParseBg(0)),
                Focus = new Attribute(ParseFg(6), ParseBg(6)),
                HotNormal = new Attribute(ParseFg(12), ParseBg(12)),
                HotFocus = new Attribute(ParseFg(18), ParseBg(18)),
                Disabled = new Attribute(ParseFg(24), ParseBg(24)),
                Highlight = new Attribute(ParseFg(6), ParseBg(6), TextStyle.Italic),
            };
        }
        else if (argCount >= 12)
        {
            // Normal + Focus (Hot odvozen od Focus, Disabled ztmaveno)
            var normalAttr = new Attribute(ParseFg(0), ParseBg(0));
            var focusAttr = new Attribute(ParseFg(6), ParseBg(6));
            var highlightAttr = new Attribute(ParseFg(6), ParseBg(6), TextStyle.Italic);
            return new Scheme
            {
                Normal = normalAttr,
                Focus = focusAttr,
                HotNormal = normalAttr,
                HotFocus = focusAttr,
                Disabled = new Attribute(new Color(100, 100, 100), ParseBg(0)),
                Highlight = highlightAttr,
            };
        }
        else
        {
            // 6 hodnot: všech 5 stavů stejně
            var attr = new Attribute(ParseFg(0), ParseBg(0));
            var highlightAttr = new Attribute(ParseFg(0), ParseBg(0), TextStyle.Italic);
            return new Scheme
            {
                Normal = attr,
                Focus = attr,
                HotNormal = attr,
                HotFocus = attr,
                Disabled = new Attribute(new Color(100, 100, 100), ParseBg(0)),
                Highlight = highlightAttr,
            };
        }
    
    }

    // ── SetScheme: vytvoří nebo přepíše schéma v Colors.ColorSchemes ──────────
    public static int SetScheme(IntPtr L)
    {
        string name = ToLuaString(L, 1);
        int argCount = GetTop(L) - 1;
        var scheme = BuildColorScheme(L, 2, argCount);
        SchemeManager.Schemes[name] = scheme;
        App.TopRunnableView?.SetNeedsDraw();
        return 0;
    }

    // tu reset LUACode;
    public static int ResetSGUI(IntPtr L)
    {

        App.Invoke(() =>
        {
            ExcelManager.CloseAllWorkbooks();
            DestroyAllElements(IntPtr.Zero);
            luaGUI.Instance.ResetLua();
            luaGUI.SandboxLua();

            RegisterLuaGUIFunctions(luaGUI);
            RegisterLuaGUIConstants(luaGUI);

            luaGUI.Init("CLua", "");
        });

        return 0;
    }

}
