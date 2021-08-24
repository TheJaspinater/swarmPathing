using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 dragOffset;
    public GameObject tempPrefab;
    private GameObject temp;
    private HeatMap heatMap;

    private void Start()
    {
        heatMap = GameObject.Find("HeatMap").GetComponent<HeatMap>();
    }

    void OnMouseDown()
    {
        Vector3 mouse = GetMousePos();
        dragOffset = transform.position - mouse;
        Vector3 p = new Vector3(mouse.x, mouse.y, 0);
        temp = Instantiate(tempPrefab, p, Quaternion.identity);
    }

    void OnMouseUp()
    {
        transform.SetPositionAndRotation(GetMousePos() + dragOffset, Quaternion.identity);
        Destroy(temp);
        heatMap.clearHeatMap();
    }

    private void OnMouseDrag()
    {
        temp.transform.SetPositionAndRotation(GetMousePos() + dragOffset, Quaternion.identity);
    }

    Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0, 0, Camera.main.transform.position.z));
        return mousePos;
    }
}
