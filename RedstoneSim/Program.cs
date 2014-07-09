using RedstoneLib;
using RedstoneLib.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneSim {
	class Program {
		static void Main(string[] args) {
			Playground2();
		}

		private static void Playground() {
			var engine = new RSEngine();

			var rs = new ShiftRegister[9];

			for(int i = 0; i < rs.Length; i++) {
				rs[i] = new ShiftRegister(engine) { Label = ((char)('A' + i)).ToString() };
				if(i > 0) RSBridge.Connect(rs[i - 1].Output, rs[i].Input);

				rs[i].CellCount = 1;
			}
			RSBridge.Connect(rs.Last().Output, rs[0].Input);

			var clock = new Clock(engine);
			clock.HighWidth = clock.LowWidth = 2;
			clock.Enabled = true;

			RSBridge.Connect(clock.Output, rs[0].Input);


			var logicGate = new LogicGate(engine);
			logicGate.Logic = clock.Output.ToLogic() & clock.Output.ToLogic();

			var sw = new Stopwatch();



			while(true) {
				Console.Write("T" + engine.CurrentTick + "\t");

				sw.Restart();
				engine.DoTick();
				sw.Stop();

				for(int i = 0; i < rs.Length; i++) Console.Write(rs[i]);
				//Console.Write(string.Concat(rs.Select(x => x.Output.PowerLevel > 0 ? "1" : "0")));
				Console.WriteLine(" after " + sw.ElapsedMilliseconds + "ms");
			}
		}

		private static void Playground2() {
			var engine = new RSEngine();

			var register = new ShiftRegister(engine);
			register.CellCount = 16;

			var edgeDetector = new EdgeDetector(engine) {
				Delay = 1,
				IdlePowerLevel = RSEngine.MaxPowerLevel,
				PulsePowerLevel = 0,
				PulseWidth = 16
			};

			var signal = new SignalQueue(engine);
			signal.AddSequence(new[] { 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 });

			RSBridge.Connect(signal.Output, register.Input, edgeDetector.Input);
			RSBridge.Connect(register.Lock, edgeDetector.Output);


			while(true) {
				Console.Write("T" + engine.CurrentTick + "\t");
				Console.WriteLine(register);

				engine.DoTick();
			}
		}
	}
}
