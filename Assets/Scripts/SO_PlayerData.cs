using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Custom/Player Data")]
public class SO_PlayerData : ScriptableObject
{
    public Vector3 position { get; private set; }
    public int hitPoints { get; private set; }
    public int charges {  get; private set; }

    public Vector3Int GetIntPosition()
    {
        return Vector3Int.FloorToInt(position);
    }

    public void SetPosition(Vector3 pos)
    {
        position = pos;
    }

    public void TakeDamage(int amount)
    {
        hitPoints = Mathf.Clamp(hitPoints - amount, 0, 10);
    }

    public void Heal(int amount)
    {
        hitPoints = Mathf.Clamp(hitPoints + amount, 0, 10);
    }

    public void SetHitPoints(int amount)
    {
        hitPoints = amount;
    }

    public void AddCharges(int amount)
    {
        charges = Mathf.Clamp(charges + amount, 0, 4);
    }

    public void RemoveCharges(int amount)
    {
        charges = Mathf.Clamp(charges - amount, 0, 4);
    }
}
