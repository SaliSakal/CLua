
using System.Runtime.InteropServices;
using System.Text;


namespace Lua
{
    public delegate int LuaFunctionDelegate(IntPtr L);
    public partial class LuaNative : IDisposable
    {
 

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int LuaCFunction(IntPtr L);

        private static readonly LuaCFunction _errorHandlerDelegate = LuaMessageHandler;



        public IntPtr MainState { get; private set; }

        public LuaNative()
        {
            MainState = NewState();
            if (MainState == IntPtr.Zero)
            {
                // Nelze inicializovat LUA!
                throw new Exception("❌ Unable to initialize Lua!");
            }

            OpenLibs(MainState);

            PushLuaLightUserdata(MainState, MainState);
            SetField(MainState, LUA_REGISTRYINDEX, "__mainState");

        }

        public static IntPtr GetMainLuaState(IntPtr L)
        {
            GetField(L, LUA_REGISTRYINDEX, "__mainState");
            IntPtr mainState = ToLuaUserdata(L, -1);
            Pop(L, 2); // pop registry + mainState
            return mainState;
        }

        public class LuaReference
        {
            public enum LuaType { Table, Function }
            public LuaType Type;
            public int Ref;
        }

        public static object LuaValueToObject(IntPtr L, int idx)
        {
            if (IsLuaNumber(L, idx))
            {
                // ⭐ Zkontroluj, jestli je to integer nebo float
                if (IsLuaInteger(L, idx))
                    return ToLuaInteger(L, idx);
                else
                    return ToLuaNumber(L, idx); // double/float
            }
            else if (IsLuaString(L, idx))
                return ToLuaString(L, idx);
            else if (IsLuaBoolean(L, idx))
                return ToLuaBoolean(L, idx);
            else if (IsLuaNil(L, idx))
                return null;
            else if (IsLuaTable(L, idx))
            {
                int tableRef = LuaLRef(L, idx);
                return new LuaReference { Type = LuaReference.LuaType.Table, Ref = tableRef };
            }
            else if (IsLuaFunction(L, idx))
            {
                int funcRef = LuaLRef(L, idx);
                return new LuaReference { Type = LuaReference.LuaType.Function, Ref = funcRef };
            }

            return null; // Fallback pro nepodporované typy // Fallback for unsupported types
        }


        // Spuštění Lua ze souboru lua.
        public void RunScript(string filePath, int envRef = -1)
        {

            // Spouštím LUA skript:
            Console.WriteLine($"📂 Running LUA script: {filePath}");

            int loadResult = luaLoadFile(MainState, filePath, "bt");
            if (loadResult != 0)
            {
                // Chyba při načítání LUA souboru
                PrintLuaError("❌ Error loading LUA file ", -1);
                return;
            }

            if (envRef != -1)
            {
                // 📥 Pushni env tabulku
                // 📥 Push env table
                RawGetI(MainState, envRef);

                // 🌍 Nastav jako _ENV (1. upvalue chunku)
                // 🌍 Set as _ENV (1st upvalue of the chunk)
                SetUpValue(MainState, - 2, 1);
            }

            int baseStackSize = GetTop(MainState);
            int runResult = Docall(MainState, 0,-1); // Použití Docallu místo lua_pcallku // Use Docall instead of lua_pcallk

            if (runResult != 0)
            {
                // Chyba při spuštění LUA skriptu    
                // Error when running LUA script
                if (GetTop(MainState) > baseStackSize)  // únik paměti // memory leak
                {
                    Console.WriteLine("❌ Stack leak during LUA error handling");
                }

                PrintLuaError("❌ Error when running LUA script", -1);
                CleanStackTo(MainState, baseStackSize - 1);
                return;

            }

            if (GetTop(MainState) != (baseStackSize -1))
            {
                // Chyba ve stacku po vykonání LUA kódu
                // Stack error after executing LUA code
                PrintLuaError("❌ Stack error after executing LUA code ", -1);
            }
        }

        public void RunChunk(string code, string chunkName, int envRef = -1)
        {
            int loadResult;

            Console.WriteLine($"📂 Running LUA script: {chunkName}");

            byte[] codeBytes = Encoding.UTF8.GetBytes(code);
            unsafe
            {
                fixed (byte* p = codeBytes)
                {
                    loadResult = LoadBuffer(MainState, (IntPtr)p, (UIntPtr)codeBytes.Length, "@" + chunkName, null);
                }
            }


            if (loadResult != 0)
            {
                PrintLuaError("❌ Error loading LUA chunk " + chunkName, -1);
                return;
            }

            if (envRef != -1)
            {
                // 📥 Pushni referencovaný env zpět na stack
                // 📥 Push referenced env back to stack
                RawGetI(MainState, envRef);

                // 🌍 Nastav jako _ENV (1. upvalue)
                // 🌍 Set as _ENV (1st upvalue)
                SetUpValue(MainState, -2, 1);
            }

            int baseStackSize = GetTop(MainState);
            int runResult = Docall(MainState, 0, -1);
            if (runResult != 0)
            {
                // Chyba při spuštění LUA skriptu    
                // Error when running LUA chunk
                if (GetTop(MainState) > baseStackSize)  // unik paměti // memory leak
                {
                    Console.WriteLine("❌ Stack leak during LUA error handling");
                }

                PrintLuaError("❌ Error when running LUA chunk", -1);
                CleanStackTo(MainState, baseStackSize - 1);
                return;
            }

            if (GetTop(MainState) != (baseStackSize -1))
            {
                // Chyba ve stacku po vykonání LUA kódu
                // Stack error after executing LUA chunk
                PrintLuaError("❌ Stack error after executing LUA code", -1);
            }
        }

        public void PrintLuaError(string message, int index)
        {
            PrintLuaError(MainState, message, index);
        }
        // Přidání funkce pro výpis detailních chyb
        // Detailed error printing function
        public static void PrintLuaError(IntPtr L, string message, int index)
        {
            if (L == IntPtr.Zero)
            {
                // Lua state je vadný
                Console.WriteLine($"❌ {message}: Lua state is faulty.");
                return;
            }

            int stackSize = GetTop(L);
            if (stackSize < Math.Abs(index))
            {
                // (Lua stack je prázdný, žádná chyba)
                // (Lua stack is empty, no error)
                Console.WriteLine($"❌ {message}: (Lua stack is empty, no error)");
                return;
            }

            string errorMsg = ToLuaString(L, index);
            if (errorMsg != "")
            {
                Console.WriteLine($"{message}: {errorMsg}");
            }
            else
            {
                // (žádná chybová zpráva v Lua stacku)
                // (no error message in Lua stack)
                Console.WriteLine($"❌ {message}: (no error message in Lua stack)");
            }
        }

        // pcall s přesnějším chybovým hlášením  -- Docall volat jen do MainStatu
        // pcall with more precise error reporting -- Docall to be called only on MainState
        public static int Docall(IntPtr L, int narg, int nres)
        {
            int baseIndex = GetTop(L) - narg; // Index funkce / function index

            // Použij naši statickou proměnnou, aby GC nemohl delegate odstranit
            // Use our static variable so GC cannot remove the delegate
            IntPtr errorFuncPtr = Marshal.GetFunctionPointerForDelegate(_errorHandlerDelegate);

            PushLuaCClosure(L, errorFuncPtr, 0); // Handler pro chybové hlášení // Error reporting handler
            LuaRotate(L, baseIndex, 1);

            int status = PCall(L, narg, nres, baseIndex, 0, IntPtr.Zero);

            LuaRemove(L, baseIndex); // Odstranění handleru // Remove the handler
            return status;
        }

        private static int LuaMessageHandler(IntPtr L)
        {
            string msg = ToLuaString(L, 1);

            if (msg == "")
            {
                if (LuaCallMeta(L, -1, "__tostring") != 0 && LuaType(L, -1) == LUA_TSTRING) // 4 = LUA_TSTRING
                {
                    return 1;
                }
                else
                {
                    msg = $"(error object is a {LuaTypeName(L, 1)} value)";
                    PushLuaString(L, msg);
                }
            }

            LuaTraceback(L, L, msg, 1);
            return 1;
        }





        public void RunString(string script)
        {
            if (string.IsNullOrEmpty(script))
                return;

            // Převod řetězce na UTF8 bajty
            // Console.WriteLine($"[DEBUG] Odesílám do Lua: {script}");
            byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
            int loadResult = 0;

            unsafe
            {
                fixed (byte* pScript = scriptBytes)
                {
                    // Předáváme pointer, délku a chunk name
                    // Passing pointer, length and chunk name
                    loadResult = LoadBuffer(MainState, (IntPtr)pScript, (UIntPtr)scriptBytes.Length, new string(script.Take(20).ToArray()), null);
                }
            }

            if (loadResult != 0)
            {
                // Chyba při načítání LUA kódu
                // Error loading LUA code
                PrintLuaError("❌ Error loading LUA code", -1);
                return;
            }

            int baseStackSize = GetTop(MainState);
            int runResult = Docall(MainState, 0, -1); // Spuštění Lua kódu přes náš handler // Running Lua code via our handler

            if (runResult != 0)
            {
                // Chyba při spuštění LUA kódu
                // Error when running LUA code
                PrintLuaError("❌ Error when running LUA code", -1);
            }

            if (GetTop(MainState) > baseStackSize)
            {
                // Chyba ve stacku po vykonání LUA kódu
                // Stack error after executing LUA code
                PrintLuaError("❌ Stack error after executing LUA code", -1);
            }

            return;
        }





        public void RegisterFunction(string name, LuaFunctionDelegate function)
        {
            IntPtr fnPtr = Marshal.GetFunctionPointerForDelegate(function);
            PushLuaCClosure(MainState, fnPtr, 0); // Použití správné metody Lua 5.4
            SetGlobal(MainState, name);

            //Console.WriteLine($"📑 LUA Funkce {name} zaregistrována");
        }

        
        public void RegisterGlobalConstant(string name, object value)
        {
            if (value is int)
                PushLuaInteger(MainState, (int)value);
            else if (value is float || value is double)
                PushLuaNumber(MainState, Convert.ToDouble(value));
            else if (value is string)
                PushLuaString(MainState, (string)value);
            else
            {
                //  Chyba: Nelze uložit hodnotu typu {value.GetType()} do globální konstanty {name}!
                //  Error: Cannot store value of type {value.GetType()} in global constant {name}!
                Console.WriteLine($"❌Error: Cannot store value of type {value.GetType()} in global constant {name}!");
                return;
            }

            SetGlobal(MainState, name); // Nastaví hodnotu jako globální v Lua
        }



        public void DumpStack(IntPtr luaState, string label = "")
        {
            int top = GetTop(luaState);
            Console.WriteLine($"[Stack {label}] top = {top}");
            if (top <= 0)
            {
                Console.WriteLine("  (empty)");
                return;  // ⭐ Nečti indexy, když je top 0
            }

            for (int i = 1; i <= top; i++)
            {
                string type = LuaTypeName(luaState, i);
                Console.WriteLine($"  [{i}] = {type}");
            }
        }

 


        /// <summary>
        ///  Vymazaní z paměti
        /// </summary>
        public void Dispose()
        {
            if (MainState != IntPtr.Zero)
            {
                LuaClose(MainState);
                MainState = IntPtr.Zero;
            }
        }



    }
}
