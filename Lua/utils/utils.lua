----------------------------------------------
--  Name:      Base Util Functions
--  Category:  Utils
----------------------------------------------
-- Legacy Support
clock = os.clock;
debugTracker = debug.traceback;


--- just create COORs table
---@class COORS
---@field X number X
---@field Y number Y
---@field W number width
---@field H number height
---@param SX number
---@param SY number
---@param SW number
---@param SH number
function XYWH(SX,SY,SW,SH)
	return {X = SX, Y = SY, W = SW, H = SH};
end;


--- create RGBA table
---@class RGBA
---@field red number 0-255
---@field green number 0-255
---@field blue number 0-255
---@field alpha number 0-255
---@param red number 0-255
---@param green number 0-255
---@param blue number 0-255
---@param alpha number 0-255
---@return RGBA
function RGBA(red,green,blue,alpha)
	return {red=red,green=green,blue=blue,alpha=alpha};
end;
--- create RGBA table with 255 alpha
---@param red number 0-255
---@param green number 0-255
---@param blue number 0-255
---@return RGBA
function RGB(red,green,blue)
	return RGBA(red,green,blue,255);
end;

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
--- Create #RRGGBB from RGBA table
---@param rgb table RGBA
---@return string
function GetHexColor(rgb)
	if rgb == nil then
		rgb = RGB(200,200,200);
	end;

	return ('#' ..string.format("%02x%02x%02x", rgb.red,rgb.green,rgb.blue));
end;

--- Trims whitespace before text
function string.trim(s)
    return string.match(tostring(s), "^%s*(.-)$")
end
--- Trims whitespace after text
function string.rtrim(s)
    return string.match(tostring(s), "^(.-)%s*$")
end
--- Trims whitespace on both side of text
function string.ctrim(s)
   return string.match(tostring(s), "^%s*(.-)%s*$" );
end;


--legacy
CleanTrim = string.ctrim;
--- if string start on prefix
---@param self string 
---@param prefix string
---@return boolean
function string.startswith(self, prefix)
    return self:sub(1, #prefix) == prefix;
end;

--- WARNING:  
--- Never serialize _G or _ENV in global scope.  
--- They contain circular references and will cause infinite recursion.	
---@param val any
---@param name string 
---@param inOneLine boolean
---@param depth number
---@return string
function table.serialize(val, name, inOneLine, depth)
    inOneLine = inOneLine or false;
    depth = depth or 0;

    local tmp = string.rep(" ", depth);

    if name then
        tmp = tmp .. name .. " = "; end;

    if type(val) == "table" then
        tmp = tmp .. "{" .. (not inOneLine and "\n" or " ");

        for k, v in pairs(val) do

            if type(k) == "number" then
                k = "[".. k .. "]";
            else
                k = "[".. string.format("%q", k) .. "]";
            end;
            tmp =  tmp .. table.serialize(v, k, inOneLine, (not inOneLine and depth + 2 or 0)) .. "," .. (not inOneLine and "\n" or " ");
        end;

        tmp = tmp .. string.rep(" ", depth) .. "}";
    elseif type(val) == "number" then
        tmp = tmp .. tostring(val);
    elseif type(val) == "string" then
        tmp = tmp .. string.format("%q", val);
    elseif type(val) == "boolean" then
        tmp = tmp .. (val and "true" or "false");
	elseif type(val) == "function" then
		tmp = tmp .. string.format("%q", tostring(val));
    else
        tmp = tmp .. "\"[unserializeable datatype: " .. type(val) .. "]\"";
    end;

    return tmp;
end;


--- Note: copytable performs a deep copy of data only.   
--- Metatable must be reapplied manually.
---@param orig table|any
---@return table|any
function table.copytable(orig)
    local orig_type = type(orig)
    local copy;
    if orig_type == 'table' then
        copy = {};
        for orig_key, orig_value in next, orig, nil do
            copy[table.copytable(orig_key)] = table.copytable(orig_value);
        end;
        --setmetatable(copy, copytable(getmetatable(orig)))
    else -- number, string, boolean, etc
        copy = orig;
    end;
    return copy;
end;

--- check if table contain value
---@param tbl table
---@param val any
---@return boolean
function table.contains(tbl, val)
	
    for k, v in pairs(tbl) do
        if v == val then 
            return true;
        end;
    end;
	
    return false;
end;

--- Sequential table only.  
--- Removes all occurrences of value from table (changes the original table)
---@param tbl table
---@param val any
function table.removeValue(tbl, val)
	for i = #tbl, 1, -1 do
		if tbl[i] == val then
			table.remove(tbl, i);
		end;
	end;
end;

--- Sequential table only.  
--- Remove all Values inside table B from table A (changes the original table A)
---@param tblA table
---@param tblB table
function table.diff(tblA, tblB)
	local set = {}
	for _, v in ipairs(tblB) do
		set[v] = true;
	end;

	for i = #tblA, 1, -1 do
		if set[tblA[i]] then
			table.remove(tblA, i);
		end;
	end;
end;

--[[ Usage:
```
 switch(value):caseof{
   [key]     = function(value, switch) ... end,  -- parameters are optional
   ...
   missing  = function(...) ... end, -- called when value is nil/false
   default  = function(...) ... end, -- fallback when no key matches (or when value is nil/false and no missing handler is provided)
 }
 ```
 Simple inline dispatch helper.  
 Calls a matching case function from the provided table.  
 If a case function returns a value, caseof returns it.  
]]
---@param c any
---@return table
function switch(c)
  local swtbl = {
    casevar = c,
    caseof = function (self, code)
      local f;
      if (self.casevar) then
        f = code[self.casevar] or code.default;
      else
        f = code.missing or code.default;
      end;
      if f then
        if type(f)=="function" then
          return f(self.casevar,self);
        else
          error("case "..tostring(self.casevar).." not a function");
        end;
      end;
    end;
  };
  return swtbl;
end;

---@param number number
---@return boolean
function math.odd(number)
	if (math.mod(number,2) == 0) then
		return false;
	else
		return true;
	end
end;

---@param number number
---@return boolean
function math.even(number)
	if (math.mod(number,2) == 0) then
		return true;
	else
		return false;
	end
end;

math.roundup = math.sign

function math.sign(x)
  return (x<0 and -1) or 1;
end

math.mod = math.fmod;

---@param A number
---@param B number
---@return integer
function math.div(A,B)
    return math.floor(A/B);
end;
-- rounds towards zero, away from both infinities.
math.trunc = function(n) return n >= 0.0 and n-n% 1 or n-n%-1 end;
-- rounds away from zero, towards both infinities.
math.round = function(n) return n >= 0.0 and n-n%-1 or n-n% 1 end;

math.minmax = function(VALUE,MIN,MAX)
	return math.max(math.min(VALUE,MAX),MIN);
end;

--- Take int seconds to make str [hours:]minuts:seconds
---@param t integer
---@return string
function getTime(t)
	local function addZeros(INPUT)
		return (INPUT < 10) and "0" .. INPUT or INPUT;
	end;
	
	local seconds = tonumber(string.format("%.4f", t));--t;
	local minutes = math.div(seconds,60);
	if minutes > 0 then
		seconds = (seconds - minutes*60);
	end;
	local hours = math.div(minutes,60);
	if hours > 0 then
		minutes = (minutes - hours*60);
	else
		return (minutes .. ":" .. addZeros(seconds));
	end;

	return (hours..':'..addZeros(minutes).. ':' .. addZeros(seconds));
end;

--- Calls function f(...) in a protected context.  
--- Returns (ok, ...):
--- >  ok == true  -> subsequent values are the function return values  
--- >  ok == false -> the next value is the error message with traceback  
---comment
---@param f function
---@param ... any
---@return boolean StateOK, any|string ErrorMessage-Or-Returns
function safeCall(f, ...)
	local args = {...};
	return xpcall(
		function() return f(table.unpack(args)); end,
		debug.traceback
	);
end;

