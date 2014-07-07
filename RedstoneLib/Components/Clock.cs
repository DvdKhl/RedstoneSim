using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Clock : RSComponent {
		public RSConnection Output { get; private set; }

		private bool enabled;
		public bool Enabled { get { return enabled; } set { enabled = value; EnabledChangedHandler(); } }

		public int LowPowerLevel { get; set; }
		public int HighPowerLevel { get; set; }
		public int HighWidth { get; set; }
		public int LowWidth { get; set; }

		public Clock(RSEngine engine) : base(engine) {
			Output = CreateOutput("Out");
			ScheduleAction(EnabledChangedHandler, CurrentTick + 1);

			LowPowerLevel = 0;
			HighPowerLevel = RSEngine.MaxPowerLevel;
		}

		private bool isLow = false;
		private void EnabledChangedHandler() {
			if(!enabled) return;

			ScheduleStimulus(Output, isLow ? LowPowerLevel : HighPowerLevel);
			ScheduleAction(EnabledChangedHandler, CurrentTick + (isLow ? LowWidth : HighWidth));
			isLow = !isLow;
		}

	}
}
