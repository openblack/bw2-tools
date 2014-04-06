using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace ScriptReader
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: chlex [in.chl] [out.chl]");
                return 1;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("{0} does not exist!", args[0]);
                return 1;
            }

            var chlFile = ChallengeFile.LoadFromStream(new FileStream(args[0], FileMode.Open));

            Directory.CreateDirectory(Path.GetDirectoryName(args[1]));
            var file = File.CreateText(args[1]);
            ASMOut.Convert(chlFile, file);
            file.Close();

            return 0;
        }
    }
}
