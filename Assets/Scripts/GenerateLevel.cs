using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateLevel : MonoBehaviour
{
    [SerializeField] GameObject small;
    [SerializeField] GameObject large;
    [SerializeField] GameObject enemy;
    [SerializeField] SO_GameData gameData;
    [SerializeField] NavMeshSurface levelSurface;

    [SerializeField] SO_LevelPool easyPool;
    [SerializeField] SO_LevelPool hardPool;
    [SerializeField] SO_LevelPool veryHardPool;

    [SerializeField] int width;
    [SerializeField] int height;

    [SerializeField] float scale;
    [SerializeField] float offsetX;
    [SerializeField] float offsetY;

    [Range(0f, 1f)]
    [SerializeField] float threshold;

    Dictionary<Vector2Int, bool> EnemyPos = new Dictionary<Vector2Int, bool>();
    Vector3 startPos;

    Color RandomColor { get {
            Vector3 v = new Vector3(Random.value, Random.value, Random.value).normalized;
            return new Color(v.x, v.y, v.z);
        }
    }

    void Start()
    {
        startPos = transform.position;
        GenerateFloor();
    }

    void GenerateFloor()
    {
        float rnd = Random.Range(0f, 100f);

        Vector2Int pOffset = PlayerOffset(rnd);

        Color rndColor = RandomColor;

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                float xCoord = (float)x / (width - 2) * scale + rnd + pOffset.x;
                float yCoord = (float)y / (height - 2) * scale + rnd + pOffset.y;

                float perlin = Mathf.PerlinNoise(xCoord, yCoord);

                GroundBehaviour l = gameData.GroundPool.Get();
                l.transform.parent = transform;
                l.transform.position = transform.position + new Vector3(x + offsetX, 1, y + offsetY);

                l.GroundColor = rndColor;

                if (perlin > Mathf.Clamp(threshold, 0, 1))
                    l.Type = GroundBehaviour.GroundType.Tall;
                else
                    EnemyPos.Add(new Vector2Int(x, y), false);
            }
        }

        levelSurface.BuildNavMesh();

        PlayerCheck();

        if (GameHandler.instance.levelNumber <= 5)
        {
            SpawnEnemies(easyPool);
        }
        else if (GameHandler.instance.levelNumber <= 20)
        {
            SpawnEnemies(hardPool);
        }
        else
        {
            SpawnEnemies(veryHardPool);
        }
    }

    void PlayerCheck()
    {
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                Vector2Int key = new Vector2Int(Mathf.FloorToInt(gameData.position.x - offsetX) + (x - 3), Mathf.FloorToInt(gameData.position.z - offsetY) + (y - 3));

                if (EnemyPos.ContainsKey(key))
                {
                    EnemyPos[key] = true;
                }
            }
        }
    }

    Vector2Int PlayerOffset(float random)
    {
        int xOffset = 0;
        int yOffset = 0;

        while (true)
        {
            int goodSpot = 0;

            for(float x = 0; x < 5; x++)
            {
                for (float y = 0; y < 5; y++)
                {
                    if (Mathf.PerlinNoise((Mathf.FloorToInt(gameData.position.x - offsetX) + (x - 2)) / (width - 2) * scale + random + xOffset, (Mathf.FloorToInt(gameData.position.z - offsetY) + (y - 2)) / (height - 2) * scale + random + yOffset) < Mathf.Clamp(threshold, 0, 1))
                    {
                        goodSpot++;
                    }
                }
            }

            if (goodSpot == 25)
            {
                return new Vector2Int(xOffset, yOffset);
            }
            else
            {
                xOffset++;
                yOffset++;
            }
        }
    }

    void SpawnEnemies(SO_LevelPool pool)
    {
        int success = 0;

        int rand = Random.Range(0, pool.LevelDataPool.Length);

        int len;

        if (GameHandler.instance.levelNumber <= 5)
        {
            len = pool.LevelDataPool[GameHandler.instance.levelNumber - 1].enemyBehaviours.Length;
        }
        else
        {
            len = pool.LevelDataPool[rand].enemyBehaviours.Length;
        }

        while(success < len)
        {
            Vector2Int pos = new Vector2Int(Random.Range(1, width), Random.Range(1, height));

            if (EnemyPos.ContainsKey(pos))
            {
                if (!EnemyPos[pos])
                {
                    GameObject obj = Instantiate(enemy, transform.position + new Vector3(pos.x + offsetX, 1.5f, pos.y + offsetY), Quaternion.Euler(0, 0, 0), transform);
                    if (GameHandler.instance.levelNumber <= 5)
                    {
                        obj.GetComponent<EnemyMovement>().behaviour = pool.LevelDataPool[GameHandler.instance.levelNumber - 1].enemyBehaviours[success];
                    }
                    else
                    {
                        obj.GetComponent<EnemyMovement>().behaviour = pool.LevelDataPool[rand].enemyBehaviours[success];
                    }

                    for (int x = 0; x < 5; x++)
                    {
                        for (int y = 0; y < 5; y++)
                        {
                            if (EnemyPos.ContainsKey(new Vector2Int(pos.x + (x - 2), pos.y + (y - 2))))
                            {
                                EnemyPos[new Vector2Int(pos.x + (x - 2), pos.y + (y - 2))] = true;
                            }
                        }
                    }

                    GameHandler.instance.Enemies.Add(obj);
                    obj.GetComponent<EnemyMovement>().hitPoints += Mathf.FloorToInt(0.1f * GameHandler.instance.levelNumber);
                    success++;

                    EnemyPos[pos] = true;
                }
            }
        }

        StartCoroutine(MoveUp());
    }

    IEnumerator MoveUp()
    {
        float t = 0;

        if (GameHandler.instance.levelNumber == 1)
        {
            while (t < GameHandler.instance.audio.clip.length - 0.01f)
            {
                t = GameHandler.instance.audio.time;
                transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * 40, t / GameHandler.instance.audio.clip.length);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (t < 2)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * 40, t / 2);
                yield return new WaitForEndOfFrame();
            }
        }
        GameHandler.instance.PlayTheme();

        GameHandler.instance.player.GetComponent<PlayerMovement>().enabled = true;
        GameHandler.instance.player.GetComponent<PlayerMovement>().canTakeDamage = true;
        GameHandler.instance.gun.GetComponent<GunBehaviour>().canShoot = true;
        GameHandler.instance.AwakenEnemies();
        GameHandler.instance.levelCreated = false;
    }
}
