using UnityEngine;

public class FixedNode : GeneralPlaceable
{
    public override bool[] Dof()
    {
        return new bool[] { false, false };
    }
    protected override void GetOriginalMaterial()
    {
        materialInfoList.Add(new MaterialObjectInfo(GetComponent<MeshRenderer>().material, gameObject));
        Transform kornyezet = transform.GetChild(0);
        materialInfoList.Add(new MaterialObjectInfo(kornyezet.gameObject.GetComponent<MeshRenderer>().material, kornyezet.gameObject));
    }
}