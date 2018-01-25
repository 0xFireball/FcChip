using System;
using System.Collections.Generic;
using System.IO;
using FcAssembler.Tokens;
using FcChip;

namespace FcAssembler {
    public class ChipAssembler {
        private Dictionary<string, ushort> labelSymbols = new Dictionary<string, ushort>(); // name, offset

        private List<KeyValuePair<string, ushort>>
            fwLabels = new List<KeyValuePair<string, ushort>>(); // referenced name, position

        private ushort offset = 0;

        public byte[] AssembleProgram(List<string> sourceLines) {
            var tokenizer = new Tokenizer();
            var programNodes = tokenizer.Tokenize(sourceLines);

            // emit code
            var outputProgram = new MemoryStream();

            using (var bw = new BinaryWriter(outputProgram)) {
                offset = 0;
                foreach (var node in programNodes) {
                    switch (node) {
                        case LabelNode label:
                            labelSymbols[label.name] = offset;
                            break;
                        case Instruction instr:
                            var emit = EmitInstruction(instr);
                            bw.Write(emit);
                            offset += (ushort) emit.Length;
                            break;
                    }
                }
            }

            var programBytes = outputProgram.ToArray();

            // replace forward symbols
            foreach (var fwLabel in fwLabels) {
                var labelAddress = BitConverter.GetBytes(labelSymbols[fwLabel.Key]);
                programBytes[fwLabel.Value] = labelAddress[0];
                programBytes[fwLabel.Value + 1] = labelAddress[1];
            }

            return programBytes;
        }

        public byte[] EmitInstruction(Instruction instruction) {
            var result = new List<byte>();
            switch (instruction.opCode) {
                case FcOpCode.Nop: {
                    result.Add((byte) FcInternalOpCode.Nop);
                    break;
                }
                case FcOpCode.Slp: {
                    result.Add((byte) FcInternalOpCode.Slp);
                    break;
                }
                case FcOpCode.Mov: {
                    switch (instruction.operands[0]) {
                        case RegisterOperand registerOperand:
                            result.Add((byte) FcInternalOpCode.MovR);
                            result.Add((byte) registerOperand.register);
                            break;
                        case ValueOperand valueOperand:
                            result.Add((byte) FcInternalOpCode.MovV);
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
                case FcOpCode.Swp: {
                    result.Add((byte) FcInternalOpCode.Swp);
                    if (!(instruction.operands[0] is RegisterOperand)) {
                        throw new AssemblerException("expected R operand");
                    }

                    result.Add((byte) ((RegisterOperand) instruction.operands[0]).register);

                    if (!(instruction.operands[1] is RegisterOperand)) {
                        throw new AssemblerException("expected R operand");
                    }

                    result.Add((byte) ((RegisterOperand) instruction.operands[1]).register);

                    break;
                }
                case FcOpCode.Add:
                case FcOpCode.Sub:
                case FcOpCode.Mul:
                case FcOpCode.Shl:
                case FcOpCode.Shr: {
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
                    var testReg = -1;
                    if (instruction.operands[0] is RegisterOperand sourceRegisterOperand) {
                        testReg = (byte) sourceRegisterOperand.register;
                    } else {
                        throw new AssemblerException("expected R operand");
                    }

                    switch (instruction.operands[1]) {
                        case RegisterOperand registerOperand:
                            result.Add((byte) FcInternalOpCode.CmpR);
                            result.Add((byte) testReg);
                            result.Add((byte) registerOperand.register);
                            break;
                        case ValueOperand valueOperand:
                            result.Add((byte) FcInternalOpCode.CmpV);
                            result.Add((byte) testReg);
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
                            fwLabels.Add(new KeyValuePair<string, ushort>(labelOperand.label,
                                (ushort) (offset + result.Count)));
                            result.Add(0);
                            result.Add(0);
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

            return result.ToArray();
        }

        public class AssemblerException : Exception {
            public AssemblerException(string message) : base(message) { }
        }
    }
}