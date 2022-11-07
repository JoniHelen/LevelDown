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

    List<Projectile> projectiles = new List<Projectile>();

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

        if ((player.powerupMask & PlayerMovement.PowerupMask.DamageBoost) != 0)
            BuffDamage();

        AudioHandler.instance.PlaySound("Player_Shoot", audio);
    }

    void BuffDamage() => projectiles.ForEach(p => p.Damage = 3);

    void InitProjectiles(bool multi)
    {
        projectiles.Clear();

        Projectile p1 = playerData.ProjectilePool.Get();
        p1.Initialize()

        if (multi)
        {
            p1.Initialize(dir);
            p1.Shake = true;

            Projectile p2 = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            p2.Direction = Quaternion.AngleAxis(17.5f, Vector3.up) * dir.normalized;
            p2.Rotation = Vector3.zero;
            p2.Shake = true;

            projectiles.Add(p2);

            Projectile p3 = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            p3.Direction = Quaternion.AngleAxis(-17.5f, Vector3.up) * dir.normalized;
            p3.Rotation = Vector3.zero;
            p3.Shake = true;

            projectiles.Add(p3);
        }
        else
        {

        }

        projectiles.Add(p1);
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
            dir = (rh.point - player.transform.position).normalized;
            dir.y = 0;

            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    public void StickAim(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<Vector2>() != Vector2.zero)
        {
            dir = new Vector3(ctx.ReadValue<Vector2>().x, 0, ctx.ReadValue<Vector2>().y).normalized;

            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void AttemptShoot()
    {
        if (charge >= chargeNeeded && playerData.charges > 0 && canShoot)
        {
            Projectile obj = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            AudioHandler.instance.PlaySound("Charged_Shot", audio);
            obj.Direction = dir;
            obj.transform.localScale = transform.GetChild(0).localScale;
            obj.Charged = true;
            playerData.RemoveCharges(1);
            GameHandler.instance.uiManager.UpdateCharges();
        }
        charge = 0;
    }
}
