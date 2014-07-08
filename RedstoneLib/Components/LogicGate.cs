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

		private long lastUpdateTick;

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
		private void SignalChangedHandler(object sender, int oldLevel) {
			if(lastUpdateTick == CurrentTick) return;
			ScheduleAction(UpdateState, CurrentTick + 1);
			lastUpdateTick = CurrentTick;
		}
		private void UpdateState() {
			var powerLevel = Logic.Evaluate();
			if(powerLevel != Output.PowerLevel) {
				ScheduleStimulus(Output, powerLevel);
			}
		}

		public LogicGate(RSEngine engine)
			: base(engine) {
			Output = CreateOutput("Out");
		}

	}

	public abstract class CompositeLogic {
		public abstract int Evaluate();
		public abstract event EventHandler<int> SignalChanged;

		public static implicit operator CompositeLogic(RSConnection connection) { return new CompositeLogicConnection(connection); }

		public static CompositeLogic operator &(CompositeLogic a, CompositeLogic b) { return new CompositeLogicAnd(a, b); }
		public static CompositeLogic operator |(CompositeLogic a, CompositeLogic b) { return new CompositeLogicOr(a, b); }
		public static CompositeLogic operator !(CompositeLogic input) { return new CompositeLogicNot(input); }
	}
	public class CompositeLogicConnection : CompositeLogic {
		public RSConnection Connection { get; private set; }

		public CompositeLogicConnection(RSConnection connection) { Connection = connection; }

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
		public static CompositeLogic ToLogic(this RSConnection connection) { return (CompositeLogic)connection; }
	}
}
