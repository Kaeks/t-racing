using Godot;
using System;

[GlobalClass]
public partial class ShipStats : Resource
{
    [Export]
    public float thrust;
    [Export]
    public float stabilizer;
    [Export]
    public float weight;
    [Export]
    public float area;
    [Export]
    public float dragCoeff;
}
