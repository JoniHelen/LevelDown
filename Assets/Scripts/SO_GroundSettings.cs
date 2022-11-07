using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Ground Settings", menuName = "Custom/Ground Settings")]
public class SO_GroundSettings : ScriptableObject
{
    public float Glow;
    public float FlashBrightness;
    public float FlashDuration;
}
