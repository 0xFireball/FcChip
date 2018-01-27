namespace FcChip {
    public enum FcOpCode : byte {
        // General purpose instructions
        Nop,
        Mov,
        Swp,

        Slp,
        Hlt,

        // Arithmetic instructions
        Add,
        Sub,
        Mul,

        Shl,
        Shr,

        // Logical instructions
        Cmp,

        // Flow instructions
        Jmp,
        Jeq,
        Jne,

        // Memory access
        Ldr, // ldr R
        Str, // str R 

        // Stack operations
        Push,
        Pop,
        Call,
        Ret,
    }

    public enum FcInternalOpCode : byte {
        Nop = 0x01, // nop
        MovR = 0x02,
        MovV = 0x03,
        Swp = 0x05,

        Slp = 0x0A,
        Hlt = 0x0B,

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

        // Memory access
        Ldr = 0x50,
        Str = 0x51,

        // Stack operations
        Push = 0x52,
        Pop = 0x53,
        Call = 0x54,
        Ret = 0x55,
    }
}