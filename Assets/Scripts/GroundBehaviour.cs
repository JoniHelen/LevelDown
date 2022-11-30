using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

[RequireComponent(typeof(Rigidbody))]
public class GroundBehaviour : MonoBehaviour // POOLED
{
    public enum GroundType { Short, Tall }

    GroundType groundType = GroundType.Short;

    float Glow;
    bool Destroyed = false;

    Rigidbody rb;
    Renderer rend;
    MaterialPropertyBlock block;

    [SerializeField] SO_GroundSettings groundSettings;
    [SerializeField] SO_GameData gameData;

    Color currentColor;
    public Color GroundColor
    {
        get 
        {
            if (Type == GroundType.Short)
                return currentColor;

            if (Type == GroundType.Tall)
                return Color.black;

            return currentColor;
        }
        set 
        {
            currentColor = value;
            if (Type == GroundType.Short)
                SetColorAndStrength(value, Glow);

            if (Type == GroundType.Tall)
                SetColorAndStrength(Color.black, Glow);

            SetColorAndStrength(value, Glow);
        }
    }
    Color RandomColor { get {
            Vector3 v = new Vector3(Random.value, Random.value, Random.value).normalized;
            return new Color(v.x, v.y, v.z);
        }
    }
    public GroundType Type { 
        get { return groundType; } 
        set 
        {
            switch (value)
            {
                case GroundType.Short:
                    Glow = groundSettings.Glow;
                    if (groundType == GroundType.Tall)
                        transform.Translate(Vector3.down * 0.5f);
                    transform.localScale = Vector3.one;
                    break;

                case GroundType.Tall:
                    Glow = 0f;
                    if (groundType == GroundType.Short)
                        transform.Translate(Vector3.up * 0.5f);
                    transform.localScale = new Vector3(1, 2, 1);
                    break;

                default:
                    break;
            }
            groundType = value;
            SetColorAndStrength(GroundColor, Glow);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        Glow = groundSettings.Glow;
    }
    private void OnDisable()
    {
        transform.rotation = Quaternion.identity;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    private void OnEnable()
    {
        Type = GroundType.Short;
        Destroyed = false;
    }

    void SetColorAndStrength(Color color, float strength)
    {
        block.SetColor("_EmissionColor", color);
        block.SetFloat("_EmissionStrength", strength);
        rend.SetPropertyBlock(block);
    }

    public void StartDestruction()
    {
        if (!Destroyed) MainThreadDispatcher.StartEndOfFrameMicroCoroutine(Destruction(RandomColor));
    }
    IEnumerator Destruction(Color flashColor)
    {
        float t = 0;
        Destroyed = true;
        rb.useGravity = true;
        rb.isKinematic = false;

        while (t < 1)
        {
            SetColorAndStrength(Color.Lerp(flashColor, GroundColor, t), (1 - t) * groundSettings.FlashBrightness);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            t += Time.deltaTime;
            yield return null;
        }
        SetColorAndStrength(GroundColor, 0);
        gameData.GroundPool.Release(this);
    }
    public void FlashColor() => MainThreadDispatcher.StartEndOfFrameMicroCoroutine(Flash(RandomColor));
    public void FlashColor(Color col) => MainThreadDispatcher.StartEndOfFrameMicroCoroutine(Flash(col));
    IEnumerator Flash(Color flashColor)
    {
        float Change = groundSettings.FlashBrightness - Glow;
        float Duration = groundSettings.FlashDuration;
        float Elapsed = 0f;

        while (Elapsed < Duration)
        {
            SetColorAndStrength(Color.Lerp(flashColor, GroundColor, Elapsed / Duration), (1 - Elapsed / Duration) * Change);
            Elapsed += Time.deltaTime;
            yield return null;
        }
        SetColorAndStrength(GroundColor, Glow);
    }
}
