using static CLua.LuaManager;

namespace CLua
{
     public partial class CLua
    {

        public static void RegisterLuaGUIFunctions(LuaManager Instance)
        {
            //  Registrace funkcí...
            Console.WriteLine("🔗 Registering functions...");

            Instance.RegisterTable("GUI", new LuaTableEntry[]
            {
                new LuaFunctionEntry { Name = "CreateWindow",   Function = GuiLuaBridge.CreateWindow },
                new LuaFunctionEntry { Name = "CreateFrame",   Function = GuiLuaBridge.CreateFrame },
                new LuaFunctionEntry { Name = "CreateButton",   Function = GuiLuaBridge.CreateButton },
                new LuaFunctionEntry { Name = "CreateLabel",   Function = GuiLuaBridge.CreateLabel },
                new LuaFunctionEntry { Name = "CreateProgressBar",   Function = GuiLuaBridge.CreateProgressBar },
                new LuaFunctionEntry { Name = "CreateCheckbox",   Function = GuiLuaBridge.CreateCheckbox },
                new LuaFunctionEntry { Name = "CreateScrollbox",   Function = GuiLuaBridge.CreateScrollbox },
                new LuaFunctionEntry { Name = "CreateRadio",   Function = GuiLuaBridge.CreateRadio },
                new LuaFunctionEntry { Name = "CreateTextField",   Function = GuiLuaBridge.CreateTextField },
                new LuaFunctionEntry { Name = "CreateTextView",   Function = GuiLuaBridge.CreateTextView },

                new LuaFunctionEntry { Name = "SetProperty",   Function = GuiLuaBridge.SetProperty },

                new LuaFunctionEntry { Name = "SetActiveWindow",   Function = GuiLuaBridge.SetActiveWindow },
                new LuaFunctionEntry { Name = "UpdateMenuItem",   Function = GuiLuaBridge.UpdateMenuItem },
                new LuaFunctionEntry { Name = "AddMenuItem",   Function = GuiLuaBridge.AddMenuItem },
                new LuaFunctionEntry { Name = "IsCheckboxChecked",   Function = GuiLuaBridge.IsCheckboxChecked },
                new LuaFunctionEntry { Name = "GetRadioSelected",   Function = GuiLuaBridge.GetRadioSelected },
                new LuaFunctionEntry { Name = "SetRadioSelected",   Function = GuiLuaBridge.SetRadioSelected },
            

                new LuaFunctionEntry { Name = "DestroyElement",   Function = GuiLuaBridge.DestroyElement },

                new LuaFunctionEntry { Name = "DestroyAllElements",   Function = GuiLuaBridge.DestroyAllElements },
                new LuaFunctionEntry { Name = "RemoveMenuItem",   Function = GuiLuaBridge.RemoveMenuItem },
                new LuaFunctionEntry { Name = "DisableMenu",   Function = GuiLuaBridge.DisableMenu },
                new LuaFunctionEntry { Name = "reset",   Function = GuiLuaBridge.ResetSGUI },

                new LuaValueEntry { Name = "Ver",  Value = "TGUI 1.2" },

            });

            Instance.RegisterTable("XLSX", new LuaTableEntry[]
            {
                // Workbook-level functions
                new LuaFunctionEntry { Name = "Load",   Function = ExcelManager.LoadXLSX },
                new LuaFunctionEntry { Name = "Save",   Function = ExcelManager.SaveXLSX },
                new LuaFunctionEntry { Name = "SaveAs", Function = ExcelManager.SaveAsXLSX},
                new LuaFunctionEntry { Name = "WorkbookExists", Function = ExcelManager.TableExists },
                /////////////////////////////            (NOVÉ)
                new LuaFunctionEntry { Name = "New", Function = ExcelManager.NewXLSX },
                new LuaFunctionEntry { Name = "AddSheet", Function = ExcelManager.AddSheet },
                new LuaFunctionEntry { Name = "DeleteSheet", Function = ExcelManager.DeleteSheet },
                new LuaFunctionEntry { Name = "RenameSheet", Function = ExcelManager.RenameSheet },

                // Sheet-level functions
                new LuaFunctionEntry { Name = "GetSheetNames",   Function = ExcelManager.GetSheetNames },
                new LuaFunctionEntry { Name = "ContainSheet",    Function = ExcelManager.ContainSheet },
                new LuaFunctionEntry { Name = "GetSheet",    Function = ExcelManager.GetSheetObject },

                new LuaFunctionEntry { Name = "CloseWorkbook",   Function = ExcelManager.CloseWorkbook },
                new LuaFunctionEntry { Name = "CloseAllWorkbooks",   Function = ExcelManager.CloseAllWorkbooks },

                // Row formatting
                new LuaFunctionEntry { Name = "SetRowBackgroundColor", Function = ExcelManager.SetRowBackgroundColor },
                new LuaFunctionEntry { Name = "SetRowBackgroundColorRGB", Function = ExcelManager.SetRowBackgroundColorRGB },
                new LuaFunctionEntry { Name = "SetRowBold", Function = ExcelManager.SetRowBold },
                new LuaFunctionEntry { Name = "SetRowFontColor", Function = ExcelManager.SetRowFontColor },
                new LuaFunctionEntry { Name = "SetRowFontColorRGB", Function = ExcelManager.SetRowFontColorRGB },

                // Column formatting
                new LuaFunctionEntry { Name = "SetColumnBackgroundColor", Function = ExcelManager.SetColumnBackgroundColor },
                new LuaFunctionEntry { Name = "SetColumnBackgroundColorRGB", Function = ExcelManager.SetColumnBackgroundColorRGB },
                new LuaFunctionEntry { Name = "SetColumnBold", Function = ExcelManager.SetColumnBold },
                new LuaFunctionEntry { Name = "SetColumnFontColor", Function = ExcelManager.SetColumnFontColor },
                new LuaFunctionEntry { Name = "SetColumnFontColorRGB", Function = ExcelManager.SetColumnFontColorRGB },

                // FreezePanel
                new LuaFunctionEntry { Name = "FreezePanes", Function = ExcelManager.FreezePanes },
                new LuaFunctionEntry { Name = "FreezeRows", Function = ExcelManager.FreezeRows },
                new LuaFunctionEntry { Name = "FreezeColumns", Function = ExcelManager.FreezeColumns },
                new LuaFunctionEntry { Name = "UnfreezePanes", Function = ExcelManager.UnfreezePanes },
                /////////////////////////////            (NOVÉ)
                // Range Operations 
                new LuaFunctionEntry { Name = "SetRangeBackgroundColor", Function = ExcelManager.SetRangeBackgroundColor },
                new LuaFunctionEntry { Name = "SetRangeBackgroundColorRGB", Function = ExcelManager.SetRangeBackgroundColorRGB },
                new LuaFunctionEntry { Name = "SetRangeBold", Function = ExcelManager.SetRangeBold },
                new LuaFunctionEntry { Name = "SetRangeBorder", Function = ExcelManager.SetRangeBorder },
                new LuaFunctionEntry { Name = "MergeRange", Function = ExcelManager.MergeRange },

                // Column/Row Operations 
                new LuaFunctionEntry { Name = "SetColumnWidth", Function = ExcelManager.SetColumnWidth },
                new LuaFunctionEntry { Name = "AutoFitColumn", Function = ExcelManager.AutoFitColumn },
                new LuaFunctionEntry { Name = "SetRowHeight", Function = ExcelManager.SetRowHeight },
                new LuaFunctionEntry { Name = "DeleteRow", Function = ExcelManager.DeleteRow },
                new LuaFunctionEntry { Name = "DeleteColumn", Function = ExcelManager.DeleteColumn },
                new LuaFunctionEntry { Name = "InsertRowAbove", Function = ExcelManager.InsertRowAbove },
                new LuaFunctionEntry { Name = "InsertColumnBefore", Function = ExcelManager.InsertColumnBefore },

                // Get Dimensions
                new LuaFunctionEntry { Name = "GetColumnWidth", Function = ExcelManager.GetColumnWidth },
                new LuaFunctionEntry { Name = "GetRowHeight", Function = ExcelManager.GetRowHeight },

                new LuaFunctionEntry { Name = "GetSheetRange",   Function = ExcelManager.GetSheetRange },

                new LuaFunctionEntry { Name = "FindInSheet",   Function = ExcelManager.FindInSheet },
                new LuaFunctionEntry { Name = "FindAllInSheet",   Function = ExcelManager.FindAllInSheet },
                new LuaFunctionEntry { Name = "FindInRow",   Function = ExcelManager.FindInRow },
                new LuaFunctionEntry { Name = "FindInColumn",   Function = ExcelManager.FindInColumn },
                new LuaFunctionEntry { Name = "FindAllInRow",   Function = ExcelManager.FindAllInRow },
                new LuaFunctionEntry { Name = "FindAllInColumn",   Function = ExcelManager.FindAllInColumn },
                new LuaFunctionEntry { Name = "FindInRange",   Function = ExcelManager.FindInRange },
                new LuaFunctionEntry { Name = "FindAllInRange",   Function = ExcelManager.FindAllInRange },
                // Cell-level functions
                // Basic Set/Get Cell
                new LuaFunctionEntry { Name = "GetCellEx",   Function = ExcelManager.GetCellEx },
                new LuaFunctionEntry { Name = "SetCell",   Function = ExcelManager.SetCell },
                new LuaFunctionEntry { Name = "GetCell",   Function = ExcelManager.GetCell },

                /////////////////////////////            (NOVÉ)
                // Typované hodnoty 
                // Set/Get Cell Value
                new LuaFunctionEntry { Name = "SetCellValue", Function = ExcelManager.SetCellValue },
                new LuaFunctionEntry { Name = "GetCellValue", Function = ExcelManager.GetCellValue },

                // Background Colors 
                // Set Cell Background Color
                new LuaFunctionEntry { Name = "SetCellBackgroundColor", Function = ExcelManager.SetCellBackgroundColor },
                new LuaFunctionEntry { Name = "SetCellBackgroundColorRGB", Function = ExcelManager.SetCellBackgroundColorRGB },
                new LuaFunctionEntry { Name = "SetCellBackgroundColorTheme", Function = ExcelManager.SetCellBackgroundColorTheme },
                new LuaFunctionEntry { Name = "SetCellBackgroundColorIndexed", Function = ExcelManager.SetCellBackgroundColorIndexed },

                // Font Colors
                new LuaFunctionEntry { Name = "SetCellFontColor", Function = ExcelManager.SetCellFontColor },
                new LuaFunctionEntry { Name = "SetCellFontColorRGB", Function = ExcelManager.SetCellFontColorRGB },
                new LuaFunctionEntry { Name = "SetCellFontColorTheme", Function = ExcelManager.SetCellFontColorTheme },

                // Font Formatting 
                new LuaFunctionEntry { Name = "SetCellBold", Function = ExcelManager.SetCellBold },
                new LuaFunctionEntry { Name = "SetCellItalic", Function = ExcelManager.SetCellItalic },
                new LuaFunctionEntry { Name = "SetCellUnderline", Function = ExcelManager.SetCellUnderline },
                new LuaFunctionEntry { Name = "SetCellStrikethrough", Function = ExcelManager.SetCellStrikethrough },
                new LuaFunctionEntry { Name = "SetCellFontSize", Function = ExcelManager.SetCellFontSize },
                new LuaFunctionEntry { Name = "SetCellFontName", Function = ExcelManager.SetCellFontName },

                // Alignment 
                new LuaFunctionEntry { Name = "SetCellAlignment", Function = ExcelManager.SetCellAlignment },
                new LuaFunctionEntry { Name = "SetCellWrapText", Function = ExcelManager.SetCellWrapText },

                // Borders 
                new LuaFunctionEntry { Name = "SetCellBorder", Function = ExcelManager.SetCellBorder },
                new LuaFunctionEntry { Name = "SetCellBorderOutside", Function = ExcelManager.SetCellBorderOutside },
                new LuaFunctionEntry { Name = "SetCellBorderTop", Function = ExcelManager.SetCellBorderTop },
                new LuaFunctionEntry { Name = "SetCellBorderBottom", Function = ExcelManager.SetCellBorderBottom },
                new LuaFunctionEntry { Name = "SetCellBorderLeft", Function = ExcelManager.SetCellBorderLeft },
                new LuaFunctionEntry { Name = "SetCellBorderRight", Function = ExcelManager.SetCellBorderRight },

                // Number Format
                new LuaFunctionEntry { Name = "SetCellNumberFormat", Function = ExcelManager.SetCellNumberFormat },



                // Clear Functions
                new LuaFunctionEntry { Name = "ClearCell", Function = ExcelManager.ClearCell },
                new LuaFunctionEntry { Name = "ClearCellContents", Function = ExcelManager.ClearCellContents },
                new LuaFunctionEntry { Name = "ClearCellFormats", Function = ExcelManager.ClearCellFormats },


                // Formula
                new LuaFunctionEntry { Name = "SetCellFormula", Function = ExcelManager.SetCellFormula },
                new LuaFunctionEntry { Name = "GetCellFormula", Function = ExcelManager.GetCellFormula },
                new LuaFunctionEntry { Name = "HasCellFormula", Function = ExcelManager.HasCellFormula },

                // Get Colors
                new LuaFunctionEntry { Name = "GetCellBackgroundColor", Function = ExcelManager.GetCellBackgroundColor },
                new LuaFunctionEntry { Name = "GetCellFontColor", Function = ExcelManager.GetCellFontColor },

                // Get Font
                new LuaFunctionEntry { Name = "GetCellBold", Function = ExcelManager.GetCellBold },
                new LuaFunctionEntry { Name = "GetCellItalic", Function = ExcelManager.GetCellItalic },
                new LuaFunctionEntry { Name = "GetCellUnderline", Function = ExcelManager.GetCellUnderline },
                new LuaFunctionEntry { Name = "GetCellStrikethrough", Function = ExcelManager.GetCellStrikethrough },
                new LuaFunctionEntry { Name = "GetCellFontSize", Function = ExcelManager.GetCellFontSize },
                new LuaFunctionEntry { Name = "GetCellFontName", Function = ExcelManager.GetCellFontName },

                // Get Alignment
                new LuaFunctionEntry { Name = "GetCellHorizontalAlignment", Function = ExcelManager.GetCellHorizontalAlignment },
                new LuaFunctionEntry { Name = "GetCellVerticalAlignment", Function = ExcelManager.GetCellVerticalAlignment },
                new LuaFunctionEntry { Name = "GetCellWrapText", Function = ExcelManager.GetCellWrapText },

                // Get Borders
                new LuaFunctionEntry { Name = "GetCellBorderTop", Function = ExcelManager.GetCellBorderTop },
                new LuaFunctionEntry { Name = "GetCellBorderBottom", Function = ExcelManager.GetCellBorderBottom },
                new LuaFunctionEntry { Name = "GetCellBorderLeft", Function = ExcelManager.GetCellBorderLeft },
                new LuaFunctionEntry { Name = "GetCellBorderRight", Function = ExcelManager.GetCellBorderRight },

                // Get Number Format
                new LuaFunctionEntry { Name = "GetCellNumberFormat", Function = ExcelManager.GetCellNumberFormat },



                // Get Merged
                new LuaFunctionEntry { Name = "IsCellMerged", Function = ExcelManager.IsCellMerged },
                new LuaFunctionEntry { Name = "GetCellMergedRange", Function = ExcelManager.GetCellMergedRange },



                // Recalculate formulas
                new LuaFunctionEntry { Name = "CalculateCell", Function = ExcelManager.CalculateCell },
                new LuaFunctionEntry { Name = "RecalculateSheet", Function = ExcelManager.RecalculateSheet },
                new LuaFunctionEntry { Name = "Recalculate", Function = ExcelManager.RecalculateWorkbook },


                new LuaValueEntry { Name = "Ver",  Value = "1.3" },

            } );



            //  FileManager
            Instance.RegisterNewFunction("SaveFile", FileManager.SaveFile);
            Instance.RegisterNewFunction("LoadFile", FileManager.LoadFile);

            Instance.RegisterNewFunction("FileExists", FileManager.FileExists);
            Instance.RegisterNewFunction("GetFiles", FileManager.GetFilesLua);

            Instance.RegisterNewFunction("GetDirs", FileManager.GetDirsLua);

        }

        public static void RegisterLuaGUIConstants(LuaManager Instance)
        {


            //Console.WriteLine("🔗 Registering constants...");

            Instance.RegisterNewConstant("LANG", ConfigManager.LoadSetting("Language", "English"));

            Instance.RegisterNewConstant("PROB_TEXT", "Text");
            Instance.RegisterNewConstant("PROB_VISIBLE", "Visible");
            Instance.RegisterNewConstant("PROB_PROGRESS", "Progress");
            Instance.RegisterNewConstant("PROB_CHECKED", "Checked");
            Instance.RegisterNewConstant("PROB_CALLBACK", "Clicked");
            Instance.RegisterNewConstant("PROB_CALLBACK_KEYUP", "OnUpKey"); 
            Instance.RegisterNewConstant("PROB_READONLY", "ReadOnly");
            Instance.RegisterNewConstant("PROB_SELECTED", "Selected");
            Instance.RegisterNewConstant("PROB_X", "X");
            Instance.RegisterNewConstant("PROB_Y", "Y");
            Instance.RegisterNewConstant("PROB_W", "Width");
            Instance.RegisterNewConstant("PROB_H", "Height");
            Instance.RegisterNewConstant("PROB_XY", "XY");
            Instance.RegisterNewConstant("PROB_WH", "WH");
            Instance.RegisterNewConstant("PROB_XYWH", "XYWH");

            //jiné konstatny
            Instance.RegisterNewConstant("VERSION", CLua_ver);

        }
    }
}
