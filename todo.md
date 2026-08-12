# FamiMan learning roadmap

The goal is to learn how an NES works by building a small, understandable emulator. The target is not a fast, cycle-perfect, feature-complete emulator. A good first finish line is:

> Boot an NROM game such as Super Mario Bros., show stable graphics, accept controller input, and play through the first level. Sound can come afterward.

Use a ROM you obtained legally and keep ROM files out of source control.

## Where the project is now

- The solution builds in Debug and Release.
- `FamiMan.Platform` now hides SDL and can display an ARGB framebuffer, draw debug text, and report keyboard events.
- The CPU has implementations and tests for many official 6502 instructions.
- 144 of 146 current tests pass. The known failures are `CLD` and `RTI`.
- The iNES loader and an NROM/mapper-0 class have been started, but their ROM offsets and address translation need correcting.
- The CPU bus is partly mapped, but it cannot yet model memory-mapped register side effects correctly.
- The PPU is mostly a register placeholder; `Ppu.Tick()` is not implemented.
- There is no controller implementation.
- The APU is only a register placeholder. That is fine for now: an NES game can become playable without sound.

## Suggested working style

- [ ] Complete one milestone at a time and keep the emulator runnable after each one.
- [ ] Add a focused test before fixing a CPU, memory, or register behavior.
- [ ] Prefer clear code and explicit state over clever or highly optimized code.
- [ ] Keep a trace/debug mode that can show PC, opcode, registers, flags, stack pointer, and cycle count.
- [ ] Do not start audio, extra mappers, shaders, or UI polish until an NROM game is playable silently.

## Milestone 0: get reoriented

The result of this milestone is a repeatable baseline and an easy way to launch a ROM.

- [ ] Fix the two known tests: `CLD` must clear the decimal flag, and `RTI` must restore status and PC correctly.
- [ ] Run the complete test suite and record the result here.
- [ ] Let the GUI accept a ROM path from the command line instead of hard-coding `files/nestest.nes`.
- [ ] Remove the forced `PC = 0xC000` from normal startup. Use the reset vector at `$FFFC-$FFFD`; only force `$C000` in the special `nestest` runner.
- [ ] Add a friendly error for unsupported or malformed ROMs.
- [ ] Add a separate headless `nestest` mode that prints one trace line per instruction.

Checkpoint:

- [ ] `dotnet test FamiMan.sln` passes.
- [ ] A chosen ROM loads from a command-line path and reaches its reset vector.

## Milestone 1: make the CPU trustworthy

Do not aim for transistor-level timing. First make every official instruction produce the right architectural result, then add the cycle counts needed to coordinate with the PPU.

### CPU state and stack

- [ ] Rework the status register so reading `Carry` or any other flag has no side effects. The current carry getter clears the flag.
- [ ] Give each status flag its documented bit position and ensure bit 5 is handled correctly when pushing/pulling status.
- [ ] Put stack reads and writes in page `$0100-$01FF`, using address `$0100 | SP`.
- [ ] Verify the stack direction and exact order for `PHA`, `PLA`, `PHP`, `PLP`, `JSR`, `RTS`, `BRK`, `RTI`, NMI, and IRQ.
- [ ] Implement realistic reset state: load the reset vector, set the interrupt-disable flag, and initialize the stack pointer.

### Instructions and addressing

- [ ] Verify all 56 official 6502 instructions and all official addressing modes.
- [ ] Fix zero-page indexed addressing so it wraps within `$00-$FF`.
- [ ] Fix indirect addressing wraparound, including the 6502 `JMP ($xxFF)` page-wrap behavior.
- [ ] Verify `(indirect,X)` and `(indirect),Y` pointer reads and zero-page wrapping.
- [ ] Make memory versions of shifts and rotates write back to memory rather than the accumulator.
- [ ] Verify `BIT`: Z comes from `A & value`, while N and V come directly from bits 7 and 6 of the memory value.
- [ ] Verify compare instructions without implementing them by temporarily mutating the accumulator or carry flag.
- [ ] Ensure loads, transfers, increments, decrements, shifts, and pulls update N and Z exactly where required.
- [ ] Implement `BRK` as an interrupt instead of treating it only as a permanent halt.
- [ ] Treat unofficial opcodes as a later compatibility task. Super Mario Bros. does not require them.

### Timing

- [ ] Track the base cycle count for every official opcode.
- [ ] Add the taken-branch cycle and page-crossing branch cycle.
- [ ] Add page-crossing cycles for indexed read instructions where required.
- [ ] Let the CPU finish an instruction and report how many CPU cycles it consumed. This is simpler to learn from than a micro-operation engine and is sufficient for the first emulator milestone.
- [ ] Service pending NMI/IRQ at instruction boundaries with the correct vectors and stack state.

Checkpoint:

- [ ] Match the official-instruction portion of `nestest.log` starting at `$C000`.
- [ ] Pass the official-opcode portions of `instr_test-v5`.
- [ ] Pass a branch timing test before depending on sprite-0 timing.

## Milestone 2: correct the cartridge loader and buses

Explicit `Read(address)` and `Write(address, value)` methods will be much easier to reason about than returning bytes by `ref`. Reads and writes to PPU, controller, and APU registers have different side effects.

### iNES and NROM

- [ ] Parse and validate all four magic bytes: `NES` followed by `$1A`.
- [ ] Correct the current off-by-one PRG-ROM and CHR-ROM slices. Data begins after the 16-byte header, not at byte 15.
- [ ] Honor the optional 512-byte trainer when calculating data offsets.
- [ ] Parse mapper number, nametable mirroring, battery flag, PRG size, and CHR size from the header.
- [ ] Reject unsupported mappers with a clear message.
- [ ] Implement mapper 0/NROM correctly:
  - [ ] 16 KiB PRG-ROM mirrors into both `$8000-$BFFF` and `$C000-$FFFF`.
  - [ ] 32 KiB PRG-ROM maps directly across `$8000-$FFFF`.
  - [ ] 8 KiB CHR-ROM maps at PPU `$0000-$1FFF`.
  - [ ] If CHR size is zero, allocate 8 KiB of writable CHR-RAM.
- [ ] Add unit tests for 16 KiB and 32 KiB NROM mapping and both mirroring modes.

### CPU bus

- [ ] Replace the `ref byte` bus API with explicit reads and writes.
- [ ] Map 2 KiB CPU RAM at `$0000-$07FF` and mirror it through `$1FFF`.
- [ ] Map PPU registers at `$2000-$2007` and mirror them every 8 bytes through `$3FFF`.
- [ ] Route `$4014` to OAM DMA, not to the APU.
- [ ] Route `$4016-$4017` to controllers for reads and `$4016` for the controller strobe.
- [ ] Keep APU register writes at `$4000-$4017` as harmless stored state until audio is implemented.
- [ ] Return a consistent open-bus/default value for genuinely unmapped reads instead of silently aliasing RAM byte zero.

### PPU bus

- [ ] Keep the PPU address space separate from the CPU address space.
- [ ] Route `$0000-$1FFF` through the cartridge to CHR-ROM or CHR-RAM.
- [ ] Add 2 KiB of nametable RAM for `$2000-$2FFF`.
- [ ] Implement horizontal and vertical nametable mirroring from the cartridge header. Super Mario Bros. relies on horizontal scrolling with vertical nametable arrangement.
- [ ] Mirror `$3000-$3EFF` to `$2000-$2EFF`.
- [ ] Add 32 bytes of palette RAM at `$3F00-$3F1F`, including the special universal-background-color mirrors.
- [ ] Mirror all PPU addresses above `$3FFF` back into the 14-bit address space.

Checkpoint:

- [ ] Unit tests can read reset/NMI/IRQ vectors through the CPU bus.
- [ ] Unit tests can write/read nametable and palette data through the PPU bus with correct mirroring.

## Milestone 3: implement PPU registers and frame timing

Build the register behavior before rendering pixels. Games use these registers to initialize graphics and wait for vblank.

- [ ] Track PPU scanline, dot, and frame number for an NTSC PPU: 262 scanlines and 341 dots per scanline.
- [ ] Clock the PPU three times for every CPU cycle.
- [ ] Set the vblank flag at the start of vblank and clear it on the pre-render line.
- [ ] Generate an NMI when vblank begins and NMI is enabled in `PPUCTRL`.
- [ ] Implement `PPUCTRL` (`$2000`) and `PPUMASK` (`$2001`) stored state.
- [ ] Implement `PPUSTATUS` (`$2002`) read behavior: return status, clear vblank, and reset the `$2005/$2006` write latch.
- [ ] Implement `OAMADDR` and `OAMDATA` (`$2003/$2004`) with 256 bytes of OAM.
- [ ] Implement the two-write latch for `PPUSCROLL` (`$2005`).
- [ ] Implement the two-write latch for `PPUADDR` (`$2006`).
- [ ] Implement `PPUDATA` (`$2007`) reads/writes, address increment by 1 or 32, and the delayed read buffer outside palette RAM.
- [ ] Implement OAM DMA on `$4014`: copy 256 CPU-bus bytes into OAM and stall the CPU for 513 or 514 cycles.

Checkpoint:

- [ ] A small test ROM can wait for vblank without hanging.
- [ ] Pass basic PPU register, palette, vblank, and NMI test ROMs before starting full rendering.

## Milestone 4: render a picture in small stages

Write directly into a `uint[256 * 240]` ARGB framebuffer and give it to `GameWindow.DrawFrame`. There is no need for sprites, textures, or tiles in the platform library; decoding NES graphics is the PPU's job.

### Background first

- [ ] Add a fixed 64-color NES palette lookup table.
- [ ] Decode one 8x8 CHR tile from its two bitplanes and test the resulting color indices.
- [ ] Render a single nametable without scrolling.
- [ ] Apply attribute-table palette selection.
- [ ] Apply background palette RAM and the universal background color.
- [ ] Respect background enable and left-edge clipping bits in `PPUMASK`.
- [ ] Present one completed 256x240 framebuffer through `FamiMan.Platform`.

### Scrolling

- [ ] Implement coarse X/Y, fine X/Y, and nametable selection from `$2000`, `$2005`, and `$2006`.
- [ ] Render across nametable boundaries using the cartridge's mirroring mode.
- [ ] Move scrolling state at the correct PPU dots, or begin with a scanline-based approximation and improve it when tests or Super Mario Bros. expose a problem.

### Sprites

- [ ] Render 8x8 sprites from OAM.
- [ ] Implement sprite palette selection, horizontal/vertical flip, background priority, and left-edge clipping.
- [ ] Limit a scanline to eight visible sprites. Exact sprite-overflow bug emulation can wait.
- [ ] Implement sprite-0 hit. Super Mario Bros. uses it to split the fixed status bar from the scrolling playfield.
- [ ] Add 8x16 sprite support after 8x8 sprites are working.

Checkpoint:

- [ ] A graphics test ROM displays correct tiles and palettes.
- [ ] Super Mario Bros. reaches a recognizable title screen.
- [ ] The title screen and first level scroll without tearing, and the status bar remains stable.

## Milestone 5: controller input and a frame-based run loop

- [ ] Represent the eight controller buttons: A, B, Select, Start, Up, Down, Left, Right.
- [ ] Map keyboard keys from `FamiMan.Platform` to controller 1.
- [ ] Implement the `$4016` strobe/latch behavior.
- [ ] Return one latched button bit per read from `$4016`, in NES button order.
- [ ] Implement controller 2 at `$4017` only after controller 1 works.
- [ ] Change the GUI loop to run the emulated machine until the PPU completes a frame, then present that frame.
- [ ] Keep emulation near NTSC speed without tying emulated CPU correctness to wall-clock timing.
- [ ] Add pause, reset, and optional single-frame/single-instruction debug controls.

Checkpoint — first playable milestone:

- [ ] Start Super Mario Bros. from its title screen.
- [ ] Move, jump, enter a pipe, and finish or die in World 1-1.
- [ ] Video and input remain stable for several minutes.

## Milestone 6: add sound after the game is playable

Sound is a valuable NES topic, but it should not block the first playable result.

- [ ] Add an audio-sample submission API to `FamiMan.Platform`.
- [ ] Implement APU frame-counter timing.
- [ ] Implement one pulse channel and verify pitch, duty cycle, envelope, and length counter.
- [ ] Add the second pulse channel, triangle channel, and noise channel.
- [ ] Mix channels using a simple approximation first.
- [ ] Add sweep units and more accurate nonlinear mixing when the basic music is recognizable.
- [ ] Implement DMC last; handle its IRQ and CPU stalls only when needed.
- [ ] Buffer/resample output so normal frame-time variation does not cause crackling.

Checkpoint:

- [ ] Super Mario Bros. music and common sound effects are recognizable and remain synchronized with gameplay.

## Deliberately out of scope until later

- Other cartridge mappers or broad game compatibility.
- PAL/Dendy timing.
- Unofficial 6502 opcodes unless a chosen test requires them.
- Cycle-exact dummy reads/writes except where a game-visible behavior requires them.
- Exact sprite-overflow hardware bugs, PPU open bus, analog NTSC artifacts, and obscure DMA conflicts.
- Save states, rewind, netplay, achievements, shader filters, installers, and polished ROM browsers.
- Performance optimization before profiling demonstrates a real problem.

## Useful references and tests

- [NESdev emulator tests](https://www.nesdev.org/wiki/Emulator_tests) — use `nestest` first, then focused CPU/PPU test ROMs.
- [CPU memory map](https://www.nesdev.org/wiki/CPU_memory_map)
- [CPU interrupts](https://www.nesdev.org/wiki/CPU_interrupts)
- [iNES file format](https://www.nesdev.org/wiki/INES)
- [NROM / mapper 0](https://www.nesdev.org/wiki/NROM)
- [PPU overview](https://www.nesdev.org/wiki/PPU)
- [PPU registers](https://www.nesdev.org/wiki/PPU_registers)
- [PPU memory map](https://www.nesdev.org/wiki/PPU_memory_map)
- [PPU rendering](https://www.nesdev.org/wiki/PPU_rendering)
- [Nametable mirroring](https://www.nesdev.org/wiki/PPU_nametables)
- [OAM DMA](https://www.nesdev.org/wiki/DMA)
- [Controller reading](https://www.nesdev.org/wiki/Controller_reading)

## What to do next

Start with Milestone 0, then the first three CPU-state tasks in Milestone 1:

1. Fix `CLD` and `RTI` until the current suite is green.
2. Replace the status-register representation so flag reads are harmless and bit positions are obvious.
3. Correct all stack accesses to use page `$0100`.
4. Add a `nestest` trace comparison and use the first mismatching line as the next small problem.

Do not start the PPU until the official portion of `nestest` agrees with the reference trace. That checkpoint will save a great deal of confusing cross-component debugging later.
