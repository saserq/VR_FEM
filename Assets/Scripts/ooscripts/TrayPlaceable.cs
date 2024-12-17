using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrayPlaceable : GeneralTrayElement
{
    TrayPlaceable[] ilyenek;
    public override void Start()
    {
        ilyenek = FindObjectsOfType<TrayPlaceable>();
        base.Start();
    }
    public override GameObject GetSelectedObject()
    {
        for (int i = 0; i < ilyenek.Length; i++)
        {
            ilyenek[i].DeselectObject();
        }
        SelectObject();
        return prefabObject;
    }
}
