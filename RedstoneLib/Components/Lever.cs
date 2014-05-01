using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Lever : RSComponent {
		public RSConnection Output { get; private set; }

		public bool IsActivated { get; private set; }
		private bool actionPending;

		public Lever(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
		}

		public void SetState(bool active) {
			IsActivated = active;

			if(!actionPending) {
				actionPending = true;
				ScheduleAction(UpdateState, CurrentTick + 1);
			}
		}

		private void UpdateState() {
			if(Output.PowerLevel == 0) {
				ScheduleStimulus(Output, RSEngine.MaxPowerLevel);
				if(!IsActivated) ScheduleAction(UpdateState, CurrentTick + 1);
			} else if(!IsActivated) {
				ScheduleStimulus(Output, 0);
			}
			actionPending = false;
		}
	}
}
