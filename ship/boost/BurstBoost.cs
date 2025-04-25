using Godot;

public partial class BurstBoost : BoostSystem
{

    private float cooldown = 5;
    private double sinceLast = -1;

    private double duration = 0.5f;

    private bool toBoost = false;

    public override void _Process(double delta)
    {
        if (sinceLast > -1) sinceLast += delta;
        if (sinceLast > cooldown) sinceLast = -1;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (sinceLast > duration || sinceLast == -1) return;
        if (toBoost) { sinceLast = 0; toBoost = false; }
        Vector3 forward = -ship.Basis.Z.Normalized();
        float weight = 2 * RatioAt(sinceLast);
        ship.ApplyForce(forward * strength * weight);

    }

    public override void OnTrigger()
    {
        // ready
        if (sinceLast != -1) return;
        GD.Print("GO");
        toBoost = true;
        sinceLast = 0;
    }

    private float RatioAt(double t) {
        if (t < 0 || t >= duration) return 0;
        if (t <= duration / 3) return Mathf.Lerp(0, 1, (float) (3 * t / duration));
        return Mathf.Lerp(1, 0, (float) (3 * t / duration - 1) / 2);
    }

}
