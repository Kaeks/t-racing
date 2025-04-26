using Godot;

public enum Manufacturer {
    POOPEN, FARTEN, BYORZOJ
}

[GlobalClass]
public partial class ShipMeta : Resource
{

    [Export]
    public Manufacturer manufacturer;

}
