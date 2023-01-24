using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Pickup : MonoBehaviour
{
    public enum PickupType { HP, Multishot, DamageBoost }

    public PickupType type;
    public float Duration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement pm))
        {
            if (type == PickupType.HP)
            {
                pm.OnHeal();
                Destroy(gameObject);
            }
            else
            {
                pm.OnPickupPowerup(this);
                Destroy(gameObject);
            }
        }
    }

    public static implicit operator PickupData(Pickup p) => new PickupData(p.Duration, p.type);

    public void OnLevelDown()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.down * 2, GetComponent<SphereCollider>().radius * transform.localScale.z, Vector3.down, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.TryGetComponent(out GroundBehaviour gb))
            {
                if (gb.Type == GroundBehaviour.GroundType.Tall)
                {
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}
