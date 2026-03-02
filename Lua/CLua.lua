----------------------------------------------
--  Name:      First lua File - Inicializing
--  Author:    Sali
--  Category:  Core
----------------------------------------------
include('Loc/English');
if LANG and LANG ~= "English" then
	tryInclude('Loc/' .. LANG);
end;

include('constants');

GUI.UpdateMenuItem("_Program",TID_MFILE);
GUI.UpdateMenuItem("_Program","_Reset CLua", TID_MRESET);
GUI.UpdateMenuItem("_Program","_Exit", TID_MEXIT);
GUI.UpdateMenuItem("_File",TID_MPROJECTS);
GUI.UpdateMenuItem("_Help", TID_HELP);
GUI.UpdateMenuItem("_Help","_About", TID_ABOUT);

local luaFiles = GetFiles("lua/utils", "lua", true);
include("utils/utils");
table.diff(luaFiles,{"utils"});
for _, luaFile in ipairs(luaFiles) do
    include("utils/" .. luaFile);
end;

tryInclude("OWMacros");

luaFiles = GetFiles("lua", "lua", true);
table.diff(luaFiles,{"CLua","constants","OWMacros"});
for _, luaFile in ipairs(luaFiles) do
    include(luaFile);

end;
