using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ISzerszam
{
    bool inhand { get; set; }
}
public class ToolBelt : MonoBehaviour
{
    [SerializeField] private List<GameObject> toolList = new List<GameObject>();
    [SerializeField] private List<GameObject> slotList = new List<GameObject>();
    [SerializeField] private GameObject mainCamera;
    List<ISzerszam> tools = new List<ISzerszam>();
    List<Quaternion> rotations = new List<Quaternion>();
    private void Awake()
    {
        for (int i = 0; i < toolList.Count; i++)
        {
            tools.Add(toolList[i].GetComponent<ISzerszam>());
            rotations.Add(toolList[i].transform.rotation);
        }
    }
    private void Start()
    {
        StartCoroutine(AlignToolCoroutine());
    }
    void Update()
    {
        for (int i = 0; i < tools.Count; i++)
        {
            if (!tools[i].inhand)
            {
                toolList[i].transform.position = slotList[i].transform.position;
                toolList[i].transform.rotation = rotations[i];
            }
        }
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - 0.5f, mainCamera.transform.position.z);
    }
    private IEnumerator AlignToolCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Vector3 irany = mainCamera.transform.forward;
        irany.y = 0;
        transform.forward = irany;
    }
}
