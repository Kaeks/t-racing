using Godot;
using System;

public partial class Man01Spedometer : CommonSpedometer
{
	[Export]
	public AnimatedSprite2D sprite;

	private int frames;

    public override void _Ready()
    {
        base._Ready();
		frames = sprite.SpriteFrames.GetFrameCount("default");
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
		int frame = (int) Mathf.Round(Mathf.Lerp(0, frames - 1, speed / maxSpeed));
		sprite.SetFrameAndProgress(frame, 0);
    }

}
