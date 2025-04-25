using Godot;
using System;

public struct ShipInput {
	public float Thrust { get; set; }
	public readonly float Steer { get {
		return SteerRight - SteerLeft;
	} }
	public float SteerLeft { get; set; }
	public float SteerRight { get; set; }
	public float AirbrakeLeft { get; set; }
	public float AirbrakeRight { get; set; }
	public bool Boost { get; set; }
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
	[Export]
	public BoostSystem _boostSystem;

	public Node3D CameraAnchor { get; private set; }
	public Node3D Model { get; private set; }

	// air density
	private float density = 1.2f;

	// roll for turning
	private float maxTilt = 0.5f;
	private float tilt;
	private float tiltGoal;

	public override void _Ready()
	{

		CameraAnchor = GetNode<Node3D>("%CameraAnchor");
		Model = GetNode<Node3D>("%ShipModel");

		GD.Print(CameraAnchor);

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
		if (@event.IsAction("ship_left")) {
			_input.SteerLeft = @event.GetActionStrength("ship_left");
		}
		if (@event.IsAction("ship_right")) {
			_input.SteerRight = @event.GetActionStrength("ship_right");
		}
		if (@event.IsAction("ship_airbrake_left")) {
			_input.AirbrakeLeft = @event.GetActionStrength("ship_airbrake_left");
		}
		if (@event.IsAction("ship_airbrake_right")) {
			_input.AirbrakeRight = @event.GetActionStrength("ship_airbrake_right");
		}
		if (@event.IsAction("ship_boost")) {
			_input.Boost = @event.IsActionPressed("ship_boost");
		}
    }


	public override void _Process(double delta)
	{
		if (_input.Boost) {
			_boostSystem.OnTrigger();
		}
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
		float stabilizingStrength = Math.Clamp(impulse / maxForce, 0.5f, 1) * maxForce;
		Vector3 slideStabilization = - SideVelocity.Normalized() * stabilizingStrength;
		ApplyForce(slideStabilization);

		float rotPerSec;

		tiltGoal = _input.Steer * maxTilt;

		float tiltEpsilon = 0.1f;
		if (Math.Abs(tiltGoal - tilt) > tiltEpsilon) {
			tilt += _stats.tiltRate * (float) delta * Math.Sign(tiltGoal - tilt);
		} else {
			tilt = tiltGoal;
		}

		Model.Rotation = Vector3.Forward * tilt;

		if (Math.Abs(tilt) > 0) {
			float turn = tilt / maxTilt * _stats.maxTurn;
			float deltaTheta = - turn * (float) delta;
			float omega = deltaTheta / (float) delta;
			float turnRadius = LinearVelocity.Length() / omega;
			RotateY(deltaTheta);

			// effectiveness of circle path force depends on if the ship is even facing correctly
			Vector3 facing = - Basis.Z;
			float angleBetween = facing.SignedAngleTo(LinearVelocity, Vector3.Up);
			// left should be minus, right should be plus
			float epsilon = 0.02f;
			float maxAngle = (float) Math.PI; // loses all effectiveness
			float effectiveness;
			if (Math.Abs(angleBetween) > maxAngle) {
				effectiveness = 0;
			} else if (_input.Steer * angleBetween > 0 || Math.Abs(angleBetween) < epsilon) { // same sign
				effectiveness = 1;
			} else {
				float i = Math.Max(0, 1 - Math.Abs(angleBetween) / maxAngle);
				float raw = (float) Math.Pow(i, 2);
				effectiveness = raw;
			}
			Vector3 direction = LinearVelocity.Normalized().Rotated(Vector3.Up, (float) (Math.PI / 2));
			Vector3 centripetal = direction * Mass * LinearVelocity.Length() * omega;
			ApplyForce(centripetal * effectiveness);
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
