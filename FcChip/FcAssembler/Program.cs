using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace FcAssembler {
    class Program {
        class Options {
            [Value(0, Min = 1)]
            public IEnumerable<string> inputFiles { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output file.")]
            public string outputFile { get; set; }
        }

        static void Main(string[] args) {
            Console.WriteLine("FCAssembler");

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunProgram(opts))
                .WithNotParsed(errs => {
                    Console.WriteLine("Usage: FCAssembler [OPTIONS] <input files>");
                });
        }

        static void RunProgram(Options options) {
            foreach (var inputFile in options.inputFiles) {
                // read input file
                var sourceLines = new List<string>();
                using (var sr = new StreamReader(File.OpenRead(inputFile))) {
                    while (!sr.EndOfStream) {
                        var line = sr.ReadLine();
                        sourceLines.Add(line);
                    }
                }

                var assembler = new ChipAssembler();
                var program = assembler.AssembleProgram(sourceLines);
                var outputFile = options.outputFile ?? $"{Path.GetFileNameWithoutExtension(inputFile)}.fc";

                using (var bw = new BinaryWriter(File.Create(outputFile))) {
                    bw.Write(program);
                }
            }
        }
    }
}