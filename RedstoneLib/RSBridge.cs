using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib {
	public sealed class RSBridge : RSObject {
		private List<RSConnection> connections; private IReadOnlyCollection<RSConnection> connectionsRO;
		public IReadOnlyCollection<RSConnection> Connections { get { return connectionsRO ?? (connectionsRO = connections.AsReadOnly()); } }

		public int PowerLevel { get; private set; }

		private long lastPowerLevelChangeOn;


		public static RSBridge Create(params RSConnection[] connections) { return new RSBridge(connections[0].Engine, connections); }
		public static RSBridge Create(IEnumerable<RSConnection> connections) { return new RSBridge(connections.First().Engine, connections); }

		private RSBridge(RSEngine engine, IEnumerable<RSConnection> connections)
			: base(engine) {
			this.connections = new List<RSConnection>();

			foreach(var c in connections) {
				if(c.Engine != engine) throw new InvalidOperationException("Engine instances don't match");

				this.connections.Add(c);
				if(c.Direction == ConnectionDirection.Out) c.SignalChanging += OnOutSignalChanging;
			}
			OnOutSignalChanging(null, -1);
		}

		private void OnOutSignalChanging(object sender, int newPowerLevel) {
			var outConnection = (RSConnection)sender;

			var oldPowerLevel = PowerLevel;
			PowerLevel = connections.Max(c => c.Direction == ConnectionDirection.Out ? (c == outConnection ? newPowerLevel : c.PowerLevel) : 0);

			if("B1".Equals(Label) || "B2".Equals(Label)) Debug.WriteLine(Label + " " + oldPowerLevel + " " + PowerLevel);

			if(PowerLevel != oldPowerLevel) {
				if(lastPowerLevelChangeOn == CurrentTick) {
					if(PowerLevel > oldPowerLevel) return;
					throw new InvalidOperationException("Bridge PowerLevel was raised twice within one tick");
				}
				lastPowerLevelChangeOn = CurrentTick;

				foreach(var inConnection in connections) {
					if(sender != inConnection && inConnection.Direction == ConnectionDirection.In && inConnection.PowerLevel != PowerLevel) { //Powerlevel check wrong for multiple bridges on one connection?
						ScheduleStimulus(inConnection, PowerLevel);
					}
				}
			}
		}

		public override string ToString() {
			return string.Format("RSBridge( Label={0}, PowerLevel={1} )", Label, PowerLevel);
		}

	}
}
