﻿using System;
using System.IO;

namespace FcChip {
    public class FcVirtualChip {
        private Stream programStream;

        public enum State {
            Ready,
            Running,
            Stopped
        }

        public State state { get; private set; } = State.Stopped;

        public class Registers {
            public ushort A;
            public ushort B;

            public ushort C;

            public bool E;
            public bool L;

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
                    case FcRegister.L:
                        L = value > 0;
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
                    case FcRegister.L:
                        return (ushort) (L ? 1 : 0);
                    default:
                        return 0;
                }
            }
        }

        public FcVirtualChip() {
            Initialize();
        }

        private Registers registers;

        public void Initialize() {
            registers = new Registers();
            state = State.Ready;
        }

        public void LoadProgram(Stream programStream) {
            this.programStream = programStream;
        }

        public void Tick() {
            programStream.Position = registers.Get(FcRegister.C);
            if (programStream.Position < programStream.Length - 1) {
                state = State.Running;
                var opCodeByte = (byte) programStream.ReadByte();
                if (!Enum.IsDefined(typeof(FcInternalOpCode), opCodeByte)) {
                    // unrecognized opcode
                    throw new FcVirtualMachineExecutionException(
                        $"unrecognized opcode at offset {programStream.Position - 1}");
                }

                var machineOpCode = (FcInternalOpCode) opCodeByte;

                ProcessInstruction(machineOpCode);
                programStream.Position = registers.Get(FcRegister.C);
            } else {
                state = State.Stopped;
            }
        }

        private void ProcessInstruction(FcInternalOpCode opCode) {
            var readOffset = 0;
            switch (opCode) {
                case FcInternalOpCode.Nop:
                    break;
                case FcInternalOpCode.Slp:
                    break;
                case FcInternalOpCode.MovR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    var destReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(destReg, registers.Get(srcReg));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.MovV: {
                    var srcVal = (byte) programStream.ReadByte();
                    var destReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(destReg, srcVal);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Swp: {
                    var reg1 = (FcRegister) (byte) programStream.ReadByte();
                    var reg2 = (FcRegister) (byte) programStream.ReadByte();
                    var val1 = registers.Get(reg1);
                    var val2 = registers.Get(reg2);
                    registers.Set(reg1, val2);
                    registers.Set(reg2, val1);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.AddR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) + registers.Get(srcReg)));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.SubR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) - registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.MulR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) * registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShrR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) >> registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShlR: {
                    var srcReg = (FcRegister) (byte) programStream.ReadByte();
                    registers.Set(FcRegister.A, (ushort) (registers.Get(FcRegister.A) << registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.CmpR: {
                    var testReg = (FcRegister) (byte) programStream.ReadByte();
                    var valReg = (FcRegister) (byte) programStream.ReadByte();
                    var test = registers.Get(testReg);
                    var val = registers.Get(valReg);
                    registers.Set(FcRegister.E, (ushort) (test == val ? 1 : 0));
                    registers.Set(FcRegister.L, (ushort) (test < val ? 1 : 0));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Jmp: {
                    var addressBytes = new byte[2];
                    programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    registers.Set(FcRegister.C, jmpAddress);
                    readOffset = -1;
                    break;
                }
                case FcInternalOpCode.Jeq: {
                    var addressBytes = new byte[2];
                    programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (registers.Get(FcRegister.E) > 0) {
                        registers.Set(FcRegister.C, jmpAddress);
                        readOffset = -1;
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
                case FcInternalOpCode.Jne: {
                    var addressBytes = new byte[2];
                    programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (registers.Get(FcRegister.E) == 0) {
                        registers.Set(FcRegister.C, jmpAddress);
                        readOffset = -1;
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
            }

            registers.Set(FcRegister.C, (ushort) (registers.Get(FcRegister.C) + readOffset + 1));
        }

        public class FcVirtualMachineExecutionException : Exception {
            public FcVirtualMachineExecutionException(string message) : base(message) { }
        }
    }
}