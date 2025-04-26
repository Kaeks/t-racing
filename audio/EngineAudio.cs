using Godot;

public partial class EngineAudio : AudioStreamPlayer3D
{
    private AudioStreamWav _resource;
    private float _sampleHz;
    private ShipController _ship;

    public override void _Ready()
    {
        _ship = GetParent<ShipController>();
        _resource = Stream as AudioStreamWav;
    }

    public override void _Process(double delta)
    {
        float speed = _ship.ForwardVelocity.Length();
        float thrust = _ship.Thrust;
        float val = thrust * 0.3f + Mathf.Sqrt(speed / 300) * 0.7f;
        PitchScale = Mathf.Lerp(0.35f, 1.5f, val);
        VolumeDb = Mathf.Lerp(-10, 0, val);
        Play();
    }


}
