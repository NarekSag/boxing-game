using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour, IHitDetector
{
    [SerializeField]
    private BoxCollider boxCollider;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private HurtBoxMask hurtBoxMask = HurtBoxMask.Enemy;

    private float thickness = 0.025f;
    private IHitResponder hitResponder;

    public IHitResponder HitResponder { get => hitResponder; set => hitResponder = value; }

    public void CheckHit()
    {
        Vector3 scaledSize = new Vector3(
            boxCollider.size.x * transform.lossyScale.x,
            boxCollider.size.y * transform.lossyScale.y,
            boxCollider.size.z * transform.lossyScale.z);

        float distance = scaledSize.y - thickness;
        Vector3 direction = transform.up;
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 start = center - direction * (distance / 2);
        Vector3 halfExtents = new Vector3(scaledSize.x, thickness, scaledSize.z) / 2;
        Quaternion orientation = transform.rotation;

        HitData hitData = null;
        IHurtBox hurtBox = null;
        RaycastHit[] hits = Physics.BoxCastAll(start, halfExtents, direction, orientation, distance, layerMask);
        foreach(RaycastHit hit in hits)
        {
            hurtBox = hit.collider.GetComponent<IHurtBox>();
            if(hurtBox != null)
            {
                if(hurtBox.Active && hurtBoxMask.HasFlag((HurtBoxMask)hurtBox.Type))
                {
                    //Generate data
                    hitData = new HitData
                    {
                        damage = hitResponder == null ? 0 : hitResponder.Damage,
                        hitPoint = hit.point == Vector3.zero ? center : hit.point,
                        hitNormal = hit.normal,
                        hurtbox = hurtBox,
                        hitDetector = this
                    };

                    //Validate
                    if(hitData.Validate())
                    {
                        hitData.hitDetector.HitResponder?.Response(hitData);
                        hitData.hurtbox.HurtResponder?.Response(hitData);
                    }
                }
            }
        }
    }
}
