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


		public CompositeLogic ToLogic() { return new CompositeLogicConnection(this); }

		public abstract class CompositeLogic {
			public abstract int Evaluate();

			protected virtual RSEngine GetEngine() { return Inputs.Select(x => x.GetEngine()).First(x => x != null); }
			protected IEnumerable<CompositeLogic> Inputs { get; private set; }
			public virtual event EventHandler<int> SignalChanged {
				add { foreach(var input in Inputs) input.SignalChanged += value; }
				remove { foreach(var input in Inputs) input.SignalChanged -= value; }
			}


			public CompositeLogic(IEnumerable<CompositeLogic> inputs) { this.Inputs = inputs; }


			public RSConnection ToConnection() {
				var engine = GetEngine();
				var connection = new RSConnection(engine, ConnectionDirection.Out);

				long lastUpdateTick = 0;
				Action updateState = () => {
					if(lastUpdateTick == engine.CurrentTick) return;

					var powerLevel = Evaluate();
					if(powerLevel != connection.PowerLevel) {
						engine.ScheduleStimulus(connection, powerLevel);
						lastUpdateTick = engine.CurrentTick;
					}
				};

				long lastSignalChangeTick = 0;
				EventHandler<int> signalChangedHandler = (sender, oldLevel) => {
					if(lastSignalChangeTick == engine.CurrentTick) return;
					lastSignalChangeTick = engine.CurrentTick;
					engine.ScheduleAction(updateState, engine.CurrentTick + 1);
				};

				return connection;
			}

			public static CompositeLogic operator &(CompositeLogic a, CompositeLogic b) { return new CompositeLogicAnd(a, b); }
			public static CompositeLogic operator |(CompositeLogic a, CompositeLogic b) { return new CompositeLogicOr(a, b); }
			public static CompositeLogic operator +(CompositeLogic a, CompositeLogic b) { return new CompositeLogicAdd(a, b); }
			public static CompositeLogic operator +(CompositeLogic a, int powerLevel) { return new CompositeLogicConstantAdd(a, powerLevel); }
			public static CompositeLogic operator -(CompositeLogic a, CompositeLogic b) { return new CompositeLogicSubtract(a, b); }
			public static CompositeLogic operator -(CompositeLogic a, int powerLevel) { return new CompositeLogicConstantSubtract(a, powerLevel); }
			public static CompositeLogic operator !(CompositeLogic input) { return new CompositeLogicNot(input); }
			public static CompositeLogic operator ~(CompositeLogic input) { return new CompositeLogicInvert(input); }
			public static CompositeLogic operator <<(CompositeLogic input, int delay) { return new CompositeLogicDelay(input, delay); }
		}
		private class CompositeLogicConnection : CompositeLogic {
			public RSConnection Connection { get; private set; }

			public CompositeLogicConnection(RSConnection connection)
				: base(new CompositeLogic[0]) {
				Connection = connection;
				if(connection.Direction != ConnectionDirection.Out) throw new ArgumentException("Connection direction must be out");
			}

			protected override RSEngine GetEngine() { return Connection.Engine; }
			public override int Evaluate() { return Connection.PowerLevel; }
			public override event EventHandler<int> SignalChanged {
				add { Connection.SignalChanged += value; }
				remove { Connection.SignalChanged -= value; }
			}
		}
		private class CompositeLogicAnd : CompositeLogic {
			public CompositeLogicAnd(IEnumerable<CompositeLogic> inputs) : base(inputs) { }
			public CompositeLogicAnd(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

			public override int Evaluate() {
				foreach(var input in Inputs) if(input.Evaluate() == 0) return 0;
				return RSEngine.MaxPowerLevel;
			}
		}
		private class CompositeLogicOr : CompositeLogic {
			public CompositeLogicOr(IEnumerable<CompositeLogic> inputs) : base(inputs) { }
			public CompositeLogicOr(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

			public override int Evaluate() {
				foreach(var input in Inputs) if(input.Evaluate() != 0) return RSEngine.MaxPowerLevel;
				return 0;
			}
		}
		private class CompositeLogicSubtract : CompositeLogic {
			public CompositeLogicSubtract(IEnumerable<CompositeLogic> inputs) : base(inputs) { }
			public CompositeLogicSubtract(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

			public override int Evaluate() {
				var iterator = Inputs.GetEnumerator();
				if(!iterator.MoveNext()) return 0;

				var value = iterator.Current.Evaluate();
				while(iterator.MoveNext()) value -= iterator.Current.Evaluate();
				return value < 0 ? 0 : value;
			}
		}
		private class CompositeLogicAdd : CompositeLogic {
			public CompositeLogicAdd(IEnumerable<CompositeLogic> inputs) : base(inputs) { }
			public CompositeLogicAdd(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

			public override int Evaluate() {
				var value = 0;
				foreach(var input in Inputs) value += input.Evaluate();
				return value;
			}
		}
		private class CompositeLogicNot : CompositeLogic {
			private CompositeLogic input;
			public CompositeLogicNot(CompositeLogic input) : base(new[] { input }) { this.input = input; }

			public override int Evaluate() { return input.Evaluate() == 0 ? RSEngine.MaxPowerLevel : 0; }
		}
		private class CompositeLogicInvert : CompositeLogic {
			private CompositeLogic input;
			public CompositeLogicInvert(CompositeLogic input) : base(new[] { input }) { this.input = input; }

			public override int Evaluate() { return RSEngine.MaxPowerLevel - input.Evaluate(); }
		}
		private class CompositeLogicConstantAdd : CompositeLogic {
			private CompositeLogic input;
			private int powerLevel;

			public CompositeLogicConstantAdd(CompositeLogic input, int powerLevel)
				: base(new[] { input }) {
				this.input = input;
				this.powerLevel = powerLevel;
			}

			public override int Evaluate() { return Math.Min(input.Evaluate() + powerLevel, RSEngine.MaxPowerLevel); }
		}
		private class CompositeLogicConstantSubtract : CompositeLogic {
			private CompositeLogic input;
			private int powerLevel;

			public CompositeLogicConstantSubtract(CompositeLogic input, int powerLevel)
				: base(new[] { input }) {
				this.input = input;
				this.powerLevel = powerLevel;
			}

			public override int Evaluate() { return Math.Max(0, input.Evaluate() - powerLevel); }
		}
		private class CompositeLogicDelay : CompositeLogic {
			private CompositeLogic input;
			private int delay;

			public CompositeLogicDelay(CompositeLogic input, int delay)
				: base(new[] { input }) {
				this.input = input;
				this.delay = delay;
			}

			public override int Evaluate() { throw new NotImplementedException(); }
		}
	}


	[Flags]
	public enum ConnectionDirection { None = 0, In = 1, Out = 2, InOut = 3 }
}
