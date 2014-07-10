using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Lever : RSComponent {
		public RSConnection Output { get; private set; }

		public bool IsActivated { get; private set; }

		public Lever(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
		}

		private bool actionPending;
		public void SetState(bool active) {
			IsActivated = active;

			if(!actionPending) {
				actionPending = true;
				ScheduleAction(UpdateState, CurrentTick + 1);
			}
		}

		private void UpdateState() {
			if(IsActivated != (Output.PowerLevel != 0)) {
				ScheduleStimulus(Output, IsActivated ? RSEngine.MaxPowerLevel : 0);
			}
			actionPending = false;
		}
	}
}
