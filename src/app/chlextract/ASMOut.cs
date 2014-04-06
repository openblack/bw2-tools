using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptReader
{
    public static class ASMOut
    {
        private static ChallengeFile CHLFile;
        private static TextWriter Output;
        private static Dictionary<int, int> Labels = new Dictionary<int, int>(); // <address, label>

        public static void Convert(ChallengeFile chlFile, TextWriter output)
        {
            CHLFile = chlFile;
            Output = output;

            OutputHeader();
            OutputGlobals();
            Output.WriteLine();

            Array.ForEach(CHLFile.FunctionTable, x => { ParseFunction(x); Output.WriteLine(); });
        }

        private static void ParseInstructions(ChallengeFile.Function func)
        {
            var instructions = new List<ChallengeFile.Instruction>();
            var instruction = func.InstructionAddress;

            while (true)
            {
                var inst = CHLFile.InstructionTable[instruction];

                if (inst.Type == 0)
                    break;
                instructions.Add(inst);

                instruction++;
            }

            /* let's find out where we need labels too first. */

            int _curLabel = 0;

            instructions.ForEach(i =>
            {
                if (i.Type != 1 && i.Type != 22 && i.Type != 24)
                    return;

                Int32 address = i.Parameter.ReadInt32();
                Labels[address] = _curLabel++;
            });

            instructions.ForEach(i =>
            {
                if (Labels.ContainsKey(i.Address))
                    Output.WriteLine("LABEL_" + Labels[i.Address]);

                i.Parameter.BaseStream.Seek(0, SeekOrigin.Begin);

                switch (i.Type)
                {
                    case 0: // ret
                        Output.WriteLine("\tret");
                        break;
                    case 1: // jmpc
                        Output.WriteLine("\tjmpc\t{0}", "LABEL_" + Labels[i.Parameter.ReadInt32()]);
                        break;
                    case 2: // push*
                        switch (i.DataType)
                        {
                            case 1:
                                Output.WriteLine("\tpushi\t{0}", i.Parameter.ReadInt32());
                                break;
                            case 2:
                                Output.WriteLine("\tpushf\t{0}", i.Parameter.ReadSingle());
                                break;
                            case 3:
                                Output.WriteLine("\tpushv3d");
                                break;
                            case 6:
                                Output.WriteLine("\tpushb\t{0}", (i.Parameter.ReadInt32() == 1).ToString());
                                break;
                            case 7:
                                Output.WriteLine("\tpushv\t[{0}]", i.Parameter.ReadSingle());
                                break;
                            default:
                                Output.WriteLine("\tpush?\t{0}", i.Parameter.ReadInt32());
                                break;
                        }
                        break;
                    case 3:
                        Output.WriteLine("\tpop\t{0}", (i.SubType == 2) ? "[" + i.Parameter.ReadInt32() + "]" : "");
                        break;
                    case 5:
                        Output.WriteLine("\tengcall\t{0}", i.Parameter.ReadInt32());
                        break;
                    case 4:
                        Output.WriteLine("\t{0}", (i.DataType == 3) ? "v3dadd" : "add");
                        break;
                    case 6:
                        Output.WriteLine("\t{0}", (i.DataType == 3) ? "v3dsub" : "sub");
                        break;
                    case 7:
                        Output.WriteLine("\t{0}", (i.DataType == 3) ? "v3dneg" : "neg");
                        break;
                    case 8:
                        Output.WriteLine("\t{0}", (i.DataType == 3) ? "v3dsmul" : "mul");
                        break;
                    case 9:
                        Output.WriteLine("\t{0}", (i.DataType == 3) ? "v3dsdiv" : "div");
                        break;
                    case 10:
                        Output.WriteLine("\tpow");
                        break;
                    case 11:
                        Output.WriteLine("\tand");
                        break;
                    case 12:
                        Output.WriteLine("\tmod");
                        break;
                    case 13:
                        Output.WriteLine("\tnot");
                        break;
                    case 14:
                        Output.WriteLine("\tandb");
                        break;
                    case 15:
                        Output.WriteLine("\torb");
                        break;
                    case 16:
                        Output.WriteLine("\tcmpe");
                        break;
                    case 17:
                        Output.WriteLine("\tcmpne");
                        break;
                    case 18:
                       Output.WriteLine("\tcmpge");
                        break;
                    case 19:
                        Output.WriteLine("\tcmple");
                        break;
                    case 20:
                        Output.WriteLine("\tcmpg");
                        break;
                    case 21:
                        Output.WriteLine("\tcmpl");
                        break;
                    case 22:
                        Output.WriteLine("\tjmp\t{0}", "LABEL_" + Labels[i.Parameter.ReadInt32()]);
                        break;
                    case 23:
                        Output.WriteLine("\twait");
                        break;
                    case 24:
                        Output.WriteLine("\tregexhnd\t{0}", "LABEL_" + Labels[i.Parameter.ReadInt32()]);
                        break;
                    case 25:
                        Output.WriteLine("\tv3dpush\t{0}", i.Parameter.ReadInt32());
                        break;
                    case 26:
                        Output.WriteLine("\tcall\t{0}\t; this is a scriptid", i.Parameter.ReadInt32());
                        break;
                    case 27:
                        Output.WriteLine("\t{0}stack", ((i.SubType == 1) ? "rcl" : "sav"));
                        break;
                    case 34:
                        Output.WriteLine("\ttarget");
                        break;
                    case 35:
                        Output.WriteLine("\tstore");
                        break;
                    case 36:
                        Output.WriteLine("\tload");
                        break;
                    case 38:
                        Output.WriteLine("\ttan");
                        break;
                    case 39:
                        Output.WriteLine("\tsin");
                        break;
                    case 40:
                        Output.WriteLine("\tcos");
                        break;
                    default:
                        Output.WriteLine("\t; Unknown: {0}, {1}", i.Type, i.Parameter.ReadInt32());
                        break;
                }
            });
        }

        private static void ParseFunction(ChallengeFile.Function func)
        {
            Output.WriteLine("function {0}", func.Name);

            // Locals
            Output.WriteLine("locals");
            Array.ForEach(func.LocalVariables, x => Output.WriteLine("\t{0}", x));
            Output.WriteLine("endl");

            // Instructions
            Output.WriteLine("begin");

            ParseInstructions(func);

            Output.WriteLine("end");
        }

        private static void OutputGlobals()
        {
            Output.WriteLine("globals");
            Array.ForEach(CHLFile.GlobalTable, x => Output.WriteLine("\t{0}", x));
            Output.WriteLine("endg");
        }

        private static void OutputHeader()
        {
            Output.Write("; Generated using CHLEX" + Environment.NewLine + Environment.NewLine);
        }
    }
}