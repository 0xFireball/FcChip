using System;
using System.IO;

namespace FcChip {
    public class FcVirtualChip {
        private Stream _programStream;

        public enum State {
            Ready,
            Running,
            Stopped
        }

        public State state { get; private set; } = State.Stopped;

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
                        return ((uint)M << 16) | N;
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

        private Registers _registers;

        public void initialize() {
            _registers = new Registers();
            state = State.Ready;
        }

        public void loadProgram(Stream programStream) {
            this._programStream = programStream;
        }

        public void tick() {
            _programStream.Position = _registers.Get(FcRegister.C);
            if (_programStream.Position < _programStream.Length - 1) {
                state = State.Running;
                var opCodeByte = (byte) _programStream.ReadByte();
                if (!Enum.IsDefined(typeof(FcInternalOpCode), opCodeByte)) {
                    // unrecognized opcode
                    throw new FcVirtualMachineExecutionException(
                        $"unrecognized opcode at offset {_programStream.Position - 1}");
                }

                var machineOpCode = (FcInternalOpCode) opCodeByte;

                processInstruction(machineOpCode);
                _programStream.Position = _registers.Get(FcRegister.C);
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
                case FcInternalOpCode.MovR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    var destReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(destReg, _registers.Get(srcReg));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.MovV: {
                    var srcVal = (byte) _programStream.ReadByte();
                    var destReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(destReg, srcVal);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Swp: {
                    var reg1 = (FcRegister) (byte) _programStream.ReadByte();
                    var reg2 = (FcRegister) (byte) _programStream.ReadByte();
                    var val1 = _registers.Get(reg1);
                    var val2 = _registers.Get(reg2);
                    _registers.Set(reg1, val2);
                    _registers.Set(reg2, val1);
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.AddR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(FcRegister.A, (ushort) (_registers.Get(FcRegister.A) + _registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.SubR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(FcRegister.A, (ushort) (_registers.Get(FcRegister.A) - _registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.MulR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(FcRegister.A, (ushort) (_registers.Get(FcRegister.A) * _registers.Get(srcReg)));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShrR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(FcRegister.A, _registers.Get(FcRegister.A) >> (int) _registers.Get(srcReg));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.ShlR: {
                    var srcReg = (FcRegister) (byte) _programStream.ReadByte();
                    _registers.Set(FcRegister.A, _registers.Get(FcRegister.A) << (int) _registers.Get(srcReg));
                    readOffset = 1;
                    break;
                }
                case FcInternalOpCode.CmpR: {
                    var testReg = (FcRegister) (byte) _programStream.ReadByte();
                    var valReg = (FcRegister) (byte) _programStream.ReadByte();
                    var test = _registers.Get(testReg);
                    var val = _registers.Get(valReg);
                    _registers.Set(FcRegister.E, (ushort) (test == val ? 1 : 0));
                    _registers.Set(FcRegister.L, (ushort) (test < val ? 1 : 0));
                    readOffset = 2;
                    break;
                }
                case FcInternalOpCode.Jmp: {
                    var addressBytes = new byte[2];
                    _programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    _registers.Set(FcRegister.C, jmpAddress);
                    readOffset = -1;
                    break;
                }
                case FcInternalOpCode.Jeq: {
                    var addressBytes = new byte[2];
                    _programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (_registers.Get(FcRegister.E) > 0) {
                        _registers.Set(FcRegister.C, jmpAddress);
                        readOffset = -1;
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
                case FcInternalOpCode.Jne: {
                    var addressBytes = new byte[2];
                    _programStream.Read(addressBytes, 0, addressBytes.Length);
                    var jmpAddress = BitConverter.ToUInt16(addressBytes, 0);
                    if (_registers.Get(FcRegister.E) == 0) {
                        _registers.Set(FcRegister.C, jmpAddress);
                        readOffset = -1;
                    } else {
                        readOffset = 2;
                    }

                    break;
                }
            }

            _registers.Set(FcRegister.C, (ushort) (_registers.Get(FcRegister.C) + readOffset + 1));
        }

        public class FcVirtualMachineExecutionException : Exception {
            public FcVirtualMachineExecutionException(string message) : base(message) { }
        }
    }
}