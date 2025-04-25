using Godot;
using System;
using System.Collections.Generic;

public partial class FloorGenerator : StaticBody3D
{
	[Export]
	public PackedScene tileScene;
	[Export]
	public ShipController ship;
	private readonly int chunkSize = 16;
	private readonly int tileSize = 2;
	private readonly List<Vector2I> tiledChunks = [];

	public override void _Process(double delta)
	{
		Vector3 pos = ship.Position;
		Vector2I chunk = GetChunk((int) Math.Floor(pos.X), (int) Math.Floor(pos.Z));
		if (tiledChunks.Contains(chunk)) return;
		GenerateChunk(chunk);
	}

	private Vector2I GetChunk(int x, int y) {
		int chunkX = (int) Math.Floor((double) x / (chunkSize * tileSize));
		int chunkY = (int) Math.Floor((double) y / (chunkSize * tileSize));
		return new(chunkX, chunkY);
	}

	private void GenerateChunk(Vector2I chunk) {
		tiledChunks.Add(chunk);
		int baseX = chunk.X * chunkSize * tileSize;
		int baseY = chunk.Y * chunkSize * tileSize;
		Node3D chunkNode = new();
		chunkNode.Position = new Vector3(baseX, 0, baseY);
		AddChild(chunkNode);
		for (int i = 0; i < chunkSize; i++) {
			int offsetX = i * tileSize;
			for (int j = 0; j < chunkSize; j++) {
				int offsetY = j * tileSize;
				MeshInstance3D tileNode = tileScene.Instantiate<MeshInstance3D>();
				tileNode.Position = new Vector3(offsetX, 0, offsetY);
				chunkNode.AddChild(tileNode);
			}
		}
	}
}
