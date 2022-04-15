using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class GameHandler : MonoBehaviour
{
    [SerializeField] GameObject level;
    [SerializeField] GameObject wall;
    [SerializeField] GameObject transition;
    [SerializeField] SO_PlayerData playerData;
    public AudioSource audio;
    [SerializeField] AudioSource ChargeBlip;

    [SerializeField] int width;
    [SerializeField] int height;

    [SerializeField] float offsetX;
    [SerializeField] float offsetY;

    float time = 0;

    public GameObject currentLevel;

    public static GameHandler instance;

    public bool levelCreated = false;
    public bool themePlaying = false;
    public int levelNumber = 0;
    public bool Paused = false;

    public GameObject player;
    public GameObject gun;
    public UnityEvent newLevel;
    public UI_Manager uiManager;

    public List<GameObject> Enemies = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        AudioHandler.instance.PlaySound("Start_Noise", audio);
        playerData.SetHitPoints(10);
        GenerateWalls();
        levelCreated = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        gun.GetComponent<GunBehaviour>().canShoot = false;
        NewLevel();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (playerData.charges < 3)
        {
            time += Time.deltaTime;
            if (time >= 10)
            {
                playerData.AddCharges(1);
                uiManager.UpdateCharges();
                AudioHandler.instance.PlaySound("Gain_Charge", ChargeBlip, 0.7f + playerData.charges / 10f);
                time = 0;
            }
        }
    }

    public void PlayTheme()
    {
        if (!themePlaying)
        {
            AudioHandler.instance.PlaySound("Theme_Gameplay", audio, 1f, true);
            themePlaying = true;
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (Enemies.Contains(enemy))
        {
            Enemies.Remove(enemy);
        }
        Destroy(enemy);

        if (Enemies.Count < 1)
        {
            if (!levelCreated)
            {
                levelCreated = true;
                player.GetComponent<PlayerMovement>().enabled = false;
                player.GetComponent<PlayerMovement>().canTakeDamage = false;
                gun.GetComponent<GunBehaviour>().canShoot = false;
                newLevel.Invoke();
            }
        }
    }

    public void AwakenEnemies()
    {
        foreach (GameObject enemy in Enemies)
        {
            enemy.GetComponent<EnemyMovement>().enabled = true;
        }
    }

    void GenerateWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0)
                {
                    Instantiate(wall, new Vector3(x + offsetX, 10, y + offsetY), Quaternion.Euler(0, 0, 0), transform);
                }
                else if (y == 0)
                {
                    Instantiate(wall, new Vector3(x + offsetX, 10, y + offsetY), Quaternion.Euler(0, 0, 0), transform);
                }
                else if (y == height - 1)
                {
                    Instantiate(wall, new Vector3(x + offsetX, 10, y + offsetY), Quaternion.Euler(0, 0, 0), transform);
                }
                else if (x == width - 1)
                {
                    Instantiate(wall, new Vector3(x + offsetX, 10, y + offsetY), Quaternion.Euler(0, 0, 0), transform);
                }
            }
        }
    }

    public void ClearLevel()
    {
        if (levelCreated)
        {
            currentLevel.GetComponent<NavMeshSurface>().RemoveData();
            Instantiate(transition, Vector3.zero, Quaternion.Euler(0, 0, 0), currentLevel.transform);
        }
    }

    public void NewLevel()
    {
        if (levelCreated)
        {
            levelNumber++;
            uiManager.UpdateLevel();
            StartCoroutine(ChargeTransition());
            currentLevel = Instantiate(level, -(Vector3.up * 40), Quaternion.Euler(0, 0, 0));
        }
    }

    IEnumerator ChargeTransition()
    {
        while(playerData.charges < 3)
        {
            playerData.AddCharges(1);
            uiManager.UpdateCharges();
            AudioHandler.instance.PlaySound("Gain_Charge", ChargeBlip, 0.7f + playerData.charges / 10f);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void TogglePauseGame()
    {
        if (!Paused)
        {
            Paused = true;
            player.GetComponent<PlayerInput>().DeactivateInput();
            audio.Pause();
            Time.timeScale = 0;
        }
        else
        {
            player.GetComponent<PlayerInput>().ActivateInput();
            Paused = false;
            if (audio.time > 0)
            {
                audio.Play();
            }
            Time.timeScale = 1;
        }
    }

    public void TogglePauseGame(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (!Paused)
            {
                Paused = true;
                player.GetComponent<PlayerInput>().DeactivateInput();
                audio.Pause();
                Time.timeScale = 0;
            }
            else
            {
                player.GetComponent<PlayerInput>().ActivateInput();
                Paused = false;
                if (audio.time > 0)
                {
                    audio.Play();
                }
                Time.timeScale = 1;
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !Paused)
        {
            audio.Pause();
            Time.timeScale = 0;
        }
        else if (!Paused)
        {
            if (audio.time > 0)
            {
                audio.Play();
            }
            Time.timeScale = 1;
        }
    }
}
