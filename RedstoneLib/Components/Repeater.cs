using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Repeater : RSComponent {
		public const int MaxDelay = 5;

		public bool IsActive { get; protected set; }
		public bool IsLocked { get; protected set; }
		public int Delay { get; set; }

		public RSConnection Output { get; private set; }
		public RSConnection Input { get; private set; }
		public RSConnection Lock { get; private set; }

		private Queue<bool> memory;
		private bool actionScheduled;

		public Repeater(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
			Input = CreateInput("In");
			Lock = CreateInput("Lock");

			Delay = 1;

			Input.SignalChanged += (object sender, int powerLevel) => {
				if(!actionScheduled) {
					actionScheduled = true;
					ScheduleAction(UpdateState, CurrentTick + 1);
				}
			};

			Lock.SignalChanged += (s, p) => ScheduleAction(UpdateState, CurrentTick + 1);

			memory = new Queue<bool>(MaxDelay);
			for(int i = 1; i < Delay; i++) memory.Enqueue(false);
		}

		private long lastTick;
		private void UpdateState() {
			actionScheduled = false;
			if(lastTick == CurrentTick) return;
			lastTick = CurrentTick;

			if(memory.Count < Delay) memory.Enqueue(Input.PowerLevel > 0);

			var outputLevel = memory.Count >= Delay && memory.Dequeue() ? RSEngine.MaxPowerLevel : 0;
			if(Lock.PowerLevel > 0) outputLevel = Output.PowerLevel;

			if(outputLevel != Output.PowerLevel) ScheduleStimulus(Output, outputLevel);

			if(!memory.All(x => x == outputLevel > 0)) {
				ScheduleAction(UpdateState, CurrentTick + 1);
				actionScheduled = true;
			}

			IsLocked = Lock.PowerLevel > 0;
			IsActive = outputLevel > 0;
		}


		public override string ToString() {
			var outStr = Output.PowerLevel > 0 ? "1" : "0";
			var inStr = Input.PowerLevel > 0 ? "1" : "0";
			var lockA = Lock.PowerLevel > 0 ? "<" : "(";
			var lockB = Lock.PowerLevel > 0 ? ">" : ")";
			var memStr = string.Join(",", memory.Reverse().Select(x => x ? "1" : "0"));

			return inStr + "=>" + lockA + memStr + lockB + "=>" + outStr;
		}
	}
}
