using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDestroyer : MonoBehaviour
{
    SphereCollider col;
    [SerializeField] float speed;
    [SerializeField] new AudioSource audio;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        AudioHandler.instance.PlaySound("Level_Transition", audio);
    }

    // Update is called once per frame
    void Update()
    {
        col.radius += Time.deltaTime * speed;
        if (col.radius > 19)
        {
            Destroy(transform.parent.gameObject, 2f);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Level"))
        {
            other.gameObject.GetComponent<GroundBehaviour>().StartDestruction();
        }
        else if (other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.GetComponent<Pickup>().OnLevelDown();
        }
        else if (other.gameObject.CompareTag("EnemyProj"))
        {
            Destroy(other.gameObject);
        }
    }
}
