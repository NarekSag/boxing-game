using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Targetable : MonoBehaviour, ITargetable
{
    [Header("Targetable")]
    [SerializeField]
    private bool targetable;
    [SerializeField]
    private Transform targetTransform;

    bool ITargetable.Targetable { get => targetable; }
    Transform ITargetable.TargetTransform { get => targetTransform; }
}
