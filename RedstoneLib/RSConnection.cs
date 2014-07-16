using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedstoneLib {
	public class RSConnection : RSObject {
		public event EventHandler<int> SignalChanged = delegate { };

		public ConnectionDirection Direction { get; private set; }
		public int PowerLevel { get; private set; }

		internal RSBridge Bridge { get; set; }

		internal RSConnection(RSEngine engine, ConnectionDirection direction)
			: base(engine) {

			if(direction != ConnectionDirection.In && direction != ConnectionDirection.Out) throw new NotImplementedException("Only In and Out directions implemented");

			Direction = direction;
		}

		private long lastChangeOn;
		internal void StimulateSignal(int powerLevel) {
			if(PowerLevel == powerLevel) {
				lastChangeOn = CurrentTick; //Refresh
				return;
			}

			if(lastChangeOn == CurrentTick) {
				if(PowerLevel > powerLevel) return;
				throw new InvalidOperationException("Connection PowerLevel was raised twice within one tick");
			}
			lastChangeOn = CurrentTick;

			var oldPowerLevel = PowerLevel;
			PowerLevel = powerLevel;

			SignalChanged(this, oldPowerLevel);
		}

		public override string ToString() {
			return string.Format("Conn({0}, {1}, {2})", Label, Direction, PowerLevel);
		}
	}


	[Flags]
	public enum ConnectionDirection { None = 0, In = 1, Out = 2, InOut = 3 }
}
