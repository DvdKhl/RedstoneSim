using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	class SignalGenerator : RSComponent {
		public RSConnection Output { get; private set; }

		public Func<long, int> Next { get; set; }

		public SignalGenerator(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");

			ScheduleAction(UpdateState, CurrentTick + 1);
		}

		private long lastUpdateTick;
		private void UpdateState() {
			if(lastUpdateTick == CurrentTick) return;
			lastUpdateTick = CurrentTick;

			if(Next != null) {
				var nextPowerLevel = Next(CurrentTick);
				if(nextPowerLevel != Output.PowerLevel) ScheduleStimulus(Output, nextPowerLevel);
			}
			ScheduleAction(UpdateState, CurrentTick + 1);
		}
	}
}
