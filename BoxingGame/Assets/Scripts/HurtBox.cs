using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour, IHurtBox
{
    [SerializeField]
    private bool active = true;
    [SerializeField]
    private GameObject owner = null;
    [SerializeField]
    private HurtBoxType hurtBoxType = HurtBoxType.Enemy; 
    private IHurtResponder hurtResponder;

    public bool Active { get => active; }

    public GameObject Owner { get => owner; }

    public Transform Transform { get => transform; }

    public IHurtResponder HurtResponder { get => hurtResponder; set => hurtResponder = value; }

    public HurtBoxType Type { get => hurtBoxType; }

    public bool CheckHit(HitData data)
    {
        if(hurtResponder == null)
        {
            Debug.Log("No Responder");
        }

        return true;
    }
}
