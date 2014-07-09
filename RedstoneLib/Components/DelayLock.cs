using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class DelayLock : RSComponent {
		public int Delay { get; set; }

		public RSConnection Output { get; private set; }
		public RSConnection Input { get; private set; }
		public RSConnection Lock { get; private set; }

		private Queue<int> memory = new Queue<int>();

		public DelayLock(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
			Input = CreateInput("In");
			Lock = CreateInput("Lock");

			Input.SignalChanged += SignalChangedHandler;
			Lock.SignalChanged += SignalChangedHandler;

			Delay = 1;
		}

		private long lastSignalChangeTick;
		private void SignalChangedHandler(object sender, int oldLevel) {
			if(lastSignalChangeTick == CurrentTick) return;
			lastSignalChangeTick = CurrentTick;

			if(sender == Input) {
				memory.Enqueue(Input.PowerLevel);
				ScheduleAction(UpdateState, CurrentTick + Delay);

			} else if(memory.Count == 0) ScheduleAction(UpdateState, CurrentTick + 1);
		}

		private long lastUpdateTick;
		private void UpdateState() {
			if(lastUpdateTick == CurrentTick) return;
			lastUpdateTick = CurrentTick;

			var powerLevel = memory.Dequeue();
			if(Lock.PowerLevel == 0) ScheduleStimulus(Output, powerLevel);
		}
	}
}
