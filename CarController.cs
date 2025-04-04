using Godot;
using System;

public partial class CarController : RigidBody3D
{

    [Export]
    public CollisionShape3D _CarCollider;
    [Export]
    public CameraController _FollowCam;
    [Export]
    public MeshInstance3D _CarMesh;

    private float throttle = 0.0F;
    private float drag = 0.1F;
    private float idleDrag = 0.3F;
    private float angleDrag = 0.3F;
    private float goalTilt = 0.0F;
    private float maxTilt = 10.0F;
    private float tiltRate = 1F;

    private float epsilon = 1F;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("Forward")) {
            GD.Print("CAR GOES!!!");
            throttle = 1.0F;
        }
        if (@event.IsActionReleased("Forward")) {
            throttle = 0.0F;
            GD.Print("woke NONSENSE");
        }
        if (@event.IsActionPressed("Left")) {
            goalTilt = -maxTilt;
        }
        if (@event.IsActionReleased("Left")) {
            goalTilt = 0;
        }
        if (@event.IsActionPressed("Right")) {
            goalTilt = maxTilt;
        }
        if (@event.IsActionReleased("Right")) {
            goalTilt = 0;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _FollowCam.UpdateView(LinearVelocity);
    }



    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Vector3 ItMakesCarsGo =  Basis.Z * 10 * throttle;
        ApplyCentralForce(ItMakesCarsGo);
        float linearDrag = throttle > 0 ? drag : idleDrag;
        ApplyCentralForce(- (LinearVelocity * LinearVelocity.Length()) * linearDrag);
        Vector3 meshRotation = _CarMesh.RotationDegrees;
        if (meshRotation.Z > goalTilt + epsilon) {
            _CarMesh.RotateZ(-tiltRate * (float)delta);
        } else if (meshRotation.Z < goalTilt - epsilon) {
            _CarMesh.RotateZ(tiltRate * (float)delta);
        }
        if (Math.Abs(goalTilt - meshRotation.Z) <= 2 * epsilon) {
            _CarMesh.RotationDegrees = new(meshRotation.X, meshRotation.Y, goalTilt);
        }
        Vector3 Turner = Vector3.Up * 0.1F * -meshRotation.Z;
        ApplyTorque(Turner);
        ApplyTorque(- (AngularVelocity * AngularVelocity.Length()) * angleDrag);
    }
}
