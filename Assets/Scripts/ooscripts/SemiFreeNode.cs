using System.Collections.Generic;
using UnityEngine;

public class SemiFreeNode : GeneralPlaceable
{
    public override bool[] Dof()
    {
        if ((gameObject.transform.rotation.eulerAngles.z / 90) % 2 == 0)
        {
            return new bool[] { true, false };
        }
        else
        {
            return new bool[] { false, true };
        }
    }
    protected override void GetOriginalMaterial()
    {
        materialInfoList.Add(new MaterialObjectInfo (GetComponent<MeshRenderer>().material, gameObject ));
        Transform kornyezet = transform.GetChild(0);
        materialInfoList.Add(new MaterialObjectInfo(kornyezet.gameObject.GetComponent<MeshRenderer>().material, kornyezet.gameObject));
        foreach (Transform t in kornyezet)
        {
            materialInfoList.Add(new MaterialObjectInfo(t.gameObject.GetComponent<MeshRenderer>().material, t.gameObject));
        }
    }
}