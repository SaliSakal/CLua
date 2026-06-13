----------------------------------------------
--  Name:      GUI Color Helpers
--  Category:  Utils
----------------------------------------------

-- ── Předdefinované barvy (RGB) ─────────────────────────────────────────────

--- create RGBA table with same RGB and different A
---@param gray number 0-255
---@param alpha number 0-255
---@return RGBA
function GRAYA(gray,alpha)
	return RGBA(gray,gray,gray,alpha);
end;

--- create RGBA table with same RGB and 255 Alpha
---@param gray number 0-255
---@return RGBA
function GRAY(gray)
	return GRAYA(gray,255);
end;
--- create RGBA table with black color with custom Alpha
---@param alpha number 0-255
---@return RGBA
function BLACKA(alpha)
	return RGBA(0,0,0,alpha);
end;
--- create RGBA table with black color with 255 Alpha
---@return RGBA
function BLACK()
	return BLACKA(255);
end;
--- create RGBA table with white color with custom Alpha
---@param alpha number 0-255
---@return RGBA
function WHITEA(alpha)
	return RGBA(255,255,255,alpha);
end;
--- create RGBA table with white color with 255 Alpha
---@return RGBA
function WHITE()
	return WHITEA(255);
end;


-- Standard 16 terminal colours + extended named colours
-- Use with setColor, setColorNormal, etc.:
--   setColor(el, COLOR.BrightGreen, COLOR.Black)

COLOR = {
    -- Standard terminal colors
    Black         = RGB(  0,   0,   0),
    Red           = RGB(128,   0,   0),
    Green         = RGB(  0, 128,   0),
    Yellow        = RGB(128, 128,   0),
    Blue          = RGB(  0,   0, 128),
    Magenta       = RGB(128,   0, 128),
    Cyan          = RGB(  0, 128, 128),
    White         = RGB(192, 192, 192),

    -- Bright variants
    BrightBlack   = RGB(128, 128, 128),
    BrightRed     = RGB(255,  85,  85),
    BrightGreen   = RGB( 85, 255,  85),
    BrightYellow  = RGB(255, 255,  85),
    BrightBlue    = RGB( 85,  85, 255),
    BrightMagenta = RGB(255,  85, 255),
    BrightCyan    = RGB( 85, 255, 255),
    BrightWhite   = RGB(255, 255, 255),

    -- Dark variants (~60 % of base terminal colour)
    DarkGray      = RGB( 64,  64,  64),
    DarkRed       = RGB( 75,   0,   0),
    DarkGreen     = RGB(  0,  75,   0),
    DarkBlue      = RGB(  0,   0,  75),
    DarkCyan      = RGB(  0,  75,  75),

    -- Pure / maximum-saturation variants
    PureRed       = RGB(255,   0,   0),
    PureGreen     = RGB(  0, 255,   0),
    PureBlue      = RGB(  0,   0, 255),
    PureYellow    = RGB(255, 255,   0),
    PureMagenta   = RGB(255,   0, 255),
    PureCyan      = RGB(  0, 255, 255),

    -- Grays
    Gray          = RGB(128, 128, 128),  -- alias: BrightBlack
    LightGray     = RGB(192, 192, 192),  -- alias: White
    Silver        = RGB(192, 192, 192),
    Gainsboro     = RGB(220, 220, 220),

    -- Reds & pinks
    Crimson       = RGB(220,  20,  60),
    Tomato        = RGB(255,  99,  71),
    Coral         = RGB(255, 127,  80),
    Salmon        = RGB(250, 128, 114),
    Pink          = RGB(255, 192, 203),
    HotPink       = RGB(255, 105, 180),
    DeepPink      = RGB(255,  20, 147),

    -- Oranges & browns
    OrangeRed     = RGB(255,  69,   0),
    Orange        = RGB(255, 165,   0),
    Gold          = RGB(255, 215,   0),
    Chocolate     = RGB(210, 105,  30),
    SaddleBrown   = RGB(139,  69,  19),
    Brown         = RGB(165,  42,  42),
    Tan           = RGB(210, 180, 140),

    -- Greens
    ForestGreen   = RGB( 34, 139,  34),
    Olive         = RGB(128, 128,   0),  -- alias: Yellow
    LimeGreen     = RGB( 50, 205,  50),
    Lime          = RGB(  0, 255,   0),  -- alias: PureGreen
    SpringGreen   = RGB(  0, 255, 127),

    -- Blues
    Navy          = RGB(  0,   0, 128),  -- alias: Blue
    RoyalBlue     = RGB( 65, 105, 225),
    SteelBlue     = RGB( 70, 130, 180),
    DodgerBlue    = RGB( 30, 144, 255),
    SkyBlue       = RGB(135, 206, 235),
    LightBlue     = RGB(173, 216, 230),

    -- Purples
    Indigo        = RGB( 75,   0, 130),
    DarkViolet    = RGB(148,   0, 211),
    Purple        = RGB(128,   0, 128),  -- alias: Magenta
    MediumPurple  = RGB(147, 112, 219),
    Violet        = RGB(238, 130, 238),
    Orchid        = RGB(218, 112, 214),
    Plum          = RGB(221, 160, 221),

    -- Teals & cyans
    Teal          = RGB(  0, 128, 128),  -- alias: Cyan
    Turquoise     = RGB( 64, 224, 208),
    Aquamarine    = RGB(127, 255, 212),
};

-- ── Interní helper: rozbalí RGB/RGBA table → r, g, b, a ──────────────────────
local function unpackRGBA(c)
    return c.red, c.green, c.blue, c.alpha;
end;

local function unpackRGB(c)
    return c.red, c.green, c.blue;
end;

-- ── Funkce pro nastavení barev elementu ────────────────────────────────────

--- Nastaví všech 5 stavů (Normal, Focus, HotNormal, HotFocus, Disabled) na stejnou barvu.
---@param el Element
---@param fg RGBA # barva textu (popředí)
---@param bg RGBA # barva pozadí
function setColor(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví pouze stav Normal.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorNormal(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_NORMAL, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví pouze stav Focus.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorFocus(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_FOCUS, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví pouze stav HotNormal.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorHotNormal(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_HOT_NORMAL, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví pouze stav HotFocus.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorHotFocus(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_HOT_FOCUS, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví HotNormal + HotFocus najednou.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorHot(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_HOT, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Nastaví pouze stav Disabled.
---@param el Element
---@param fg RGBA
---@param bg RGBA
function setColorDisabled(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_DISABLED, fr, fg, fb, br, bg, bb);
    return el;
end;

function setColorHighlight(el, fg, bg)
    local fr, fg, fb = unpackRGB(fg);
    local br, bg, bb = unpackRGB(bg);
    GUI.SetProperty(el.ID, PROB_COLOR_HIGHLIGHT, fr, fg, fb, br, bg, bb);
    return el;
end;

--- Assigns a named schema to an element.
---@param el Element
---@param name string # schema name (must already exist)
function applyScheme(el, name)
    GUI.SetProperty(el.ID, PROB_SCHEME, name);
    return el;
end;

-- ── Scheme management functions ───


--- Creates a SchemeColors table from positional colour arguments.
--- Arguments must be passed in complete groups — mixing is not allowed:
---
---   **2 colors** (normalFg, normalBg)
---   → all states = normal
---
---   **4 colors** (+ focusFg, focusBg)
---   → Normal + Focus; Hot* = Normal/Focus
---
---   **10 colors** (+ hotNormalFg/Bg, hotFocusFg/Bg, disabledFg/Bg)
---   → Normal, Focus, HotNormal, HotFocus, Disabled; Highlight = Focus
---
---   **12 colors** (+ highlightFg, highlightBg)
---   → full scheme
---@param normalFg     RGBA
---@param normalBg     RGBA
---@param focusFg?     RGBA
---@param focusBg?     RGBA
---@param hotNormalFg? RGBA
---@param hotNormalBg? RGBA
---@param hotFocusFg?  RGBA
---@param hotFocusBg?  RGBA
---@param disabledFg?  RGBA
---@param disabledBg?  RGBA
---@param highlightFg? RGBA
---@param highlightBg? RGBA
---@return SchemeColors
function SchemeColors(normalFg, normalBg, focusFg, focusBg,
                      hotNormalFg, hotNormalBg, hotFocusFg, hotFocusBg,
                      disabledFg, disabledBg, highlightFg, highlightBg)
    return {
        normalFg    = normalFg,
        normalBg    = normalBg,
        focusFg     = focusFg,
        focusBg     = focusBg,
        hotNormalFg = hotNormalFg,
        hotNormalBg = hotNormalBg,
        hotFocusFg  = hotFocusFg,
        hotFocusBg  = hotFocusBg,
        disabledFg  = disabledFg,
        disabledBg  = disabledBg,
        highlightFg = highlightFg,
        highlightBg = highlightBg,
    };
end;

---
--- Creates or overwrites a named scheme from a SchemeColors table.
--- The mode is determined by which fields are present:
---   normalFg+normalBg only          → 2-color: all states = fg/bg
---   + focusFg+focusBg               → 4-color: Normal + Focus
---   + hotNormal*+hotFocus*+disabled* → 10-color: + Hot/Disabled (Highlight = Focus)
---   + highlightFg+highlightBg       → 12-color: full scheme
---@param name string        # scheme name
---@param c    SchemeColors  # SchemeColors table
function SetScheme(name, c)

    if type(c) ~= "table" or c.normalFg == nil or c.normalBg == nil then
        print("⚠️ The scheme '" .. name .."' cannot be set, SchemeColors does not even have normal colors.");
        return;
    end;
    local nfr, nfg, nfb = unpackRGB(c.normalFg);
    local nbr, nbg, nbb = unpackRGB(c.normalBg);

    if c.highlightFg == nil or c.highlightBg == nil then
        -- 2 colors: all states = normalFg/normalBg
        GUI.SetScheme(name, nfr, nfg, nfb, nbr, nbg, nbb);
        return;
    end;

    local ffr, ffg, ffb = unpackRGB(c.focusFg);
    local fbr, fbg, fbb = unpackRGB(c.focusBg);

    if c.hotNormalFg == nil or c.hotNormalBg == nil or
       c.hotFocusFg  == nil or c.hotFocusBg  == nil or
       c.disabledFg  == nil or c.disabledBg  == nil then
        -- 4 colors: Normal + Focus
        GUI.SetScheme(name,
            nfr, nfg, nfb, nbr, nbg, nbb,
            ffr, ffg, ffb, fbr, fbg, fbb);
        return;
    end;

    local hnfr, hnfg, hnfb = unpackRGB(c.hotNormalFg);
    local hnbr, hnbg, hnbb = unpackRGB(c.hotNormalBg);
    local hffr, hffg, hffb = unpackRGB(c.hotFocusFg);
    local hfbr, hfbg, hfbb = unpackRGB(c.hotFocusBg);
    local dfr,  dfg,  dfb  = unpackRGB(c.disabledFg);
    local dbr,  dbg,  dbb  = unpackRGB(c.disabledBg);

    if c.highlightFg == nil or c.highlightBg == nil  then
        -- 10 colors: Normal + Focus + HotNormal + HotFocus + Disabled
        GUI.SetScheme(name,
            nfr,  nfg,  nfb,  nbr,  nbg,  nbb,
            ffr,  ffg,  ffb,  fbr,  fbg,  fbb,
            hnfr, hnfg, hnfb, hnbr, hnbg, hnbb,
            hffr, hffg, hffb, hfbr, hfbg, hfbb,
            dfr,  dfg,  dfb,  dbr,  dbg,  dbb);
        return;
    end;

    local hfr, hfg, hfb = unpackRGB(c.highlightFg);
    local hbr, hbg, hbb = unpackRGB(c.highlightBg);

    -- 12 colors: full scheme
    GUI.SetScheme(name,
        nfr,  nfg,  nfb,  nbr,  nbg,  nbb,
        ffr,  ffg,  ffb,  fbr,  fbg,  fbb,
        hnfr, hnfg, hnfb, hnbr, hnbg, hnbb,
        hffr, hffg, hffb, hfbr, hfbg, hfbb,
        dfr,  dfg,  dfb,  dbr,  dbg,  dbb,
        hfr,  hfg,  hfb,  hbr,  hbg,  hbb);
end;

--[[ example

-- COLOR.* constants can be used anywhere a raw RGB() value is expected
local myColors = SchemeColors(
    COLOR.PureMagenta, COLOR.DarkRed,       -- normalFg,    normalBg
    COLOR.Magenta,     COLOR.Gray,           -- focusFg,     focusBg
    COLOR.PureBlue,    COLOR.DarkGreen,      -- hotNormalFg, hotNormalBg
    COLOR.PureCyan,    COLOR.DarkBlue,       -- hotFocusFg,  hotFocusBg
    COLOR.BrightBlack, COLOR.DarkCyan,       -- disabledFg,  disabledBg
    COLOR.Green,       COLOR.DarkRed         -- highlightFg, highlightBg
);

-- mixing COLOR.* and RGB() is fine too
local myColors2 = SchemeColors(
    COLOR.BrightWhite, RGB(20, 20, 40),      -- normalFg,    normalBg
    COLOR.PureCyan,    RGB(10, 10, 60)       -- focusFg,     focusBg
);

SetScheme("Base",    myColors);
SetScheme("Menu",    myColors);
SetScheme("Label",   myColors);
SetScheme("Button",  myColors);
SetScheme("logView", myColors);

-- or: named scheme applied to an element
SetScheme("BestColours", myColors);
Button:ApplyScheme("BestColours");
]]--

-- standart schemes inside code
-- Base, Menu, Dialog, Label, Button, ProgressBar, Checkbox, textField, logView    (Base is default for Windows, Frames, Scrollbars (of scrollbox) etc)

-- ── Add metods into ElementClass ──────────────────────────────────────────
-- (ElementClass is defined inside elements.lua, its loaded before colors)

ElementClass.SetColor         = setColor;
ElementClass.SetColorNormal   = setColorNormal;
ElementClass.SetColorFocus    = setColorFocus;
ElementClass.SetColorHotNormal= setColorHotNormal;
ElementClass.SetColorHotFocus = setColorHotFocus;
ElementClass.SetColorHot      = setColorHot;
ElementClass.SetColorDisabled = setColorDisabled;
ElementClass.SetColorHighlight = setColorHighlight;
ElementClass.ApplyScheme      = applyScheme;

WindowClass.ApplyScheme       = applyScheme;