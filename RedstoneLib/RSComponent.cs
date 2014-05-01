using RedstoneLib;

public abstract class RSComponent : RSObject {
	public RSComponent(RSEngine engine) : base(engine) {

	}

	protected RSConnection CreateInput(string label = null) {
		var connection = new RSConnection(this, ConnectionDirection.In) { Label = label };
		return connection;
	}
	protected RSConnection CreateOutput(string label = null) {
		var connection = new RSConnection(this, ConnectionDirection.Out) { Label = label };
		return connection;
	}


}
