using System.Collections;
using System.Collections.Generic;

public class PickupData
{
    public Pickup.PickupType Type;
    public float Duration { get; private set; }
    public float activeTime;

    public PickupData(float duration, Pickup.PickupType type)
    {
        Duration = duration;
        activeTime = 0;
        Type = type;
    }
}
