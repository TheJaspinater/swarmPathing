using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraZoom : MonoBehaviour
{
    private Camera cam;
    private float startingFOV;

    public float minFOV;
    public float maxFOV;
    public float zoomRate;

    private float currentFOV;

    void Start()
    {
        cam = GetComponent<Camera>();
        startingFOV = cam.fieldOfView;
    }

    void Update()
    {
        currentFOV = cam.fieldOfView;

        if (Input.GetKey(KeyCode.I))
        {
            currentFOV -= zoomRate;
        }

        if (Input.GetKey(KeyCode.O))
        {
            currentFOV += zoomRate;
        }

        currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
        cam.fieldOfView = currentFOV;
    }
}
