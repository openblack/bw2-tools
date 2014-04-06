using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScriptReader
{
    public static class Helper
    {
        public static string ReadNulledString(Stream input)
        {
            StringBuilder builder = new StringBuilder();

            while (true)
            {
                var next = input.ReadByte();
                if (next == 0)
                    break;

                builder.Append((char)next);
            }

            return builder.ToString();
        }
    }

    public class ChallengeFile
    {
        #region Public Struct

        public struct Instruction
        {
            public Int32 Address;
            public Int32 Type;
            public Int32 SubType;
            public Int32 DataType;
            public BinaryReader Parameter;
            public Int32 LineNumber;
        }

        public struct Function
        {
            public String Name;
            public String SourceFile;
            public String[] LocalVariables;
            public Int32 InstructionAddress;
            public Int32 ScriptID;

            public Int32 Unknown1;
            public Int32 VariableOffset;
            public Int32 NumParameters;
        }

        public struct SavedGlobal
        {
            public String Name;
            public int Unknown;
            public float Value;
        }

        #endregion

        #region Public Fields

        public string[] GlobalTable;
        public Instruction[] InstructionTable;
        public Function[] FunctionTable;
        public byte[] DataTable;
        public SavedGlobal[] SavedGlobalTable;

        #endregion

        public static ChallengeFile LoadFromStream(Stream input)
        {
            ChallengeFile chlFile = new ChallengeFile();
            BinaryReader reader = new BinaryReader(input);

            #region Header

            if (reader.ReadInt32() != 0x4D56484C) // LHVM
                throw new Exception("Invalid LHVM file");

            if (reader.ReadInt32() != 12) // this is new in bw2 from bw1
                throw new Exception("twelve");

            #endregion

            #region Global Section

            chlFile.GlobalTable = new string[reader.ReadInt32()];

            Console.Write("Reading {0} globals... ", chlFile.GlobalTable.Length);

            for (int i = 0; i < chlFile.GlobalTable.Length; i++)
                chlFile.GlobalTable[i] = Helper.ReadNulledString(reader.BaseStream);

            Console.WriteLine("Done.");

            #endregion

            #region Instructions Section

            chlFile.InstructionTable = new Instruction[reader.ReadInt32()];
            Console.Write("Reading {0} instructions... ", chlFile.InstructionTable.Length);

            for (int i = 0; i < chlFile.InstructionTable.Length; i++)
            {
                var instruction = new Instruction();
                instruction.Address = i;
                instruction.Type = reader.ReadInt32();
                instruction.SubType = reader.ReadInt32();
                instruction.DataType = reader.ReadInt32();
                instruction.Parameter = new BinaryReader(new MemoryStream(reader.ReadBytes(4)));
                instruction.LineNumber = reader.ReadInt32();

                chlFile.InstructionTable[i] = instruction;
            }
            Console.WriteLine("Done.");

            #endregion

            #region Unknown Section

            // unknown stuff, 1 lots of 1??
            var unknown = reader.ReadInt32();
            for (int i = 0; i < unknown; i++)
                reader.ReadInt32();

            #endregion

            #region Functions Section

            chlFile.FunctionTable = new Function[reader.ReadInt32()];
            Console.Write("Reading {0} functions... ", chlFile.FunctionTable.Length);

            for (int i = 0; i < chlFile.FunctionTable.Length; i++)
            {
                var functionName = Helper.ReadNulledString(reader.BaseStream);
                var sourceFile = Helper.ReadNulledString(reader.BaseStream);
                var unknown1 = reader.ReadInt32();
                var unknown2 = reader.ReadInt32();
                var arrParameters = new String[reader.ReadInt32()];

                // local variables
                // script treats parameters as local variables
                for (int j = 0; j < arrParameters.Length; j++)
                    arrParameters[j] = Helper.ReadNulledString(reader.BaseStream);

                var address = reader.ReadInt32();
                var NumParameters = reader.ReadInt32();
                var ID = reader.ReadInt32();

                // always 1 apart from StandardBanter and StandardReminder functions ( 2 )?
                if ( unknown1 != 1 )
                    Console.WriteLine("{0} : {1}", functionName, unknown1);

                var function = new Function();
                function.Name = functionName;
                function.SourceFile = sourceFile;
                function.LocalVariables = arrParameters;
                function.InstructionAddress = address;
                function.ScriptID = ID;
                function.Unknown1 = unknown1;
                function.VariableOffset = unknown2;
                function.NumParameters = NumParameters;

                chlFile.FunctionTable[i] = function;
            }

            Console.WriteLine("Done.");

            #endregion

            // text data
            var dataLength = reader.ReadInt32();
            Console.Write("Reading data section of {0} bytes... ", dataLength);
            chlFile.DataTable = reader.ReadBytes(dataLength);
            Console.WriteLine("Done.");

            // 4096 null bytes...
            // random ass padding
            reader.ReadBytes(4096);

            var unknown4 = reader.ReadInt32(); // might be for something else that needs to be read if it's not 0
            if (unknown4 != 0)
                throw new Exception("Not 0");

            #region Saved Global Section

            chlFile.SavedGlobalTable = new SavedGlobal[reader.ReadInt32()];

            Console.Write("Reading {0} saved globals... ", chlFile.SavedGlobalTable.Length);

            for (int i = 0; i < chlFile.SavedGlobalTable.Length; i++)
            {
                SavedGlobal global = new SavedGlobal();
                global.Unknown = reader.ReadInt32();
                global.Value = reader.ReadSingle();
                global.Name = Helper.ReadNulledString(reader.BaseStream);

                chlFile.SavedGlobalTable[i] = global;
            }

            Console.WriteLine("Done.");

            #endregion

            // that's all folks.
            Console.WriteLine("CHL File Read: {0} / {1}", reader.BaseStream.Position, reader.BaseStream.Length);

            return chlFile;
        }
    }
}
