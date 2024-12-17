using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class WrechClass : CustomRayInteractorForGrabbable
{
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LayerMask rajzolhatoLayer;
    [SerializeField] protected LayerMask talcaLayer;
    protected int targetLayerint;
    protected int rajzolhatoLayerint;
    protected int talcaLayerint;
    [SerializeField] protected GameObject aktelem, aktelem2;
    protected override void Awake()
    {
        base.Awake();
        targetLayerint = (int)Mathf.Log(targetLayer.value, 2);
        rajzolhatoLayerint = (int)Mathf.Log(rajzolhatoLayer.value, 2);
        talcaLayerint = (int)Mathf.Log(talcaLayer.value, 2);
    }
    protected override void PrimaryActionStarted(InputAction.CallbackContext context)
    {
        //csak egy bemenetet fogadjon el egyszerre
        if (triggerbutton) return;

        RaycastHit hit;
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask))
        {
            if (hit.transform.gameObject.GetComponent<IHoverable>() is GeneralPlaceable)
            {
                SendHaptics();
                manipulaltObject = Instantiate(aktelem2, hit.transform.position, Quaternion.identity);

                hit.transform.gameObject.GetComponent<GeneralPlaceable>().AddNeighbour(manipulaltObject);
                primarybutton = true;

                manipulaltObject.GetComponent<GeneralOverPlaceable>().PlacingStart(axisinput, RaycastOrigin);
            }
        }
    }

    protected override void PrimaryActionEnded(InputAction.CallbackContext context)
    {
        if (primarybutton)
        {
            //visszajelzo = true;
            if(manipulaltObject != null)
            {
                if (manipulaltObject.GetComponent<GeneralOverPlaceable>().PlacingEnd()) SendHaptics();
                manipulaltObject = null;
            }

            primarybutton = false;
        }
    }

    protected override void SecondaryActionStarted(InputAction.CallbackContext context)
    {
        //csak egy bemenetet fogadjon el egyszerre
        if (primarybutton || triggerbutton) return;

        RaycastHit hit;
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask))
        {
            hit.transform.GetComponent<IDeleteable>()?.DeleteObject();
            SendHaptics();
        }
    }

    protected override void TriggerActionStarted(InputAction.CallbackContext context)
    {
        //csak egy bemenetet fogadjon el egyszerre
        if (primarybutton) return;

        RaycastHit hit;
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask))
        {
            if (hit.transform.gameObject.layer == targetLayerint)
            {
                //mozgatas, amig fel nem engedik a trigger gombot
                if (hit.transform.gameObject.GetComponent<IMoveable>() != null)
                {
                    SendHaptics();
                    triggerbutton = true;
                    if (jobbkezben)
                    {
                        locomotionSystem.gameObject.GetComponent<ActionBasedSnapTurnProvider>().enabled = false;
                    }
                    else
                    {
                        locomotionSystem.gameObject.GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
                    }
                    manipulaltObject = hit.transform.gameObject;
                    manipulaltObject.GetComponent<IMoveable>().MovingStart(axisinput, RaycastOrigin);
                }
            }
            else if (hit.transform.gameObject.layer == rajzolhatoLayerint)
            {
                SendHaptics();
                //uj lehelyezheto elem letrehozasa
                Instantiate(aktelem, hit.point, Quaternion.identity);
            }
            else if (hit.transform.gameObject.layer == talcaLayerint)
            {
                SendHaptics();
                //ket csoport van a talcan, az egyik a lehelyezheto, a masik a rahelyezheto
                ITrayInteractable talcaInteractable = hit.collider.gameObject.GetComponent<ITrayInteractable>();
                if (talcaInteractable is TrayPlaceable)
                {
                    aktelem = talcaInteractable.GetSelectedObject();
                }
                else if (talcaInteractable is TrayOverPlaceable)
                {
                    aktelem2 = talcaInteractable.GetSelectedObject();
                }
            }
        }
    }

    protected override void TriggerActionEnded(InputAction.CallbackContext context)
    {
        if (triggerbutton)
        {
            if(manipulaltObject!=null)
            {
                manipulaltObject.GetComponent<IMoveable>().MovingEnd();
                manipulaltObject = null;
            }

            triggerbutton = false;
            if (jobbkezben)
            {
                locomotionSystem.gameObject.GetComponent<ActionBasedSnapTurnProvider>().enabled = true;
            }
            else
            {
                locomotionSystem.gameObject.GetComponent<ActionBasedContinuousMoveProvider>().enabled = true;
            }
        }
    }
}
