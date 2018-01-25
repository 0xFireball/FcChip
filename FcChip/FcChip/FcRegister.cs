namespace FcChip {
    public enum FcRegister : byte {
        
        // general purpose
        A = 0xA0, // 16b "A" accumulator register
        B = 0xA1, // 16b "B" register
        M = 0xA2, // 16b "M" register
        N = 0xA3, // 16b "N" register
        
        BL = 0xA4, // 8b lower 8 bits of B
        BH = 0xA5, // 8b upper 8 bits of B
        
        MN = 0xAA, // 32b "MN" {MN} register, used for memory addressing
        
        // special
        C = 0xB0, // Program counter register
        
        // flags
        E = 0xC0,
        L = 0xC1,
    }
}