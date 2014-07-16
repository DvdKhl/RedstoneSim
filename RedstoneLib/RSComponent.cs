using RedstoneLib;

public abstract class RSComponent : RSObject {
	protected RSComponent(RSEngine engine) : base(engine) { }

	protected RSConnection CreateInput(string label = null) { return CreateConnection(label, ConnectionDirection.In); }
	protected RSConnection CreateOutput(string label = null) { return CreateConnection(label, ConnectionDirection.Out); }
	private RSConnection CreateConnection(string label, ConnectionDirection direction) {
		var connection = new RSConnection(this.Engine, direction) {
			Label = string.IsNullOrEmpty(label) && string.IsNullOrEmpty(this.Label) ? "[NoName]" : (this.Label ?? "[NoName]") + "." + (label ?? "[NoName]")
		};
		return connection;
	}


}
