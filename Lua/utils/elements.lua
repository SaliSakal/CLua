----------------------------------------------
--  Name:      Creating Elements
--  Category:  Utils
----------------------------------------------


-- show specific window and hide other
function setActiveWindow(el)
	GUI.SetActiveWindow(el.ID);

	return el;
end;

-- show/hide element
function setVisible(el, value)
	GUI.SetProperty(el.ID, PROB_VISIBLE, value);
    el.visible = value;

	return el;
end;

function setText(el, value)
	GUI.SetProperty(el.ID, PROB_TEXT, value);
    el.text = value;

	return el;
end;

function getText(el)
	return GUI.GetProperty(el.ID, PROB_TEXT);
end;

function setProgress(el, value)
	GUI.SetProperty(el.ID, PROB_PROGRESS, value);

	return el;
end;

function setChecked(el,VALUE)
	GUI.SetProperty(el.ID,PROB_CHECKED,VALUE);
    el.checked = VALUE;

	return el;
end;

function setX(el,VALUE)
	GUI.SetProperty(el.ID,PROB_X, VALUE);
	el.x = VALUE;

	return el;
end;

function setY(el, VALUE)
	GUI.SetProperty(el.ID, PROB_Y, VALUE);
	el.y = VALUE;

	return el;
end;

function setW(el, VALUE)
	GUI.SetProperty(el.ID, PROB_W,VALUE);
	el.width = VALUE;

	return el;
end;

function setH(el,VALUE)
	GUI.SetProperty(el.ID, PROB_H,VALUE);
	el.height = VALUE;

	return el;
end;

function setXY(el, X, Y)
	GUI.SetProperty(el.ID,PROB_XY, X, Y);
	el.x = X;
	el.y =Y;

	return el;
end;

function setWH(el, W, H)
	GUI.SetProperty(el.ID, PROB_WH,W,H);
	el.width = W;
	el.height = H;

	return el;
end;

function setXYWH(el, XYWH)
	GUI.SetProperty(el.ID,PROB_XYWH, XYWH.X, XYWH.Y, XYWH.W, XYWH.H);
	el.x = XYWH.X;
	el.y = XYWH.Y;
	el.width = XYWH.W;
	el.height = XYWH.H;

	return el;
end;

function getChecked(el)
	return GUI.IsCheckboxChecked(el.ID);
end;

-- callback can be either:
--   - a string: evaluated in the global scope on each call
--   - a function: called directly with ... as parameters (support string, numbers, bool, nil, table and function).
function setCallback(el,VALUE, ...)
	if (VALUE ~= nil) then
		GUI.SetProperty(el.ID,PROB_CALLBACK,VALUE, ...);
	end;

	return el;
end;

function setRadioSelected(el,value)
	GUI.SetRadioSelected(el.ID, value);

	return el;
end;

function getRadioSelected(el)
	return GUI.GetRadioSelected(el.ID);
end;

function setReadOnly(el, value)
    GUI.SetProperty(el.ID, PROB_READONLY, value);

	return el;
end;

function getReadOnly(el)
    return GUI.GetProperty(el.ID, PROB_READONLY);
end;

function setBaseProperty(el)

	if el.visible ~= nil then
		el:SetVisible(el.visible);
	end;
	if el.checked ~= nil then
		el:SetChecked(el.checked);
	end;
	if el.radioSelected ~= nil then
		el:SetRadioSelected(el.radioSelected);
	end;

	return el;
end;

--legacy
AddMenuItem = GUI.AddMenuItem;


---@class WindowElement
---@field ID number
---@field visible boolean
---@field [string] any
WindowClass = {};
WindowClass.SetVisible = setVisible;
WindowClass.SetActive = setActiveWindow;

---@param name string
---@param visible boolean
---@param el table
---@return WindowElement
function getWindow(name, visible, el)

	el.name = name;
	el.visible = visible;
	el.ID = GUI.CreateWindow(el.name, el.visible);


    setmetatable(el, { __index = WindowClass });
	
	return el;
	
end;

---@class Element
---@field parent Element|WindowElement
---@field ID number
---@field x number
---@field y number
---@field width number
---@field height number
---@field visible boolean
ElementClass = {};
ElementClass.SetX = setX;
ElementClass.SetY = setY;
ElementClass.SetW = setW;
ElementClass.SetH = setH;
ElementClass.SetXY = setXY;
ElementClass.SetWH = setWH;
ElementClass.SetXYWH = setXYWH;
ElementClass.SetVisible = setVisible;

---@class FrameElement : Element
---@field [string] any
FrameClass = {};
setmetatable(FrameClass, { __index = ElementClass });

---@param parent WindowElement|Element
---@param coors COORS
---@param name string
---@param el table
---@return FrameElement
function getFrame(parent, coors,name,  el)

	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.width = coors.W;
	el.height = coors.H;
	el.name = name;

	el.ID = GUI.CreateFrame(el.parent, el.name, el.x , el.y, el.width, el.height);


    setmetatable(el, { __index = FrameClass });
	
	setBaseProperty(el);
	
	return el;
	
end;

---@class ScrollboxElement : Element
---@field [string] any
ScrollboxClass ={};
setmetatable(ScrollboxClass, { __index = ElementClass });

---@param parent WindowElement|Element
---@param coors COORS
---@param el table
---@return ScrollboxElement
function getScrollbox(parent, coors, el)

	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.width = coors.W;
	el.height = coors.H;

	el.ID = GUI.CreateScrollbox(el.parent, el.x , el.y, el.width, el.height);


    setmetatable(el, { __index = ScrollboxClass });

	setBaseProperty(el);

	return el;
	
end;

--- callback can be either:
---   - a string: evaluated in the global scope on each call
---   - a function: called directly with el.params as parameters
---			- use "__self" for referenc on new created button
---@class ButtonElement : Element
---@field name string
---@field callback string|function|nil
---@field params table
---@field [string] any
ButtonClass = {};
ButtonClass.SetCallback = setCallback;
setmetatable(ButtonClass, { __index = ElementClass });

---@param parent WindowElement|Element
---@param coors COORS
---@param name string
---@param callback string|function|nil
---@param el table
---@return ButtonElement
function getButton(parent,coors,name,callback,el)


	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.width = coors.W;
	el.height = coors.H;
	el.name = name;
	el.callback = callback;

	if el.params then 
		for i, param in ipairs(el.params) do
			if param == "__self" then 
				el.params[i] = el;  -- ✅ Nahrazení za zkutečný object
			end;
		end;
	end;
	el.ID = GUI.CreateButton(el.parent, el.name, el.x , el.y, el.callback, el.width, el.height, table.unpack(el.params or {}));

	el.params = nil;
    setmetatable(el, { __index = ButtonClass });

	setBaseProperty(el);
	
	return el;
end;
	
---@class LabelElement : Element
---@field text string
---@field [string] any
LabelClass = {};
LabelClass.SetText = setText;
setmetatable(LabelClass, { __index = ElementClass });

---@param parent WindowElement|Element
---@param coors COORS
---@param text string
---@param el table
---@return LabelElement
function getLabel(parent, coors, text, el)

	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.width = coors.W;
	el.height = coors.H;
	el.text = text;
	el.ID = GUI.CreateLabel(el.parent, el.text, el.x, el.y, el.width, el.height );

	setmetatable(el, { __index = LabelClass });

	setBaseProperty(el);
	
	return el;
end;

---@class ProgressBarElement : Element
---@field [string] any
ProgressBarClass ={};
ProgressBarClass.SetProgress = setProgress;
setmetatable(ProgressBarClass, { __index = ElementClass });

---@param parent WindowElement|Element
---@param coors COORS
---@param el table
---@return ProgressBarElement
function getProgressBar(parent, coors,  el)

	el.parent = parent.ID
	el.x = coors.X;
	el.y = coors.Y;
	el.width = coors.W;

	el.ID = GUI.CreateProgressBar(el.parent, el.x, el.y, el.width);

	setmetatable(el, { __index = ProgressBarClass });
	setBaseProperty(el);
	return el;
end;

---@class CheckBoxElement : Element
---@field text string
CheckBoxClass = {};
CheckBoxClass.SetChecked = setChecked;
CheckBoxClass.GetChecked = getChecked;
CheckBoxClass.SetText = setText;
setmetatable(CheckBoxClass, { __index = ElementClass });

---
---@param parent WindowElement|Element
---@param coors COORS
---@param text string
---@param el table
---@return CheckBoxElement
function getCheckbox(parent, coors, text, el)

	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.text = text;
	el.ID = GUI.CreateCheckbox(el.parent, el.text, el.x, el.y );

	setmetatable(el, { __index = CheckBoxClass });
	setBaseProperty(el);
	return el;
end;

---@class RadioElement : Element
---@field items table
---@field orientation "vertical"|"horizontal"|nil
RadioClass = {};
RadioClass.SetRadioSelected = setRadioSelected;
RadioClass.GetRadioSelected = getRadioSelected;
setmetatable(RadioClass, { __index = ElementClass });

---
---@param parent Element|WindowElement
---@param coors COORS
---@param items table # items of radio
---@param el table
---@return RadioElement
function getRadio(parent,coors,items,el)

	el.parent = parent.ID;
	el.x = coors.X;
	el.y = coors.Y;
	el.items = items;

	if not el.orientation then
		el.orientation = "vertical";
	end;
	el.ID = GUI.CreateRadio(el.parent, el.items, el.x, el.y, el.orientation);

	setmetatable(el, { __index = RadioClass });

	setBaseProperty(el);

	return el;
end;


---@class FieldElement : Element
---@field secret boolean|nil
---@field [string] any
TextFieldClass = {};
TextFieldClass.SetVisible = setVisible;
TextFieldClass.GetText = getText;
TextFieldClass.SetText = setText;
TextFieldClass.SetReadOnly = setReadOnly;
TextFieldClass.GetReadOnly = getReadOnly;
function TextFieldClass:SetCallbackKeyUp(callback, ...)
	 GUI.SetCallback(self, PROB_CALLBACK_KEYUP,callback, ...);
end;
setmetatable(TextFieldClass, { __index = ElementClass });

--- el.callback_keyup can be either:
---   - use parameters "%k" for key, "%c" for char and "%id" for ID of element 
---   - a string: evaluated in the global scope on each call
---   - a function: called directly with el.params as parameters  
---			- use "__self" for referenc on new created button, 
--- 
---@param parent Element|WindowElement
---@param coors COORS
---@param text string # base text
---@param el table # el.callback_keyup string|function, el.secret boolean
---@return FieldElement
function getTextField(parent, coors, text, el)
    el.parent = parent.ID;
    el.x = coors.X;
    el.y = coors.Y;
    el.text = text or "";
    el.width = coors.W;
	el.height = coors.H;
    el.secret = el.secret or false;
    
    el.ID = GUI.CreateTextField(el.parent, el.text, el.x, el.y, el.width, el.secret);
    
	if el.params then 
		for i, param in ipairs(el.params) do
			if param == "__self" then 
				el.params[i] = el;  -- ✅ Nahrazení za zkutečný object
			end;
		end;
	end;
    -- Nastavení callbacku, pokud existuje
    if el.callback_keyup then
        GUI.SetProperty (el.ID, PROB_CALLBACK_KEYUP, el.callback_keyup, table.unpack(el.params or {}));
    end;
    
	el.params = nil;
    setmetatable(el, { __index = TextFieldClass });
    setBaseProperty(el);
    
    return el;
end;

---@class TextBoxElement : Element
---@field readOnly boolean
---@field [string] any
TextViewClass = {};
TextViewClass.SetVisible = setVisible;
TextViewClass.GetText = getText;
TextViewClass.SetText = setText;
TextViewClass.SetReadOnly = setReadOnly;
TextViewClass.GetReadOnly = getReadOnly;
function TextViewClass:SetCallbackKeyUp(callback, ...)
	GUI.SetCallback(self, PROB_CALLBACK_KEYUP, callback, ...);
end;
setmetatable(TextViewClass, { __index = ElementClass });

--- el.callback_keyup can be either:
---   - use parameters "%k" for key, "%c" for char and "%id" for ID of element 
---   - a string: evaluated in the global scope on each call
---   - a function: called directly with el.params as parameters  
---			- use "__self" for reference on new created element
---@param parent Element|WindowElement
---@param coors COORS
---@param text string # base text
---@param el table # el.callback_keyup string|function, el.readOnly boolean
---@return TextBoxElement
function getTextBox(parent, coors, text, el)
    el.parent = parent.ID;
    el.x = coors.X;
    el.y = coors.Y;
    el.text = text or "";
    el.width = coors.W;
    el.height = coors.H;
    el.readOnly = el.readOnly or false;
    
    el.ID = GUI.CreateTextView(el.parent, el.text, el.x, el.y, el.width, el.height, el.readOnly);
    
    if el.params then 
        for i, param in ipairs(el.params) do
            if param == "__self" then 
                el.params[i] = el;  -- ✅ Nahrazení za skutečný objekt
            end;
        end;
    end;
    
    -- Nastavení callbacku, pokud existuje
    if el.callback_keyup then
        GUI.SetProperty(el.ID, PROB_CALLBACK_KEYUP, el.callback_keyup, table.unpack(el.params or {}));
    end;
    
    el.params = nil;
    setmetatable(el, { __index = TextViewClass });
    setBaseProperty(el);
    
    return el;
end;

--[[  Examle of TextField a TextBox

TestView = getWindow(TID_EXPORTTITLE, true, {});

function TestView.PrintKeys (key,char,id,string)
    print(key,char,id, " ... ", string);
end;

TestView.TextField = getTextField(TestView, XYWH(2,2,20,2),"",{callback_keyup = TestView.PrintKeys, params={"%k","%c","%id", "%k %c %id"}});
TestView.TextBox = getTextBox(TestView,XYWH(2,5,20,5),"",{callback_keyup = "TestView.PrintKeys(%k,'%c',%id,'%k %c %id');"});

-- ready only will protected TextBox to overwrite, by keybord, but still call callback_keyup
TestView.TextBox2 = getTextBox(TestView,XYWH(2,5+6,20,5),"read Only",{readOnly = true, callback_keyup = TestView.PrintKeys, params={"%k","%c","%id", "%k %c %id"}});


GUI.AddMenuItem(TID_MEXPORT, "setActiveWindow(TestView);");


]]--
