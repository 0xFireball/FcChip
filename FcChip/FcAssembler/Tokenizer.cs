using System;
using System.Collections.Generic;
using System.Globalization;
using FcAssembler.Tokens;
using FcChip;

namespace FcAssembler {
    public class Tokenizer {
        public List<ProgramNode> Tokenize(List<string> programSource) {
            var program = new List<ProgramNode>();
            for (var i = 0; i < programSource.Count; i++) {
                var line = programSource[i];
                var codePortion = line.Split(';')[0]; // ignore comments
                codePortion = codePortion.Trim(); // ignore whitespace
                codePortion = codePortion.Replace(",", ""); // delete commas
                if (string.IsNullOrWhiteSpace(codePortion)) {
                    continue;
                }

                var segments = codePortion.Split(' ');
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

            if (literal.EndsWith(":")) {
                // it is a label
                return new LabelNode(literal.Substring(0, literal.Length - 1));
            }

            var operands = ParseOperands(segments);
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

                if (ushort.TryParse(literal, out var value)) {
                    operands.Add(new ValueOperand(value));
                    continue;
                }

                if (Enum.TryParse(typeof(FcRegister), literal, true, out var register)) {
                    operands.Add(new RegisterOperand((FcRegister) register));
                    continue;
                }

                if (literal.StartsWith(":")) {
                    operands.Add(new LabelOperand(literal.Substring(1)));
                    continue;
                }

                if (literal.StartsWith("@")) {
                    if (ushort.TryParse(literal.Substring(1), out var address)) {
                        operands.Add(new AddressOperand(address));
                    }

                    continue;
                }

                if (literal.StartsWith("$")) {
                    if (ushort.TryParse(literal.Substring(1), NumberStyles.HexNumber, null, out var fromHexValue)) {
                        operands.Add(new ValueOperand(fromHexValue));
                        continue;
                    }
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