using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public interface IHoverable
{
    bool IsHighlighted { get; set; }
    bool IsHovered { get; set; }
    void HoverEnter();
    void HoverExit();
    void SelectObject();
    void DeselectObject();
}
public interface ITrayInteractable
{
    GameObject GetSelectedObject();
}
public interface IMoveable
{
    public bool moving {get; }
    void Move(Vector3 hova, Vector2 szog);
    public void MovingStart(InputAction axisinput, Transform RaycastOrigin);
    public void MovingEnd();
}
public interface IDeleteable
{
    void DeleteObject();
}
public interface IOverpaceable
{
    void Build(Vector3 hova, Vector3 kezdopont);
    void Redraw();
    public void PlacingStart(InputAction axisinput, Transform RaycastOrigin);
    public bool PlacingEnd();
}



[RequireComponent(typeof(Outline))]
public abstract class BaseCustomRayInteractable : MonoBehaviour, IHoverable
{
    protected Outline outline;
    public bool IsHighlighted { get; set; }

    public bool IsHovered { get; set; }

    public virtual void Awake()
    {
        outline = GetComponent<Outline>();
    }
    public virtual void Start()
    {
        IsHovered = false;
        IsHighlighted = false;
        outline.enabled = false;
    }
    public virtual void HoverEnter()
    {
        if (!IsHighlighted)
        {
            outline.enabled = true;
        }
        IsHovered = true;
    }
    public virtual void HoverExit()
    {
        if (!IsHighlighted)
        {
            outline.enabled = false;
        }
        IsHovered = false;
    }
    public virtual void SelectObject()
    {
        IsHighlighted = true;
        outline.enabled = true;
    }
    public virtual void DeselectObject()
    {
        IsHighlighted = false;
        if (!IsHovered)
        {
            outline.enabled = false;
        }
    }
}

public abstract class BuildingBlock : BaseCustomRayInteractable, IDeleteable, IMoveable
{
    public bool moving { get; protected set; }
    public string information { get; set; }
    //protected Material[] originalMaterials;
    protected List<MaterialObjectInfo> materialInfoList;
    public List<GameObject> neighbours { get; set; }

    public override void Awake()
    {
        base.Awake();
        materialInfoList = new List<MaterialObjectInfo>();
        neighbours = new List<GameObject>();
        GetOriginalMaterial();
    }
    public override void Start()
    {
        base.Start();
        moving = false;
        //mask = ~0 - mask;
    }
    protected InputAction axisinput;
    protected Transform RaycastOrigin;
    [SerializeField] protected LayerMask rajzolhatoLayer;
    [SerializeField] protected LayerMask targetLayer;
    //[SerializeField] LayerMask mask;
    public virtual void MovingStart(InputAction axisinput, Transform RaycastOrigin)
    {
        this.axisinput = axisinput;
        this.RaycastOrigin = RaycastOrigin;
        moving = true;
        SelectObject();
    }
    public virtual void MovingEnd()
    {
        moving = false;
        DeselectObject();
    }
    protected RaycastHit hit;
    public virtual void Update()
    {
        if(moving)
        {
            UpdateMovement();
        }
    }
    public virtual void UpdateMovement()
    {
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, rajzolhatoLayer))
        {
            Move(hit.point, axisinput.ReadValue<Vector2>());
        }
    }
    public abstract void AddNeighbour(GameObject szomszed);
    public virtual void RemoveNeighbour(GameObject szomszed)
    {
        if (neighbours.Contains(szomszed))
        {
            neighbours.Remove(szomszed);
        }
    }
    public abstract void DeleteObject();
    public abstract void Move(Vector3 hova, Vector2 szog);
    public virtual float? SnapAngle(Vector2 v)
    {
        if (v == null) { return null; }
        //  Get the angle of the given vector
        float angle = (float)Math.Atan2(v.y, v.x);

        //  Get the hypotenuse (length) of the given vector
        float hypotenuse = (float)Math.Sqrt((v.x * v.x) + (v.y * v.y));
        if (hypotenuse < (float)0.5)
        {
            return null;
        }

        // Convert 90degress to radians
        float rad90 = (float)(90 * (Math.PI / 180.0));

        //  Determine the nearest cardinal/intercardinal angle to snap to
        float snapTo = ((float)Math.Round(angle / rad90) + (float)1) * 90;

        return snapTo;
    }
    public virtual Vector3? SnapDirection(Vector2 v)
    {
        if (v == null) { return null; }
        //  Get the angle of the given vector
        float angle = (float)Math.Atan2(v.y, v.x);

        //  Get the hypotenuse (length) of the given vector
        float hypotenuse = (float)Math.Sqrt((v.x * v.x) + (v.y * v.y));
        if (hypotenuse < (float)0.5)
        {
            return null;
        }

        // Convert 45degress to radians
        float rad45 = (float)(45 * (Math.PI / 180.0));

        //  Determine the nearest cardinal/intercardinal angle to snap to
        float snapTo = (float)Math.Round(angle / rad45) * rad45;

        Vector3 snappedVector = new();

        // Calculate the x of the vector from the snapped angle
        snappedVector.x = (float)(Math.Cos(snapTo)/* * hypotenuse*/);

        //  Calculate the y of the vector from the snapped angle
        snappedVector.y = (float)(Math.Sin(snapTo)/* * hypotenuse*/);

        //return snappedVector;
        snappedVector.z = 0;
        return snappedVector;
    }
    public virtual void SetTag(string tag)
    {
        gameObject.tag = tag;
    }
    protected virtual void GetOriginalMaterial()
    {
        materialInfoList.Add(new MaterialObjectInfo(GetComponent<MeshRenderer>().material, gameObject));
    }
    public virtual void SetOriginalMaterial()
    {
        foreach (MaterialObjectInfo materialObjectInfo in materialInfoList)
        {
            materialObjectInfo.gameObject.GetComponent<MeshRenderer>().material = materialObjectInfo.material;
        }
    }
    public virtual void SetMaterial(Material material)
    {
        foreach (MaterialObjectInfo materialObjectInfo in materialInfoList)
        {
            materialObjectInfo.gameObject.GetComponent<MeshRenderer>().material = material;
        }
    }
}
public struct MaterialObjectInfo
{
    public MaterialObjectInfo(Material material, GameObject gameObject)
    {
        this.material = material;
        this.gameObject = gameObject;
    }
    public Material material;
    public GameObject gameObject;
}
public abstract class GeneralPlaceable : BuildingBlock
{
    public override void Move(Vector3 hova, Vector2 Axis2D)
    {
        float? angle = SnapAngle(Axis2D);
        if (angle != null)
        {
            transform.SetPositionAndRotation(hova, Quaternion.Euler(0, 0, (float)angle));
        }
        else
        {
            transform.position = hova;
        }
        foreach (var szomszed in neighbours)
        {
            szomszed.GetComponent<GeneralOverPlaceable>().Redraw();
        }
    }
    public override void DeleteObject()
    {
        for (int i = neighbours.Count - 1; i >= 0; i--)
        {
            neighbours[i].GetComponent<GeneralOverPlaceable>().DeleteObject();
        }
        Destroy(gameObject);
    }
    public override void AddNeighbour(GameObject szomszed)
    {
        neighbours.Add(szomszed);
        szomszed.GetComponent<GeneralOverPlaceable>().AddNeighbour(gameObject);
    }
    public abstract bool[] Dof();
}

public abstract class GeneralOverPlaceable : BuildingBlock, IOverpaceable
{
    protected bool placing { get; set; }
    public override void Awake()
    {
        base.Awake();
        placing = false;
    }
    public virtual void PlacingStart(InputAction axisinput, Transform RaycastOrigin)
    {
        this.axisinput = axisinput;
        this.RaycastOrigin = RaycastOrigin;
        placing = true;
        SelectObject();
        foreach (var szomszed in neighbours)
        {
            szomszed.GetComponent<BaseCustomRayInteractable>().SelectObject();
        }
    }
    public abstract bool PlacingEnd();

    public override void Update()
    {
        if (moving)
        {
            UpdateMovement();
        }
        else if (placing)
        {
            UpdateBuilding();
        }
    }
    public virtual void UpdateBuilding()
    {
        if (Physics.Raycast(RaycastOrigin.position, RaycastOrigin.forward, out hit, Mathf.Infinity, rajzolhatoLayer))
        {
            Build(hit.point, neighbours[0].transform.position);
        }
    }
    public abstract void Redraw();
    public override void AddNeighbour(GameObject szomszed)
    {
        neighbours.Add(szomszed);
    }
    public abstract void Build(Vector3 hova, Vector3 honnan);
    public override void DeleteObject()
    {
        for (int i = neighbours.Count - 1; i >= 0; i--)
        {
            neighbours[i].GetComponent<GeneralPlaceable>().RemoveNeighbour(gameObject);
        }
        Destroy(gameObject);
    }
}

public abstract class GeneralTrayElement : BaseCustomRayInteractable, ITrayInteractable
{
    [Tooltip("Only one should be true by type")]
    [SerializeField] protected bool defaultObject = false;
    [SerializeField] protected GameObject prefabObject;
    public abstract GameObject GetSelectedObject();
    public override void Start()
    {
        base.Start();
        if (defaultObject)
        {
            GetSelectedObject();
        }
    }
}