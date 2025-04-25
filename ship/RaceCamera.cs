using Godot;

public partial class RaceCamera : Camera3D
{
	[Export]
	public ShipController ship;
	[Export]
	public CameraMode mode = CameraMode.FOLLOW_FAR;

	public enum CameraMode {
		FOLLOW_FAR, FOLLOW_NEAR, COCKPIT, NOSE
	}
	
	private Node3D _pivot;
	private Node3D _anchor;

    public override void _Ready()
    {
		_anchor = GetParent<Node3D>();
		_pivot = _anchor.GetParent<Node3D>();
    }

	private Vector3 facing = Vector3.Forward;
	private Vector3 offset;

	private float maxSideShift = 2;
	private float maxDriftAngle = Mathf.Pi / 2;

	private float minDistance = -0.5f;
	private float maxDistance = 1f;
	private float minFov = 60;
	private float maxFov = 90;
	private float maxDecel = -10;
	private float maxAccel = 10;

	private float smoothing = 2.5f;

	private Vector3 lastShipVelocity;

    public override void _PhysicsProcess(double delta)
    {
		// Grab the ship speed and convert to local coordinates.
		Vector3 localShipVelocity = ship.LinearVelocity * ship.Basis;
		localShipVelocity.Y = 0; // ignore Y speed

		Vector3 shift = new();

		// To rotate the pivot so that we always face where we are going
		if (localShipVelocity.Length() > 1) {
			facing = facing.Lerp(localShipVelocity.Normalized(), (float) (smoothing * delta));
		}
		facing = facing.Lerp(Vector3.Forward, 0.1f);
		Vector3 rotation = RotationTo(facing);
		_pivot.Rotation = rotation;

		// To shift the camera sideways when we are drifting
		if (localShipVelocity.Length() > 1) {
			float driftAngle = Vector3.Forward.SignedAngleTo(localShipVelocity, Vector3.Up);
			float sideShiftWeight = Mathf.Lerp(0, 1, driftAngle / maxDriftAngle);
			float sideShift = Mathf.Lerp(0, maxSideShift, sideShiftWeight);
			shift.X = sideShift;
		}

		float rawAccel = (float) ((localShipVelocity.Length() - lastShipVelocity.Length()) / delta);
		
		float accel = Mathf.Clamp(rawAccel, maxDecel, maxAccel);
		float distanceWeight = Mathf.Lerp(0, 1, (accel - maxDecel) / (maxAccel - maxDecel));
		float distanceShift = Mathf.Lerp(minDistance, maxDistance, distanceWeight);
		shift.Z = distanceShift;

		float fov = Mathf.Lerp(minFov, maxFov, distanceWeight);
		Fov = Mathf.Lerp(Fov, fov, (float) (smoothing * delta));

		Position = Position.Lerp(shift, (float) (smoothing * delta));
		lastShipVelocity = localShipVelocity;
    }

	private Vector3 RotationTo(Vector3 direction) {
		return _pivot.Transform.LookingAt(direction, Vector3.Up).Basis.GetEuler();
	}

	
}
