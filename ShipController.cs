using Godot;
using System;
using System.Collections.Generic;

public struct ShipInput {
	public float Thrust { get; set; }
	public float Steer { get; set; }
	public float AirbrakeLeft { get; set; }
	public float AirbrakeRight { get; set; }
}

public partial class ShipController : RigidBody3D
{

	private ShipInput _input = new();
	[Export]
	public ShipStats _stats;
	[Export]
	public Airbrake[] _airbrakesLeft = [];
	[Export]
	public Airbrake[] _airbrakesRight = [];

	// air density
	private float density = 1.2f;

	public override void _Ready()
	{
		Mass = _stats.weight;
		foreach (Airbrake brake in _airbrakesLeft) {
			GD.Print(brake.Position);
		}
		foreach (Airbrake brake in _airbrakesRight) {
			GD.Print(brake.Position);
		}
	}

	public Vector3 ForwardVelocity { 
		get { return Basis.Z * Basis.Z.Dot(LinearVelocity); } 
	}

	public Vector3 SideVelocity {
		get { return Basis.X * Basis.X.Dot(LinearVelocity); }
	}

    public override void _Input(InputEvent @event)
    {
		if (@event.IsAction("ship_thrust")) {
			_input.Thrust = @event.GetActionStrength("ship_thrust");
		}
		if (@event.IsAction("ship_left") || @event.IsAction("ship_right")) {
			_input.Steer = @event.GetActionStrength("ship_right") - @event.GetActionStrength("ship_left");
		}
		if (@event.IsAction("ship_airbrake_left")) {
			_input.AirbrakeLeft = @event.GetActionStrength("ship_airbrake_left");
		}
		if (@event.IsAction("ship_airbrake_right")) {
			_input.AirbrakeRight = @event.GetActionStrength("ship_airbrake_right");
		}
    }


	public override void _Process(double delta)
	{

	}

    public override void _PhysicsProcess(double delta)
    {
		Vector3 thrust = GetThrustVector(_input.Thrust);
		ApplyForce(thrust);

		Vector3 drag = GetDragVector();
		ApplyForce(drag);

		Vector3 angleBrake = new(0, -AngularVelocity.Y, 0); 
		ApplyTorque(angleBrake * Mass);

		float maxForce = _stats.thrust * _stats.stabilizer;
		float impulse = Mass * SideVelocity.Length();
		float stabilizingStrength = Math.Max(Math.Min(impulse / maxForce, 1), 0.5f) * maxForce;
		Vector3 slideStabilization = - SideVelocity.Normalized() * stabilizingStrength;
		ApplyForce(slideStabilization);

		float rotPerSec;

		if (Math.Abs(_input.Steer) > 0) {
			float deltaTheta = - _input.Steer * (float) delta;
			float omega = deltaTheta / (float) delta;
			float turnRadius = ForwardVelocity.Length() / omega;
			RotateY(deltaTheta);
			Vector3 centripetal = - Basis.X * Mass * ForwardVelocity.Length() / omega;
			ApplyForce(centripetal);
		}

		if (_input.AirbrakeLeft > 0) {
			foreach (Airbrake airbrake in _airbrakesLeft) {
				ApplyAirbrake(airbrake, _input.AirbrakeLeft);
			}
		}
		if (_input.AirbrakeRight > 0) {
			foreach (Airbrake airbrake in _airbrakesRight) {
				ApplyAirbrake(airbrake, _input.AirbrakeRight);
			}
		}
    }

	private Vector3 GetThrustVector(float throttle) {
		return - Basis.Z * _stats.thrust * throttle;
	}

	private Vector3 GetDragVector() {
		float magnitude = 0.5f * density * _stats.area * _stats.dragCoeff * LinearVelocity.LengthSquared();
		return -1 * LinearVelocity.Normalized() * magnitude;
	}

	private void ApplyAirbrake(Airbrake airbrake, float amt) {
		Vector3 pos = airbrake.GlobalPosition - GlobalPosition;
		GD.Print(pos);
		float magnitude = 0.5f * density * airbrake.size * ForwardVelocity.LengthSquared() * amt;
		float adjusted = Math.Max(magnitude, 0);
		Vector3 airbrakeDrag = -1 * ForwardVelocity.Normalized() * adjusted;
		ApplyForce(airbrakeDrag, pos);
	}

}
