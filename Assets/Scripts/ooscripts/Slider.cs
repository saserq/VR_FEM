using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using TMPro;

[RequireComponent(typeof(Outline))]
public class Slider : MonoBehaviour
{
    protected XRIDefaultInputActions inputActions;
    protected XRBaseInteractable interactable;
    private bool grabbed;

    [SerializeField] Transform rail;
    [SerializeField] private TextMeshProUGUI numericalIndicator;

    private Outline outline;
    private float value;
    
    [SerializeField] GameObject Cshuzo;
    protected virtual void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        outline = GetComponent<Outline>();
    }
    protected virtual void Start()
    {
        rail.gameObject.GetComponent<MeshRenderer>().material.color = Color.gray;
        interactable.selectEntered.AddListener(OnGrab);
        interactable.firstHoverEntered.AddListener(OnHoverEnter);
        interactable.selectExited.AddListener(OnRelease);
        interactable.lastHoverExited.AddListener(OnHoverExit);
        GetComponent<MeshRenderer>().material.color= Color.white;
        value = 0;
        numericalIndicator.text = "Deformation\nMagnification:\n" + ((value + 0.5f) * 100).ToString("F0");
        outline.enabled = false;
    }
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        outline.enabled = true;
    }
    public void OnHoverExit(HoverExitEventArgs args)
    {
        if(!grabbed)
            outline.enabled = false;
    }
    protected virtual void OnGrab(SelectEnterEventArgs args)
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
        grabbed = true;
    }
    protected virtual void OnRelease(SelectExitEventArgs args)
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
        grabbed = false;
        //set the magnification value at the screwdriver script
        Cshuzo.GetComponent<ScrewdriverClass>().magnification = ((value + 0.5f) * 100);
        //start the animation of the slider
        StartCoroutine(WhenDisable(new Vector3(rail.transform.position.x, rail.transform.position.y, rail.position.z-value)));
    }
    private void Update()
    {
        if (grabbed)
        {
            value = (rail.position.z-transform.position.z);
            //if the slider is moved too far, it will be limited to a certain value
            if (value > 0.5f)
            {
                value = 0.5f;
            }
            else if (value < -0.5f)
            {
                value = -0.5f;
            }
            numericalIndicator.text = "Deformation\nMagnification:\n" + ((value + 0.5f) * 100).ToString("F0");
        }
    }
    //this coroutine is used to animate the slider when it is released
    private IEnumerator WhenDisable(Vector3 finish)
    {
        bool animation = true;
        Vector3 start = transform.position;
        float time = 0.5f;
        while (animation)
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                animation = false;
                yield return null;
            }
            transform.position = Vector3.Lerp(finish, start, time*2);
            yield return null;
        }
        transform.position = finish;
        transform.rotation = Quaternion.identity;
    }
}
