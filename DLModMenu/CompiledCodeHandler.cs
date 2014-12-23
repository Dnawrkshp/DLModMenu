using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dhx;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace DLModMenu
{
    public class CodeCompiler
    {
        public ICompiledCode ExecuteCode(string exepath, string code, string namespacename, string classname,
            string functionname, ICompiledCodeHost host, string[] refs, params object[] args)
        {
            if (!Directory.Exists(exepath + "\\OutputCS\\"))
                Directory.CreateDirectory(exepath + "\\OutputCS\\");

            string path = Path.GetTempFileName() + "_" + classname;
            string name = exepath + "\\OutputCS\\" + classname + ".cs";
            File.WriteAllText(name, code);
            Assembly asm = BuildAssembly(code, exepath, refs, path, name);

            foreach (Type pluginType in asm.GetTypes())
            {
                if (pluginType.IsPublic) //Only look at public types
                {
                    if (!pluginType.IsAbstract)  //Only look at non-abstract types
                    {
                        //Gets a type object of the interface we need the plugins to match
                        Type typeInterface = pluginType.GetInterface("dhx.ICompiledCode", true);

                        if (typeInterface != null)
                        {

                            ICompiledCode instance = null;
                            //Type type = asm.GetType(namespacename + "." + classname);
                            //object inst = asm.CreateInstance(namespacename + "." + classname);
                            //instance = (ICompiledCode)asm.CreateInstance(namespacename + "." + classname);
                            instance = (ICompiledCode)Activator.CreateInstance(pluginType);
                            instance.host = host;

                            if (functionname != null && functionname != "")
                            {
                                MethodInfo method = pluginType.GetMethod(functionname);
                                if (method != null)
                                    method.Invoke(instance, args);
                            }

                            return instance;
                        }
                    }

                }

            }

            return null;
        }

        /// <summary>
        /// This private function builds the assembly file into memory after compiling the code
        /// </summary>
        /// <param name="code">C# code to be compiled</param>
        /// <returns>the compiled assembly object</returns>
        private Assembly BuildAssembly(string code, string exepath, string[] refs, string path, string name)
        {
            if (refs == null)
                refs = new string[0];
            Array.Resize(ref refs, refs.Length + 2);
            refs[refs.Length - 2] = exepath + "\\DLModMenu.dll";
            refs[refs.Length - 1] = exepath + "\\CodeCompiler.dll";

            Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerparams = new CompilerParameters(refs, path + ".dll", true);
            compilerparams.GenerateExecutable = true;
            compilerparams.GenerateInMemory = false;
            CompilerResults results = provider.CompileAssemblyFromFile(compilerparams, name); //provider.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                int ind = code.IndexOf("class") + 6;
                string className = ctlMain.NCInterface.sMid(code, ind, code.IndexOf(" : ICompiledCode") - ind);

                StringBuilder errors = new StringBuilder("Compiler Errors (" + className + ") :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }
    }

    public class CompiledCodeHandler : ICompiledCodeHost
    {
        public object FindObj(string name)
        {
            return ctlMain.Instance.FindHUDObjectUControlFromName(name);
        }

        public byte[] GetMem(uint addr, int size)
        {
            return ctlMain.NCInterface.GetMemory((ulong)addr, size);
        }

        public void GetMem(uint addr, ref byte[] bytes)
        {
            ctlMain.NCInterface.GetMemory((ulong)addr, ref bytes);
        }

        public void SetMem(uint addr, byte[] b)
        {
            ctlMain.NCInterface.SetMemory((ulong)addr, b);
        }

        /* Equivalent to VB6's Left function (grabs length many left most characters in text) */
        public string sLeft(string text, int length)
        {
            return ctlMain.NCInterface.sLeft(text, length);
        }

        /* Equivalent to VB6's Right function (grabs length many right most characters in text) */
        public string sRight(string text, int length)
        {
            return ctlMain.NCInterface.sRight(text, length);
        }

        /* Equivalent to VB6's Mid function */
        public string sMid(string text, int off, int length)
        {
            return ctlMain.NCInterface.sMid(text, off, length);
        }

        /* Adds a code to the ConstCodes array */
        public uint ConstCodeAdd(string code, bool state)
        {
            return ctlMain.NCInterface.ConstCodeAdd(code, state);
        }

        /* Removes a code to the ConstCodes array */
        public void ConstCodeRemove(uint ID)
        {
            ctlMain.NCInterface.ConstCodeRemove(ID);
        }

        /* Sets a codes state to state */
        public void ConstCodeSetState(uint ID, bool state)
        {
            ctlMain.NCInterface.ConstCodeSetState(ID, state);
        }

        /* Gets a codes state */
        public bool ConstCodeGetState(uint ID)
        {
            return ctlMain.NCInterface.ConstCodeGetState(ID);
        }

        /* Connects and Attaches to PS3 */
        public bool ConnectAndAttach()
        {
            return ctlMain.NCInterface.ConnectAndAttach();
        }

        /* Returns the state of the main forms connection */
        public int ConnectionState()
        {
            return ctlMain.NCInterface.ConnectionState();
        }


        private enum ButtonBits : byte
        {
            L2 = 0,
            R2 = 1,
            L1 = 2,
            R1 = 3,
            Tri = 4,
            O = 5,
            X = 6,
            Sqr = 7,
            Select = 16,
            L3 = 17,
            R3 = 18,
            Start = 19,
            Up = 20,
            Right = 21,
            Down = 22,
            Left = 23
        }

        public bool AreButtonsOnlyPressed(string[] buttons)
        {
            bool isPressed = AreButtonsOnlyDown(buttons);

            if (isPressed)
                while (AreButtonsOnlyDown(buttons))
                    System.Threading.Thread.Sleep(10);

            return isPressed;
        }

        public bool AreButtonsOnlyDown(string[] buttons)
        {
            byte[] bs = ctlMain.NCInterface.GetMemory(0x00B122F8, 4);
            uint val = (uint)(bs[0] << 24) | (uint)(bs[1] << 16) | (uint)(bs[2] << 8) | (uint)(bs[3] << 0);

            uint newVal = 0;
            foreach (string button in buttons)
            {
                int shift = (int)((byte)Enum.Parse(typeof(ButtonBits), button));
                newVal |= (uint)(1 << shift);
            }

            return newVal == val;
        }

        public bool AreButtonsPressed(string[] buttons)
        {
            bool isPressed = AreButtonsDown(buttons);

            if (isPressed)
                while (AreButtonsDown(buttons))
                    System.Threading.Thread.Sleep(10);

            return isPressed;
        }

        /* 
         * Returns true if the button is being pressed
         * Buttons are defined as:
         * - "X", "O", "Tri", "Sqr"
         * - "L1", "L2", "R1", "R2"
         * - "Up", "Down", "Left", "Right"
         * - "Start", "Select", "L3", "R3"
         */
        public bool AreButtonsDown(string[] buttons)
        {
            if (ConnectionState() < 2)
                ConnectAndAttach();

            byte[] bs = ctlMain.NCInterface.GetMemory(0x00B122F8, 4);
            uint val = (uint)(bs[0] << 24) | (uint)(bs[1] << 16) | (uint)(bs[2] << 8) | (uint)(bs[3] << 0);

            if (val == 0)
                return false;
            else if (val == 0xFFFFFFFF)
                return true;

            foreach (string button in buttons)
            {
                byte bit = (byte)Enum.Parse(typeof(ButtonBits), button);
                if (!isBitSet(val, bit))
                    return false;
            }

            return true;
        }

        private bool isBitSet(uint val, byte bit)
        {
            return ((val << (31 - bit)) >> 31) == 1;
        }

    }
}
