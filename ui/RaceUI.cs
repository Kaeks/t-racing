using Godot;
using System;

public partial class RaceUI : Control
{

	[Export]
	public ShipController _shipController;

	[Export]
	public Label _speedLabel;

	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{
		// meters per second
		float shipSpeed = _shipController.LinearVelocity.Length();
		float kmh = shipSpeed * 3.6F;
		_speedLabel.Text = ParseSpeed(kmh);
	}

	private string ParseSpeed(float speed) {
		int rounded = (int) Math.Round(speed);
		string parsed = rounded.ToString("000");
		return parsed;
	}
}
