namespace FcChip {
    public enum FcRegister : byte {
        // general purpose
        A = 0x20, // 16b "A" accumulator register
        B = 0x21, // 16b "B" register
        M = 0x22, // 16b "M" register
        N = 0x23, // 16b "N" register
        
        BL = 0x14, // 8b lower 8 bits of B
        BH = 0x15, // 8b upper 8 bits of B
        
        MN = 0x3A, // 32b "MN" {MN} register, used for memory addressing
        
        // special
        C = 0xB0, // Program counter register
        
        // flags
        F = 0xC0, // 16b FLAGS register
    }

    public enum FcFlags {
        Equal = 0,
        Less = 1,
    }
}