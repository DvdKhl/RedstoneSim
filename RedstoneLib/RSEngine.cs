using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib {
	public class RSEngine {
		public const int MaxPowerLevel = 15;
		public const int MaxFutureTicks = 32;

		public long CurrentTick { get; private set; }

		private int stimuliCount;
		private int highestStimuliPowerLevel;
		private Queue<RSConnection>[] stimuli;
		private Queue<Action>[] actions;


		public RSEngine() {
			stimuli = new Queue<RSConnection>[MaxPowerLevel + 1];
			for(int i = 0; i < stimuli.Length; i++) stimuli[i] = new Queue<RSConnection>();

			actions = new Queue<Action>[MaxFutureTicks];
			for(int i = 0; i < actions.Length; i++) actions[i] = new Queue<Action>();

		}

		public void ScheduleStimulus(RSConnection connection, int powerLevel) {
			if(0 > powerLevel || powerLevel > MaxPowerLevel) throw new InvalidOperationException("Invalid PowerLevel(" + powerLevel + ")");
			stimuli[powerLevel].Enqueue(connection);

			if(highestStimuliPowerLevel < powerLevel) highestStimuliPowerLevel = powerLevel;
			stimuliCount++;
		}

		public void ScheduleAction(Action action, long executeOn) {
			actions[executeOn % MaxFutureTicks].Enqueue(action);
		}


		public void DoTick() {
			var tickActions = actions[CurrentTick % MaxFutureTicks];
			while(tickActions.Count != 0) tickActions.Dequeue()();

			while(stimuliCount != 0) {
				var connection = stimuli[highestStimuliPowerLevel].Dequeue();
				stimuliCount--;

				connection.StimulateSignal(highestStimuliPowerLevel);

				while(highestStimuliPowerLevel >= 0 && stimuli[highestStimuliPowerLevel].Count == 0) highestStimuliPowerLevel--;
			}

			CurrentTick++;
		}


	}
}
