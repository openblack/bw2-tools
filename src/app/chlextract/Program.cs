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
        static void Main(string[] args)
        {
            var chlFile = ChallengeFile.LoadFromStream(new FileStream(args[0], FileMode.Open));

            var file = File.CreateText(args[1]);
            ASMOut.Convert(chlFile, file);
            file.Close();
        }
    }
}
