using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedstoneLib.Components {
	//TODO: Create IRSConnection and move logic to RSCompositeConnection which derives from IRSConnection
	//      That would make nice shortcuts possible (i.e. RSBridge.Connect(connectionA & connectionB) )
	//public class LogicGate : RSComponent {
	//	public RSConnection Output { get; private set; }


	//	private int delay;
	//	public int Delay {
	//		get { return delay; }
	//		set {
	//			if(delay <= 0) throw new InvalidOperationException("Delay must be longer than 0");
	//			if(delay > RSEngine.MaxFutureTicks) throw new InvalidOperationException("Delay must be shorter than RSEngine.MaxFutureTicks (" + RSEngine.MaxFutureTicks + ")");
	//			delay = value;
	//		}
	//	}

	//	private CompositeLogic logic;
	//	public CompositeLogic Logic {
	//		get { return logic; }
	//		set {
	//			var oldLogic = logic;
	//			logic = value;

	//			if(oldLogic != null) oldLogic.SignalChanged -= SignalChangedHandler;
	//			if(logic != null) logic.SignalChanged += SignalChangedHandler;
	//		}
	//	}

	//	private long lastSignalChangeTick;
	//	private void SignalChangedHandler(object sender, int oldLevel) {
	//		if(lastSignalChangeTick == CurrentTick) return;
	//		lastSignalChangeTick = CurrentTick;
	//		ScheduleAction(UpdateState, CurrentTick + Delay);
	//	}

	//	private long lastUpdateTick;
	//	private void UpdateState() {
	//		if(lastUpdateTick == CurrentTick) return;

	//		var powerLevel = Logic.Evaluate();
	//		if(powerLevel != Output.PowerLevel) {
	//			ScheduleStimulus(Output, powerLevel);
	//			lastUpdateTick = CurrentTick;
	//		}
	//	}

	//	public LogicGate(RSEngine engine)
	//		: base(engine) {
	//		Output = CreateOutput("Out");

	//		Delay = 1;
	//	}

	//}
}
