using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateSize : MonoBehaviour
{
    private Vector3 m_Size;
    [SerializeField] private Vector3 m_Offset;
    [SerializeField] private float m_speed;
    private void Awake()
    {
        m_Size = transform.localScale;
    }
    void Update()
    {
        transform.localScale = Vector3.Lerp(m_Size, m_Offset, (Mathf.Sin(Time.time * m_speed) + 1) / 2);
    }
}
