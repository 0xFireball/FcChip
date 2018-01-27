using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using FcChip;

namespace FcChipTool {
    class Program {
        [Verb("run", HelpText = "Run a program in bytecodeformat.")]
        class RunOptions {
            [Value(0, Min = 2)]
            public IEnumerable<string> inputFiles { get; set; }

            [Option('m', "memory", Required = false, Default = -1, HelpText = "The memory size for the chip")]
            public long memorySize { get; set; }

            [Option('d', "dump", Required = false, Default = false, HelpText = "Dump chip information after halt")]
            public bool dump { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<RunOptions>(args)
                .MapResult(
                    opts => programRun(opts),
                    errs => 1);
        }

        private static int programRun(RunOptions opts) {
            var inputStream = File.OpenRead(opts.inputFiles.Skip(1).First());

            var chip = new FcVirtualChip();
            if (opts.memorySize > 0) {
                chip.memorySize = (uint) opts.memorySize;
            }

            chip.initialize();
            chip.loadProgram(inputStream);
            while (chip.state != FcVirtualChip.State.Stopped) {
                chip.tick();
            }

            if (opts.dump) {
                Console.WriteLine("== MEMORY DUMP ==");
                for (var i = 0; i < chip.memory.Length; i++) {
                    Console.Write($"{chip.memory[i]:x2} ");
                }
                Console.WriteLine();
                Console.WriteLine("== REGISTER DUMP ==");
                var registerIds = (FcRegister[]) Enum.GetValues(typeof(FcRegister));
                foreach (var registerId in registerIds) {
                    Console.WriteLine($"{registerId} {chip.registers.Get(registerId):x4}");
                }
            }

            return 0;
        }
    }
}