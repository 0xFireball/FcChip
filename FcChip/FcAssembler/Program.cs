using System;
using System.Collections.Generic;
using System.IO;

namespace FcAssembler {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("FCAssembler");

            if (args.Length <= 1) {
                Console.WriteLine("Usage: FCAssembler <input> [OPTIONS]");
                return;
            }

            var inputFile = args[0];

            // read input file
            var sourceLines = new List<string>();
            using (var sr = new StreamReader(File.OpenRead(inputFile))) {
                var line = sr.ReadLine();
                sourceLines.Add(line);
            }
            
            var assembler = new ChipAssembler();
            assembler.AssembleProgram(sourceLines);
        }
    }
}