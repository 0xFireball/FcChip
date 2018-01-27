
# FcChip [FCH] Architecture

## Architecture quick facts

- Endianness: Little Endian
- Word size: 16 bits
- Memory addressable with 32-bit addresses

## Register Set

- `A` - 16-bit accumulator register
- `B` - 16-bit general purpose register
- `M` - 16-bit general purpose register
- `N` - 16-bit general purpose register
- `BL` - 8-bit register that is the lower 8 bits of B
- `BH` - 8-bit register that is the higher 8 bits of B
- `MN` - 32-bit pseudo-register that contains {MN}
- `C` - 16-bit program counter register
- `C` - 16-bit stack pointer register
- `R0`, `R1`, `R2`, `R3`, `R4`, `R5`, `R6`, `R7` - 16-bit data registers
- `F` - 16-bit flags register [Carry,Less,...(reserved)]


## Instruction Set

### General
- `nop` - no operation
- `mov R/V R` - copy data to a register
- `swp R R` - swap data in two registers
- `slp` - sleep (reserved)
- `hlt` - halt program execution

### Arithmetic
- `add R/V` - add data to accumulator
- `sub R/V` - sub data to accumulator
- `mul R/V` - mul data to accumulator
- `shl R/V` - shift left data to accumulator
- `shr R/V` - shift right data to accumulator
- `cmp R R/V` - compare data, setting E flag if equal, and L flag if first value is less
- `jmp L/A` - jump to an address
- `jeq L/A` - jump to an address if E flag is set
- `jne L/A` - jump to an address if E flag is unset

### Memory
- `ldr R` - load data in memory into R
- `ldr R` - load data in memory into R

### Stack
- `push R` - push R to the stack (increments stack pointer by 2)
- `pop R` - pop R from the stack (decrements stack pointer by 2)
- `call L/A` - push the current program counter incremented by 2 (size of call instruction) to the stack and jump to address
- `ret` - pop the program counter from the stack and jump

## Memory addressing

- When memory is accessed using the memory access instructions, the MN pseudo-register is used to address the location in memory.

## Simulated Components

- approximately 65k bytes of memory
