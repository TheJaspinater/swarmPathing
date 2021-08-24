using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using TMPro;

public class MapGen : MonoBehaviour
{
    //Tiles
    public Tile barrier;

    //Tile map
    public Tilemap tilemap;
    private int[,] map;

    //Tile map params
    public int width;
    public int height;
    [Range(0, 100)]
    public int fillPercentage;
    public int wallCountModifier;
    public int smoothingEvents;
    private string seed;
    private System.Random psuedoRandom;

    //Ants params
    public GameObject ants;
    public GameObject ant;
    public int numOfAnts;

    //HeatMap Connector
    public HeatMap heatMap;
    public Canvas canvas;

    //DestroyMap
    private int prevWidth;
    private int prevHeight;

    //Food/Home objects
    public GameObject homePrefab;
    public GameObject foodPrefab;
    private GameObject home;
    private GameObject food;
    private bool homeSpawned = false;
    private bool foodSpawned = false;

    //GameCamera
    public Camera camera;

    //Settings
    public TMP_InputField inputWidth;
    public TMP_InputField inputHeight;
    public TMP_InputField inputFillPercentage;
    public TMP_InputField inputWallCountModifier;
    public TMP_InputField inputSmoothingEvents;
    public TMP_InputField inputNumberOfAnts;

    void Start()
    {
        prevWidth = width;
        prevHeight = height;
        seed = DateTime.Now.ToString();
        psuedoRandom = new System.Random(seed.GetHashCode());
        map = new int[width, height];
        initializeCamera();
        initializeHeatMap();
        generateNoise();
        smoothMap();
        DrawMap();
        if (numOfAnts > 0)
        {
            spawnAntsHomeFood();
        }
    }

    void Update()
    {
        if (home != null) heatMap.boostPOI(home);
        if (food != null) heatMap.boostPOI(food);
    }

    void generateNoise()
    {
        for (int x = 0; x < width; x++) //Populate map with noise
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (psuedoRandom.Next(0, 100) < fillPercentage) ? 1 : 0;

            }
        }
    }

    void smoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int adjacentTitlesCount = CountAdjacentTiles(x, y, 1, 1, 1, 1);
                if (adjacentTitlesCount > wallCountModifier)
                {
                    map[x, y] = 1;
                }
                else if (adjacentTitlesCount < wallCountModifier)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int CountAdjacentTiles(int mapX, int mapY, int startX, int endX, int startY, int endY) //returns count of nearby tiles to be used for smoothing and item spawning
    {
        int adjacentTitlesCount = 0;

        for (int nextToX = mapX - startX; nextToX <= mapX + endX; nextToX++)
        {
            for (int nextToY = mapY - startY; nextToY <= mapY + endY; nextToY++)
            {
                if (nextToX >= 0 && nextToX < width && nextToY >= 0 && nextToY < height) //check for out of bounds
                {
                    if (nextToX != mapX || nextToY != mapY) //Do not count position being measured
                    {
                        adjacentTitlesCount += map[nextToX, nextToY];
                    }
                }
            }
        }
        return adjacentTitlesCount;
    }

    void DrawMap()
    {
        //Debug.Log("Running DrawMap method.");
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] == 1 || x == 0 || x == width - 1 || y == 0 || y == height - 1)//draw border
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, barrier);
                    }
                    else
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, null);
                    }
                }
            }
        }
    }

    void spawnAntsHomeFood()
    {
        Debug.Log("Spawning Ants...");
        int spawnAwayFromWallsX = width / 4;
        int spawnAwayFromWallsY = height / 4;
        int randX = UnityEngine.Random.Range(spawnAwayFromWallsX, width - spawnAwayFromWallsX);
        int randY = UnityEngine.Random.Range(spawnAwayFromWallsY, height - spawnAwayFromWallsY);

        for (int x = randX; x < width; x++)
        {
            for (int y = randY; y < height; y++)
            {
                if (CountAdjacentTiles(x, y, 1, 1, 1, 1) == 0)
                {
                    if (homeSpawned == false)
                    {
                        Vector3 p = new Vector3(x, y, 0);
                        home = Instantiate(homePrefab, p, Quaternion.identity);
                        homeSpawned = true;
                    }
                    GameObject newAnt;
                    for (int i = 0; i < numOfAnts; i++)
                    {
                        Vector3 p;
                        if (homeSpawned == true)
                        {
                            p = home.transform.position;
                        }
                        else
                        {
                            p = new Vector3(x, y, -2);
                        }
                        newAnt = Instantiate(ant, p, Quaternion.identity);
                        newAnt.transform.parent = ants.transform;
                    }
                    x = width;
                    y = height;
                }
            }
        }

        if (foodSpawned == false)
        {
            randX = UnityEngine.Random.Range(spawnAwayFromWallsX, width - spawnAwayFromWallsX);
            randY = UnityEngine.Random.Range(spawnAwayFromWallsY, height - spawnAwayFromWallsY);
            for (int x = randX; x < width; x++)
            {
                for (int y = randY; y < height; y++)
                {
                    if (CountAdjacentTiles(x, y, 1, 1, 1, 1) == 0)
                    {
                        Vector3 p = new Vector3(x, y, 0);
                        food = Instantiate(foodPrefab, p, Quaternion.identity);
                        x = width;
                        y = height;
                        foodSpawned = true;
                    }
                }
            }
        }
    }

    public void initializeHeatMap()
    {
        canvas.transform.position = new Vector3(width / 2, height / 2, 0);
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, height);
        heatMap.initializeHeatMap(width, height);
    }
    void initializeCamera()
    {
        camera.transform.position = new Vector3(width / 2, height / 2, -180);
    }

    public void generateNewMap()
    {
        print("Generating New Map...");

        foodSpawned = false;
        homeSpawned = false;
        
        //Clear all tiles and reset map
        for (int x = 0; x < prevWidth; x++)
        {
            for (int y = 0; y < prevHeight; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
        prevWidth = width; //once width is changed you can not refference it for deletion
        prevHeight = height;// so we store them as previous values
        map = new int[width, height];

        //remove food and home objects
        Destroy(GameObject.Find("Home(Clone)"));
        Destroy(GameObject.Find("Food(Clone)"));
        
        initializeCamera();
        initializeHeatMap();
        generateNoise();
        smoothMap();
        DrawMap();
        resetAnts();
    }

    public void resetAnts()
    {
        initializeHeatMap();
        if (ants.transform.childCount > 0)
        {
            foreach (Transform child in ants.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (numOfAnts > 0)
        {
            spawnAntsHomeFood();
        }
    }

    public void changeWidth()
    {
        int newWidth;
        int.TryParse(inputWidth.text, out newWidth);
        width = newWidth;
    }

    public void changeHeight()
    {
        int newHeight;
        int.TryParse(inputHeight.text, out newHeight);
        height = newHeight;
    }

    public void changeFillPercentage()
    {
        int newFillPercentage;
        int.TryParse(inputFillPercentage.text, out newFillPercentage);
        fillPercentage = newFillPercentage;
    }

    public void changeWallCountModifier()
    {
        int newWallCountModifier;
        int.TryParse(inputWallCountModifier.text, out newWallCountModifier);
        wallCountModifier = newWallCountModifier;
    }

    public void changeSmoothingEvents()
    {
        int newSmoothingEvents;
        int.TryParse(inputSmoothingEvents.text, out newSmoothingEvents);
        smoothingEvents = newSmoothingEvents;
    }

    public void changeNumberOfAnts()
    {
        int newNumberOfAnts;
        int.TryParse(inputNumberOfAnts.text, out newNumberOfAnts);
        numOfAnts = newNumberOfAnts;
    }
}