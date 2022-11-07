using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using UniRx.Toolkit;



[CreateAssetMenu(fileName = "New Game Data", menuName = "Custom/Game Data")]
public class SO_GameData : ScriptableObject
{
    private ObjectPool<GroundBehaviour> m_GroundPool;
    public ObjectPool<GroundBehaviour> GroundPool { get {
            if (m_GroundPool == null)
                m_GroundPool = new ObjectPool<GroundBehaviour>(OnCreateGround, OnTakeGround, OnReturnGround);
            return m_GroundPool;
        }
    }
    [SerializeField] GroundBehaviour GroundPrefab;
    GroundBehaviour OnCreateGround() => Instantiate(GroundPrefab);
    void OnReturnGround(GroundBehaviour ground) => ground.gameObject.SetActive(false);
    void OnTakeGround(GroundBehaviour ground) => ground.gameObject.SetActive(true);

    private ObjectPool<Projectile> m_ProjectilePool;
    public ObjectPool<Projectile> ProjectilePool
    { get {
            if (m_ProjectilePool == null)
                m_ProjectilePool = new ObjectPool<Projectile>(OnCreateColor, OnTakeColor, OnReturnColor);
            return m_ProjectilePool;
        }
    }
    [SerializeField] Projectile ProjectilePrefab;
    Projectile OnCreateColor() => Instantiate(ProjectilePrefab);
    void OnReturnColor(Projectile proj) => proj.gameObject.SetActive(false);
    void OnTakeColor(Projectile proj) => proj.gameObject.SetActive(true);

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
