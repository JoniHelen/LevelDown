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
    [SerializeField] AudioSource audio;

    float charge = 0;
    bool charging = false;
    public bool canShoot = true;
    const float chargeNeeded = 0.5f;
    Vector3 dir;
    Vector3 newDir;

    byte chargeState = 0;
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

        if (charge == 0)
        {
            chargeState = 0;
        }

        if (charge >= chargeNeeded && ChargeEffect.isPlaying)
        {
            ChargeEffect.Stop();
        }
    }

    public void PrimaryFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canShoot && charge == 0)
        {
            Projectile obj = Instantiate(projectile, gun.transform.position, Quaternion.Euler(0, 0, 0));
            obj.direction = dir.normalized;
            obj.rotation = Vector3.zero;

            AudioHandler.instance.PlaySound("Player_Shoot", audio);
        }
    }

    public void AlternateFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && playerData.charges > 0 && canShoot)
        {
            charging = true;
            ChargeEffect.Play();
        }
        
        if (ctx.canceled && playerData.charges > 0 && canShoot)
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

            float singleStep = turnSpeed * Time.deltaTime;

            newDir = Vector3.RotateTowards(transform.forward, dir, singleStep, 0);

            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    public void StickAim(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<Vector2>() != Vector2.zero)
        {
            dir = new Vector3(ctx.ReadValue<Vector2>().x, 0, ctx.ReadValue<Vector2>().y).normalized;

            float singleStep = turnSpeed * Time.deltaTime;

            newDir = Vector3.RotateTowards(transform.forward, dir, singleStep, 0);

            transform.rotation = Quaternion.LookRotation(newDir);
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
