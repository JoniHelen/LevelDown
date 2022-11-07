using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;


public class Projectile : MonoBehaviour // POOLED
{
    Vector3 Direction = Vector3.zero;
    Vector3 Rotation = Vector3.zero;
    bool Charged;
    bool Shake;
    int BaseDamage = 1;
    int Damage = 1;

    public float Speed;
    [SerializeField] GameObject playerExplosion;
    [SerializeField] GameObject enemyExplosion;
    [SerializeField] GameObject small;
    [SerializeField] GameObject flash;
    [SerializeField] CinemachineImpulseSource impulse;
    [SerializeField] SO_GameData playerData;

    bool damageDealt = false;

    void Update()
    {
        transform.Translate(Speed * Time.deltaTime * Direction.normalized, Space.World);
        transform.Rotate(Rotation * Time.deltaTime, Space.Self);
    }

    public void Initialize(Vector3 pos = default, Vector3 dir = default, Vector3 rot = default, int dmg = 1, bool charged = false, bool shake = false)
    {
        transform.position = pos;
        Direction = dir;
        Rotation = rot;
        Damage = dmg;
        Charged = charged;
        Shake = shake;
    }

    // TODO: IMPLEMENT AUDIO ON HIT

    private void OnTriggerEnter(Collider other)
    {
        // If current projectile is from enemy
        if (gameObject.CompareTag("EnemyProj"))
        {
            // If projectile hits player
            if (other.gameObject.CompareTag("Player") && !damageDealt)
            {
                // Player takes damage
                impulse.GenerateImpulse(0.7f);
                GameHandler.instance.LargeRumble();
                damageDealt = true;

                // Play damage animation and take damage
                other.gameObject.GetComponent<PlayerMovement>().OnDamaged();

                // Spawn explosion particles
                GameObject obj = Instantiate(enemyExplosion, transform.position, Quaternion.Euler(0, 0, 0));

                // Destroy particles and self
                Destroy(obj, 1f);
                Destroy(gameObject);
            }
            else if (other.gameObject.CompareTag("Level") || other.gameObject.CompareTag("Wall")) // If projectile hits level
            {
                other.gameObject.TryGetComponent(out GroundBehaviour gb);
                if (gb != null) gb.FlashColor(Color.red);

                // Spawn explosion
                GameObject obj = Instantiate(enemyExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                // Destroy explosion and self
                Destroy(obj, 1f);
                Destroy(gameObject);
            }
        }
        else if (gameObject.CompareTag("PlayerProj")) // If current projectile is from player
        {
            // If projectile hits enemy
            if (other.gameObject.CompareTag("Enemy") && !damageDealt)
            {
                damageDealt = true;

                // Spawn level flash
                GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));

                // Determine charged status
                if (Charged)
                {
                    f.GetComponent<ColorExplosion>().Charged = true;
                    // Enemy takes damage
                    impulse.GenerateImpulse(0.7f);
                    GameHandler.instance.LargeRumble();
                    other.gameObject.GetComponent<EnemyMovement>().TakeDamage(Damage + 3, Direction, Charged);
                }
                else
                {
                    f.GetComponent<ColorExplosion>().Charged = false;
                    // Enemy takes damage
                    /*if (shake)*/ impulse.GenerateImpulse(0.2f);
                    GameHandler.instance.SmallRumble();
                    other.gameObject.GetComponent<EnemyMovement>().TakeDamage(Damage, Direction, Charged);
                }

                // Spawn player particles and destroy them with self
                GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                Destroy(obj, 1f);
                
                Destroy(gameObject);
            }
            else if (other.gameObject.CompareTag("Level") || other.gameObject.CompareTag("Wall")) // If projectile hits level
            {
                // if the projectile didn't hit a wall and was charged
                if (other.gameObject.CompareTag("Level") && Charged)
                {
                    impulse.GenerateImpulse(0.7f);
                    GameHandler.instance.LargeRumble();

                    // Replace taller level with smaller
                    GameObject tm = Instantiate(small, other.transform.position + Vector3.up * -0.5f, Quaternion.Euler(0, 0, 0), GameHandler.instance.currentLevel.transform);
                    tm.GetComponent<Renderer>().material.SetColor("Emission_Color", other.gameObject.GetComponent<Renderer>().material.GetColor("Emission_Color"));
                    Destroy(other.gameObject);

                    // Spawn particles and flash
                    GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                    GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));
                    f.GetComponent<ColorExplosion>().Charged = true;

                    // Rebuild NavMesh for enemies
                    GameHandler.instance.currentLevel.GetComponent<NavMeshSurface>().RemoveData();
                    GameHandler.instance.currentLevel.GetComponent<NavMeshSurface>().BuildNavMesh();

                    // Destroy particles and self
                    Destroy(obj, 1f);
                    Destroy(gameObject);
                }
                else
                {
                    // Spawn particles and flash
                    GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                    GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));

                    // Determine charged state
                    if (Charged)
                    {
                        impulse.GenerateImpulse(0.7f);
                        GameHandler.instance.LargeRumble();
                        f.GetComponent<ColorExplosion>().Charged = true;
                    }
                    else
                    {
                        if (Shake)
                        {
                            impulse.GenerateImpulse(0.2f);
                            GameHandler.instance.SmallRumble();
                        }
                        f.GetComponent<ColorExplosion>().Charged = false;
                    }

                    // Destroy particles and self
                    Destroy(obj, 1f);
                    Destroy(gameObject);
                }
            }
        }
        
    }
}
