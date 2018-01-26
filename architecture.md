
# FcChip [FCH] Architecture

## Architecture summary

Word size: 16 bits

## Register Set

- `A` - 16-bit accumulator register
- `B` - 16-bit general purpose register
- `M` - 16-bit general purpose register
- `N` - 16-bit general purpose register
- `BL` - 8-bit register that is the lower 8 bits of B
- `BH` - 8-bit register that is the higher 8 bits of B
- `MN` - 32-bit pseudo-register that contains {MN}
- `C` - 16-bit program counter register


## Instruction Set

### General
- `nop` - no operation
- `mov R/V R` - copy data to a register
- `swp R R` - swap data in two registers
- `slp` - sleep (reserved)

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

## Memory addressing

When memory is accessed using the memory access instructions, the MN pseudo-register is used to address the location in memory.

