using Godot;
using System;

[GlobalClass]
public partial class ShipLoader : Resource
{
    [Export]
    public ShipStats stats;

    [Export]
    public ShipMeta meta;
}
