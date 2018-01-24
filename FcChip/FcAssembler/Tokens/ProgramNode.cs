namespace FcAssembler.Tokens {
    public abstract class ProgramNode { }

    public class Label : ProgramNode {
        public Label(string name) {
            this.name = name;
        }

        public string name;
    }
}