using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrayOverPlaceable : GeneralTrayElement
{
    TrayOverPlaceable[] ilyenek;
    public override void Start()
    {
        ilyenek = FindObjectsOfType<TrayOverPlaceable>();
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
