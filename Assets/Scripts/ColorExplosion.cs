using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ColorExplosion : MonoBehaviour // POOLED
{
    SphereCollider Sphere;

    [HideInInspector] public bool Charged;

    [SerializeField] float Speed;
    [SerializeField] AudioSource Audio;
    [SerializeField] SO_GameData gameData;

    void Awake() => Sphere = GetComponent<SphereCollider>();

    void Update()
    {
        Sphere.radius += Time.deltaTime * Speed;

        //if (Charged && Sphere.radius > 5) gameData.ColorPool.Release(this);

        //if (!Charged && Sphere.radius > 1.5) gameData.ColorPool.Release(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GroundBehaviour gb)) gb.FlashColor();
    }
}
