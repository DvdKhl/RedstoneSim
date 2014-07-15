using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	//TODO: Create IRSConnection and move logic to RSCompositeConnection which derives from IRSConnection
	//      That would make nice shortcuts possible (i.e. RSBridge.Connect(connectionA & connectionB) )
	public class LogicGate : RSComponent {
		public RSConnection Output { get; private set; }


		private int delay;
		public int Delay {
			get { return delay; }
			set {
				if(delay <= 0) throw new InvalidOperationException("Delay must be longer than 0");
				if(delay > RSEngine.MaxFutureTicks) throw new InvalidOperationException("Delay must be shorter than RSEngine.MaxFutureTicks (" + RSEngine.MaxFutureTicks + ")");
				delay = value;
			}
		}

		private CompositeLogic logic;
		public CompositeLogic Logic {
			get { return logic; }
			set {
				var oldLogic = logic;
				logic = value;

				if(oldLogic != null) oldLogic.SignalChanged -= SignalChangedHandler;
				if(logic != null) logic.SignalChanged += SignalChangedHandler;
			}
		}

		private long lastSignalChangeTick;
		private void SignalChangedHandler(object sender, int oldLevel) {
			if(lastSignalChangeTick == CurrentTick) return;
			lastSignalChangeTick = CurrentTick;
			ScheduleAction(UpdateState, CurrentTick + Delay);
		}

		private long lastUpdateTick;
		private void UpdateState() {
			if(lastUpdateTick == CurrentTick) return;

			var powerLevel = Logic.Evaluate();
			if(powerLevel != Output.PowerLevel) {
				ScheduleStimulus(Output, powerLevel);
				lastUpdateTick = CurrentTick;
			}
		}

		public LogicGate(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");

			Delay = 1;
		}

	}

	public abstract class CompositeLogic {
		public abstract int Evaluate();
		public abstract event EventHandler<int> SignalChanged;

		public static CompositeLogic operator &(CompositeLogic a, CompositeLogic b) { return new CompositeLogicAnd(a, b); }
		public static CompositeLogic operator |(CompositeLogic a, CompositeLogic b) { return new CompositeLogicOr(a, b); }
		public static CompositeLogic operator +(CompositeLogic a, CompositeLogic b) { return new CompositeLogicAdd(a, b); }
		public static CompositeLogic operator -(CompositeLogic a, CompositeLogic b) { return new CompositeLogicSubtract(a, b); }
		public static CompositeLogic operator !(CompositeLogic input) { return new CompositeLogicNot(input); }
	}
	public class CompositeLogicConnection : CompositeLogic {
		public RSConnection Connection { get; private set; }

		public CompositeLogicConnection(RSConnection connection) {
			Connection = connection;
			if(connection.Direction != ConnectionDirection.Out) throw new ArgumentException("Connection direction must be out");
		}

		public override int Evaluate() { return Connection.PowerLevel; }
		public override event EventHandler<int> SignalChanged {
			add { Connection.SignalChanged += value; }
			remove { Connection.SignalChanged -= value; }
		}

	}
	public class CompositeLogicAnd : CompositeLogic {
		public IEnumerable<CompositeLogic> Inputs { get; private set; }

		public CompositeLogicAnd(IEnumerable<CompositeLogic> inputs) { Inputs = inputs; }
		public CompositeLogicAnd(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

		public override int Evaluate() {
			foreach(var input in Inputs) if(input.Evaluate() == 0) return 0;
			return RSEngine.MaxPowerLevel;
		}
		public override event EventHandler<int> SignalChanged {
			add { foreach(var input in Inputs) input.SignalChanged += value; }
			remove { foreach(var input in Inputs) input.SignalChanged -= value; }
		}

	}
	public class CompositeLogicOr : CompositeLogic {
		public IEnumerable<CompositeLogic> Inputs { get; private set; }

		public CompositeLogicOr(IEnumerable<CompositeLogic> inputs) { Inputs = inputs; }
		public CompositeLogicOr(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

		public override int Evaluate() {
			foreach(var input in Inputs) if(input.Evaluate() != 0) return RSEngine.MaxPowerLevel;
			return 0;
		}
		public override event EventHandler<int> SignalChanged {
			add { foreach(var input in Inputs) input.SignalChanged += value; }
			remove { foreach(var input in Inputs) input.SignalChanged -= value; }
		}

	}
	public class CompositeLogicSubtract : CompositeLogic {
		public IEnumerable<CompositeLogic> Inputs { get; private set; }

		public CompositeLogicSubtract(IEnumerable<CompositeLogic> inputs) { Inputs = inputs; }
		public CompositeLogicSubtract(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

		public override int Evaluate() {
			var iterator = Inputs.GetEnumerator();
			if(!iterator.MoveNext()) return 0;

			var value = iterator.Current.Evaluate();
			while(iterator.MoveNext()) value -= iterator.Current.Evaluate();
			return value < 0 ? 0 : value;
		}
		public override event EventHandler<int> SignalChanged {
			add { foreach(var input in Inputs) input.SignalChanged += value; }
			remove { foreach(var input in Inputs) input.SignalChanged -= value; }
		}
	}
	public class CompositeLogicAdd : CompositeLogic {
		public IEnumerable<CompositeLogic> Inputs { get; private set; }

		public CompositeLogicAdd(IEnumerable<CompositeLogic> inputs) { Inputs = inputs; }
		public CompositeLogicAdd(params CompositeLogic[] inputs) : this((IEnumerable<CompositeLogic>)inputs) { }

		public override int Evaluate() {
			var value = 0;
			foreach(var input in Inputs) value += input.Evaluate();
			return value;
		}
		public override event EventHandler<int> SignalChanged {
			add { foreach(var input in Inputs) input.SignalChanged += value; }
			remove { foreach(var input in Inputs) input.SignalChanged -= value; }
		}
	}
	public class CompositeLogicNot : CompositeLogic {
		public CompositeLogic Input { get; private set; }

		public CompositeLogicNot(CompositeLogic input) { Input = input; }

		public override int Evaluate() { return Input.Evaluate() == 0 ? RSEngine.MaxPowerLevel : 0; }
		public override event EventHandler<int> SignalChanged {
			add { Input.SignalChanged += value; }
			remove { Input.SignalChanged -= value; }
		}

	}

	public static class CompositeLogicExt {
		public static CompositeLogic ToLogic(this RSConnection connection) { return new CompositeLogicConnection(connection); }
	}
}
