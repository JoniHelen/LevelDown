using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Pool", menuName = "Custom/Level Pool")]
public class SO_LevelPool : ScriptableObject
{
    [SerializeField] public SO_LevelData[] LevelDataPool;
}
