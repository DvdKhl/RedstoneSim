using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib {
	public sealed class RSConnection : RSObject {

		public event EventHandler<int> SignalChanging = delegate { };

		public string FullName { get { return (Component.Label != null ? Component.Label + "." : "") + Label; } }

		public ConnectionDirection Direction { get; private set; }

		public int PowerLevel { get; private set; }

		internal RSComponent Component { get; private set; }

		internal RSConnection(RSComponent component, ConnectionDirection direction)
			: base(component.Engine) {

			if(direction != ConnectionDirection.In && direction != ConnectionDirection.Out) throw new NotImplementedException("Only In and Out directions implemented");

			Direction = direction;
			Component = component;
		}

		private long lastChangeOn;
		internal void StimulateSignal(int powerLevel) {
			if(PowerLevel == powerLevel) return;

			if(lastChangeOn == CurrentTick) {
				if(PowerLevel > powerLevel) return;
				throw new InvalidOperationException("Connection PowerLevel was raised twice within one tick");
			}
			lastChangeOn = CurrentTick;

			SignalChanging(this, powerLevel);
			PowerLevel = powerLevel;
		}

		public override string ToString() {
			return string.Format("RSConnection( FullName={0}, Direction={1}, PowerLevel={2} )", FullName, Direction, PowerLevel);
		}
	}

	[Flags]
	public enum ConnectionDirection { None = 0, In = 1, Out = 2, InOut = 3 }
}
