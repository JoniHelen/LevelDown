using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDeathSound : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    private void Awake()
    {
        AudioHandler.instance.PlaySound("Enemy_Death", audio);
    }
}
