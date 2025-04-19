using Godot;
using System;

public partial class ShipController_Legacy : RigidBody3D
{

    public class ShipControls {
        public float airbrakeLeft;
        public float airbrakeRight;
        
        public float turn;
        public float nose;

        public float thrust;

        public bool boost;

        public bool use;
        public bool absorb;
        public bool camera;
    }

    [Export]
    public CollisionShape3D _shipCollider;
    [Export]
    public CameraController _followCam;
    [Export]
    public MeshInstance3D _shipMesh;

    [Export]
    public ShipStats_Legacy _shipStats;

    [Export]
    public Node3D[] _airbrakesLeft;
    [Export]
    public Node3D[] _airbrakesRight;

    private readonly ShipControls _shipControls = new();


    // TODO: Get density from medium
    private static readonly float _density = 1.2F;

    // Maximum tilt on turn
    private const float _tiltMax = 10.0F;
    // How fast the ship can tilt
    private const float _tiltRate = 1F;
    
    public static class UniversalAttributes {
        // Angular speed below which the ship will be made to stop turning to prevent asymptotic turning
        public const float angularSlowdownSpeed = 1F;
        // Small-value for rotation stopping
        public const float angularSpeedEpsilon = 0.05F;
        // How strong the angular stopping force should be
        public const float angularSlowdownStrength = 5F;
        public const float angularBrakeForce = 5F;

        // Speed below which the ship will be made to stop to prevent asymptotic motion
        public const float slowdownSpeed = 50F;
        // Small=value for motion stopping
        public const float speedEpsilon = 1F;
        // How strong the stopping force should be
        public const float slowdownStrength = 500F;
        // Small-value for tilt calculation
        public const float tiltEpsilon = 1F;
    }

    // Current goal tilt
    private float tiltGoal = 0.0F;

    // Flags

    private bool turning = false;
    private bool thrusting = false;

    private DebugDraw3D debugDraw;

    public override void _Ready()
    {
        base._Ready();
        Mass = _shipStats.Weight;
        debugDraw = DebugOverlay.Draw;
        debugDraw.AddArrow(this, PropertyName._parallelForce, 1, 5, new("red"));
        debugDraw.AddArrow(this, PropertyName._perpendicularForce, 1, 5, new("red"));
        debugDraw.AddArrow(this, PropertyName.ForwardVelocity, 1, 5, new("blue"));
    }

    public Vector3 _parallelForce;
    public Vector3 _perpendicularForce;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsAction("ship_thrust")) {
            _shipControls.thrust = @event.GetActionStrength("ship_thrust");
        }
        float turn = 0;
        bool turnAction = false;
        if (@event.IsAction("ship_left")) {
            turn -= @event.GetActionStrength("ship_left");
            turnAction = true;
        }
        if (@event.IsAction("ship_right")) {
            turn += @event.GetActionStrength("ship_right");
            turnAction = true;
        }
        if (turnAction) {
            _shipControls.turn = turn;
        }
        if (@event.IsAction("ship_airbrake_left")) {
            _shipControls.airbrakeLeft = @event.GetActionStrength("ship_airbrake_left");
        }
        if (@event.IsAction("ship_airbrake_right")) {
            _shipControls.airbrakeRight = @event.GetActionStrength("ship_airbrake_right");
        }

        tiltGoal = _shipControls.turn * _tiltMax;
        turning = tiltGoal != 0;
        thrusting = _shipControls.thrust > 0;

        if (@event.IsAction("ship_boost")) {
            _shipControls.boost = @event.GetActionStrength("ship_boost") == 1;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _followCam.UpdateView(LinearVelocity);
    }

    public Vector3 ForwardVelocity { get { return Basis.Z * Basis.Z.Dot(LinearVelocity); } }

    public override void _PhysicsProcess(double delta)
    {
        GD.Print(Basis);
        base._PhysicsProcess(delta);
        Vector3 thrustForce =  Basis.Z * _shipStats.Thrust * _shipControls.thrust;
        ApplyCentralForce(thrustForce);

        float dragMagnitude = 0.5F * _density * LinearVelocity.LengthSquared() * _shipStats.DragCoeff * _shipStats.Area;
        Vector3 movementDirection = LinearVelocity.Normalized();
        Vector3 dragForce = -1 * movementDirection * dragMagnitude;
        ApplyCentralForce(dragForce);

        if (!thrusting) {
            float speed = LinearVelocity.Length();
            if (speed <= UniversalAttributes.speedEpsilon) {
                // Reached speed to just stop the ship
                LinearVelocity *= 0;
            } else if (speed <= UniversalAttributes.slowdownSpeed) {
                // Apply a strong slowdown
                float t = (speed - UniversalAttributes.speedEpsilon) / (UniversalAttributes.slowdownSpeed - UniversalAttributes.speedEpsilon);
                float lerp = 1 - t;
                Vector3 forcedSlowdown = -1 * UniversalAttributes.slowdownStrength * LinearVelocity * lerp;
                ApplyForce(forcedSlowdown);
                //GD.Print("Forcing slowdown: t=", t, " lerp=", lerp);
            }
            
            // Apply a brake force
            Vector3 brakeForce = -1 * LinearVelocity.Normalized() * dragMagnitude * 5;
            ApplyCentralForce(brakeForce);
        }        

        if (!turning && (_shipControls.airbrakeLeft == _shipControls.airbrakeRight)) {
            float angularSpeed = AngularVelocity.Length();
            if (angularSpeed <= UniversalAttributes.angularSpeedEpsilon) {
                AngularVelocity *= 0;
            } else if (angularSpeed <= UniversalAttributes.angularSlowdownSpeed) {
                float t = (angularSpeed - UniversalAttributes.angularSpeedEpsilon) / (UniversalAttributes.angularSlowdownSpeed - UniversalAttributes.angularSpeedEpsilon);
                float lerp = 1 - t;
                Vector3 forcedAngularSlowdown = -1 * UniversalAttributes.angularSlowdownStrength * AngularVelocity * lerp;
                //GD.Print(forcedAngularSlowdown);
                ApplyTorque(forcedAngularSlowdown);
            }

            // Apply a brake force
            Vector3 brakeForce = -1 * AngularVelocity.Normalized() * Mass * UniversalAttributes.angularBrakeForce;
            ApplyTorque(brakeForce);
        }

        float _hardcoded_brake_size = 5F;

        Vector3 rotation = new();

        float maxAirbrakeForce = 0.5F * _density * ForwardVelocity.LengthSquared() * _hardcoded_brake_size;

        _parallelForce = Vector3.Zero;
        if (_shipControls.airbrakeLeft > 0) {
            Vector3 airbrakeForceLeft = -1 * LinearVelocity.Normalized() * maxAirbrakeForce * _shipControls.airbrakeLeft;
            foreach (Node3D brake in _airbrakesLeft) {
                rotation += ApplyAirbrake(delta, brake, airbrakeForceLeft);
            }
        }

        if (_shipControls.airbrakeRight > 0) {
            Vector3 airbrakeForceRight = -1 * LinearVelocity.Normalized() * maxAirbrakeForce * _shipControls.airbrakeRight;
            foreach (Node3D brake in _airbrakesRight) {
                rotation += ApplyAirbrake(delta, brake, airbrakeForceRight);
            }
        }

        Vector3 meshRotation = _shipMesh.RotationDegrees;
        if (meshRotation.Z > tiltGoal + UniversalAttributes.tiltEpsilon) {
            _shipMesh.RotateZ(-_tiltRate * (float)delta);
        } else if (meshRotation.Z < tiltGoal - UniversalAttributes.tiltEpsilon) {
            _shipMesh.RotateZ(_tiltRate * (float)delta);
        }
        if (Math.Abs(tiltGoal - meshRotation.Z) <= 2 * UniversalAttributes.tiltEpsilon) {
            _shipMesh.RotationDegrees = new(meshRotation.X, meshRotation.Y, tiltGoal);
        }
        // force to keep ship on a circular course

        float adjustedSpeed = LinearVelocity.Length() / 200F;
        adjustedSpeed = adjustedSpeed < 1 ? 1 : adjustedSpeed;
        rotation.Y += 0.15F * -meshRotation.Z / adjustedSpeed;
        
        RotateY(rotation.Y * (float)delta);

        if (meshRotation.Z != 0 && ForwardVelocity.Length() != 0) {
            float circleRadius = ForwardVelocity.Length() / (rotation.Y + AngularVelocity.Y);
            float centripetalMagnitude = Mass * ForwardVelocity.LengthSquared() / circleRadius;
            Vector3 centripetal = Basis.X * centripetalMagnitude;
            ApplyForce(centripetal);
        }

        //Vector3 sidewaysSpeed = Basis.X * Basis.X.Dot(LinearVelocity);
        //GD.Print("Sideways speed ", sidewaysSpeed);
    }

    private Vector3 ApplyAirbrake(double delta, Node3D brake, Vector3 airbrakeForce) {
        Vector3 brakePos = Basis * brake.Position;
        brakePos.Y = 0;
        Vector3 parallelForce = brakePos.Normalized() * brakePos.Normalized().Dot(airbrakeForce);
        Vector3 perpendicularForce = airbrakeForce - parallelForce;
        Vector3 torque = perpendicularForce.Cross(brakePos);
        Vector3 angularMomentum = (float) delta * torque;
        GD.Print("parallelForce ", Basis.Inverse() * parallelForce);
        GD.Print("perpendicularForce ", Basis.Inverse() * perpendicularForce);
        Vector3 parallelForceResult = parallelForce.Length() * -1 * Basis.Z;
        _parallelForce = -1 * parallelForce;
        _perpendicularForce = perpendicularForce;
        ApplyForce(-1 * parallelForce);
        GD.Print("parallel to Z", (Basis.Inverse() * parallelForce).Z);
        GD.Print("rot:", -angularMomentum / (Mass * brakePos.LengthSquared()));
        return -angularMomentum / (Mass * brakePos.LengthSquared());
    }

    private float TerminalVelocity() {
        return (float) Math.Sqrt(2 * _shipStats.Thrust / (_density * _shipStats.DragCoeff * _shipStats.Area));
    }
}
