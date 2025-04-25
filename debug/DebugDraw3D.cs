using System;
using System.Collections.Generic;
using Godot;

public sealed partial class DebugDraw3D : Control
{
    public partial class Arrow(Node3D obj, string property, float scale, float width, Color color) {
        readonly Node3D obj = obj;
        readonly string property = property;
        readonly float scale = scale;
        readonly float width = width;
		readonly Color color = color;

		public void Draw(Control node, Camera3D camera) {
			Vector2 start = camera.UnprojectPosition(obj.GlobalTransform.Origin);
			Variant prop = obj.Get(property);
			if (prop.VariantType != Variant.Type.Vector3) {
				throw new ArgumentException("Expected Vector3");
			}
			Vector3 propVector = prop.AsVector3();
			Vector2 end = camera.UnprojectPosition(obj.GlobalTransform.Origin + propVector * scale);
			if (!IsOnScreen(end)) return;
			node.DrawLine(start, end, color, width);
			DrawTriangle(node, end, start.DirectionTo(end), width * 2, color);
		}

		private static bool IsOnScreen(Vector2 pos) {
			return pos.X < 0 || pos.Y < 0 || pos.X > 2000 || pos.Y > 2000;
		}

		private static void DrawTriangle(Control node, Vector2 pos, Vector2 dir, float size, Color color) {
			Vector2 a = pos + dir * size;
			Vector2 b = pos + dir.Rotated((float)(2 * Math.PI / 3)) * size;
			Vector2 c = pos + dir.Rotated((float)(4 * Math.PI / 3)) * size;
			GD.Print(a, b, c);
			node.DrawColoredPolygon([a, b, c], color);
			GD.Print("PRINTED");
		}
    }

	public List<Arrow> arrows = [];

    public override void _Ready()
    {
        base._Ready();
    }


    public override void _Draw()
    {
        base._Draw();
		Camera3D cam = GetViewport().GetCamera3D();
		foreach (Arrow arrow in arrows) {
			arrow.Draw(this, cam);
		}
		Vector2 a = new(10, 20);
		Vector2 b = new(30, 30);
		DrawLine(a, b, new("blue"), 10);
	}

	public void AddArrow(Node3D obj, string property, float scale, float width, Color color) {
		arrows.Add(new(obj, property, scale, width, color));
	}
}
