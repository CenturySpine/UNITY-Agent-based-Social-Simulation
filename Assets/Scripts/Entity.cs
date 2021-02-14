using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandidateNode
{
    public Vector2 Position;
    public float F_Score { get; set; }
    public float H_Score { get; set; }
    public int G_Score { get; set; }

    public CandidateNode Parent { get; set; }
}

public class PathFinder
{
    private static CandidateNode current;
    private static CandidateNode startLoc;
    private static Vector2 targetLoc;
    private static List<CandidateNode> openList;
    private static List<CandidateNode> closedList;
    private static int g;
    private static float proximityRadius = 20;

    private static void Init(Vector2 start, Vector2 destination)
    {
        current = null;
        startLoc = current;
        targetLoc = destination;
        openList = new List<CandidateNode>();
        closedList = new List<CandidateNode>();
        g = 0;
        openList.Add(startLoc);
    }

    private static void Start()
    {
        while (openList.Count > 0)
        {
            // algorithm's logic goes here
            // get the square with the lowest F score
            var lowest = openList.Min(l => l.F_Score);
            current = openList.First(l => Math.Abs(l.F_Score - lowest) < 0.05);

            // add the current square to the closed list
            closedList.Add(current);

            // remove it from the open list
            openList.Remove(current);

            //if (closedList.FirstOrDefault(l => l.Position.x == targetLoc.x && l.Position.y == targetLoc.y) != null) //Original
            if (closedList.FirstOrDefault(l => Math.Abs(l.Position.x - targetLoc.x) < 0.05 && Math.Abs(l.Position.y - targetLoc.y) < 0.05) != null)
                break;

            var adjacentNodes = GetWalkableAdjacentNodes(current.Position, GlobalGameManager.Instance.validMapNodes);
            g++;

            foreach (var adjacentSquare in adjacentNodes)
            {
                // if this adjacent square is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.Position == adjacentSquare.Position) != null)
                    continue;

                // if it's not in the open list...
                if (openList.FirstOrDefault(l => l.Position == adjacentSquare.Position) == null)
                {
                    // compute its score, set the parent
                    adjacentSquare.G_Score = g;
                    adjacentSquare.H_Score = ComputeHScore(adjacentSquare.Position, targetLoc);
                    adjacentSquare.F_Score = adjacentSquare.G_Score + adjacentSquare.H_Score;
                    adjacentSquare.Parent = current;

                    // and add it to the open list
                    openList.Insert(0, adjacentSquare);
                }
                else
                {
                    // test if using the current G score makes the adjacent square's F score
                    // lower, if yes update the parent because it means it's a better path
                    if (g + adjacentSquare.H_Score < adjacentSquare.F_Score)
                    {
                        adjacentSquare.G_Score = g;
                        adjacentSquare.F_Score = adjacentSquare.G_Score + adjacentSquare.H_Score;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        }


    }
    static List<CandidateNode> GetWalkableAdjacentNodes(Vector2 currentLoc, List<Vector2> map)
    {
        //draw proximity area around current position
        Bounds proximityBounds = new Bounds(new Vector3(currentLoc.x - proximityRadius, currentLoc.y - proximityRadius, 0), new Vector3(proximityRadius * 2, proximityRadius * 2, 0));

        return
            //get map nodes which falls inside the proximity bounds
            map.Where(mapNode => proximityBounds.Contains(mapNode))
                //create walkable candidate Node
                .Select(node => new CandidateNode() { Position = new Vector2(node.x, node.y) })
                .ToList();
    }
    private static float ComputeHScore(Vector2 current, Vector2 destination)
    {
        return Math.Abs(destination.x - current.x) + Math.Abs(destination.y - current.y);
    }
}

public class Entity : MonoBehaviour
{
    public enum Direction
    {
        Top,
        Left,
        Right,
        Bottom
    }

    private List<GlobalGameManager.Goal> goals;
    public double Audacity = 0.00;
    public double Speed = 1.0f;
    public int Id;
    public Direction CurrentDirection;

    private Rigidbody2D rb;
    private BoxCollider2D _entityCollider;
    private GlobalGameManager.Goal _currentGoal;

    // Start is called before the first frame update
    private void Start()
    {
        goals = new List<GlobalGameManager.Goal>();
        CurrentDirection = (Direction)(int)Random.Range(0, 4);
        rb = gameObject.GetComponent<Rigidbody2D>();
        _entityCollider = gameObject.GetComponent<BoxCollider2D>();
        //_entityCollider.offset = new Vector2(0.002f, 0.002f);
        //_entityCollider.size = new Vector2(0.5f, 0.5f);
    }

    //private GameObject lineObject;

    private LineRenderer lineRend;

    // Update is called once per frame
    private void FixedUpdate()
    {
        Speed = GlobalGameManager.Instance.GlobalSpeed;

        //current ongoing objective
        if (_currentGoal != null)
        {
            //objective is reached
            if (transform.position == (Vector3)_currentGoal._goalPosition)
            {
                //remove the reached goal to avoid being picked again
                goals.Remove(_currentGoal);

                //no more objective, it will either pick next one or re start moving randomly
                _currentGoal = null;
                DeleteGoalLine();
            }
            //objective not reached, continue moving toward objective
            else
            {
                if (lineRend == null)
                {
                    DrawGoalLine();
                }
                transform.position = Vector3.MoveTowards(transform.position, _currentGoal._goalPosition, (float)(Speed * Time.deltaTime)); //<= WORKING DO NOT REMOVE
            }
        }

        //register objective click even if currently moving
        if (Input.GetMouseButton(0) && !GlobalGameManager.Instance.GetIsMenuHit())
        {
            //get point
            Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RegisterNewGoal(pz);
        }

        //some objectives are available and no current ongoing goal
        if (goals.Any() && _currentGoal == null)
        {
            //assign next goal to current;
            _currentGoal = goals.First();

            //move toward objective will start on next frame
        }
        //finally, if no ongoing goal, move normally
        else if (_currentGoal == null)
        {
            Audacity = GlobalGameManager.Instance.GlobalAudacity;

            ChangeDirection();
            float horizontalMovement = 0.0f;
            float verticalMovement = 0.0f;
            switch (CurrentDirection)
            {
                case Direction.Left:
                    horizontalMovement = (float)(-1 * Speed);
                    break;

                case Direction.Right:
                    horizontalMovement = (float)(1 * Speed);
                    break;

                case Direction.Top:
                    verticalMovement = (float)(-1 * Speed);
                    break;

                case Direction.Bottom:
                    verticalMovement = (float)(1 * Speed);
                    break;
            }
            Vector2 directionOfMovement = new Vector3(horizontalMovement, verticalMovement) * Time.deltaTime;

            // apply movement to player's transform
            transform.Translate(directionOfMovement /** Time.deltaTime*/);
            //rb.AddForce(directionOfMovement);
        }
    }

    private void RegisterNewGoal(Vector2 pz)
    {
        //do not add if already exists
        if (pz != Vector2.zero && goals.All(g => g._goalPosition != pz))
        {
            //add objective to the list
            goals.Add(new GlobalGameManager.Goal() { _goalPosition = new Vector2(pz.x, pz.y) });
        }
    }

    private void DeleteGoalLine()
    {
        lineRend.positionCount = 0;

        Destroy(lineRend.gameObject);
        Destroy(lineRend);

        lineRend = null;
    }

    private void DrawGoalLine()
    {
        var lineObject = new GameObject("Line");
        lineRend = lineObject.AddComponent<LineRenderer>();
        lineRend.gameObject.SetActive(true);
        lineRend.startColor = Color.black;
        lineRend.endColor = Color.black;
        lineRend.startWidth = 1.0f;
        lineRend.endWidth = 1.0f;
        Vector3 sp = transform.position;
        Vector3 ep = _currentGoal._goalPosition;
        lineRend.positionCount = 2;
        lineRend.SetPosition(0, sp);
        lineRend.SetPosition(1, ep);
    }

    private void ChangeDirection()
    {
        var audacityChangeProb = Random.Range(0.0f, 1.0f);
        var audacityInfluence = audacityChangeProb < Audacity;

        if (audacityInfluence)
        {
            var newDir = CurrentDirection;
            while (newDir == CurrentDirection)
            {
                newDir = (Direction)Random.Range(0, 4);
            }

            CurrentDirection = newDir;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.ToLowerInvariant().Contains("entity"))
            return;

        //_handleCollide = false;
        switch (CurrentDirection)
        {
            case Direction.Bottom:
                CurrentDirection = Direction.Top;

                break;

            case Direction.Top:
                CurrentDirection = Direction.Bottom;

                break;

            case Direction.Left:
                CurrentDirection = Direction.Right;

                break;

            case Direction.Right:
                CurrentDirection = Direction.Left;

                break;
        }
    }

    //Overlapping a collider 2D
    private void OnTriggerStay2D(Collider2D collision)
    {
    }

    //Just stop overlapping a collider 2D
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Do something
        //_handleCollide = true;
    }
}