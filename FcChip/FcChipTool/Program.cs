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
            chip.loadProgram(inputStream);
            while (chip.state != FcVirtualChip.State.Stopped) {
                chip.tick();
            }

            return 0;
        }
    }
}