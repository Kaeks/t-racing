using Godot;

public partial class DebugOverlay : CanvasLayer
{

	public static DebugDraw3D Draw { get; private set; }
	public static DebugOverlay Instance { get; private set; }

	public override void _Ready() {
		Instance = this;
		Draw = GetNode<DebugDraw3D>("DebugDraw3D");
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		if (!Visible) {
			return;
		}
		Draw.QueueRedraw();
    }

}
