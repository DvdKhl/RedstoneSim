using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class SignalQueue : RSComponent {

		public RSConnection Output { get; private set; }

		private Queue<int> memory = new Queue<int>();

		public SignalQueue(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");

		}

		public void AddSequence(int[] seq) {
			if(seq.Length == 0) return;

			if(memory.Count == 0) ScheduleAction(UpdateState, CurrentTick + 1);
			foreach(var item in seq) memory.Enqueue(item);
		}

		private long lastUpdateTick;
		private void UpdateState() {
			if(lastUpdateTick == CurrentTick) return;
			lastUpdateTick = CurrentTick;

			ScheduleStimulus(Output, memory.Dequeue());
			if(memory.Count != 0) ScheduleAction(UpdateState, CurrentTick + 1);
		}

	}
}
