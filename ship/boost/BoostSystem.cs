using Godot;
using System;

public abstract partial class BoostSystem : Node
{

	[Export]
	public ShipController ship;

	public float strength;

    public override void _Ready()
    {
		strength = ship._stats.BoostStrength;
    }


	public virtual void OnTrigger() {
		// base does nothing
	}

	public virtual void OnTriggerLeave() {
		// base does nothing
	}

}
