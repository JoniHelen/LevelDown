using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Custom/Level Data")]
public class SO_LevelData : ScriptableObject
{
    [SerializeField] public EnemyBehaviour[] enemyBehaviours;
}
