using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UniRx;

public class GameHandler : MonoBehaviour
{
    [SerializeField] GameObject level;
    [SerializeField] GameObject wall;
    [SerializeField] LevelDestroyer transition;
    [SerializeField] Material screenGlitch;
    [SerializeField] SO_GameData playerData;
    public new AudioSource audio;
    [SerializeField] AudioSource ChargeBlip;
    [SerializeField] GameObject endScreen;
    [SerializeField] GameObject endScreenTip;

    [SerializeField] Volume blurVolume;

    [SerializeField] int width;
    [SerializeField] int height;

    [SerializeField] float offsetX;
    [SerializeField] float offsetY;

    [SerializeField] float RandomRate;

    float time = 0;

    PlayerInput input;

    bool gameEnded = false;

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


    float randomElapsed = 0;

    // Start is called before the first frame update
    void Start()
    {
        input = player.GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Confined;
        GenerateWalls();
        StartGame();
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

        Observable.FromMicroCoroutine(UpdateScreenRandom, false, FrameCountType.EndOfFrame).Subscribe().AddTo(this);
    }

    private IEnumerator UpdateScreenRandom()
    {
        while (true)
        {
            randomElapsed = 0;
            screenGlitch.SetVector("_RandomSeed", new Vector2(Random.value, Random.value));
            while (randomElapsed < 1 / RandomRate)
            {
                randomElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
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

    public void StartGame()
    {
        AudioHandler.instance.PlaySound("Start_Noise", audio);
        player.GetComponent<PlayerMovement>().HealToFull();
        SetGlitchAmount(0.0025f);
        levelCreated = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        gun.GetComponent<GunBehaviour>().canShoot = false;
        NewLevel();
    }

    public void LargeRumble()
    {
        if (input.currentControlScheme == "Gamepad")
            StartCoroutine(RumbleForSeconds(0.15f, true));
    }

    public void SmallRumble()
    {
        if (input.currentControlScheme == "Gamepad")
            StartCoroutine(RumbleForSeconds(0.15f, false));
    }

    IEnumerator RumbleForSeconds(float time, bool intense)
    {
        float elapsed = 0;

        if (intense)
        {
            Gamepad.current.SetMotorSpeeds(1f, 1f);
        }
        else
        {
            Gamepad.current.SetMotorSpeeds(0.25f, 0.25f);
        }

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Gamepad.current.SetMotorSpeeds(0,0);
    }

    IEnumerator FlashGlitch()
    {
        float original = screenGlitch.GetFloat("_Intensity");
        float peak = 0.1f;

        float delta = peak - original;

        float duration = 1f;

        while (duration > 0)
        {
            screenGlitch.SetFloat("_Intensity", original + delta * duration);
            duration -= Time.deltaTime * 10;
            yield return new WaitForEndOfFrame();
        }

        screenGlitch.SetFloat("_Intensity", original);
    }

    IEnumerator GameOver()
    {
        float time = 1f;

        while (time > 0)
        {
            Time.timeScale = time;
            audio.pitch = time;
            time -= Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 0;
        audio.Pause();
        endScreen.SetActive(true);
        endScreenTip.SetActive(true);
        input.SwitchCurrentActionMap("Game Over");
    }

    public void EndGame()
    {
        if (!gameEnded) {
            gameEnded = true;
            input.currentActionMap.Disable();
            StartCoroutine(GameOver());
        }
    }

    public void ResetScene()
    {
        input.SwitchCurrentActionMap("Player");
        uiManager.ResetEffects();

        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        GameObject[] enemyProjectiles = GameObject.FindGameObjectsWithTag("EnemyProj");
        GameObject[] playerProjectiles = GameObject.FindGameObjectsWithTag("PlayerProj");

        foreach (GameObject pickup in pickups) Destroy(pickup);
        foreach (GameObject enemyProjectile in enemyProjectiles) Destroy(enemyProjectile);
        foreach (GameObject playerProjectile in playerProjectiles) Destroy(playerProjectile);

        player.transform.position = new Vector3(0, 1.5f, 0);
        playerData.SetPosition(player.transform.position);
        if (currentLevel != null) Destroy(currentLevel);
        levelNumber = 0;
        themePlaying = false;
        gameEnded = false;
        endScreen.SetActive(false);
        endScreenTip.SetActive(false);
        Enemies.Clear();
        player.GetComponent<Renderer>().enabled = true;
        player.transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 1;
        StartGame();
    }

    public void SetGlitchAmount(float amount)
    {
        screenGlitch.SetFloat("_Intensity", amount);
    }

    public void FlashScreenGlitch()
    {
        StartCoroutine(FlashGlitch());
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
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            enemy.GetComponent<EnemyMovement>().enabled = true;
            enemy.GetComponent<EnemyMovement>().Awake();
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
            transition.gameObject.SetActive(true);
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
            blurVolume.weight = 1;
            input.SwitchCurrentActionMap("Paused");
            audio.Pause();
            Time.timeScale = 0;
        }
        else
        {
            Paused = false;
            blurVolume.weight = 0;
            input.SwitchCurrentActionMap("Player");
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
                blurVolume.weight = 1;
                input.SwitchCurrentActionMap("Paused");
                audio.Pause();
                Time.timeScale = 0;
            }
            else
            {
                Paused = false;
                blurVolume.weight = 0;
                input.SwitchCurrentActionMap("Player");
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
        if (!focus && !Paused && !gameEnded)
        {
            audio.Pause();
            Time.timeScale = 0;
        }
        else if (!Paused && !gameEnded)
        {
            if (audio.time > 0)
            {
                audio.Play();
            }
            Time.timeScale = 1;
        }
    }
}
