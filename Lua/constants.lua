----------------------------------------------
--  Name:      Constants Helper
--  Category:  Core
----------------------------------------------
Encoding = {
            ANSI = "ansi", 
            UTF8 = "utf8",
            UTF8BOM = "utf8bom",
            UTF16 = "utf16", 
            UTF16LE = "utf16le", 
            UTF16BE = "utf16be",
			Unicode = "utf16le"
};

CR = string.char(13); -- CR - \\r (Konec řádku pre-OSX - starý MacOS)
LF = string.char(10); -- LF - \\n (Konec řádku Unix/MacOS)
CRLF = CR .. LF;      -- CR + LF - \\r\\n (Konec řádku Windows);
t = string.char(9);   -- Tab
Q = string.char(34);  -- "
a = string.char(39);  -- '


