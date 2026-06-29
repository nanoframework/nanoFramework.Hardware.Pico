[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_nanoFramework.Hardware.Pico&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_nanoFramework.Hardware.Pico) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_nanoFramework.Hardware.Pico&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_nanoFramework.Hardware.Pico) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.Hardware.Pico.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Hardware.Pico/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

### Welcome to the .NET **nanoFramework** nanoFramework.Hardware.Pico Library repository

## Programmable I/O (PIO)

This library provides a managed API for the **Programmable I/O (PIO)** subsystem of the Raspberry Pi Pico family: the RP2040 (Pico 1 / Pico W) and the RP2350 (Pico 2). PIO lets you implement deterministic, hardware-timed digital protocols on dedicated state machines instead of bit-banging them from the CPU.

The API mirrors the [Pico SDK](https://www.raspberrypi.com/documentation/pico-sdk/)'s PIO model while staying idiomatic C#:

* `PioAssembler` — an inline assembler that builds a PIO program from fluent instruction calls (`Jmp`, `Wait`, `In`, `Out`, `Set`, `Mov`, `Push`, `Pull`, `Irq`, `Nop`, ...) with labels, side-set and delay support.
* `PioProgram` — a fully assembled program (encoded instructions plus wrap, side-set and shift metadata) produced by `PioAssembler.Build()`. You can also wrap externally produced opcodes (for example from the stand-alone `pioasm` tool) with `PioProgram.FromEncoded`.
* `PioBlock` — one of the PIO blocks (two on RP2040 / RP2350A, three on RP2350B). It loads programs into the shared instruction memory, claims state machines, routes GPIOs to the block, and raises managed events on PIO interrupts.
* `PioStateMachine` — one of a block's four state machines. Initialize it from a `PioStateMachineConfig`, enable it, and exchange 32-bit words through the TX/RX FIFOs (`Put`/`Get`, `TryPut`/`TryGet`).
* `PioStateMachineConfig` — fluent configuration (pin mappings, clock divider, shift behaviour, FIFO join, ...). Seed it from a program with `PioStateMachineConfig.FromProgram` so wrap, side-set and shift defaults are applied automatically.

The entry point is the static `Pio.Get(index)` which returns a `PioBlock`.

### Usage

The following is a tiny "blink" PIO program. It toggles a GPIO continuously, with the on/off duration controlled by the value pushed into the state machine's TX FIFO:

```csharp
using nanoFramework.Hardware.Pico.Pio;
using System.Threading;

const int LedPin = 25;

// Assemble a minimal blink program:
//   pull a delay count from the FIFO, drive the pin high for that long,
//   then low for that long, and wrap.
var asm = new PioAssembler();

var loop = asm.DefineLabel();
var high = asm.DefineLabel();
var low = asm.DefineLabel();

asm.MarkLabel(loop);
asm.Pull();                          // OSR <- TX FIFO
asm.Mov(PioDest.X, PioSrc.Osr);      // X = delay count
asm.Set(PioDest.Pins, 1);            // pin high
asm.MarkLabel(high);
asm.Jmp(PioCondition.XPostDec, high).Delay(1);

asm.Mov(PioDest.X, PioSrc.Osr);      // reload delay count
asm.Set(PioDest.Pins, 0);            // pin low
asm.MarkLabel(low);
asm.Jmp(PioCondition.XPostDec, low).Delay(1);

asm.Jmp(loop);

PioProgram program = asm.Build();

// Load it onto a PIO block and claim a free state machine.
PioBlock block = Pio.Get(0);
uint offset = block.AddProgram(program);
PioStateMachine sm = block.ClaimStateMachine();

// Route the LED GPIO to the block and configure the state machine from the program.
block.InitGpio(LedPin);

PioStateMachineConfig config = PioStateMachineConfig
    .FromProgram(program, (int)offset)
    .SetPins(LedPin, 1)              // SET targets the LED pin
    .ClockDivisor(65536.0f);         // slow it right down so the blink is visible

sm.Init(offset, config);
sm.SetConsecutivePinDirs(LedPin, 1, true); // LED pin is an output
sm.Enabled = true;

// Drive it: each value is the on/off duration (in SM cycles) for one blink.
while (true)
{
    sm.Put(50);
    Thread.Sleep(500);
}
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
