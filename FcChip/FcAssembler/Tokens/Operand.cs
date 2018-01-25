using FcChip;

namespace FcAssembler.Tokens {
    public abstract class Operand { }

    public class RegisterOperand : Operand {
        // R
        public RegisterOperand(FcRegister register) {
            this.register = register;
        }

        public FcRegister register;

        public override string ToString() {
            return register.ToString().ToLower();
        }
    }

    public class ValueOperand : Operand {
        // V
        public ValueOperand(ushort value) {
            this.value = value;
        }

        public ushort value;

        public override string ToString() {
            return value.ToString();
        }
    }

    public class LabelOperand : Operand {
        // L
        public LabelOperand(string label) {
            this.label = label;
        }

        public string label;

        public override string ToString() {
            return $":{label}";
        }
    }

    public class AddressOperand : Operand {
        // A
        public AddressOperand(ushort address) {
            this.address = address;
        }

        public ushort address;

        public override string ToString() {
            return address.ToString();
        }
    }
}