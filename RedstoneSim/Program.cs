﻿using RedstoneLib;
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
			Playground();
		}

		private static void Playground() {
			var engine = new RSEngine();

			var rs = new SerialMemory[9];

			for(int i = 0; i < rs.Length; i++) {
				rs[i] = new SerialMemory(engine) { Label = ((char)('A' + i)).ToString() };
				if(i > 0) RSBridge.Connect(rs[i - 1].Output, rs[i].Input);

				rs[i].CellCount = 1;
			}
			RSBridge.Connect(rs.Last().Output, rs[0].Input);

			var clock = new Clock(engine);
			clock.PulseWidth = clock.IdleWidth = 2;
			clock.Enabled = true;

			RSBridge.Connect(clock.Output, rs[0].Input);


			var compositeConnection = ((clock.Output.ToLogic() & clock.Output.ToLogic()) + 10).ToConnection();

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

			var register = new SerialMemory(engine);
			register.CellCount = 16;

			var edgeDetector = new EdgeDetector(engine) {
				Delay = 1,
				IdlePowerLevel = RSEngine.MaxPowerLevel,
				PulsePowerLevel = 0,
				PulseWidth = 17
			};

			var delayLock = new DelayLock(engine);

			var signal = new SignalQueue(engine);
			signal.AddSequence(new[] { 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 });

			RSBridge.Connect(signal.Output, register.Input, edgeDetector.Input);
			RSBridge.Connect(edgeDetector.Output, register.Lock, delayLock.Lock);
			RSBridge.Connect(register.Output, delayLock.Input);

			while(true) {
				Console.Write("T" + engine.CurrentTick + "\t");
				Console.WriteLine(register + "  " + delayLock.Output.PowerLevel);

				engine.DoTick();
			}
		}


		private static void Playground3() {
			var engine = new RSEngine();

			var rs = new DelayLock[10241];

			for(int i = 0; i < rs.Length; i++) {
				rs[i] = new DelayLock(engine) { Delay = 1 };
				if(i > 0) RSBridge.Connect(rs[i - 1].Output, rs[i].Input);
			}
			RSBridge.Connect(rs.Last().Output, rs[0].Input, rs[1].Output);

			var sw = new Stopwatch();
			while(true) {
				Console.Write("T" + engine.CurrentTick + "\t");
				sw.Restart();
				engine.DoTick();
				sw.Stop();

				if(engine.CurrentTick == 1) engine.ScheduleStimulus(rs[0].Input, 9);
				if(engine.CurrentTick == 2) engine.ScheduleStimulus(rs[0].Input, 0);

				Console.WriteLine(sw.ElapsedMilliseconds + "ms");
				//Console.WriteLine(string.Concat(rs.Take(60).Select(x => x.Input.PowerLevel)));
			}
		}
	}
}
