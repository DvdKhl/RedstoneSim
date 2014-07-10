RedstoneSim
===========

##How it works
RedstoneLib is a *connection* based system (each connection represented by ``RSConnection``), which can be linked by *bridges* (``RSBridge``). Each connection has a direction (*In*, *Out*) and a *powerlevel* (from ``0`` to ``RSEngine.MaxPowerLevel`` as ints). If multiple connections are linked (one connection may only have one bridge at a time), the bridge takes the highest out powerlevel and feeds it to all in connections.  
Similar to Minecraft's Redstone, the simulation is done in discrete steps (*ticks*). In each tick, bridges react to connection powerlevel changes and behaves as described above.  
To avoid endless loops in one simulation step and for performance reasons, the powerlevel for each connection/bridge may only be raised once per tick (lower powerlevels will simply be ignored). This may sound like a strong limitation but works quite well if design requirements are followed by component developers.

Until now I only talked about how connections and bridges *react*, but to do something useful we need to be able to manipulate the powerlevel of connections and a place to store the logic for it. This is done by *components*, each component contains one or more connections (zero is techically possible but quite useless) and is able to listen to changes of and manipulate the connection's powerlevel. For example a simple delay component would listen to its *input connection* and after some ticks it passes the signal to the *output connections*.  
Since the previous limitation is in effect, a design requirement is that the manipulation of a connection's powerlevel can only happen in the next tick and not in the current. This makes instant transmission over multiple components impossible, which I think is a good limitation since it avoids a lot of ambiguous situations and one could just implement another component to get around that limitation.

The next step is the *engine* (``RSEngine``) which sets everything is motion.  
Each component holds exactly one instance of ``RSEngine`` and components with different RSEngine instances may not be connected to each other, making them *independed Redstone circuitry*. Components can *schedule actions* (i.e. callbacks) on the engine at specific *future* ticks (but only ``RSEngine.MaxFutureTicks`` in the future) and *stimulate* connections which will alter the powerlevel of a connection on the next tick.  
To do a simulation step ``DoTick()`` is called on the engine instance, which will first execute all scheduled actions for that tick and then stimulate all scheduled connections with the specified powerlevel.  
To make the simulation with the above mentioned limitation (powerlevel may only be raised once per tick) at all possible, the stimulations with the highest powerlevels are done first, so multiple stimulations for the same connection will not cause an exception (i.e. the lower powerlevel stimulation will be ignored).

####Component Example:
```csharp
public class DelayLock : RSComponent {
	public int Delay { get; set; }
	public RSConnection Output { get; private set; }
	public RSConnection Input { get; private set; }
	public RSConnection Lock { get; private set; }

	private Queue<int> memory = new Queue<int>();

	public DelayLock(RSEngine engine)
		: base(engine) {
		Output = CreateOutput("Out");
		Input = CreateInput("In");
		Lock = CreateInput("Lock");

		Input.SignalChanged += (s, l) => {
			memory.Enqueue(Input.PowerLevel);
			ScheduleAction(UpdateState, CurrentTick + Delay);
		};

		Lock.SignalChanged += (s, l) => {
			if(memory.Count == 0 && Lock.PowerLevel == 0) {
				ScheduleAction(UpdateState, CurrentTick + 1);
			}
		};
		Delay = 1;
	}

	private long lastUpdateTick;
	private void UpdateState() {
		if(lastUpdateTick == CurrentTick) return;
		lastUpdateTick = CurrentTick;

		ScheduleStimulus(Output, memory.Count > 0 ? memory.Dequeue() : Input.PowerLevel);
	}
}
```
**Other components**:
[Clock](../blob/master/RedstoneLib/Components/Clock.cs),  
[EdgeDetector](../blob/master/RedstoneLib/Components/EdgeDetector.cs), 
[Lever](../blob/master/RedstoneLib/Components/Lever.cs), 
[LogicGate](../blob/master/RedstoneLib/Components/LogicGate.cs), 
[ShiftRegister](../blob/master/RedstoneLib/Components/ShiftRegister.cs), 
[SignalQueue](../blob/master/RedstoneLib/Components/SignalQueue.cs)

####Performance
For a quick test I used ``10000`` ``DelayLocks`` (comparable to *Repeaters* in Minecraft) and chained them linearly.
Feeding the first ``DelayLock`` a continuous ``10`` sequence and waiting ``10000`` ticks, after which all DelayLocks are active and not idling, each tick took ``10ms`` to simulate.  
Having ``n``-Cores, you could run ``n`` of those chains in parallel with the same amount of time.

####How it could be implemented into Minecraft
One of the goals should be to divide Redstone into independed circuits as much as possible without affecting their behaviour. The biggest advantage of doing this is the ability to parallelize the independed circuits. This may not seem majorly beneficial for single player games but it would be very valuable for big servers.  
Doing the division into independed circuits on the lowest level is too complex (i.e. on blocklevel). So the next best thing is doing it on cube level (16x16x16). Each cube would then contain an redstone engine id and circuits spanning over multiple cubes would contain the same id. Cubes with the same ids will have the same RSEngine instance.  
For each cube the circuitry is synthesized from the blocks (note, no component may span multiple cubes) and stored in memory. Neighbour cubes will connect their circuitry on load. The simulation is preferably started when all cubes with the same id have been loaded, synthesized and connected.  
Should a circuit expand into another cube, the engine id is added to that cube, or if there is already an engine id, the engines are merged. The splitting of cubes into different engine ids should however only happen in longer intervals (maybe at world save), to avoid continuous merging and splitting in short intervals.
