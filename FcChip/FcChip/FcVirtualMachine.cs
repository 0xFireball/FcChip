using System;
using System.IO;

namespace FcChip {
    public class FcVirtualMachine {
        private Stream programStream;

        public class Registers {
            public ushort A;
            public ushort B;

            public ushort C;

            public bool E;

            public void Set(FcRegister registerId, ushort value) {
                switch (registerId) {
                    case FcRegister.A:
                        A = value;
                        break;
                    case FcRegister.B:
                        B = value;
                        break;
                    case FcRegister.C:
                        C = value;
                        break;
                    case FcRegister.E:
                        E = value > 0;
                        break;
                }
            }

            public ushort Get(FcRegister registerId) {
                switch (registerId) {
                    case FcRegister.A:
                        return A;
                    case FcRegister.B:
                        return B;
                    case FcRegister.C:
                        return C;
                    case FcRegister.E:
                        return (ushort) (E ? 1 : 0);
                    default:
                        return 0;
                }
            }
        }

        public FcVirtualMachine() {
            Initialize();
        }

        private Registers registers;

        public void Initialize() {
            registers = new Registers();
        }

        public void LoadProgram(Stream programStream) {
            this.programStream = programStream;
        }

        public void Execute() {
            while (true) {
                programStream.Position = registers.C;
                var opCodeByte = (byte) programStream.ReadByte();
                if (!Enum.IsDefined(typeof(FcInternalOpCode), opCodeByte)) {
                    // unrecognized opcode
                    throw new FcVirtualMachineExecutionException(
                        $"unrecognized opcode at offset {programStream.Position - 1}");
                }

                var machineOpCode = (FcInternalOpCode) opCodeByte;

                ProcessInstruction(machineOpCode);
            }
        }

        private void ProcessInstruction(FcInternalOpCode opCode) {
            switch (opCode) {
                case FcInternalOpCode.Nop:
                    break;
                case FcInternalOpCode.MovR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    var destReg = (FcRegister) (byte) programStream.ReadByte();

                    registers.Set(destReg, registers.Get(srcReg));

                    break;
                }
                case FcInternalOpCode.MovV: {
                    var srcVal = (byte) programStream.ReadByte();
                    var destReg = (FcRegister) (byte) programStream.ReadByte();

                    registers.Set(destReg, srcVal);

                    break;
                }
                case FcInternalOpCode.AddR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) + registers.Get(srcReg)));
                    
                    break;
                }
            }
        }

        public class FcVirtualMachineExecutionException : Exception {
            public FcVirtualMachineExecutionException(string message) : base(message) { }
        }
    }
}