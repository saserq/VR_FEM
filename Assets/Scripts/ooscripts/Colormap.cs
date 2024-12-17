using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Colormap : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mintext;
    [SerializeField] TextMeshProUGUI maxtext;
    [SerializeField] TextMeshProUGUI zerotext;
    [SerializeField] TextMeshProUGUI fixzerotext;
    [SerializeField] Image fullcolormap;
    [SerializeField] Image halfcolormap;
    
    private void Start()
    {
        Erase();
    }
    public void SetValues(double min,double max)
    {
        mintext.enabled = true;
        maxtext.enabled = true;
        mintext.text = "Minimum stress:\n" + (min / 1000000).ToString("F2") + "[MPa]" + "\n|";
        maxtext.text = "Maximum stress:\n" + (max / 1000000).ToString("F2") + "[MPa]" + "\n|";
        fullcolormap.enabled = true;
        if(min<0&&max>0)
        {
            zerotext.enabled = true;
            Vector3 pos = zerotext.GetComponent<RectTransform>().localPosition;
            pos.x = Mathf.Abs((float)(min / (max - min)) * 1200 + 21);
            zerotext.GetComponent<RectTransform>().localPosition = pos;
        }
    }
    public void Setvalues(double max)
    {
        fixzerotext.enabled = true;
        maxtext.enabled = true;
        maxtext.text = "Maximum absolute stress:\n" + (max/1000000).ToString("F2") + "[MPa]" + "\n|";
        halfcolormap.enabled = true;
    }
    public void Erase()
    {
        mintext.enabled = false;
        maxtext.enabled = false;
        zerotext.enabled = false;
        fullcolormap.enabled = false;
        halfcolormap.enabled = false;
        fixzerotext.enabled = false;
    }
}
