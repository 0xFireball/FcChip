using FcChip;

namespace FcAssembler.Tokens {
    public abstract class Operand { }

    public class RegisterOperand : Operand {
        // R
        public RegisterOperand(FcRegister register) {
            this.register = register;
        }

        public FcRegister register;
    }

    public class ValueOperand : Operand {
        // V
        public ValueOperand(ushort value) {
            this.value = value;
        }

        public ushort value;
    }

    public class LabelOperand : Operand {
        // L
        public LabelOperand(string label) {
            this.label = label;
        }

        public string label;
    }

    public class AddressOperand : Operand {
        // A
        public AddressOperand(ushort address) {
            this.address = address;
        }

        public ushort address;
    }
}