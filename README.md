
# FcChip

A simple RISC-inspired CPU architecture

## Cool stuff

- Some kind of inter-chip I/O system is planned soon.

## Contents

- `FcAssembler` - An implementation of a simple assembler for FcChip assembly
- `FcChip` - A library defining the instruction set and register set of FcChip. Contains the `FcVirtualChip` implementation of a virtual executor for programs in binary format (produced by FcAssembler).
- `FcChipTool` - A command-line utility to run FcChip binary programs on virtual chips with customized parameters.

## FcChip Architecture

See the [Architecture Documentation](architecture.md) for a detailed overview of the FcChip architecture.

## License
Copyright &copy; Nihal Talur (0xFireball) 2018. All Rights Reserved.
