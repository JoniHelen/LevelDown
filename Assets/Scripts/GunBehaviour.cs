using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunBehaviour : MonoBehaviour
{
    [SerializeField] GameObject gun;
    [SerializeField] Projectile projectile;
    [SerializeField] PlayerMovement player;
    [SerializeField] SO_GameData playerData;
    [SerializeField] ParticleSystem ChargeEffect;
    [SerializeField] new AudioSource audio;
    [SerializeField] float turnSpeed;
    [SerializeField] LayerMask rayMask;

    public bool canShoot = true;
    bool charging = false;
    bool shooting = false;

    float charge = 0;
    const float chargeNeeded = 0.5f;
    [SerializeField] float rateOfFire = 0.2f;
    float fireTimer = 0;

    byte chargeState = 0;
    Vector3 dir = Vector3.forward;

    void Update()
    {
        transform.GetChild(0).localScale = Vector3.one * ((4 * charge + 3) / 10);

        if (charge < chargeNeeded && charging)
        {
            charge += Time.deltaTime;
            CheckChargeState();
        }

        if (shooting && canShoot)
        {
            fireTimer += Time.deltaTime;

            if(fireTimer >= rateOfFire)
            {
                Shoot();
                fireTimer = 0;
            }
        }
    }

    void CheckChargeState()
    {
        if (charge >= chargeNeeded * (1f / 3f) && chargeState == 0)
        {
            AudioHandler.instance.PlaySound("Charge_Shot", audio, 0.6f);
            chargeState++;
        }
        else if (charge >= chargeNeeded * (2f / 3f) && chargeState == 1)
        {
            AudioHandler.instance.PlaySound("Charge_Shot", audio, 0.8f);
            chargeState++;
        }
        else if (charge >= chargeNeeded && chargeState == 2)
        {
            AudioHandler.instance.PlaySound("Charge_Shot", audio);
            chargeState++;
            ChargeEffect.Stop();
        }
    }

    void Shoot()
    {
        InitProjectiles((player.powerupMask & PlayerMovement.PowerupMask.Multishot) != 0);
        AudioHandler.instance.PlaySound("Player_Shoot", audio);
    }

    void InitProjectiles(bool multi)
    {
        playerData.ProjectilePool.Get()
            .Initialize(Projectile.ProjectileType.player, gun.transform.position, dir, player.powerupMask);

        if (multi)
        {
            playerData.ProjectilePool.Get()
                .Initialize(Projectile.ProjectileType.player, gun.transform.position, Quaternion.AngleAxis(17.5f, Vector3.up) * dir, player.powerupMask);

            playerData.ProjectilePool.Get()
                .Initialize(Projectile.ProjectileType.player, gun.transform.position, Quaternion.AngleAxis(-17.5f, Vector3.up) * dir, player.powerupMask);
        }
    }

    public void PrimaryFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && canShoot && charge == 0)
        {
            shooting = true;
            //Shoot();
        }

        if (ctx.canceled)
        {
            shooting = false;
            //fireTimer = 0;
        }
    }

    public void AlternateFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && playerData.charges > 0 && canShoot && !shooting)
        {
            charging = true;
            if (chargeState != 3) ChargeEffect.Play();
        }
        
        if (ctx.canceled && playerData.charges > 0)
        {
            charging = false;
            chargeState = 0;
            ChargeEffect.Stop();
            AttemptShoot();
        }
    }

    public void MouseAim(InputAction.CallbackContext ctx)
    {
        Ray r = Camera.main.ScreenPointToRay(ctx.ReadValue<Vector2>());

        if (Physics.Raycast(r, out RaycastHit rh, Mathf.Infinity, rayMask, QueryTriggerInteraction.Collide))
        {
            // TODO: Make mouse position update insted of direction
            Vector3 d = rh.point - player.transform.position;
            dir = new Vector3(d.x, 0, d.z).normalized;

            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    public void StickAim(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            dir = new Vector3(input.x, 0, input.y).normalized;

            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void AttemptShoot()
    {
        if (charge >= chargeNeeded && playerData.charges > 0 && canShoot)
        {
            playerData.ProjectilePool.Get()
                .Initialize(Projectile.ProjectileType.player, gun.transform.position, dir, player.powerupMask, true)
                .transform.localScale = transform.GetChild(0).localScale;

            playerData.RemoveCharges(1);
            AudioHandler.instance.PlaySound("Charged_Shot", audio);
            GameHandler.instance.uiManager.UpdateCharges();
        }
        charge = 0;
    }
}
