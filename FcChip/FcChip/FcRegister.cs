namespace FcChip {
    public enum FcRegister : byte {
        
        // general purpose
        A = 0xA0, // "A" accumulator register
        B = 0xA1, // "B" register
        
        // special
        C = 0xA5, // Program counter register
        
        // flags
        E = 0xA8,
        L = 0xA9,
    }
}