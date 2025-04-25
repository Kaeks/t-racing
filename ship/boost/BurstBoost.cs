using Godot;

public partial class BurstBoost : BoostSystem
{

    private float cooldown = 5;
    private double sinceLast = -1;

    public override void _Process(double delta)
    {
        if (sinceLast > -1) sinceLast += delta;
        if (sinceLast > cooldown) sinceLast = -1;
    }


    public override void OnTrigger()
    {
        // ready
        if (sinceLast != -1) return;
        GD.Print("GO");

        Vector3 forward = -ship.Basis.Z.Normalized();
        ship.ApplyImpulse(forward * strength);
        sinceLast = 0;
    }

}
