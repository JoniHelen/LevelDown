using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{

    Vector3 originalPos;
    private void Awake()
    {
        originalPos = transform.position;
    }

    private void Update()
    {
        transform.position = originalPos + Mathf.Sin(Time.time * 4) * Vector3.forward / 4;
    }
}
