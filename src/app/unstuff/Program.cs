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
        private const string usageText = "Usage: unstuff Everything.stuff outputdir";

        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(usageText);
                return 1;
            }

            string stuffFilePath = "D:\\Program Files (x86)\\Lionhead Studios\\Black & White 2\\Data\\Everything.stuff";

            BinaryReader binaryReader = new BinaryReader((Stream)File.Open(stuffFilePath, FileMode.Open, FileAccess.Read));
            binaryReader.BaseStream.Seek(-4L, SeekOrigin.End);

            Int32 TOC = binaryReader.ReadInt32();
            if (TOC < 4 || TOC > binaryReader.BaseStream.Length - 32)
            {
                Console.WriteLine("Invalid TOC");
                throw new Exception();
            }

            Int32 fileLength = (int)binaryReader.BaseStream.Length;
            Int32 dicLength = (int)binaryReader.BaseStream.Length - TOC - 4;
            Int32 numEntries = dicLength / 0x10C;

            Console.WriteLine("\t({0} bytes, TOC = {1}, {2} entries)", fileLength, TOC, numEntries);

            binaryReader.BaseStream.Seek(TOC, SeekOrigin.Begin);

            for (int i = 0; i < numEntries; i++)
            {
                string str = Encoding.ASCII.GetString(binaryReader.ReadBytes(256)).Trim(new char[1]);
                int num1 = (int)binaryReader.ReadUInt32();
                int num3 = binaryReader.ReadInt32();
                var num2 = binaryReader.ReadBytes(4);
                Console.WriteLine("{0} : {1} {2} {3}", str, num1, num3, num1 + num3);
            }

            Console.WriteLine("Done.");
            binaryReader.Close();

            Console.ReadLine();

            return 0;
        }
    }
}
