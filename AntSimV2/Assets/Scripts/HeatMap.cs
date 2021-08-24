using UnityEngine;
using UnityEngine.UI;
using System;

public class HeatMap : MonoBehaviour
{
    public float refreshRate;
    public int resolutionScale = 1;
    private Texture2D heatMapTexture;
    public RawImage heatMap;
    public bool isVisible;
    private DateTime prevRefreshTime;
    private DateTime prevEvapTime;
    public float evapRate;
    public float evapStrength;
    public Canvas canvas;
    public float angle = 0;
    public float dist = 4;
    public int scanRadius = 5;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            Debug.Log("Toggle HeatMap...");
            isVisible = !isVisible;
            canvas.GetComponent<Canvas>().enabled = !canvas.GetComponent<Canvas>().enabled;
        }

        if (prevEvapTime < DateTime.Now)
        {
            evaporateHeatMap();
            prevEvapTime = DateTime.Now.AddMilliseconds(evapRate);
        }
        if (prevRefreshTime < DateTime.Now)
        {
            heatMapTexture.Apply();
            prevRefreshTime = DateTime.Now.AddMilliseconds(refreshRate);
        }
    }

    public void initializeHeatMap(int newWidth, int newHeight)
    {
        heatMapTexture = new Texture2D(newWidth * resolutionScale, newHeight * resolutionScale);

        for (int y = 0; y < heatMapTexture.height; y++)
        {
            for (int x = 0; x < heatMapTexture.width; x++)
            {
                heatMapTexture.SetPixel(x, y, new Color(0, 0, 0, 0.0f));
            }
        }
        heatMapTexture.Apply();
        heatMap.texture = heatMapTexture;
    }

    void evaporateHeatMap()
    {
        for (int x = 0; x < heatMapTexture.width; x++)
        {
            for (int y = 0; y < heatMapTexture.height; y++)
            {
                float highestVal = 0f;
                Color evapPxl = heatMapTexture.GetPixel(x, y);
                if (evapPxl.g > evapStrength)
                {
                    evapPxl.g -= evapStrength;
                    if (evapPxl.g > highestVal) highestVal = evapPxl.g;
                }
                else
                {
                    evapPxl.g = 0f;
                    if (evapPxl.g > highestVal) highestVal = evapPxl.g;
                }

                if (evapPxl.b > evapStrength)
                {
                    evapPxl.b -= evapStrength;
                    if (evapPxl.b > highestVal) highestVal = evapPxl.b;
                }
                else
                {
                    evapPxl.b = 0f;
                    if (evapPxl.b > highestVal) highestVal = evapPxl.b;
                }
                evapPxl.a = highestVal;
                heatMapTexture.SetPixel(x, y, evapPxl);
            }
        }
    }

    public void leaveTrailMarker(bool isSearching, float x, float y, float scentStrength)
    {
        int actualX = (int)(x * resolutionScale);
        int actualY = (int)(y * resolutionScale);
        Color newColor = heatMapTexture.GetPixel(actualX, actualY);
        if (isSearching && newColor.b < 0.8)
        {
            newColor.b += scentStrength;
            if (newColor.b > newColor.g)
            {
                newColor.a = newColor.b;
            }
            else
            {
                newColor.a = newColor.g;
            }
        }
        else if (isSearching == false && newColor.g < 0.8)
        {
            newColor.g += scentStrength;
            if (newColor.b > newColor.g)
            {
                newColor.a = newColor.b;
            }
            else
            {
                newColor.a = newColor.g;
            }
        }
        heatMapTexture.SetPixel(actualX, actualY, newColor);
    }

    public Vector3 scan(bool isSearching, Transform ant)
    {
        Vector3 marker = new Vector3(-1, -1, 0);

        marker = sensor(isSearching, ant, marker, 0);
        marker = sensor(isSearching, ant, marker, -angle);
        marker = sensor(isSearching, ant, marker, angle);
        
        return new Vector2(marker.x, marker.y);
    }

    Vector3 sensor(bool isSearching, Transform ant, Vector3 marker, float angle)
    {
        Vector3 point = ant.TransformPoint(dist * (float)Math.Cos((angle + 90) * Mathf.Deg2Rad), dist * (float)Math.Sin((angle + 90) * Mathf.Deg2Rad), 0f);
        int heatMapX = (int)(point.x * resolutionScale);
        int heatMapY = (int)(point.y * resolutionScale);

        Color newColor;
        int offSet = scanRadius / 2;
        for (int i = heatMapX - offSet; i < heatMapX + offSet + 1; i++)
        {
            for (int j = heatMapY - offSet; j < heatMapY + offSet + 1; j++)
            {
                if (Math.Pow(i - heatMapX, 2) + Math.Pow(j - heatMapY, 2) <= scanRadius)
                {
                    if (isSearching == true) //Search for green: food
                    {
                        newColor = heatMapTexture.GetPixel(i, j);
                        //newColor.r = 0.01f;
                        //newColor.a = 1;
                        //heatMapTexture.SetPixel(i, j, newColor);
                        if (newColor.g > 0.2 && marker.z < newColor.g / resolutionScale)
                        {
                            return new Vector3(i, j, newColor.g) / resolutionScale;
                        }
                    }
                    else //search blue: home
                    {
                        newColor = heatMapTexture.GetPixel(i, j);
                        //newColor.r = 0.01f;
                        //newColor.a = 1;
                        //heatMapTexture.SetPixel(i, j, newColor);
                        if (newColor.b > 0.2 && marker.z < newColor.b / resolutionScale)
                        {
                            return new Vector3(i, j, newColor.b) / resolutionScale;
                        }
                    }
                }
            }
        }
        return marker;
    }

    public void boostPOI(GameObject poi)
    {
        int heatMapX = (int)(poi.transform.position.x * resolutionScale);
        int heatMapY = (int)(poi.transform.position.y * resolutionScale);
        int offSet = 10;
        Color newColor;
        for (int i = heatMapX - offSet; i < heatMapX + offSet; i++)
        {
            for (int j = heatMapY - offSet; j < heatMapY + offSet; j++)
            {
                if (Math.Pow(i - heatMapX, 2) + Math.Pow(j - heatMapY, 2) <= offSet*2)
                {
                    if (poi.tag == "Home")
                    {
                        newColor = heatMapTexture.GetPixel(i, j);
                        newColor.b = 1;
                        newColor.a = 1;
                    }
                    else
                    {
                        newColor = heatMapTexture.GetPixel(i, j);
                        newColor.g = 1;
                        newColor.a = 1;
                    }
                    heatMapTexture.SetPixel(i, j, newColor);
                }
                    
            }
        }
    }

    public void clearHeatMap()
    {
        for (int x = 0; x < heatMapTexture.width; x++)
        {
            for (int y = 0; y < heatMapTexture.height; y++)
            {
                Color pixel = heatMapTexture.GetPixel(x, y);
                pixel.r = 0;
                pixel.g = 0;
                pixel.b = 0;
                pixel.a = 0;
                heatMapTexture.SetPixel(x, y, pixel);
            }
        }
    }
}