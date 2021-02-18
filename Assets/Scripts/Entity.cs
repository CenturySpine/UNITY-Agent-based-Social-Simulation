using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathRenderer : MonoBehaviour
{
    private LineRenderer lineRend;
    public bool PathDrawn { get; set; }

    public void DeleteGoalLine()
    {
        lineRend.positionCount = 0;

        Destroy(lineRend.gameObject);
        Destroy(lineRend);

        lineRend = null;
        PathDrawn = false;
    }

    public void DrawGoalLine(Vector3 current, Vector3 destination)
    {
        var lineObject = new GameObject("Line");
        lineRend = lineObject.AddComponent<LineRenderer>();
        lineRend.gameObject.SetActive(true);
        lineRend.startColor = Color.black;
        lineRend.endColor = Color.black;
        lineRend.startWidth = 1.0f;
        lineRend.endWidth = 1.0f;
        Vector3 sp = current;
        Vector3 ep = destination;
        lineRend.positionCount = 2;
        lineRend.SetPosition(0, sp);
        lineRend.SetPosition(1, ep);
        PathDrawn = true;
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

    private PathRenderer PathRenderer;

    private PathFinder pathFinder;
    // Start is called before the first frame update
    private void Start()
    {
        pathFinder = new PathFinder();
        PathRenderer = new PathRenderer();
        goals = new List<GlobalGameManager.Goal>();
        CurrentDirection = (Direction)(int)Random.Range(0, 4);
        rb = gameObject.GetComponent<Rigidbody2D>();
        _entityCollider = gameObject.GetComponent<BoxCollider2D>();
        //_entityCollider.offset = new Vector2(0.002f, 0.002f);
        //_entityCollider.size = new Vector2(0.5f, 0.5f);
    }

    //private GameObject lineObject;



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
                PathRenderer.DeleteGoalLine();
            }
            //objective not reached, continue moving toward objective
            else
            {
                if (!PathRenderer.PathDrawn)
                {
                    PathRenderer.DrawGoalLine(transform.position, _currentGoal._goalPosition);

                }
                transform.position = Vector3.MoveTowards(transform.position, _currentGoal._goalPosition, (float)(Speed * Time.deltaTime)); //<= WORKING DO NOT REMOVE
            }
        }

        //register objective click even if currently moving
        if (Input.GetMouseButton(0) && !GlobalGameManager.Instance.GetIsMenuHit())
        {
            //get point
            Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            pathFinder.Init(
                new System.Numerics.Vector2(transform.position.x,transform.position.y), 
                new System.Numerics.Vector2(pz.x,pz.y), 
                GlobalGameManager.Instance.validMapNodes.Select(n=> new System.Numerics.Vector2(n.x, n.y) )
                    .ToList(), 20);
            
            var path = pathFinder.Start();
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