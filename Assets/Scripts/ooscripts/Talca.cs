using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Talca : MonoBehaviour,ISzerszam
{
    public bool inhand { get; set; }
    protected XRBaseInteractable interactable;
    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }
    void Start()
    {
        interactable.selectEntered.AddListener(Select);
        interactable.selectExited.AddListener(Deselect);
        inhand = false;
    }
    void Select(SelectEnterEventArgs args)
    {
        inhand = true;
    }
    void Deselect(SelectExitEventArgs args)
    {
        inhand = false;
    }
}

