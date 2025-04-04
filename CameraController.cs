using Godot;
using System;

public partial class CameraController : Camera3D
{

    private float minFov = 70;
    private float minSpeed = 1;

    private float maxFov = 100;
    private float maxSpeed = 10;

    public void UpdateView(Vector3 Velocity) {
        float magnitude = Velocity.Length();
        Fov = GetSpeedFov(magnitude);
    }

    private float GetSpeedFov(float speed) {
        float ratio = (maxFov - minFov) / (maxSpeed - minSpeed);
        float fov = ratio * (speed - minSpeed) + minFov;
        return Math.Clamp(fov, minFov, 178);
    }

}
