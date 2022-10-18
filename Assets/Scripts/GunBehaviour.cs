using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunBehaviour : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject player;
    [SerializeField] GameObject gun;
    [SerializeField] Projectile projectile;
    [SerializeField] ParticleSystem ChargeEffect;
    [SerializeField] SO_PlayerData playerData;
    [SerializeField] float turnSpeed;
    [SerializeField] LayerMask rayMask;
    [SerializeField] new AudioSource audio;

    float charge = 0;
    bool charging = false;
    public bool canShoot = true;
    const float chargeNeeded = 0.5f;
    Vector3 dir = Vector3.forward;
    Vector3 newDir;

    bool shooting = false;
    [SerializeField] float rateOfFire = 0.2f;
    float fireTimer = 0;

    byte chargeState = 0;

    List<Projectile> projectiles = new List<Projectile>();

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).localScale = Vector3.one * ((4 * charge + 3) / 10);

        if (charge < chargeNeeded && charging)
        {
            charge += Time.deltaTime;
        }

        if (charge >= chargeNeeded * (1f / 3f))
        {
            if (chargeState == 0)
            {
                AudioHandler.instance.PlaySound("Charge_Shot", audio, 0.6f);
                chargeState = 1;
            }
        }

        if (charge >= chargeNeeded * (2f / 3f))
        {
            if (chargeState == 1)
            {
                AudioHandler.instance.PlaySound("Charge_Shot", audio, 0.8f);
                chargeState = 2;
            }
        }

        if (charge >= chargeNeeded)
        {
            if (chargeState == 2)
            {
                AudioHandler.instance.PlaySound("Charge_Shot", audio);
                chargeState = 3;
            }
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

        if (charge == 0)
        {
            chargeState = 0;
        }

        if (charge >= chargeNeeded && ChargeEffect.isPlaying)
        {
            ChargeEffect.Stop();
        }
    }

    void Shoot()
    {
        switch (player.GetComponent<PlayerMovement>().powerupMask)
        {
            case (PlayerMovement.PowerupMask)1: // Multishot
                InitProjectiles(true);
                break;
            case (PlayerMovement.PowerupMask)2: // Damage boost
                InitProjectiles(false);
                BuffDamage();
                break;
            case (PlayerMovement.PowerupMask)3: // Multishot & Damage boost
                InitProjectiles(true);
                BuffDamage();
                break;
            default:
                InitProjectiles(false); // No powerup
                break;
        }

        AudioHandler.instance.PlaySound("Player_Shoot", audio);
    }

    void BuffDamage()
    {
        foreach(Projectile p in projectiles)
        {
            p.damage = 3;
        }
    }

    void InitProjectiles(bool multi)
    {
        projectiles.Clear();

        Projectile p1 = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
        p1.direction = dir.normalized;
        p1.rotation = Vector3.zero;

        if (multi)
        {
            p1.shake = true;

            Projectile p2 = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            p2.direction = Quaternion.AngleAxis(17.5f, Vector3.up) * dir.normalized;
            p2.rotation = Vector3.zero;
            p2.shake = true;

            projectiles.Add(p2);

            Projectile p3 = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            p3.direction = Quaternion.AngleAxis(-17.5f, Vector3.up) * dir.normalized;
            p3.rotation = Vector3.zero;
            p3.shake = true;

            projectiles.Add(p3);
        }

        projectiles.Add(p1);
    }

    public void PrimaryFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && canShoot && charge == 0)
        {
            shooting = true;
            Shoot();
        }

        if (ctx.canceled)
        {
            shooting = false;
            fireTimer = 0;
        }
    }

    public void AlternateFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && playerData.charges > 0 && canShoot && !shooting)
        {
            charging = true;
            ChargeEffect.Play();
        }
        
        if (ctx.canceled && playerData.charges > 0)
        {
            charging = false;
            ChargeEffect.Stop();
            AttemptShoot();
        }
    }

    public void MouseAim(InputAction.CallbackContext ctx)
    {
        Ray r = cam.ScreenPointToRay(ctx.ReadValue<Vector2>());

        if (Physics.Raycast(r, out RaycastHit rh, Mathf.Infinity, rayMask, QueryTriggerInteraction.Collide))
        {
            dir = rh.point - player.transform.position;
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
            obj.direction = dir.normalized;
            obj.rotation = Vector3.zero;
            obj.transform.localScale = transform.GetChild(0).localScale;
            obj.charged = true;
            playerData.RemoveCharges(1);
            GameHandler.instance.uiManager.UpdateCharges();
        }
        charge = 0;
    }
}
