----------------------------------------------
--  Name:      	Window with export options
--	Version		3
--  Author:    	Sali
--  Category:  	OW Modul
----------------------------------------------
local fH = #langs;
exSettingWindow = getWindow(TID_EXPORTSETTING, false, {});
exSettingWindow.langCheckboxes = {};
exSettingWindow.gameInitCheckboxes = {};
exSettingWindow.natCheckboxes = {};
exSettingWindow.Scrollbox = getScrollbox(exSettingWindow,XYWH(0,0,0,0),{});

exSettingWindow.Scrollbox.langFrame = getFrame(exSettingWindow.Scrollbox, XYWH(1,1,40,fH+4), TID_CH_LANGUAGES, {});
for i=1, #langs do
	exSettingWindow.langCheckboxes[i] = getCheckbox(exSettingWindow.Scrollbox.langFrame, XYWH(3, i,0, 0), langs[i], {});
end;

exSettingWindow.Scrollbox.gameInitFrame = getFrame(exSettingWindow.Scrollbox, XYWH(41,1,40,fH+4), TID_CH_GAMEINIT, {});
exSettingWindow.gameInitCheckboxes[1] = getCheckbox(exSettingWindow.Scrollbox.gameInitFrame, XYWH(3, 1, 0, 0), "Singleplayer", {});
exSettingWindow.gameInitCheckboxes[2] = getCheckbox(exSettingWindow.Scrollbox.gameInitFrame, XYWH(3, 2, 0, 0), "Multiplayer", {});
exSettingWindow.gameInitCheckboxes[3] = getCheckbox(exSettingWindow.Scrollbox.gameInitFrame, XYWH(3, 3, 0, 0), "Skirmish", {});

exSettingWindow.Scrollbox.natFrame = getFrame(exSettingWindow.Scrollbox, XYWH(41,1,60,fH+4), TID_Nations, {});
for i=1, #natName do
	exSettingWindow.natCheckboxes[i] = getCheckbox(exSettingWindow.Scrollbox.natFrame, XYWH(3, i+1,0, 0), natName[i], {});
end;

exSettingWindow.Scrollbox.natFrame.VideoTypeT = getLabel(exSettingWindow.Scrollbox.natFrame,XYWH(15,0,0,0),TID_VIDEOTYPE,{});
exSettingWindow.natVideoTypeRadio = {};
for i=1, #natName do
	exSettingWindow.natVideoTypeRadio[i] = getRadio(exSettingWindow.Scrollbox.natFrame, XYWH(17, i+1, 0, 0),TID_AMTYPE .. ";" .. TID_RUTYPE, {orientation="horizontal"});
end;

exSettingWindow.Scrollbox.Confirm = getButton(exSettingWindow.Scrollbox,XYWH(30,fH+6,0,0),TID_CONFIRM_LANG,"setVisible(exSettingWindow,false);", {});
exSettingWindow.Scrollbox.Back = getButton(exSettingWindow.Scrollbox,XYWH(1,fH+6,0,0),TID_BACK_EMENU,"setActiveWindow(OWExport);",{});

function GetSelectedLanguages()
	local SL = {};
	for i=1, #exSettingWindow.langCheckboxes do
		if exSettingWindow.langCheckboxes[i]:GetChecked() then
			table.insert(SL,i);
		end;
	end;
	return SL;
end;

function GetSelectedGameInits()
	local SL = {};
	for i=1, #exSettingWindow.gameInitCheckboxes do
		if exSettingWindow.gameInitCheckboxes[i]:GetChecked() then 
			table.insert(SL,i);
		end;
	end;
	return SL;
end;

function GetSelectedCampaigns()
	local SL = {};
	for i=1, #exSettingWindow.natCheckboxes do
		if exSettingWindow.natCheckboxes[i]:GetChecked() then
			table.insert(SL,i);
		end;
	end;
	return SL;
end;

function GetCampTitleTypes()
	local S = {};
	for i=1, #exSettingWindow.natVideoTypeRadio do
		if exSettingWindow.natVideoTypeRadio[i]:GetRadioSelected() == 1 then
			table.insert(S, "AM")
		else
			table.insert(S, "RU");
		end;
	end;
	return S;
end;


function GetSelectedBoth()
	return GetSelectedLanguages(), GetSelectedGameInits();
end;

function ShowSelection(callback, ifLangFrame, ifGameInitFrame, ifNatFrame)
	for i=1, #exSettingWindow.langCheckboxes do
		exSettingWindow.langCheckboxes[i]:SetChecked(true);--setChecked(exSettingWindow.langCheckboxes[i],true)
	end;

	for i=1, #exSettingWindow.gameInitCheckboxes do
		exSettingWindow.gameInitCheckboxes[i]:SetChecked(true);--setChecked(exSettingWindow.gameInitCheckboxes[i],true)
	end;

	for i=1, #exSettingWindow.natCheckboxes do
		if (i == 1) or (i == 3) then
			exSettingWindow.natCheckboxes[i]:SetChecked(true);
		else
			exSettingWindow.natCheckboxes[i]:SetChecked(false);
		end;
	end;

	exSettingWindow.Scrollbox.natFrame.VideoTypeT:SetVisible(false);
	for i=1, #exSettingWindow.natVideoTypeRadio do
		exSettingWindow.natVideoTypeRadio[i]:SetVisible(false);
	end;

	if callback == nil then
		callback = "";
	end;

	if type(callback) == "function" then
		exSettingWindow.Scrollbox.Confirm:SetCallback( function() 
															setVisible(exSettingWindow,false); 
															callback(); 
														end
													);
	elseif type(callback) == "string" then
		exSettingWindow.Scrollbox.Confirm:SetCallback("setVisible(exSettingWindow,false);".. callback);
	else
		error("showSelection - callback must be function or string");
	end;

	exSettingWindow.Scrollbox.langFrame:SetVisible(ifLangFrame);
	exSettingWindow.Scrollbox.gameInitFrame:SetVisible(ifGameInitFrame);
	if ifNatFrame and not (ifNatFrame == 0) then
		exSettingWindow.Scrollbox.natFrame:SetVisible(true);
	else
		exSettingWindow.Scrollbox.natFrame:SetVisible(false);
	end;
	exSettingWindow:SetActive();

end;
--legacy
showSelection = ShowSelection;

function exSettingWindow.AddCampTitleType()

	exSettingWindow.Scrollbox.natFrame.VideoTypeT:SetVisible(true);
	for i=1, #exSettingWindow.natVideoTypeRadio do
		if (i == 1) then
			exSettingWindow.natVideoTypeRadio[i]:SetRadioSelected(1);
		else
			exSettingWindow.natVideoTypeRadio[i]:SetRadioSelected(2);
		end;
		exSettingWindow.natVideoTypeRadio[i]:SetVisible(true);

	end;

end;