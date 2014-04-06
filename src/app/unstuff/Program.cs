using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace unstuff
{
    class Program
    {
        struct FileDictionaryEntry
        {
            public string fileName;
            public uint filePtr;
            public uint fileLength;

            public FileDictionaryEntry(BinaryReader reader)
            {
                this.fileName   = Encoding.ASCII.GetString(reader.ReadBytes(256)).Trim(new char[1]);
                this.filePtr = reader.ReadUInt32();
                this.fileLength = reader.ReadUInt32();
                reader.ReadBytes(4);
            }
        }

        private const string usageText = "Usage: unstuff [file.stuff] (outputdir)";

        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(usageText);
                return 1;
            }

            string inputFileName = args[0];
            string outputDir = ""; // assume root

            if (args.Length > 1)
                outputDir = args[1];

            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("Couldn't find specified file: {0}", inputFileName);
                return 1;
            }

            var inputFileStream = File.Open(inputFileName, FileMode.Open, FileAccess.Read);

            var reader = new BinaryReader(inputFileStream);
            reader.BaseStream.Seek(-4L, SeekOrigin.End);

            var TOC = reader.ReadUInt32();
            if (TOC < 4 || TOC > reader.BaseStream.Length - 32)
            {
                Console.WriteLine("Invalid TOC");
                return 1;
            }

            var fileLength = reader.BaseStream.Length;
            var dicLength = reader.BaseStream.Length - TOC - 4;
            var numEntries = dicLength / 0x10C;

            Console.WriteLine("\t({0} bytes, TOC = {1}, {2} entries)", fileLength, TOC, numEntries);

            reader.BaseStream.Seek(TOC, SeekOrigin.Begin);

            var fileDictionary = new FileDictionaryEntry[numEntries];

            for (int i = 0; i < numEntries; i++)
                fileDictionary[i] = new FileDictionaryEntry(reader);

            for (int i = 0; i < numEntries; i++)
            {
                var entry = fileDictionary[i];
                Console.WriteLine(" {0, -4} : {1}", i, entry.fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(outputDir + "/" + entry.fileName));
                var outputFile = File.OpenWrite(outputDir + "/" + entry.fileName);

                reader.BaseStream.Seek(entry.filePtr, SeekOrigin.Begin);
                outputFile.Write(reader.ReadBytes((int)entry.fileLength), 0, (int)entry.fileLength);

                outputFile.Close();
            }

            reader.Close();
            inputFileStream.Close();

            return 0;
        }
    }
}
