using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class ShiftRegister : RSComponent {
		public const int InitCellCapacity = 32;

		public bool IsActive { get; protected set; }
		public bool IsLocked { get; protected set; }
		public bool IsAnalog { get; set; }
		public int CellCount { get; set; }

		public RSConnection Output { get; private set; }
		public RSConnection Input { get; private set; }
		public RSConnection Lock { get; private set; }

		private Queue<int> memory;
		private bool actionScheduled;

		public ShiftRegister(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
			Input = CreateInput("In");
			Lock = CreateInput("Lock");

			CellCount = 1;

			Input.SignalChanged += SignalChangedHandler;
			Lock.SignalChanged += SignalChangedHandler;

			memory = new Queue<int>(InitCellCapacity);
		}

		private void SignalChangedHandler(object sender, int powerLevel) {
			if(!actionScheduled) {
				actionScheduled = true;
				ScheduleAction(UpdateState, CurrentTick + 1);
			}
		}

		private long lastTick;
		private void UpdateState() {
			actionScheduled = false;
			if(lastTick == CurrentTick) return;
			lastTick = CurrentTick;

			int outputLevel;
			IsLocked = Lock.PowerLevel > 0;
			if(IsLocked) {
				outputLevel = memory.Count > 0 ? memory.Peek() : Output.PowerLevel;

			} else {
				outputLevel = CellCount > 0 && memory.Count >= CellCount ? memory.Dequeue() : Input.PowerLevel;
				if(memory.Count < CellCount) memory.Enqueue(IsAnalog ? Input.PowerLevel : (Input.PowerLevel > 0 ? RSEngine.MaxPowerLevel : 0));

				if(!memory.All(x => x == outputLevel)) {
					ScheduleAction(UpdateState, CurrentTick + 1);
					actionScheduled = true;
				}

			}

			if(outputLevel != Output.PowerLevel) ScheduleStimulus(Output, outputLevel);

			IsActive = outputLevel > 0;
		}

		public override string ToString() {
			var outStr = Output.PowerLevel > 0 ? "1" : "0";
			var inStr = Input.PowerLevel > 0 ? "1" : "0";
			var lockA = Lock.PowerLevel > 0 ? "<" : "(";
			var lockB = Lock.PowerLevel > 0 ? ">" : ")";

			var memStr = string.Join(",", memory.Reverse().Select(x => IsAnalog ? x.ToString("X") : (x > 0 ? "1" : "0")));

			return inStr + "=>" + lockA + memStr + lockB + "=>" + outStr;
		}
	}
}
