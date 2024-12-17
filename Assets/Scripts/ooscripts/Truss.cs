using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truss : GeneralOverPlaceable
{
    public override void Move(Vector3 hova, Vector2 Axis2D)
    {
        Vector3? angle = SnapDirection(Axis2D);
        if (angle != null)
        {
            hova = (Vector3)angle * transform.localScale.z + transform.position;
            neighbours[1].GetComponent<GeneralPlaceable>().Move(hova, Vector2.zero);
        }
    }
    public override void Build(Vector3 hova, Vector3 honnan)
    {
        transform.position = honnan;
        
        transform.rotation = Quaternion.LookRotation(hova - honnan, new Vector3(1, 0, 0));
        transform.localScale = new Vector3(1, 1, Vector3.Distance(hova, honnan));
    }
    public override void Redraw()
    {
        if(neighbours.Count == 2) Build(neighbours[1].transform.position, neighbours[0].transform.position);
    }
    public override bool PlacingEnd()
    {
        placing = false;
        DeselectObject();
        foreach (var szomszed in neighbours)
        {
            szomszed.GetComponent<BaseCustomRayInteractable>().DeselectObject();
        }
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            GeneralPlaceable lehelyezheto = hit.transform.gameObject.GetComponent<GeneralPlaceable>();

            if(lehelyezheto.gameObject == neighbours[0])
            {
                DeleteObject();
                return false;
            }
            else if (lehelyezheto != null)
            {
                lehelyezheto.AddNeighbour(this.gameObject);
                Redraw();
                return true;
            }
            else
            {
                DeleteObject();
                return false;
            }
        }
        else
        {
            DeleteObject();
            return false;
        }
    }
}
