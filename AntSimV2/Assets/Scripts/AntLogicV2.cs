using UnityEngine;
using System;
using TMPro;

public class AntLogicV2 : MonoBehaviour
{
    //Movement
    public float maxSpeed = 2f;
    public float steerStegnth = 2f;
    public float wanderStrength = 0.1f;
    private Vector2 position;
    private Vector2 velocity;
    private Vector2 desiredDirection;
    private Vector2 previousLocation;
    public float velocityDampener = 0.1f;

    //Decision Making
    private string state = "Wander";
    public bool isSearching = true;

    //HeatMap
    private HeatMap heatMap;
    public float scentStrength = 0.01f;
    public float maxScentStrength = 0.8f;
    public float minScentStrength = 0.1f;
    public float markerGap = 0.01f;
    public float rateOfScentLoss = 0.01f;
    public int scentLossDelay = 1;
    private DateTime scentLossTime;

    //Object detection
    private Collider2D hit;
    private Vector2 centerSensorLocation;
    public float sensorRadius = 0.1f;
    public float sensorDistance = 1f;
    public float minShoulderDist = 1f;

    //POI Detection
    public float delay = 1f;
    private DateTime prevTime;

    //Settings
    public TMP_InputField inputMaxSpeed;

    void Start()
    {
        heatMap = GameObject.Find("HeatMap").GetComponent<HeatMap>();
        position = transform.position;
        centerSensorLocation = new Vector2(0, sensorDistance);
    }

    void Update()
    {
        Collider2D hitLeft = Physics2D.OverlapCircle(transform.TransformPoint(new Vector3(-2, 2, 0)), sensorRadius);
        if (hitLeft != null && hitLeft.tag == "Barrier")
        {
            nudge(1);
        }
        else
        {
            Collider2D hitRight = Physics2D.OverlapCircle(transform.TransformPoint(new Vector3(2, 2, 0)), sensorRadius);
            if (hitRight != null && hitRight.tag == "Barrier")
            {
                nudge(-1);
            }
        }

        hit = Physics2D.OverlapCircle(transform.TransformPoint(centerSensorLocation), sensorRadius);
        Vector2 objNormal = new Vector2(0f, 0f);
        Vector2 marker = new Vector2(0f, 0f);
        if (hit != null) //Detect Barrier/Food/Home
        {
            RaycastHit2D objHit = Physics2D.Raycast(transform.position, transform.up);
            objNormal = objHit.normal;
            state = hit.tag;
            if ((state == "Home" && prevTime > DateTime.Now) || (state == "Food" && prevTime > DateTime.Now))
            {
                state = "Wander";
            }
        }
        else //Detect trail marker
        {
            marker = heatMap.scan(isSearching, transform);
            if (marker.x > 0)
            {
                state = "Trail";
            }
        }

        // Direction: React Scan Data
        switch (state)
        {
            case "Barrier":
                if (objNormal.x == 0) //ceiling or floor
                {
                    velocity.y *= -1 * velocityDampener;
                }
                else //wall
                {
                    velocity.x *= -1 * velocityDampener;
                }
                executeTranslation();
                desiredDirection = transform.up;
                desiredDirection = (desiredDirection + UnityEngine.Random.insideUnitCircle * maxSpeed).normalized;
                state = "Wander";
                break;

            case "Food":
                scentStrength = maxScentStrength;
                isSearching = false;
                velocity.x *= -1;
                velocity.y *= -1;
                desiredDirection.x *= -1;
                desiredDirection.y *= -1;
                prevTime = DateTime.Now.AddSeconds(delay);
                break;

            case "Home":
                scentStrength = maxScentStrength;
                isSearching = true;
                velocity.x *= -1;
                velocity.y *= -1;
                desiredDirection.x *= -1;
                desiredDirection.y *= -1;
                prevTime = DateTime.Now.AddSeconds(delay);
                break;

            case "Trail":
                //Debug.DrawLine(transform.position, new Vector3(marker.x, marker.y, 0), Color.green, 0.1f);
                Vector2 relativeMarker = new Vector2(marker.x - transform.position.x, marker.y - transform.position.y);
                float angle = Mathf.Atan2(relativeMarker.y, relativeMarker.x) * Mathf.Rad2Deg;
                Quaternion target = Quaternion.Euler(0, 0, angle - 90);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime);
                desiredDirection = (relativeMarker).normalized;
                executeTranslation();
                state = "Wander";
                break;

            case "Wander":
                desiredDirection = (desiredDirection + UnityEngine.Random.insideUnitCircle * wanderStrength).normalized;
                executeTranslation();
                break;

            default:
                break;
        }

        leaveTrailMarker();
    }

    void executeTranslation()
    {
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStegnth;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStegnth) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle - 90));
    }

    void leaveTrailMarker()
    {
        if (Math.Abs(transform.position.x - previousLocation.x) >= markerGap
            || Math.Abs(transform.position.y - previousLocation.y) >= markerGap)
        {
            previousLocation.x = transform.position.x;
            previousLocation.y = transform.position.y;

            if (scentStrength >= minScentStrength && scentLossTime < DateTime.Now)
            {
                scentLossTime.AddSeconds(scentLossDelay);
                scentStrength -= rateOfScentLoss;
            }

            heatMap.leaveTrailMarker(isSearching, transform.position.x, transform.position.y, scentStrength);
        }
    }

    void nudge(int dir)
    {
        //Debug.Log("nudged...");
        if (dir == 1)
        {
            desiredDirection += new Vector2(transform.right.x, transform.right.y);
        }
        else
        {
            desiredDirection -= new Vector2(transform.right.x, transform.right.y);
        }
        executeTranslation();
    }
    public void changeMaxSpeed()
    {
        float newMaxSpeed;
        float.TryParse(inputMaxSpeed.text, out newMaxSpeed);
        maxSpeed = newMaxSpeed;
    }
}