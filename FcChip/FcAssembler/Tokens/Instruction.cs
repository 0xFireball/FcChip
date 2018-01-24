using System.Collections.Generic;
using FcChip;

namespace FcAssembler.Tokens {
    public class Instruction : ProgramNode {
        public Instruction(FcOpCode opCode) {
            this.opCode = opCode;
        }

        public Instruction(FcOpCode opCode, List<Operand> operands) : this(opCode) {
            this.operands = operands;
        }

        public FcOpCode opCode;
        public List<Operand> operands;
    }
}