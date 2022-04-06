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

        Color original = rend.material.GetColor("Emission_Color");

        Vector3 flash = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)).normalized;

        Color flashColor = new Color(flash.x, flash.y, flash.z);

        Color Current;

        float t = 0;
        while (t < 1)
        {
            Current = Color.Lerp(flashColor, original, t);

            block.SetFloat("Emission_Strength", (1 - t) * intensity);
            block.SetColor("Emission_Color", Current);
            rend.SetPropertyBlock(block);

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

        Color original = rend.material.GetColor("Emission_Color");

        Vector3 flash = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)).normalized;

        Color flashColor = new Color(flash.x, flash.y, flash.z);

        Color Current;

        while (change >= amount - floor)
        {
            Current = Color.Lerp(flashColor, original, change - floor);

            block.SetFloat("Emission_Strength", change * intensity);
            block.SetColor("Emission_Color", Current);
            rend.SetPropertyBlock(block);
            change -= Time.deltaTime * 7 * floor;
            yield return new WaitForEndOfFrame();
        }

        block.SetFloat("Emission_Strength", amount - floor);
        block.SetColor("Emission_Color", original);
        rend.SetPropertyBlock(block);
    }
}
