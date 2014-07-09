using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	public class EdgeDetector : RSComponent {
		public int Delay { get; set; }
		public int PulsePowerLevel { get; set; }
		public int IdlePowerLevel { get; set; }

		public Edge PulseOn { get; set; }
		public int PulseWidth { get; set; }


		public RSConnection Output { get; private set; }
		public RSConnection Input { get; private set; }

		public EdgeDetector(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
			Input = CreateInput("In");

			Delay = 1;
			PulsePowerLevel = RSEngine.MaxPowerLevel;
			IdlePowerLevel = 0;

			PulseWidth = 1;
			PulseOn = Edge.Rising;

			Input.SignalChanged += SignalChangedHandler;

			pulseState = PulseState.Active;
			ScheduleAction(UpdateState, CurrentTick + 1);
		}

		private PulseState pulseState;

		private long lastSignalChangeTick;
		private void SignalChangedHandler(object sender, int oldLevel) {
			if(lastSignalChangeTick == CurrentTick) return;
			lastSignalChangeTick = CurrentTick;

			var doSchedule = (PulseOn & Edge.Rising) != 0 && oldLevel == 0 && Input.PowerLevel != 0;
			doSchedule |= (PulseOn & Edge.Falling) != 0 && oldLevel != 0 && Input.PowerLevel == 0;
			doSchedule &= pulseState == PulseState.Idle;

			if(doSchedule) {
				pulseState = PulseState.Delay;
				ScheduleAction(UpdateState, CurrentTick + Delay);
			}
		}

		private long lastUpdateTick;
		private void UpdateState() {
			if(lastUpdateTick == CurrentTick) return;
			lastUpdateTick = CurrentTick;

			switch(pulseState) {
				case PulseState.Delay:
					ScheduleStimulus(Output, PulsePowerLevel);
					ScheduleAction(UpdateState, CurrentTick + PulseWidth);
					pulseState = PulseState.Active;
					break;

				case PulseState.Active:
					ScheduleStimulus(Output, IdlePowerLevel);
					pulseState = PulseState.Idle;
					break;
			}
		}

		private enum PulseState { Idle, Delay, Active }

		public enum Edge { Rising = 1, Falling = 2, Both = Rising | Falling }
	}
}
