using FamiMan.Core;
using FamiMan.Platform;
using System.Diagnostics;

namespace FamiMan.GUI.UI
{
    internal sealed class DebugOverlay
    {
        public const int SidebarWidth = 400;

        private int _waitTime;
        private bool _breaked;
        private ushort _lastPC;
        private string _debugText = string.Empty;
        private readonly Stopwatch _fpsTimer = Stopwatch.StartNew();
        private int _framesSinceFpsUpdate;
        private double _framesPerSecond;
        private string[]? _failureLines;

        public int WaitTime => _waitTime;
        public bool Visible { get; private set; }
        public int LeftInset => Visible ? SidebarWidth : 0;

        public void HandleKeyDown(Key key)
        {
            switch (key)
            {
                case Key.D:
                    Visible = !Visible;
                    break;
                case Key.Down:
                    _waitTime++;
                    break;
                case Key.Up:
                    if (_waitTime > 0)
                        _waitTime--;
                    break;
            }
        }

        public void FramePresented()
        {
            _framesSinceFpsUpdate++;

            if (_fpsTimer.Elapsed.TotalSeconds < 1)
                return;

            _framesPerSecond = _framesSinceFpsUpdate / _fpsTimer.Elapsed.TotalSeconds;
            _framesSinceFpsUpdate = 0;
            _fpsTimer.Restart();
        }

        public void ShowFailure(Exception exception, Bus bus, Cpu cpu)
        {
            Visible = true;

            Exception cause = exception.GetBaseException();
            string opcode = TryReadOpcode(bus, cpu.PC);
            var lines = new List<string>
            {
                "EMULATION HALTED",
                cause.GetType().Name,
                $"PC: ${cpu.PC:X4}  Opcode: {opcode}",
                $"A:${cpu.A:X2} X:${cpu.X:X2} Y:${cpu.Y:X2} SP:${cpu.SP:X2}",
                $"P:${cpu.P.AsByte():X2}  CPU ticks:{cpu.Ticks}",
                $"PPU scanline:{bus.Ppu.Scanline} cycle:{bus.Ppu.Cycle}",
                string.Empty
            };

            lines.AddRange(WrapText(cause.Message, 42));
            lines.Add(string.Empty);
            lines.Add("See the terminal for the stack trace.");
            _failureLines = lines.ToArray();

            Console.Error.WriteLine("Emulation halted with the following state:");
            Console.Error.WriteLine(string.Join(Environment.NewLine, _failureLines));
            Console.Error.WriteLine(exception);
        }

        public void UpdateInstructionDebug(Bus bus, Cpu cpu)
        {
            if (_breaked || cpu.PC == _lastPC || cpu.Waiting)
                return;

            Instruction instruction = cpu.CurrentInstruction;
            byte stackPos1 = (byte)(cpu.SP + 1);
            byte stackPos2 = (byte)(cpu.SP + 2);
            ushort addr = (ushort)(bus.Read((ushort)(cpu.PC + 1)) + (bus.Read((ushort)(cpu.PC + 2)) << 8));

            if (instruction == Instruction.RTS)
                _debugText = " -> " + BitConverter.ToUInt16(new byte[2] { bus.Read(stackPos2), bus.Read(stackPos1) }, 0).ToString("X");
            else if (instruction is Instruction.JSR or Instruction.JMP)
            {
                var opcode = Opcodes.Find(bus.Read(cpu.PC));
                _debugText = opcode.AddressingMode == AddressingMode.Indirect
                    ? " (->) " + addr.ToString("X")
                    : " -> " + addr.ToString("X");
            }
            else if (instruction is Instruction.BNE or Instruction.BEQ)
            {
                addr = bus.Read((ushort)(cpu.PC + 1));
                int jumpRelative = addr > 127 ? (addr - 255) : addr;
                int jumpTo = (int)cpu.PC + jumpRelative + 1 - 2;
                bool branchTaken = instruction == Instruction.BNE ? !cpu.P.Zero : cpu.P.Zero;
                _debugText = (branchTaken ? " -> " : " !! ") + jumpTo.ToString("X");
            }
            else if (instruction == Instruction.PHA)
                _debugText = " S=" + cpu.A.ToString("X") + " SP=" + (cpu.SP - 1).ToString("X");
            else
                _debugText = string.Empty;

            Console.WriteLine(cpu.PC.ToString("X") + " - " + instruction + _debugText);
            _lastPC = cpu.PC;
            if (instruction == Instruction.BRK)
                _breaked = true;
        }

        public void Render(GameWindow window, Bus bus, Cpu cpu)
        {
            if (!Visible)
                return;

            if (_failureLines is not null)
            {
                for (int row = 0; row < _failureLines.Length; row++)
                    WriteRow(window, _failureLines[row], row);
                return;
            }

            WriteRow(window, "PC: " + cpu.PC.ToString("X") + " - " + cpu.CurrentInstruction, 0);
            WriteRow(window, "A: " + cpu.A, 1);
            WriteRow(window, "X: " + cpu.X, 2);
            WriteRow(window, "Y: " + cpu.Y, 3);
            WriteRow(window, "SP: " + cpu.SP.ToString("X") + " " + (cpu.SP != 0xFD ? bus.Read(cpu.SP).ToString("X") : ""), 4);

            var statusByte = cpu.P.AsByte();
            WriteRow(window, "P: " + statusByte + " (0x" + statusByte.ToString("X2") + ")", 5);
            WriteRow(window, BuildFlagsText(cpu.P), 6);
            WriteRow(window, "Ticks: " + cpu.Ticks, 7);
            WriteRow(window, "Waiting: " + cpu.Waiting, 8);
            WriteRow(window, "Last debug: " + _debugText, 9);
            WriteRow(window, "Wait: " + _waitTime, 10);
            WriteRow(window, $"FPS: {_framesPerSecond:F1}", 11);
        }

        private static void WriteRow(GameWindow window, string text, int row) =>
            window.DrawText(text, 15, 15 + 30 * row, Color.White);

        private static string BuildFlagsText(Cpu.StatusRegisters status)
        {
            byte statusByte = status.AsByte();
            string BitValue(int bit) => ((statusByte >> bit) & 1) == 1 ? "1" : "0";
            return $"Flags N:{BitValue(7)} V:{BitValue(6)} B:{BitValue(4)} D:{BitValue(3)} I:{BitValue(2)} Z:{BitValue(1)} C:{BitValue(0)}";
        }

        private static string TryReadOpcode(Bus bus, ushort pc)
        {
            try
            {
                return $"${bus.Read(pc):X2}";
            }
            catch
            {
                return "unavailable";
            }
        }

        private static IEnumerable<string> WrapText(string text, int width)
        {
            if (string.IsNullOrWhiteSpace(text))
                return ["No error message was provided."];

            var lines = new List<string>();
            string remaining = text;

            while (remaining.Length > width)
            {
                int split = remaining.LastIndexOf(' ', width);
                if (split <= 0)
                    split = width;

                lines.Add(remaining[..split]);
                remaining = remaining[split..].TrimStart();
            }

            if (remaining.Length > 0)
                lines.Add(remaining);

            return lines;
        }
    }
}
