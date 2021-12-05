using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundBehaviour : MonoBehaviour
{
    Rigidbody rb;
    Renderer rend;
    [SerializeField] float intensity;
    [SerializeField] float amount;
    [SerializeField] float floor;

    MaterialPropertyBlock block;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }
    public void StartDestruction()
    {
        StartCoroutine(Destruction());
    }

    IEnumerator Destruction()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    public void FlashColor()
    {
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float change = amount;
        while (change >= amount - floor)
        {
            block.SetFloat("Emission_Strength", change * intensity);
            rend.SetPropertyBlock(block);
            change -= Time.deltaTime * 7 * floor;
            yield return new WaitForEndOfFrame();
        }

        block.SetFloat("Emission_Strength", amount - floor);
        rend.SetPropertyBlock(block);
    }
}
