----------------------------------------------
--  Name:        Macro utils
--  Description: Processing Progress Bars and "easy" offset text
--  Category:    Utils
----------------------------------------------

progressClass = {};
function progressClass:Init( title, progres1, progres2, progres3 );
	setText(self.title, title);
	self:SetProces(1,"");
	setVisible(self.processLabels[1], progres1 or false);
	setVisible(self.progressBars[1], progres1 or false);
	setProgress(self.progressBars[1], 0);
	self:SetProces(2,"");
	setVisible(self.processLabels[2], progres2 or false);
	setVisible(self.progressBars[2], progres2 or false);
	setProgress(self.progressBars[2], 0);
	self:SetProces(3,"");
	setVisible(self.processLabels[3], progres3 or false);
	setVisible(self.progressBars[3], progres3 or false);
	setProgress(self.progressBars[3], 0);
	
	setActiveWindow(self.window);
end;

function progressClass:SetTitle( title)
	setText(self.title,title);
end;

function progressClass:SetRange( barID, maxValue)
	if self.MaxValues[barID] then
		self.MaxValues[barID] = maxValue;
	end;
end;

function progressClass:SetProces( barID, text)
	if self.processLabels[barID] then
		setText(self.processLabels[barID], text);
	end;
end;

function progressClass:SetProgres( barID, progress)
	if self.progressBars[barID] and self.MaxValues[barID] and self.MaxValues[barID] ~= 0 then
			
		setProgress(self.progressBars[barID], progress / self.MaxValues[barID]);
	
	end;

end;

function progressClass:SetButtonVissible(bool, cancelBool)
	if cancelBool then
		setCallback(self.cancelButton, "stopRunByID(" .. getCurrentRunID() .. ");");
		setVisible(self.cancelButton, true);
	else
		setVisible(self.cancelButton, false);
		setCallback(self.cancelButton, "");
	end;
	setVisible(self.returnButton,bool);
end;

function newProgress(Window, mainLabel, processLabels, progressBars, returnButton, cancelButton)

	local obj = {}
	setmetatable(obj, { __index = progressClass });

	obj.window = Window;
	obj.title = mainLabel;
	obj.processLabels = processLabels;
	obj.progressBars = progressBars; 
	obj.returnButton = returnButton;
	obj.cancelButton = cancelButton;
	obj.MaxValues = {};  
	for i=1, #progressBars do
		table.insert(obj.MaxValues, 1);
	end;
	
	return obj;
end;


function Tab( x )
	local str = "";
	for i= 1 , x do
		str = str .. " ";
	end;
	return str;
end;


function StripParnt(inputStr)
	if inputStr == "" or inputStr == nil then
		return "";
	end;
	
	local result = "";
	local parntCnt = 0;
	
	for i = 1, #inputStr do
		local char = inputStr:sub(i, i);
		
		if char == "[" then
			parntCnt = parntCnt + 1;
		elseif char == "]" then
			parntCnt = parntCnt - 1;
			if parntCnt < 0 then
				parntCnt = 0;
			end;
		else
			if parntCnt == 0 then
				result = result .. char;
			end;
		end;
	end;
	
	return result;
end;