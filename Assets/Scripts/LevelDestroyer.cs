using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LevelDestroyer : MonoBehaviour
{
    SphereCollider Sphere;
    [SerializeField] float Speed;
    [SerializeField] AudioSource Audio;
    [SerializeField] SO_GameData gameData;

    private void Awake() => Sphere = GetComponent<SphereCollider>();

    private void OnEnable() => AudioHandler.instance.PlaySound("Level_Transition", Audio);

    private void OnDisable() => Sphere.radius = 0f;

    void Update()
    {
        Sphere.radius += Time.deltaTime * Speed;
        if (Sphere.radius > 19f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GroundBehaviour gb))
            gb.StartDestruction();

        if (other.TryGetComponent(out Pickup p))
            p.OnLevelDown();

        if (other.TryGetComponent(out Projectile pr))
            Destroy(pr.gameObject);
    }
}
