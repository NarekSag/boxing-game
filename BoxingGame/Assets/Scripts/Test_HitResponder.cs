using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_HitResponder : MonoBehaviour, IHitResponder
{
    [SerializeField] 
    private bool attack;
    [SerializeField]
    private int damage = 10;
    [SerializeField]
    private HitBox hitBox;

    public int Damage {get => damage; }

    private void Start()
    {
        hitBox.HitResponder = this;
    }

    private void Update()
    {
        if(attack)
        {
            hitBox.CheckHit();
        }
    }

    public bool CheckHit(HitData data)
    {
        return true;
    }

    public void Response(HitData data)
    {        
    }
}
