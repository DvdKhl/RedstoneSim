using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class Clock : RSComponent {
		public RSConnection Output { get; private set; }

		private bool enabled;
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; ScheduleUpdate(); }
		}

		public int IdlePowerLevel { get; set; }
		public int PulsePowerLevel { get; set; }
		public int PulseWidth { get; set; }
		public int IdleWidth { get; set; }

		public Clock(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");

			IdlePowerLevel = 0;
			PulsePowerLevel = RSEngine.MaxPowerLevel;

			ScheduleUpdate();
		}

		private bool isIdle = false;
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

			ScheduleStimulus(Output, isIdle ? IdlePowerLevel : PulsePowerLevel);
			ScheduleAction(UpdateState, CurrentTick + (isIdle ? IdleWidth : PulseWidth));
			isIdle = !isIdle;
		}
	}
}
