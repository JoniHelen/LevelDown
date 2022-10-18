using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    [SerializeField] SO_PlayerData playerPosition;
    [SerializeField] GameObject death;
    [SerializeField] float speed;
    [SerializeField] new AudioSource audio;
    Vector3 dir = Vector3.zero;

    MaterialPropertyBlock block;
    Renderer rend;
    public bool canTakeDamage = true;

    public List<PickupData> pickups = new List<PickupData>();

    [Flags]
    public enum PowerupMask
    {
        None,
        Multishot,
        DamageBoost
    }

    public PowerupMask powerupMask = PowerupMask.None;

    private void Awake()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        playerPosition.SetPosition(transform.position);
        block = new MaterialPropertyBlock();
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        characterController.Move(speed * Time.deltaTime * (dir + Physics.gravity));
        playerPosition.SetPosition(transform.position);


        foreach(PickupData p in pickups)
        {
            p.activeTime += Time.deltaTime;

            if (p.activeTime >= p.Duration)
            {
                TogglePickupOff(p);
                pickups.Remove(p);
                break;
            }
        }
    }

    public void SetPosition()
    {
        playerPosition.SetPosition(transform.position);
    }

    public void OnDamaged()
    {
        if (canTakeDamage)
        {
            playerPosition.TakeDamage(1);
            GameHandler.instance.SetGlitchAmount((-1f / 72f) * (playerPosition.hitPoints / 10f) + (59f / 3600f));
            GameHandler.instance.FlashScreenGlitch();  
            AudioHandler.instance.PlaySound("Player_Hurt", audio);
            // Update UI
            GameHandler.instance.uiManager.UpdateHP();
            StartCoroutine(Flash(Color.red));

            if (playerPosition.hitPoints == 0)
            {
                Instantiate(death, gameObject.transform.position, Quaternion.identity);
                GetComponent<Renderer>().enabled = false;
                transform.GetChild(0).gameObject.SetActive(false);
                GameHandler.instance.EndGame();
            }
        }
    }

    public void HealToFull()
    {
        playerPosition.Heal(10);
        GameHandler.instance.SetGlitchAmount((-1f / 72f) * (playerPosition.hitPoints / 10f) + (59f / 3600f));
        GameHandler.instance.uiManager.UpdateHP();
    }

    public void OnHeal()
    {
        playerPosition.Heal(1);
        GameHandler.instance.SetGlitchAmount((-1f / 72f) * (playerPosition.hitPoints / 10f) + (59f / 3600f));
        // PLAY SOUND

        GameHandler.instance.uiManager.UpdateHP();
        StartCoroutine(Flash(new Color(0, 1, 0.03644657f)));
    }

    public void OnPickupPowerup(PickupData pickup)
    {
        if (pickups.Exists(x => x.Type == pickup.Type))
            pickups.Find(x => x.Type == pickup.Type).activeTime = 0;
        else
        {
            pickups.Add(pickup);
            TogglePickupOn(pickup);
        }
    }

    void TogglePickupOn(PickupData pickup)
    {
        switch (pickup.Type)
        {
            case Pickup.PickupType.Multishot:
                powerupMask |= PowerupMask.Multishot;
                break;
            case Pickup.PickupType.DamageBoost:
                powerupMask |= PowerupMask.DamageBoost;
                break;
        }
    }

    void TogglePickupOff(PickupData pickup)
    {
        switch (pickup.Type)
        {
            case Pickup.PickupType.Multishot:
                powerupMask &= ~PowerupMask.Multishot;
                break;
            case Pickup.PickupType.DamageBoost:
                powerupMask &= ~PowerupMask.DamageBoost;
                break;
        }
    }

    IEnumerator Flash(Color col)
    {
        float amount = 0.75f;

        while (amount > 0)
        {
            Color current = Color.Lerp(col, Color.white, 1 - amount);

            block.SetColor("_BaseColor", current);
            rend.SetPropertyBlock(block);
            amount -= Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        block.SetColor("_BaseColor", Color.white);
        rend.SetPropertyBlock(block);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        dir = new Vector3(ctx.ReadValue<Vector2>().x, 0, ctx.ReadValue<Vector2>().y).normalized;
    }
}
