using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Force : GeneralOverPlaceable
{
    bool snap;
    public float size
    {
        get
        {
            return transform.GetChild(1).localScale.z;
        }
    }
    public override void Start()
    {
        base.Start();
        transform.position = OffsetPosition(transform.position);
        snap = false;
    }
    public override void MovingStart(InputAction axisinput, Transform RaycastOrigin)
    {
        base.MovingStart(axisinput, RaycastOrigin);
        snap = false;
    }
    public override void MovingEnd()
    {
        base.MovingEnd();
        snap = false;
    }
    public override void Move(Vector3 hova, Vector2 Axis2D)
    {
        Vector3? angle = SnapDirection(Axis2D);
        if(!snap)
        {
            if (angle != null)
            {
                snap = true;
                Build((Vector3)angle * Vector3.Distance(hova, neighbours[0].transform.position) + neighbours[0].transform.position, neighbours[0].transform.position);
            }
            else
            {
                Build(hova, neighbours[0].transform.position);
            }
        }
        else
        {
            if (angle != null)
            {
                Build((Vector3)angle * Vector3.Distance(hova, neighbours[0].transform.position) + neighbours[0].transform.position, neighbours[0].transform.position);
            }
            else
            {
                SetMagnitude(Vector3.Distance(hova, transform.position));
            }
        }
    }
    
    public override void Build(Vector3 hova, Vector3 honnan)
    {
        transform.rotation = Quaternion.LookRotation(hova-honnan, new Vector3(1, 0, 0));

        transform.GetChild(1).localScale = new Vector3(1, 1, Vector3.Distance(hova, honnan));
        transform.GetComponent<BoxCollider>().center = new Vector3(0, 0, Vector3.Distance(hova, neighbours[0].transform.position) + 0.02f);
        transform.GetChild(0).transform.position = OffsetPosition(hova);
    }
    public void SetMagnitude(float strength)
    {
        transform.GetChild(1).localScale = new Vector3(1, 1, strength);
        transform.GetComponent<BoxCollider>().center = new Vector3(0, 0, strength + 0.02f);
        transform.GetChild(0).transform.position = transform.rotation * Vector3.forward * strength + transform.position;
    }
    private Vector3 OffsetPosition(Vector3 hova)
    {
        return new Vector3(hova.x, hova.y, hova.z - 0.024f);
    }
    public override void Redraw()
    {
        transform.position = OffsetPosition(neighbours[0].transform.position);
    }
    public override bool PlacingEnd()
    {
        placing = false;
        DeselectObject();
        foreach (var szomszed in neighbours)
        {
            szomszed.GetComponent<BaseCustomRayInteractable>().DeselectObject();
        }
        return true;
    }
    protected override void GetOriginalMaterial()
    {
        Transform kornyezet = transform.GetChild(0);
        materialInfoList.Add(new MaterialObjectInfo(kornyezet.gameObject.GetComponent<MeshRenderer>().material, kornyezet.gameObject));
        kornyezet = transform.GetChild(1);
        materialInfoList.Add(new MaterialObjectInfo(kornyezet.gameObject.GetComponent<MeshRenderer>().material, kornyezet.gameObject));
    }

}
