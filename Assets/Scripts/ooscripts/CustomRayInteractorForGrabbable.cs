using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRBaseInteractable), typeof(LineRenderer))]
public abstract class CustomRayInteractorForGrabbable : MonoBehaviour,ISzerszam
{
    [SerializeField] protected Transform RaycastOrigin;
    [SerializeField] protected LocomotionSystem locomotionSystem;
    [SerializeField] protected LayerMask mask;
    [SerializeField] protected bool needAxisInput;

    protected XRIDefaultInputActions inputActions;
    protected InputAction kijeloles;
    protected InputAction trigger;
    protected InputAction torles;
    protected InputAction axisinput;
    protected XRBaseControllerInteractor hand;
    protected XRBaseInteractable interactable;
    protected LineRenderer lineRenderer;
    protected bool jobbkezben;
    public bool inhand { get; set; }

    //bool visszajelzo;
    protected bool primarybutton, triggerbutton;
    CustomRayInteractorForGrabbable[] tools;
    protected virtual void Awake()
    {
        inputActions = new XRIDefaultInputActions();
        interactable = GetComponent<XRBaseInteractable>();
        lineRenderer = GetComponent<LineRenderer>();
        tools = FindObjectsByType<CustomRayInteractorForGrabbable>(FindObjectsSortMode.None);
    }
    protected virtual void Start()
    {
        interactable.selectEntered.AddListener(Select);
        interactable.selectEntered.AddListener(Grabbed);
        interactable.selectExited.AddListener(Deselect);
        interactable.selectExited.AddListener(UnGrabbed);
        lineRenderer.enabled = false;

        primarybutton = false;
        triggerbutton = false;
        howeractive = false;
        mask = ~0 - mask;
        inhand = false;
    }
    protected virtual void Grabbed(SelectEnterEventArgs args)
    {
        inhand = true;
    }
    protected virtual void UnGrabbed(SelectExitEventArgs args)
    {
        inhand = false;
    }
    protected virtual void Select(SelectEnterEventArgs args)
    {
        foreach (var tool in tools)
        {
            if (/*tool != this && */tool.inhand)
            {
                tool.Deselect(default);
                tool.UnGrabbed(default);
            }
        }

        hand = (XRBaseControllerInteractor)GetComponent<XRBaseInteractable>().interactorsSelecting[0];
        if (args.interactorObject.transform.CompareTag("LeftHand"))
        {
            kijeloles = inputActions.CustomLeft.kijelol;
            trigger = inputActions.CustomLeft.trigger;
            torles = inputActions.CustomLeft.torol;
            if (needAxisInput)
            {
                axisinput = inputActions.CustomLeft.forgat;
                axisinput.Enable();
            }
            jobbkezben = false;
        }
        else if (args.interactorObject.transform.CompareTag("RightHand"))
        {
            kijeloles = inputActions.CustomRight.kijelol;
            trigger = inputActions.CustomRight.trigger;
            torles = inputActions.CustomRight.torol;
            if (needAxisInput)
            {
                axisinput = inputActions.CustomRight.forgat;
                axisinput.Enable();
            }
            jobbkezben = true;
        }
        //Mutato bekapcsolasa
        lineRenderer.enabled = true;

        kijeloles.Enable();
        kijeloles.started += PrimaryActionStarted;
        kijeloles.canceled += PrimaryActionEnded;

        trigger.Enable();
        trigger.started += TriggerActionStarted;
        trigger.canceled += TriggerActionEnded;

        torles.Enable();
        torles.performed += SecondaryActionStarted;

        SendHaptics();

        primarybutton = false;
        triggerbutton = false;
        lastHit = null;
        howeractive= true;
    }
    protected virtual void Deselect(SelectExitEventArgs args)
    {
        kijeloles.started -= PrimaryActionStarted;
        kijeloles.canceled -= PrimaryActionEnded;
        kijeloles.Disable();

        trigger.started -= TriggerActionStarted;
        trigger.canceled -= TriggerActionEnded;
        trigger.Disable();

        torles.performed -= SecondaryActionStarted;
        torles.Disable();

        if (needAxisInput)
            axisinput.Disable();

        SendHaptics();
        lineRenderer.enabled = false;
        howeractive = false;
        
        PrimaryActionEnded(default);
        TriggerActionEnded(default);
        if (lastHit != null) lastHit?.GetComponent<IHoverable>()?.HoverExit();
    }

    protected bool howeractive;
    protected RaycastHit hit;
    protected GameObject lastHit;
    protected GameObject manipulaltObject;
    protected virtual void Update()
    {
        if (howeractive)
        { 
            //highlighting howered object
            Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask);
            if (lastHit != hit.collider?.gameObject)
            {
                if (lastHit != null) lastHit?.GetComponent<IHoverable>()?.HoverExit();
                lastHit = hit.collider?.gameObject;
                lastHit?.GetComponent<IHoverable>()?.HoverEnter();
            }
            //setting pointer
            lineRenderer.SetPosition(0, RaycastOrigin.position);
            if (hit.collider?.gameObject != null)
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            else lineRenderer.SetPosition(1, RaycastOrigin.position + RaycastOrigin.forward * 10);
        }
    }
    protected abstract void SecondaryActionStarted(InputAction.CallbackContext context);
    protected abstract void TriggerActionStarted(InputAction.CallbackContext context);
    protected abstract void TriggerActionEnded(InputAction.CallbackContext context);
    protected abstract void PrimaryActionStarted(InputAction.CallbackContext context);
    protected abstract void PrimaryActionEnded(InputAction.CallbackContext context);
    public void SendHaptics()
    {
        hand.SendHapticImpulse(0.2f, 0.05f);
    }
}
