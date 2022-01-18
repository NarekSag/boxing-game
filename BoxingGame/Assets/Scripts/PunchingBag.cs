using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBag : MonoBehaviour, ITargetable, IHurtResponder
{
    [SerializeField]
    private bool targetable = true;
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Rigidbody rb;

    private List<HurtBox> hurtBoxes = new List<HurtBox>();

    public bool Targetable { get => targetable; }

    public Transform TargetTransform { get => targetTransform; }

    private void Start()
    {
        hurtBoxes = new List<HurtBox>(GetComponentsInChildren<HurtBox>());
        foreach(HurtBox hurtBox in hurtBoxes)
        {
            hurtBox.HurtResponder = this; 
        }
    }

    public bool CheckHit(HitData data)
    {
        return true;
    }

    public void Response(HitData data)
    {
        Debug.Log("HURT RESPONSE");
    }
}
