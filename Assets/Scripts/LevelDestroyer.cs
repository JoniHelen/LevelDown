using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LevelDestroyer : MonoBehaviour
{
    float Size = 0f;
    SphereCollider Sphere;
    ParticleSystem Particles;
    [SerializeField] float Speed;
    [SerializeField] AudioSource Audio;
    [SerializeField] GameObject ParticleObject;
    [SerializeField] SO_GameData gameData;

    private void Awake()  
    {
        Particles = ParticleObject.GetComponent<ParticleSystem>();
        Sphere = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        AudioHandler.instance.PlaySound("Level_Transition", Audio);
        Particles.Play();
    }

    private void OnDisable()
    {
        Sphere.radius = 0f;
        ParticleObject.transform.localScale = Vector3.zero;
        Size = 0f;
        Particles.Stop();
    }

    void Update()
    {
        Size += Time.deltaTime * Speed;
        Sphere.radius = Size;
        ParticleObject.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(6, 1, 6), Size / 19);
        if (Size > 19f)
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
