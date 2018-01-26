using System;
using System.IO;

namespace FcChip {
    public class FcVirtualChip {
        public enum State {
            Ready,
            Running,
            Stopped
        }

        public State state { get; private set; } = State.Stopped;

        public const uint PROGRAM_OFFSET = 0;

        public class Registers {
            public ushort A;
            public ushort B;
            public ushort M;
            public ushort N;

            public ushort C;

            public bool E;
            public bool L;

            public void Set(FcRegister registerId, uint value) {
                switch (registerId) {
                    case FcRegister.A:
                        A = (ushort) value;
                        break;
                    case FcRegister.B:
                        B = (ushort) value;
                        break;
                    case FcRegister.BL:
                        B = (ushort) ((ushort) (B & 0xFF00) | (ushort) (value & 0x00FF));
                        break;
                    case FcRegister.BH:
                        B = (ushort) ((ushort) (B & 0x00FF) | (ushort) (value & 0x00FF) << 8);
                        break;
                    case FcRegister.M:
                        M = (ushort) value;
                        break;
                    case FcRegister.N:
                        N = (ushort) value;
                        break;
                    case FcRegister.MN:
                        M = (ushort) (value >> 16);
                        N = (ushort) (value & 0x0000FFFF);
                        break;
                    case FcRegister.C:
                        C = (ushort) value;
                        break;
                    case FcRegister.E:
                        E = value > 0;
                        break;
                    case FcRegister.L:
                        L = value > 0;
                        break;
                }
            }

            public uint Get(FcRegister registerId) {
                switch (registerId) {
                    case FcRegister.A:
                        return A;
                    case FcRegister.B:
                        return B;
                    case FcRegister.BL:
                        return (ushort) (B & 0x00FF);
                    case FcRegister.BH:
                        return (ushort) (B >> 8);
                    case FcRegister.M:
                        return M;
                    case FcRegister.N:
                        return N;
                    case FcRegister.MN:
                        return ((uint) M << 16) | N;
                    case FcRegister.C:
                        return C;
                    case FcRegister.E:
                        return (ushort) (E ? 1 : 0);
                    case FcRegister.L:
                        return (ushort) (L ? 1 : 0);
                    default:
                        return 0;
                }
            }
        }

        public FcVirtualChip() {
            initialize();
        }

        public Registers registers;
        public byte[] memory;

        public void initialize() {
            registers = new Registers();
            memory = new byte[ushort.MaxValue];
            state = State.Ready;
        }

        public void loadProgram(Stream programStream) {
            programStream.Read(memory, (int) PROGRAM_OFFSET, (int) programStream.Length);
            registers.Set(FcRegister.C, PROGRAM_OFFSET);
        }

        private byte readProgramByte() {
            return memory[registers.Get(FcRegister.C)];
        }

        public void tick() {
            if (state != State.Stopped) {
                state = State.Running;
                var opCodeByte = readProgramByte();
                if (!Enum.IsDefined(typeof(FcInternalOpCode), opCodeByte)) {
                    // unrecognized opcode
                    throw new FcVirtualMachineExecutionException(
                        $"unrecognized opcode at offset {registers.Get(FcRegister.C)}");
                }

                var machineOpCode = (FcInternalOpCode) opCodeByte;
                registers.Set(FcRegister.C, registers.Get(FcRegister.C) + 1);
                processInstruction(machineOpCode);
            } else {
                state = State.Stopped;
            }
        }

        private void processInstruction(FcInternalOpCode opCode) {
            var readOffset = 0;
            switch (opCode) {
                case FcInternalOpCode.Nop:
                    break;
                case FcInternalOpCode.Slp:
                    break;
                case FcInternalOpCode.Hlt:
                    state = State.Stopped;
                    break;
                case FcInternalOpCode.MovR: {
                    var srcReg = (FcRegister) readProgramByte();
                    var destReg = (FcRegister) readProgramByte();
                    registers.Set(destReg, registers.Get(srcReg));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.MovV: {
                    var srcVal = readProgramByte();
                    var destReg = (FcRegister) readProgramByte();
                    registers.Set(destReg, srcVal);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Swp: {
                    var reg1 = (FcRegister) readProgramByte();
                    var reg2 = (FcRegister) readProgramByte();
                    var val1 = registers.Get(reg1);
                    var val2 = registers.Get(reg2);
                    registers.Set(reg1, val2);
                    registers.Set(reg2, val1);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.AddR: {
                    var srcReg = (FcRegister) readProgramByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) + registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.SubR: {
                    var srcReg = (FcRegister) readProgramByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) - registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.MulR: {
                    var srcReg = (FcRegister) readProgramByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) * registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShrR: {
                    var srcReg = (FcRegister) readProgramByte();
                    registers.Set(FcRegister.A, registers.Get(FcRegister.A) >> (int) registers.Get(srcReg));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShlR: {
                    var srcReg = (FcRegister) readProgramByte();
                    registers.Set(FcRegister.A, registers.Get(FcRegister.A) << (int) registers.Get(srcReg));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.CmpR: {
                    var testReg = (FcRegister) readProgramByte();
                    var valReg = (FcRegister) readProgramByte();
                    var test = registers.Get(testReg);
                    var val = registers.Get(valReg);
                    registers.Set(FcRegister.E, (ushort) (test == val ? 1 : 0));
                    registers.Set(FcRegister.L, (ushort) (test < val ? 1 : 0));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Jmp: {
                    var addressBytes = new byte[2];
                    addressBytes[0] = memory[registers.Get(FcRegister.C)];
                    addressBytes[1] = memory[registers.Get(FcRegister.C) + 1];
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    registers.Set(FcRegister.C, jmpAddress);
                    break;
                }
                case FcInternalOpCode.Jeq: {
                    var addressBytes = new byte[2];
                    addressBytes[0] = memory[registers.Get(FcRegister.C)];
                    addressBytes[1] = memory[registers.Get(FcRegister.C) + 1];
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (registers.Get(FcRegister.E) > 0) {
                        registers.Set(FcRegister.C, jmpAddress);
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
                case FcInternalOpCode.Jne: {
                    var addressBytes = new byte[2];
                    addressBytes[0] = memory[registers.Get(FcRegister.C)];
                    addressBytes[1] = memory[registers.Get(FcRegister.C) + 1];
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (registers.Get(FcRegister.E) == 0) {
                        registers.Set(FcRegister.C, jmpAddress);
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
                case FcInternalOpCode.Ldr: {
                    var destReg = (FcRegister) readProgramByte();
                    var address = registers.Get(FcRegister.MN);
                    var w0 = memory[address];
                    var w1 = memory[address + 1];
                    registers.Set(destReg, (uint) (w1 << 8 | w0));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.Str: {
                    var srcReg = (FcRegister) readProgramByte();
                    var val = registers.Get(srcReg);
                    var address = registers.Get(FcRegister.MN);
                    memory[address] = (byte) (val & 0x00FF);
                    memory[address + 1] = (byte) (val >> 8);
                    var w0 = memory[address];
                    var w1 = memory[address + 1];
                    readOffset = 1;
                    break;
                }
            }

            registers.Set(FcRegister.C, (ushort) (registers.Get(FcRegister.C) + readOffset));
        }

        public class FcVirtualMachineExecutionException : Exception {
            public FcVirtualMachineExecutionException(string message) : base(message) { }
        }
    }
}