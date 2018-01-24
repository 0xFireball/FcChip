using System;
using System.Collections.Generic;
using FcAssembler.Tokens;
using FcChip;

namespace FcAssembler {
    public class ChipAssembler {
        private Dictionary<string, ushort> labels;

        public byte[] AssembleProgram(List<string> sourceLines) {
            var tokenizer = new Tokenizer();
            var programNodes = tokenizer.Tokenize(sourceLines);

            // emit code

            ushort offset = 0;
            for (var i = 0; i < programNodes.Count; i++) {
                var node = programNodes[i];

                if (node is Label) {
                    labels[((Label) node).name] = offset;
                }

                if (node is Instruction) {
                    var emit = EmitInstruction((Instruction) node);
                    offset += (ushort) emit.Count;
                }
            }

            return new byte[0];
        }

        public List<byte> EmitInstruction(Instruction instruction) {
            var result = new List<byte>();
            switch (instruction.opCode) {
                case FcOpCode.Nop: {
                    result.Add((byte) FcInternalOpCode.Nop);
                    break;
                }
                case FcOpCode.Mov: {
                    switch (instruction.operands[0]) {
                        case RegisterOperand registerOperand:
                            result.Add((byte) FcInternalOpCode.CmpR);
                            result.Add((byte) registerOperand.register);
                            break;
                        case ValueOperand valueOperand:
                            result.Add((byte) FcInternalOpCode.CmpV);
                            result.Add((byte) valueOperand.value);
                            break;
                        default:
                            throw new AssemblerException("expected R/V operand");
                    }

                    if (instruction.operands[1] is RegisterOperand destRegisterOperand) {
                        result.Add((byte) destRegisterOperand.register);
                    } else {
                        throw new AssemblerException("expected R operand");
                    }

                    break;
                }
                case FcOpCode.Slp: {
                    result.Add((byte) FcInternalOpCode.Slp);
                    break;
                }
                case FcOpCode.Add:
                case FcOpCode.Sub:
                case FcOpCode.Mul: {
                    var opCodeStr = instruction.opCode.ToString();
                    switch (instruction.operands[0]) {
                        case RegisterOperand registerOperand: {
                            Enum.TryParse(typeof(FcInternalOpCode), opCodeStr + "R", out var code);
                            result.Add((byte) code);
                            result.Add((byte) registerOperand.register);
                            break;
                        }
                        case ValueOperand valueOperand: {
                            Enum.TryParse(typeof(FcInternalOpCode), opCodeStr + "V", out var code);
                            result.Add((byte) code);
                            result.Add((byte) valueOperand.value);
                            break;
                        }
                        default:
                            throw new AssemblerException("expected R/V operand");
                    }

                    break;
                }
                case FcOpCode.Cmp: {
                    if (instruction.operands[0] is RegisterOperand sourceRegisterOperand) {
                        result.Add((byte) sourceRegisterOperand.register);
                    } else {
                        throw new AssemblerException("expected R operand");
                    }

                    switch (instruction.operands[1]) {
                        case RegisterOperand registerOperand:
                            result.Add((byte) FcInternalOpCode.CmpR);
                            result.Add((byte) registerOperand.register);
                            break;
                        case ValueOperand valueOperand:
                            result.Add((byte) FcInternalOpCode.CmpV);
                            result.Add((byte) valueOperand.value);
                            break;
                        default:
                            throw new AssemblerException("expected R/V operand");
                    }

                    break;
                }

                case FcOpCode.Jmp:
                case FcOpCode.Jeq:
                case FcOpCode.Jne: {
                    var opCodeStr = instruction.opCode.ToString();
                    Enum.TryParse(typeof(FcInternalOpCode), opCodeStr, out var code);
                    result.Add((byte) code);

                    switch (instruction.operands[0]) {
                        case LabelOperand labelOperand:
                            var labelAddress = BitConverter.GetBytes(labels[labelOperand.label]);
                            result.Add(labelAddress[0]);
                            result.Add(labelAddress[1]);
                            break;
                        case AddressOperand addressOperand:
                            var address = BitConverter.GetBytes(addressOperand.address);
                            result.Add(address[0]);
                            result.Add(address[1]);
                            break;
                        default:
                            throw new AssemblerException("expected L/A operand");
                    }

                    break;
                }
            }

            return result;
        }

        public class AssemblerException : Exception {
            public AssemblerException(string message) : base(message) { }
        }
    }
}