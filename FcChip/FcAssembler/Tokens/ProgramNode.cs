namespace FcAssembler.Tokens {
    public abstract class ProgramNode { }

    public class LabelNode : ProgramNode {
        public LabelNode(string name) {
            this.name = name;
        }

        public string name;
    }
}