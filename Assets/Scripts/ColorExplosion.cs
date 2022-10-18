using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorExplosion : MonoBehaviour
{
    SphereCollider col;
    public bool charged;
    [SerializeField] float speed;
    [SerializeField] new AudioSource audio;
    bool soundPlayed = false;

    // Start is called before the first frame update
    void Awake()
    {
        col = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charged)
        {
            if (!soundPlayed)
            {
                AudioHandler.instance.PlaySound("Explosion", audio);
                soundPlayed = true;
            }

            if (col.radius <= 5)
            {
                col.radius += Time.deltaTime * speed;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (!soundPlayed)
            {
                AudioHandler.instance.PlaySound("Wall_Hit", audio);
                soundPlayed = true;
            }

            if (col.radius <= 1.5)
            {
                col.radius += Time.deltaTime * speed;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Level"))
        {
            other.gameObject.GetComponent<GroundBehaviour>().FlashColor();
        }
    }
}
