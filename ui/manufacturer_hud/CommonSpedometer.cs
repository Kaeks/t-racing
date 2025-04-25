using Godot;
using System;

public partial class CommonSpedometer : Control
{
	private ShipController ship;

	public float maxSpeed = 300;

	public float speed;

    public override void _Ready()
    {
		ship = GetParent<CommonHud>().ship;
    }


	public override void _Process(double delta)
	{
		speed = ToKmh(ship.ForwardVelocity.Length());
	}

	public static float ToKmh(float mps) {
		return mps * 3.6f;
	}

	public static string ParseSpeed(float speed) {
		int rounded = (int) Math.Round(speed);
		string parsed = rounded.ToString("000");
		return parsed;
	}
}
