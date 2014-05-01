using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib {
	public abstract class RSObject {
		public string Label { get; set; }

		public RSEngine Engine { get; internal set; }

		public RSObject(RSEngine engine) { Engine = engine; }



		public long CurrentTick { get { return Engine.CurrentTick; } }

		protected void ScheduleAction(Action action, long executeOn) { Engine.ScheduleAction(action, executeOn); }

		protected void ScheduleStimulus(RSConnection connection, int powerLevel) { Engine.ScheduleStimulus(connection, powerLevel); }


	}

}
