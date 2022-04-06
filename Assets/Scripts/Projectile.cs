using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Projectile : MonoBehaviour
{
    public Vector3 direction = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public float speed;
    public bool charged;

    [SerializeField] GameObject playerExplosion;
    [SerializeField] GameObject enemyExplosion;
    [SerializeField] GameObject small;
    [SerializeField] GameObject flash;
    [SerializeField] SO_PlayerData playerData;

    bool damageDealt = false;

    void Update()
    {
        transform.Translate(direction.normalized * Time.deltaTime * speed, Space.World);
        transform.Rotate(rotation * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If current projectile is from enemy
        if (gameObject.CompareTag("EnemyProj"))
        {
            // If projectile hits player
            if (other.gameObject.CompareTag("Player") && !damageDealt)
            {
                // Player takes damage
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
                if (charged)
                {
                    f.GetComponent<ColorExplosion>().charged = true;
                    // Enemy takes damage
                    other.gameObject.GetComponent<EnemyMovement>().TakeDamage(3, direction, charged);
                }
                else
                {
                    f.GetComponent<ColorExplosion>().charged = false;
                    // Enemy takes damage
                    other.gameObject.GetComponent<EnemyMovement>().TakeDamage(1, direction, charged);
                }

                // Spawn player particles and destroy them with self
                GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                Destroy(obj, 1f);
                
                Destroy(gameObject);
            }
            else if (other.gameObject.CompareTag("Level") || other.gameObject.CompareTag("Wall")) // If projectile hits level
            {
                // if the projectile didn't hit a wall and was charged
                if (other.gameObject.CompareTag("Level") && charged)
                {
                    // Replace taller level with smaller
                    GameObject tm = Instantiate(small, other.transform.position + Vector3.up * -0.5f, Quaternion.Euler(0, 0, 0), GameHandler.instance.currentLevel.transform);
                    tm.GetComponent<Renderer>().material.SetColor("Emission_Color", other.gameObject.GetComponent<Renderer>().material.GetColor("Emission_Color"));
                    Destroy(other.gameObject);

                    // Spawn particles and flash
                    GameObject obj = Instantiate(playerExplosion, transform.position, Quaternion.Euler(0, 0, 0));
                    GameObject f = Instantiate(flash, transform.position, Quaternion.Euler(0, 0, 0));
                    f.GetComponent<ColorExplosion>().charged = true;

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
                    if (charged)
                    {
                        f.GetComponent<ColorExplosion>().charged = true;
                    }
                    else
                    {
                        f.GetComponent<ColorExplosion>().charged = false;
                    }

                    // Destroy particles and self
                    Destroy(obj, 1f);
                    Destroy(gameObject);
                }
            }
        }
        
    }
}
