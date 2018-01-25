namespace FcChip {
    public enum FcOpCode : byte {
        // General purpose instructions
        Nop, // nop
        Mov, // mov R/V R
        Swp, // swp R R
        
        Slp,
        
        // Arithmetic instructions
        Add, // add R/V
        Sub, // sub R/V
        Mul, // mul R/V
        
        Shl, // shl R/V
        Shr, // shr R/V
        
        // Logical instructions
        Cmp, // cmp R R/V, sets E flag if equal
        
        // Flow instructions
        Jmp, // jmp L/A
        Jeq, // jeq L/A
        Jne, // jne L/A
    }

    public enum FcInternalOpCode : byte {
        Nop = 0x01, // nop
        MovR = 0x02,
        MovV = 0x03,
        Swp = 0x05,
        
        Slp = 0x0A,
        
        // Arithmetic instructions
        AddR = 0x21,
        SubR = 0x23,
        MulR = 0x25,
        
        ShlR = 0x27,
        ShrR = 0x29,
        
        // Logical instructions
        CmpR = 0x31,
        
        // Flow instructions
        Jmp = 0x40,
        Jeq = 0x41,
        Jne = 0x42,
    }
}