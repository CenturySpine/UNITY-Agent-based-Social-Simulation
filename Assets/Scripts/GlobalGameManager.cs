using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GlobalGameManager : MonoBehaviour
{
    public class Goal
    {
        public Vector2 _goalPosition;
    }

    public GameObject entityPrefab;
    public GameObject gridNodePrefab;
    public int EntityCount = 100;
    private EdgeCollider2D edge;
    public double GlobalSpeed = 50;
    public double GlobalAudacity = 0.001f;

    public static GlobalGameManager Instance;

    public double SpeedIncrement = 10;
    private GameObject _slider;
    private GameObject _menuShape;
    private bool _isMenuHit;
    private float _passed;
    public int NavGridInterval = 20;

    public bool GetIsMenuHit()
    {
        return _isMenuHit;
    }

    private void Awake()
    {
        _slider = GameObject.Find("SpeedSlider");

        _hoursDisplay = GameObject.Find("HoursDisplay").GetComponent<Text>();
        _speedDisplay = GameObject.Find("speeddisplay").GetComponent<Text>();
        _audacityDisplay = GameObject.Find("audacitydisplay").GetComponent<Text>();

        var menuColl = GameObject.Find("menushape").GetComponent<BoxCollider2D>();

        if (Instance == null)
            Instance = this;

        AddCameraCollider();

         validMapNodes = new List<Vector2>();

        //create nav nodes map
        int rowCount = 0;
        for (float y = edge.points[0].y; y < edge.points[1].y; y += NavGridInterval)
        {
            for (float a = edge.points[1].x; a < edge.points[2].x; a += NavGridInterval)
            {
                GameObject node;
                if (rowCount % 2 != 0)
                {
                    node = Instantiate(gridNodePrefab, new Vector3((float)a + (NavGridInterval / 2), y, 20), Quaternion.identity);//slight offset on odd line numbers (1,3,5, etc)
                }
                else
                {
                    node = Instantiate(gridNodePrefab, new Vector3((float)a, y, 20), Quaternion.identity);
                }
                //test raycasthit of current node
                RaycastHit2D hit = Physics2D.Raycast(node.transform.position, Vector2.zero);
                if (hit.collider != null)
                {
                    //if the nod collide it is invalid
                    node.GetComponent<SpriteRenderer>().color = Color.red;
                }
                else
                {
                    validMapNodes.Add(new Vector2(a, y));
                }
            }

            rowCount++;
        }

        //instantiate entities
        var offsetLeft = Math.Abs(menuColl.transform.position.x + menuColl.size.x * menuColl.transform.localScale.x);
        for (int i = 0; i < EntityCount; i++)
        {
            Instantiate(entityPrefab, new Vector3((float)Random.Range(edge.points[0].x + offsetLeft, edge.points[3].x), Random.Range(edge.points[1].y, edge.points[3].y), 0), Quaternion.identity);
        }
    }

    public List<Vector2> validMapNodes { get; set; }

    public void ChangeSpeedFromSlider(Slider s)
    {
        GlobalSpeed = s.value;
    }

    private void GetAudacity()
    {
        var audacityMod = Input.GetAxis("Horizontal");
        if (audacityMod < 0)
        {
            GlobalAudacity -= 0.01f * Time.deltaTime;
        }
        else if (audacityMod > 0)
        {
            GlobalAudacity += 0.01f * Time.deltaTime;
        }

        if (GlobalAudacity < 0) GlobalAudacity = 0.0f;
    }

    private void GetSpeed()
    {
        var speedMod = Input.GetAxis("Vertical");
        if (speedMod < 0)
        {
            GlobalSpeed -= SpeedIncrement * Time.deltaTime;
        }
        else if (speedMod > 0)
        {
            GlobalSpeed += SpeedIncrement * Time.deltaTime;
        }

        if (GlobalSpeed < 0) GlobalSpeed = 0.0f;
    }

    private void AddCameraCollider()
    {
        if (Camera.main == null) { Debug.LogError("Camera.main not found, failed to create edge colliders"); return; }

        var cam = Camera.main;
        if (!cam.orthographic) { Debug.LogError("Camera.main is not Orthographic, failed to create edge colliders"); return; }

        var bottomLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        var topLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight, cam.nearClipPlane));
        var topRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, cam.nearClipPlane));
        var bottomRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0, cam.nearClipPlane));

        // add or use existing EdgeCollider2D
        edge = GetComponent<EdgeCollider2D>() == null ? gameObject.AddComponent<EdgeCollider2D>() : GetComponent<EdgeCollider2D>();

        var edgePoints = new[] { bottomLeft, topLeft, topRight, bottomRight, bottomLeft };
        edge.points = edgePoints;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        VerifyExit();

        GetAudacity();

        GetSpeed();

        TestMenuMouseHit();

        UpdateWorldTime();

        UpdateUi();
    }

    private void VerifyExit()
    {
        if (Input.GetKey("escape"))
        {
            Exit();
        }
    }

    private void TestMenuMouseHit()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log("ray cast mouse hit : " + hit.collider.name);
                _isMenuHit = hit.collider.name.Equals("menushape");
            }
            else
            {
                _isMenuHit = false;
            }
        }
    }

    private void UpdateWorldTime()
    {
        _passed += Time.deltaTime;
        TimeSpan hour = TimeSpan.FromHours((int)_passed);
        if (hour != TimeOfDay)
            Debug.Log("time:" + hour);
        TimeOfDay = hour;
        if (hour >= TimeSpan.FromHours(24))
        {
            TimeOfDay = TimeSpan.FromHours(0);
            _passed = 0;
        }
    }

    private void UpdateUi()
    {
        _hoursDisplay.text = TimeOfDay.ToString();
        _speedDisplay.text = GlobalSpeed.ToString(CultureInfo.InvariantCulture);
        _audacityDisplay.text = GlobalAudacity.ToString(CultureInfo.InvariantCulture);
    }

    public TimeSpan TimeOfDay;
    private Text _hoursDisplay;
    private Text _speedDisplay;
    private Text _audacityDisplay;

    private void Exit()
    {
        Application.Quit();
    }
}