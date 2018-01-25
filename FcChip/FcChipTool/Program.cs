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
                    opts => ProgramRun(opts),
                    errs => 1);
        }

        private static int ProgramRun(RunOptions opts) {
            var inputStream = File.OpenRead(opts.inputFiles.Skip(1).First());

            var vm = new FcVirtualMachine();
            vm.LoadProgram(inputStream);
            vm.Execute();

            return 0;
        }
    }
}