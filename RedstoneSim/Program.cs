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
			var engine = new RSEngine();

			var rs = new Repeater[9];

			for(int i = 0; i < rs.Length; i++) {
				rs[i] = new Repeater(engine) { Label = ((char)('A' + i)).ToString() };
				if(i > 0) RSBridge.Create(rs[i - 1].Output, rs[i].Input);
			}
			var fwBridge = RSBridge.Create(rs.Last().Output, rs[0].Input, rs[1].Output);
			fwBridge.Label = "B1";

			var lever = new Lever(engine);
			RSBridge.Create(lever.Output, rs[2].Input);


			var sw = new Stopwatch();

			while(true) {
				switch(engine.CurrentTick) {
					case 1: lever.SetState(true); break;
					case 2: lever.SetState(false); break;

					//case 8: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, RSEngine.MaxPowerLevel); break;
					//case 16: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, 0); break;
					//case 6: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, RSEngine.MaxPowerLevel); break;
					//case 7: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, 0); break;
					//case 8: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, RSEngine.MaxPowerLevel); break;
					//case 9: for(int i = 0; i < rs.Length; i++) engine.ScheduleStimulus(rs[i].Lock, 0); break;
				}

				if((engine.CurrentTick % 10) < 5) {
					if((engine.CurrentTick % 2) == 0) {
						Console.ForegroundColor = ConsoleColor.Gray;
					} else {
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
				} else {
					if((engine.CurrentTick % 2) == 0) {
						Console.ForegroundColor = ConsoleColor.Gray;
					} else {
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
				}


				Console.Write("T" + engine.CurrentTick + "\t");

				sw.Restart();
				engine.DoTick();
				sw.Stop();

				for(int i = 0; i < rs.Length; i++) Console.Write(rs[i]);
				//Console.Write(string.Concat(rs.Select(x => x.Output.PowerLevel > 0 ? "1" : "0")));
				Console.WriteLine(" after " + sw.ElapsedMilliseconds + "ms");
			}


		}
	}
}
