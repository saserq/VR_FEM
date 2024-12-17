using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Threading;
using System.Net.Http.Headers;
using TMPro;
using System.ComponentModel;

public class ScrewdriverClass : CustomRayInteractorForGrabbable
{
    StructureSolverAndAnalyzer structureSolverAndAnylyzer;
    List<GeneralPlaceable> nodesVR;
    List<Force> forcesVR;
    List<Truss> trussesVR;
    [SerializeField] private GameObject freeNode;
    [SerializeField] private GameObject pinnedNode;
    [SerializeField] private GameObject rollerNode;
    [SerializeField] private GameObject truss;
    [SerializeField] private GameObject force;
    [SerializeField] private TextMeshProUGUI numericalIndicator;
    [SerializeField] private GameObject DeformedPlane;
    [SerializeField] private Material nodeDeselectMaterial;
    [SerializeField] private Material forceDeselectMaterial;
    [SerializeField] private Material trussDeselectMaterial;
    [SerializeField] private Material reakcioMaterial;
    [SerializeField] private GameObject Colormap;
    bool szalFut = false;
    bool siker = false;
    bool secondarybutton;

    protected override void Update()
    {
        if (howeractive)
        {
            //highlighting howered object
            Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask);
            //csak result taggel rendelkezõ objektumokra mûködjön a szerszám
            
            if (lastHit != hit.collider?.gameObject)
            {
                if (lastHit != null) lastHit?.GetComponent<IHoverable>()?.HoverExit();
                lastHit = hit.collider?.gameObject;
                if (hit.collider?.tag != "result") return;
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
    protected override void Select(SelectEnterEventArgs args)
    {
        base.Select(args);
        nodesVR = FindObjectsByType<GeneralPlaceable>(FindObjectsSortMode.None).ToList();
        forcesVR = FindObjectsByType<Force>(FindObjectsSortMode.None).ToList();
        trussesVR = FindObjectsByType<Truss>(FindObjectsSortMode.None).ToList();
        nodesVR.RemoveAll(obj => obj.CompareTag("result"));
        forcesVR.RemoveAll(obj => obj.CompareTag("result"));
        trussesVR.RemoveAll(obj => obj.CompareTag("result"));

        TurnGrey();
        structureSolverAndAnylyzer = new StructureSolverAndAnalyzer(ref trussesVR,ref forcesVR,ref nodesVR);

        
        torles.canceled += SecondatyActionEnded;
        secondarybutton = false;

        BackgroundWorker bw = new BackgroundWorker();

        // this allows our worker to report progress during work
        //bw.WorkerReportsProgress = true;

        // what to do in the background thread
        bw.DoWork += new DoWorkEventHandler(bgw_DoWork);
        //bw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);

        // what to do when worker completes its task (notify the user)
        bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
        szalFut = true;
        if (!bw.IsBusy)
            bw.RunWorkerAsync();
        numericalIndicator.text = "Calculating...";
    }
    private void bgw_DoWork(object sender, DoWorkEventArgs e) {
        //BackgroundWorker worker = sender as BackgroundWorker;
        // Do some work here
        // Report progress as a percentage of the total task
        //worker.ReportProgress(percentage);

        siker = structureSolverAndAnylyzer.Solve();
        if (!siker) return;
    }
    /*private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        // what to do when progress changed (update the progress bar for example)
        // Update the progress bar
        numericalIndicator.text = string.Format("{0}% Completed", e.ProgressPercentage);
    }*/
    private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        // what to do when worker completes its task (notify the user)
        if (siker)
        {
            BuildDeformedStructure(DeformedPlane.transform.position.z);
            numericalIndicator.text = "Ready";
        }
        else
        {
            numericalIndicator.text = "0 determináns";
        }
        szalFut = false;
    }
    private void TurnGrey()
    {
        foreach (var item in nodesVR)
        {
            item.SetMaterial(nodeDeselectMaterial);
        }
        foreach (var item in forcesVR)
        {
            item.SetMaterial(forceDeselectMaterial);
        }
        foreach (var item in trussesVR)
        {
            item.SetMaterial(trussDeselectMaterial);
        }
    }

    protected override void Deselect(SelectExitEventArgs args)
    {
        base.Deselect(args);
        //ki kell törölni az eredményeket
        GameObject[] results = GameObject.FindGameObjectsWithTag("result");
        for (int i = 0; i < results.Length; i++)
        {
            results[i].GetComponent<BuildingBlock>().DeleteObject();
        }
        //vissza kell színezni õket
        BuildingBlock[] epitoElemek = FindObjectsByType<BuildingBlock>(FindObjectsSortMode.None);
        foreach (var item in epitoElemek)
        {
            item.SetOriginalMaterial();
        }
        secondarybutton = false;
        numericalIndicator.text = "Output Display";
    }
    protected override void PrimaryActionStarted(InputAction.CallbackContext context)
    {
        //blocking multiple inputs at the same time
        if (/*triggerbutton*/ secondarybutton || szalFut) return;
        if(structureSolverAndAnylyzer == null) return;
        if (structureSolverAndAnylyzer?.structure.trusses.Count == 0) return;
        SendHaptics();
        primarybutton = true;
        double maxstress, minstress;
        maxstress = structureSolverAndAnylyzer.structure.trusses[0].Stress;
        minstress = maxstress;
        for (int i = 1; i < structureSolverAndAnylyzer.structure.trusses.Count; i++)
        {
            double aktstress = structureSolverAndAnylyzer.structure.trusses[i].Stress;
            if(aktstress > maxstress) maxstress = aktstress;
            if (aktstress < minstress) minstress = aktstress;
        }
        double stressvariance = (maxstress - minstress);
        foreach (TrussElement2D truss in structureSolverAndAnylyzer.structure.trusses)
        {
            truss.ShowStress(minstress, stressvariance);
        }
        numericalIndicator.text= "Max stress: " + (maxstress / 1000000).ToString("F2") + "[MPa]" + "\nMin stress: " + (minstress / 1000000).ToString("F2") + "[MPa]";
        Colormap.GetComponent<Colormap>().SetValues(minstress, maxstress);
    }
    protected override void PrimaryActionEnded(InputAction.CallbackContext context)
    {
        if (primarybutton)
        {
            primarybutton = false;
            foreach (TrussElement2D truss in structureSolverAndAnylyzer.structure.trusses)
            {
                truss.EndShowStress();
            }
            Colormap.GetComponent<Colormap>().Erase();
        }
    }
    protected override void SecondaryActionStarted(InputAction.CallbackContext context)
    {
        //csak egy bemenetet fogadjon el egyszerre
        if (primarybutton /*|| triggerbutton*/ || szalFut) return;
        if (structureSolverAndAnylyzer == null) return;
        if (structureSolverAndAnylyzer?.structure.trusses.Count == 0) return;
        SendHaptics();
        secondarybutton = true;
        double maxstress;
        maxstress = System.Math.Abs(structureSolverAndAnylyzer.structure.trusses[0].Stress);
        for (int i = 1; i < structureSolverAndAnylyzer.structure.trusses.Count; i++)
        {
            double aktstress = System.Math.Abs(structureSolverAndAnylyzer.structure.trusses[i].Stress);
            if (aktstress > maxstress) maxstress = aktstress;
        }
        foreach (TrussElement2D truss in structureSolverAndAnylyzer.structure.trusses)
        {
            truss.ShowAbsoluteStress(maxstress);
        }
        numericalIndicator.text = "Max absolute stress:\n" + (maxstress/1000000).ToString("F2")+"[MPa]";
        Colormap.GetComponent<Colormap>().Setvalues(maxstress);
    }
    private void SecondatyActionEnded(InputAction.CallbackContext context)
    {
        if (secondarybutton)
        {
            secondarybutton = false;
            foreach (TrussElement2D truss in structureSolverAndAnylyzer.structure.trusses)
            {
                truss.EndShowStress();
            }
            Colormap.GetComponent<Colormap>().Erase();
        }
    }
    protected override void TriggerActionStarted(InputAction.CallbackContext context)
    {
        //if (primarybutton || szalFut) return;
        triggerbutton = true;
        RaycastHit hit;
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, mask))
        {
            if (hit.transform.gameObject.tag == "result")
            {
                SendHaptics();
                numericalIndicator.text = hit.transform.gameObject.GetComponent<BuildingBlock>().information;
            }
        }
    }
    protected override void TriggerActionEnded(InputAction.CallbackContext context)
    {
        if (triggerbutton)
        {
            triggerbutton = false;
        }
    }
    public float magnification;
    public void BuildDeformedStructure(float z)
    {
        //GameObject tabla = GameObject.Find("Tabla");
        //Vector3 deformedPosition = tabla.transform.position + new Vector3(1f, 0f, 0f);
        foreach (Node2D node in structureSolverAndAnylyzer.structure.nodes)
        {
            switch(node.boundaryCondition)
            {
                case BoundaryCondition.Free:
                    node.CreateInstance(freeNode,magnification,z);
                    break;
                case BoundaryCondition.Pinned:
                    node.CreateInstance(pinnedNode, magnification,z);
                    break;
                case BoundaryCondition.xRoller:
                    node.CreateInstance(rollerNode, magnification,z);
                    break;
                case BoundaryCondition.yRoller:
                    node.CreateInstance(rollerNode, magnification,z);
                    break;
            }
        }
        foreach (TrussElement2D truss in structureSolverAndAnylyzer.structure.trusses)
        {
            truss.CreateInstance(this.truss, magnification,z);
        }
        foreach (Force2D force in structureSolverAndAnylyzer.structure.forces)
        {
            if(force.forceType==ForceType.Reaction)
            {
                force.CreateInstance(this.force, magnification, z).GetComponent<Force>().SetMaterial(reakcioMaterial);
            }
            else
            {
                force.CreateInstance(this.force, magnification, z);
            }
        }
    }
    public void SetMagnification(float value)
    {
        magnification = value*100;
    }
}
