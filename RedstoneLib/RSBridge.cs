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


		public static RSBridge Create(params RSConnection[] connections) {
			return Create((IEnumerable<RSConnection>)connections);
		}
		public static RSBridge Create(IEnumerable<RSConnection> connections) {
			return new RSBridge(connections.First().Engine, connections);
		}

		public static RSBridge Connect(params RSConnection[] connections) {
			return Connect((IEnumerable<RSConnection>)connections);
		}
		public static RSBridge Connect(IEnumerable<RSConnection> connections) {
			var bridge = connections.Select(c => c.Bridge).FirstOrDefault(b => b != null);
			if(bridge == null) return Create(connections);

			foreach(var connection in connections) {
				if(connection.Bridge == null) {
					bridge.Add(connection);

				} else if(bridge != connection.Bridge) {
					var otherBridge = connection.Bridge;
					foreach(var otherConnection in otherBridge.connections) {
						otherConnection.Bridge = null;
						otherConnection.SignalChanged -= otherBridge.OnOutSignalChanged;

						bridge.Add(otherConnection);
					}
					otherBridge.connections.Clear();
				}
			}

			return bridge;
		}

		private void Add(RSConnection connection) {
			if(connection.Engine != Engine) throw new InvalidOperationException("Engine instances don't match");
			if(connection.Bridge != null) throw new InvalidOperationException("A connection may only be added to one bridge");
			connection.Bridge = this;

			this.connections.Add(connection);
			if(connection.Direction == ConnectionDirection.Out) connection.SignalChanged += OnOutSignalChanged;

			PowerLevel = connections.Max(c => c.Direction == ConnectionDirection.Out ? c.PowerLevel : 0);
			UpdateConnections();

		}
		private void Remove(RSConnection connection) {
			connections.Remove(connection);
			connection.Bridge = null;
			connection.SignalChanged -= OnOutSignalChanged;

			PowerLevel = connections.Max(c => c.Direction == ConnectionDirection.Out ? c.PowerLevel : 0);
			UpdateConnections();
		}

		private RSBridge(RSEngine engine, IEnumerable<RSConnection> connections)
			: base(engine) {
			this.connections = new List<RSConnection>();

			foreach(var c in connections) Add(c);
		}

		private void OnOutSignalChanged(object sender, int oldPowerLevel) {
			var outConnection = (RSConnection)sender;

			PowerLevel = connections.Max(c => c.Direction == ConnectionDirection.Out ? c.PowerLevel : 0);

			if(PowerLevel != oldPowerLevel) {
				if(lastPowerLevelChangeOn == CurrentTick) {
					if(PowerLevel > oldPowerLevel) return;
					throw new InvalidOperationException("Bridge PowerLevel was raised twice within one tick");
				}
				lastPowerLevelChangeOn = CurrentTick;

				UpdateConnections();
			}
		}

		private void UpdateConnections() {
			foreach(var connection in connections) {
				if(connection.Direction == ConnectionDirection.In && connection.PowerLevel != PowerLevel) {
					ScheduleStimulus(connection, PowerLevel);
				}
			}
		}

		public override string ToString() {
			return string.Format("RSBridge( Label={0}, PowerLevel={1} )", Label, PowerLevel);
		}

	}
}
