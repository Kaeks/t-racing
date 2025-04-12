using Godot;
using System;

[GlobalClass]
public partial class ShipStats : Resource
{
    [Export]
    public float TurnRate { get; set; }

    [Export]
    public float Thrust { get; set; }

    [Export]
    public float Weight { get; set; }

    [Export]
    public float DragCoeff { get; set; }

    [Export]
    public float Area { get; set; }

    [Export]
    public float Boost { get; set; }

    public ShipStats() : this(0, 0, 0, 0, 0, 0) {}

    public ShipStats(float turnRate, float thrust, float weight, float dragCoeff, float area, float boost) {
        TurnRate = turnRate;
        Thrust = thrust;
        Weight = weight;
        DragCoeff = dragCoeff;
        Area = area;
        Boost = boost;
    }

}
