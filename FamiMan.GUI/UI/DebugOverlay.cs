using FamiMan.Core;
using SDL2;
using static SDL2.SDL;

namespace FamiMan.GUI.UI
{
#if DEBUG
    internal sealed class DebugOverlay
    {
        private int _waitTime;
        private bool _breaked;
        private ushort _lastPC;
        private string _debugText = string.Empty;

        public int WaitTime => _waitTime;

        public void HandleKeyDown(SDL_Keycode keycode)
        {
            switch (keycode)
            {
                case SDL_Keycode.SDLK_DOWN:
                    _waitTime++;
                    break;
                case SDL_Keycode.SDLK_UP:
                    if (_waitTime > 0)
                        _waitTime--;
                    break;
            }
        }

        public void UpdateInstructionDebug(Bus bus, Cpu cpu)
        {
            if (_breaked || cpu.PC == _lastPC || cpu.Waiting)
                return;

            byte stackPos1 = (byte)(cpu.SP + 1);
            byte stackPos2 = (byte)(cpu.SP + 2);
            ushort addr = (ushort)(bus[(ushort)(cpu.PC + 1)] + (bus[(ushort)(cpu.PC + 2)] << 8));

            if (cpu.CurrentInstructionName == "RTS")
                _debugText = " -> " + BitConverter.ToUInt16(new byte[2] { bus[stackPos2], bus[stackPos1] }, 0).ToString("X");
            else if (cpu.CurrentInstructionName == "JSR" || cpu.CurrentInstructionName == "JMP")
            {
                var opcode = Opcodes.Find(bus[cpu.PC]);
                _debugText = opcode.OpcodeVersionName == "Indirect"
                    ? " (->) " + addr.ToString("X")
                    : " -> " + addr.ToString("X");
            }
            else if (cpu.CurrentInstructionName == "BNE" || cpu.CurrentInstructionName == "BEQ")
            {
                addr = bus[(ushort)(cpu.PC + 1)];
                int jumpRelative = addr > 127 ? (addr - 255) : addr;
                int jumpTo = (int)cpu.PC + jumpRelative + 1 - 2;
                bool branchTaken = cpu.CurrentInstructionName == "BNE" ? !cpu.P.Zero : cpu.P.Zero;
                _debugText = (branchTaken ? " -> " : " !! ") + jumpTo.ToString("X");
            }
            else if (cpu.CurrentInstructionName == "PHA")
                _debugText = " S=" + cpu.A.ToString("X") + " SP=" + (cpu.SP - 1).ToString("X");
            else
                _debugText = string.Empty;

            Console.WriteLine(cpu.PC.ToString("X") + " - " + cpu.CurrentInstructionName + _debugText);
            _lastPC = cpu.PC;
            if (cpu.CurrentInstructionName == "BRK")
                _breaked = true;
        }

        public void Render(IntPtr renderer, SDL_Rect messageRect, IntPtr font, SDL_Color white, Bus bus, Cpu cpu)
        {
            UI.WriteText(renderer, messageRect, font, white, "PC: " + cpu.PC.ToString("X") + " - " + cpu.CurrentInstructionName, 0);
            UI.WriteText(renderer, messageRect, font, white, "A: " + cpu.A, 1);
            UI.WriteText(renderer, messageRect, font, white, "X: " + cpu.X, 2);
            UI.WriteText(renderer, messageRect, font, white, "Y: " + cpu.Y, 3);
            UI.WriteText(renderer, messageRect, font, white, "SP: " + cpu.SP.ToString("X") + " " + (cpu.SP != 0xFD ? bus[cpu.SP].ToString("X") : ""), 4);

            var statusByte = cpu.P.AsByte();
            UI.WriteText(renderer, messageRect, font, white, "P: " + statusByte + " (0x" + statusByte.ToString("X2") + ")", 5);
            UI.WriteText(renderer, messageRect, font, white, BuildFlagsText(cpu.P), 6);
            UI.WriteText(renderer, messageRect, font, white, "Ticks: " + cpu.Ticks, 7);
            UI.WriteText(renderer, messageRect, font, white, "Waiting: " + cpu.Waiting, 8);
            UI.WriteText(renderer, messageRect, font, white, "Last debug: " + _debugText, 9);
            UI.WriteText(renderer, messageRect, font, white, "Wait: " + _waitTime, 10);
        }

        private static string BuildFlagsText(Cpu.StatusRegisters status)
        {
            byte statusByte = status.AsByte();
            string BitValue(int bit) => ((statusByte >> bit) & 1) == 1 ? "1" : "0";
            return $"Flags N:{BitValue(7)} V:{BitValue(6)} B:{BitValue(4)} D:{BitValue(3)} I:{BitValue(2)} Z:{BitValue(1)} C:{BitValue(0)}";
        }
    }
#else
    internal sealed class DebugOverlay
    {
        public int WaitTime => 0;

        public void HandleKeyDown(SDL_Keycode keycode) { }

        public void UpdateInstructionDebug(Bus bus, Cpu cpu) { }

        public void Render(IntPtr renderer, SDL_Rect messageRect, IntPtr font, SDL_Color white, Bus bus, Cpu cpu) { }
    }
#endif
}
