using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;


public class Projectile : MonoBehaviour // POOLED
{
    public enum ProjectileType
    {
        player,
        enemy
    }

    MeshFilter meshFilter;
    Renderer rend;
    Rigidbody rb;
    ProjectileType projType;
    Vector3 Direction = Vector3.zero;
    Vector3 Rotation = Vector3.zero;
    bool Destroyed = false;
    bool Charged;
    bool DamageBoost;
    bool Shake;
    int BaseDamage = 1;
    int Damage = 1;

    ContactPoint[] points;

    public float Speed;
    [SerializeField] Mesh playerMesh;
    [SerializeField] Mesh enemyMesh;
    [SerializeField] Material playerMaterial;
    [SerializeField] Material enemyMaterial;
    [SerializeField] ParticleSystem particles;
    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] CinemachineImpulseSource impulse;
    [SerializeField] SO_GameData playerData;

    bool damageDealt = false;

    public ProjectileType Type
    {
        get { return projType; }
        set
        {
            projType = value;

            switch(value)
            {
                case ProjectileType.player:
                    meshFilter.mesh = playerMesh;
                    rend.material = playerMaterial;
                    Speed = 20f;
                    break;
                case ProjectileType.enemy:
                    meshFilter.mesh = enemyMesh;
                    rend.material = enemyMaterial;
                    Speed = 7f;
                    break;
                default:
                    break;
            }
        }
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!Destroyed)
        {
            //transform.Translate(Speed * Time.deltaTime * Direction.normalized, Space.World);
            transform.Rotate(Rotation * Time.deltaTime, Space.Self);
        }
    }

    private void OnEnable()
    {
        Destroyed = false;
        sphereCollider.enabled = true;
        rend.enabled = true;
    }

    private void OnParticleSystemStopped()
    {
        playerData.ProjectilePool.Release(this);
    }

    public Projectile Initialize(ProjectileType type, Vector3 pos, Vector3 dir, PlayerMovement.PowerupMask powerups = PlayerMovement.PowerupMask.None, bool charged = false, Vector3 rot = default)
    {
        Type = type;
        transform.position = pos;
        rb.velocity = Speed * dir;
        Direction = dir;
        Rotation = rot;
        DamageBoost = (powerups & PlayerMovement.PowerupMask.DamageBoost) != 0;
        Charged = charged;
        Shake = (powerups & PlayerMovement.PowerupMask.Multishot) != 0 && !charged;
        gameObject.SetActive(true);
        return this;
    }

    private void FlagAsDestroyed()
    {
        Destroyed = true;
        sphereCollider.enabled = false;
        rend.enabled = false;
        rb.velocity = Vector3.zero;
        particles.Play();
    }

    // TODO: IMPLEMENT AUDIO ON HIT

    private void OnCollisionEnter(Collision collision)
    {
        points = collision.contacts;
        // If current projectile is from enemy
        if (Type == ProjectileType.enemy && !Destroyed)
        {
            // If projectile hits player
            if (collision.gameObject.CompareTag("Player") && !damageDealt)
            {
                // Player takes damage
                impulse.GenerateImpulse(0.7f);
                GameHandler.instance.LargeRumble();
                damageDealt = true;

                // Play damage animation and take damage
                collision.gameObject.GetComponent<PlayerMovement>().OnDamaged();

                // Spawn explosion particles
                //GameObject obj = Instantiate(enemyExplosion, transform.position, Quaternion.Euler(0, 0, 0));

                // Destroy particles and self
                //Destroy(obj, 1f);
                FlagAsDestroyed();
            }
            else if (collision.gameObject.CompareTag("Level") || collision.gameObject.CompareTag("Wall")) // If projectile hits level
            {
                collision.gameObject.TryGetComponent(out GroundBehaviour gb);
                if (gb != null) gb.FlashColor(Color.red);

                // Spawn explosion
                //GameObject obj = Instantiate(enemyExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                // Destroy explosion and self
                //Destroy(obj, 1f);

                FlagAsDestroyed();
            }
        }
        else if (Type == ProjectileType.player && !Destroyed) // If current projectile is from player
        {
            // If projectile hits enemy
            if (collision.gameObject.CompareTag("Enemy") && !damageDealt)
            {
                damageDealt = true;

                // Spawn level flash
                //GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));

                // Determine charged status
                if (Charged)
                {
                    //f.GetComponent<ColorExplosion>().Charged = true;
                    // Enemy takes damage
                    impulse.GenerateImpulse(0.7f);
                    GameHandler.instance.LargeRumble();
                    collision.gameObject.GetComponent<EnemyMovement>().TakeDamage(Damage + 3, Direction, Charged);
                }
                else
                {
                    //f.GetComponent<ColorExplosion>().Charged = false;
                    // Enemy takes damage
                    /*if (shake)*/ impulse.GenerateImpulse(0.2f);
                    GameHandler.instance.SmallRumble();
                    collision.gameObject.GetComponent<EnemyMovement>().TakeDamage(Damage, Direction, Charged);
                }

                // Spawn player particles and destroy them with self
                //GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                //Destroy(obj, 1f);
                FlagAsDestroyed();
            }
            else if (collision.gameObject.CompareTag("Level") || collision.gameObject.CompareTag("Wall")) // If projectile hits level
            {
                // if the projectile didn't hit a wall and was charged
                if (collision.gameObject.CompareTag("Level") && Charged)
                {
                    impulse.GenerateImpulse(0.7f);
                    GameHandler.instance.LargeRumble();

                    // Replace taller level with smaller
                    //GameObject tm = Instantiate(small, other.transform.position + Vector3.up * -0.5f, Quaternion.Euler(0, 0, 0), GameHandler.instance.currentLevel.transform);
                    //tm.GetComponent<Renderer>().material.SetColor("Emission_Color", other.gameObject.GetComponent<Renderer>().material.GetColor("Emission_Color"));
                    //Destroy(other.gameObject);

                    // Spawn particles and flash
                    //GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                    //GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));
                    //f.GetComponent<ColorExplosion>().Charged = true;

                    // Rebuild NavMesh for enemies
                    //GameHandler.instance.currentLevel.GetComponent<NavMeshSurface>().RemoveData();
                    //GameHandler.instance.currentLevel.GetComponent<NavMeshSurface>().BuildNavMesh();

                    // Destroy particles and self
                    //Destroy(obj, 1f);
                    collision.gameObject.TryGetComponent(out GroundBehaviour gb);
                    if (gb != null) gb.FlashColor();
                    FlagAsDestroyed();
                }
                else
                {
                    // Spawn particles and flash
                    //GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                    //GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));

                    // Determine charged state
                    if (Charged)
                    {
                        impulse.GenerateImpulse(0.7f);
                        GameHandler.instance.LargeRumble();
                        //f.GetComponent<ColorExplosion>().Charged = true;
                    }
                    else
                    {
                        if (Shake)
                        {
                            impulse.GenerateImpulse(0.2f);
                            GameHandler.instance.SmallRumble();
                        }
                        //f.GetComponent<ColorExplosion>().Charged = false;
                    }

                    // Destroy particles and self
                    //Destroy(obj, 1f);
                    collision.gameObject.TryGetComponent(out GroundBehaviour gb);
                    if (gb != null) gb.FlashColor();
                    FlagAsDestroyed();
                }
            }
        }
        
    }

    Color RandomColor
    {
        get
        {
            Vector3 v = new Vector3(Random.value, Random.value, Random.value).normalized;
            return new Color(v.x, v.y, v.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = RandomColor;
        foreach (ContactPoint p in points)
            Gizmos.DrawSphere(p.point, 0.1f);
    }
}
