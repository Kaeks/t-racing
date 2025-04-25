using Godot;

public enum Manufacturer {
    POOPEN, FARTEN, BYORZOJ
}

public partial class ShipMeta : Resource
{

    [Export]
    public Manufacturer manufacturer;

}
