using System;
using System.Collections.Generic;
using FcAssembler.Tokens;
using FcChip;

namespace FcAssembler {
    public class Tokenizer {
        public List<ProgramNode> Tokenize(List<string> programSource) {
            var program = new List<ProgramNode>();
            for (var i = 0; i < programSource.Count; i++) {
                var line = programSource[i];
                var segments = line.Split(' ');
                try {
                    var node = ParseLine(segments);
                    program.Add(node);
                } catch (TokenizationException tex) {
                    throw new TokenizationException(tex, i + 1);
                }
            }

            return program;
        }

        private ProgramNode ParseLine(string[] segments) {
            var literal = segments[0];

            if (literal.StartsWith(":")) {
                // it is a label
                return new Label(literal.Substring(1));
            }

            var operands = ParseOperands(segments);
            Instruction instruction;
            if (Enum.TryParse(typeof(FcOpCode), literal, true, out var opCode)) {
                return new Instruction((FcOpCode) opCode, operands);
            } else {
                throw new InstructionParseException("opcode was not recognized.");
            }
        }

        private List<Operand> ParseOperands(string[] segments) {
            var operands = new List<Operand>();
            for (var i = 1; i < segments.Length; i++) {
                var literal = segments[i];
                if (Enum.TryParse(typeof(FcRegister), literal, out var register)) {
                    operands.Add(new RegisterOperand((FcRegister) register));
                    break;
                }

                if (literal.StartsWith(":")) {
                    operands.Add(new LabelOperand(literal.Substring(1)));
                    break;
                }

                if (literal.StartsWith("@")) {
                    if (ushort.TryParse(literal.Substring(1), out var address)) {
                        operands.Add(new AddressOperand(address));
                        break;
                    }
                }

                if (ushort.TryParse(literal, out var value)) {
                    operands.Add(new ValueOperand(value));
                    break;
                }
            }

            return operands;
        }

        public class TokenizationException : Exception {
            public TokenizationException(string message) : base(message) { }

            public TokenizationException(Exception innerException, int lineNumber) : base(
                $"Error tokenizing at line {lineNumber}", innerException) { }
        }

        public class InstructionParseException : TokenizationException {
            public InstructionParseException(string message) : base(message) { }
        }
    }
}