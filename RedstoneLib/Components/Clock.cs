using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Clock : RSComponent {
		public RSConnection Output { get; private set; }

		private bool enabled;
		public bool Enabled { get { return enabled; } set { enabled = value; ScheduleUpdate(); } }

		public int LowPowerLevel { get; set; }
		public int HighPowerLevel { get; set; }
		public int HighWidth { get; set; }
		public int LowWidth { get; set; }

		public Clock(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");

			LowPowerLevel = 0;
			HighPowerLevel = RSEngine.MaxPowerLevel;

			ScheduleUpdate();
		}

		private bool isLow = false;
		private bool isActionScheduled = false;

		private void ScheduleUpdate() {
			if(!enabled || isActionScheduled) return;
			ScheduleAction(UpdateState, CurrentTick + 1);
			isActionScheduled = true;
		}

		private void UpdateState() {
			if(!enabled) {
				isActionScheduled = false;
				return;
			}

			ScheduleStimulus(Output, isLow ? LowPowerLevel : HighPowerLevel);

			ScheduleAction(UpdateState, CurrentTick + (isLow ? LowWidth : HighWidth));
			isLow = !isLow;
		}

	}
}
