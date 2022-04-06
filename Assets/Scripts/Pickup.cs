using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { HP }

    public PickupType type;
    [SerializeField] SO_PlayerData playerData;

    Vector3 originalPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            switch (type)
            {
                case PickupType.HP:
                    other.gameObject.GetComponent<PlayerMovement>().OnHeal();
                    Destroy(gameObject);
                    break;
                default:
                    break;
            }
        }
    }

    private void Awake()
    {
        originalPos = transform.position;
    }

    private void Update()
    {
        transform.position = originalPos + Mathf.Sin(Time.time * 4) * Vector3.forward / 4;  
    }
}
