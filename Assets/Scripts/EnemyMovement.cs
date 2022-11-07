using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class EnemyMovement : MonoBehaviour
{
    Vector3 direction = Vector3.zero;
    [SerializeField] Projectile projectile;
    [SerializeField] SO_GameData playerData;
    [SerializeField] GameObject enemyDeath;
    [SerializeField] GameObject[] pickups;
    [SerializeField] new AudioSource audio;

    bool inRange = false;

    [SerializeField] LayerMask layerMask;

    public EnemyBehaviour behaviour;
    public int hitPoints;

    public NavMeshAgent agent;

    MaterialPropertyBlock block;
    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        switch (behaviour)
        {
            case EnemyBehaviour.ShootAround:
                StartCoroutine(ShootEverywhere());
                break;

            case EnemyBehaviour.ShootAtPlayer:
                StartCoroutine(ShootAtPlayer());
                break;

            default:
                break;
        }
    }

    public void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        block = new MaterialPropertyBlock();
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inRange)
        {
            agent.Move(agent.speed * Time.deltaTime * direction);
        }
        else
        {
            agent.destination = playerData.position;
        }
    }

    public void MoveAround()
    {
        StartCoroutine(SwitchDirection());
    }

    IEnumerator SwitchDirection()
    {
        while (inRange)
        {
            float rndX = Random.value * 2;
            float rndY = Random.value * 2;

            direction = new Vector3(rndX - 1, 0, rndY - 1).normalized;
            yield return new WaitForSeconds(Mathf.Clamp(Random.value, 0.2f, 1));
        }
        direction = Vector3.zero;
    }

    IEnumerator ShootAtPlayer()
    {
        yield return new WaitForSeconds(Random.value * 0.3f);
        while (true)
        {
            Projectile obj = Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, 0), transform.parent);
            obj.Direction = (playerData.position - transform.position).normalized;
            obj.direction.y = 0;
            obj.Rotation = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);
            obj.Speed = 7;

            AudioHandler.instance.PlaySound("Player_Shoot", audio, 0.7f);
            yield return new WaitForSeconds(1);
        }
    }

    public void TakeDamage(int amount, Vector3 dir, bool charged)
    {
        hitPoints -= amount;

        int intensity;

        if (charged)
        {
            intensity = 30;
        }
        else
        {
            intensity = 10;
        }

        if (hitPoints <= 0)
        {
            Die();
        }
        else
        {
            AudioHandler.instance.PlaySound("Enemy_Hurt", audio);
            StartCoroutine(FlashDamage());
            StartCoroutine(MoveAway(dir, intensity));
        }
    }

    IEnumerator MoveAway(Vector3 dir, int intensity)
    {
        float t = 0.03f;
        while (t > 0)
        {
            agent.isStopped = true;
            agent.Move(intensity * Time.deltaTime * dir);
            t -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        agent.isStopped = false;
    }

    void Die()
    {
        if (!agent.isStopped)
            agent.isStopped = true;

        GameObject death = Instantiate(enemyDeath, transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(death, 1f);

        int rnd = Random.Range(0, 5) + 1;

        int rndIndex = Random.Range(0, pickups.Length);

        if (rnd == 5) 
            Instantiate(pickups[rndIndex], transform.position, Quaternion.identity);

        GameHandler.instance.RemoveEnemy(gameObject);

        Destroy(gameObject);
    }

    IEnumerator FlashDamage()
    {
        float amount = 0.75f;

        while (amount > 0)
        {
            Vector3 current = Vector3.Lerp(Vector3.one, Vector3.right, 1 - amount);

            block.SetColor("_BaseColor", new Color(current.x, current.y, current.z));
            rend.SetPropertyBlock(block);
            amount -= Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        block.SetColor("_BaseColor", Color.red);
        rend.SetPropertyBlock(block);
    }

    IEnumerator ShootEverywhere()
    {
        yield return new WaitForSeconds(Random.value * 0.3f);
        while (true)
        {
            float offset = Random.value * 45;

            for (int i = 0; i < 8; i++)
            {
                Projectile obj = Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, 0), transform.parent);
                obj.Direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * i * 45 + offset), 0, Mathf.Sin(Mathf.Deg2Rad * i * 45 + offset));
                obj.Rotation = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);
                obj.Speed = 4;
            }

            AudioHandler.instance.PlaySound("Player_Shoot", audio, 0.7f);
            yield return new WaitForSeconds(1.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inRange)
        {
            if (other.gameObject.CompareTag("EnemyRange") && !agent.Raycast(agent.destination, out NavMeshHit nh))
            {
                inRange = true;
                agent.isStopped = true;
                MoveAround();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Physics.Raycast(transform.position, playerData.position - transform.position, out RaycastHit rh, Mathf.Infinity, layerMask))
        {
            if (rh.collider.gameObject.CompareTag("Player"))
            {
                if (!inRange)
                {
                    inRange = true;
                    agent.isStopped = true;
                    MoveAround();
                } 
            }
            else if (rh.collider.gameObject.CompareTag("Wall") || rh.collider.gameObject.CompareTag("Level"))
            {
                inRange = false;
                agent.isStopped = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyRange"))
        {
            agent.isStopped = false;
            inRange = false;
        }
    }
}

public enum EnemyBehaviour
{
    ShootAtPlayer,
    ShootAround
}
