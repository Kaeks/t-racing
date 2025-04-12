using Godot;
using System;

public partial class CameraController : Camera3D
{

    private float minFov = 70;
    private float minSpeed = 25;

    private float maxFov = 100;
    private float maxSpeed = 250;

    public void UpdateView(Vector3 Velocity) {
        float magnitude = Velocity.Length();
        Fov = GetSpeedFov(magnitude);
    }

    private float GetSpeedFov(float speed) {
        float t = (speed - minSpeed) / (maxSpeed - minSpeed);
        float lerp = minFov + (maxFov - minFov) * t;
        return Math.Clamp(lerp, minFov, 178);
    }

}
