using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { HP, Multishot, DamageBoost }

    public PickupType type;

    public float Duration;
    [HideInInspector] public float activeTime = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (type == PickupType.HP)
            {
                other.gameObject.GetComponent<PlayerMovement>().OnHeal();
                Destroy(gameObject);
            }
            else
            {
                other.gameObject.GetComponent<PlayerMovement>().OnPickupPowerup(new PickupData(Duration, type));
                Destroy(gameObject);
            }
        }
    }

    public void OnLevelDown()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.down * 2, GetComponent<SphereCollider>().radius * transform.localScale.z, Camera.main.transform.forward, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            if(hit.transform.localScale.y == 2)
            {
                Destroy(gameObject);
                break;
            }
        }
    }
}
