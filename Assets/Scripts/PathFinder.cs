using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;


public class PathFinder
{
    private static CandidateNode _current;
    private static CandidateNode _startLoc;
    private static Vector2 _targetLoc;
    private static List<CandidateNode> _openList;
    private static List<CandidateNode> _closedList;
    private static int _g;
    private static float _proximityRadius = 20;
    /*private PathRenderer Renderer;
    private List<PathRenderer> pathRenderers;*/
    private List<Vector2> _instanceValidMapNodes;

    public void Init(Vector2 start, Vector2 destination, List<Vector2> instanceValidMapNodes, float proximityRadius)
    {
        _proximityRadius = proximityRadius;
        _instanceValidMapNodes = instanceValidMapNodes;
        /*pathRenderers?.Clear();
        pathRenderers = new List<PathRenderer>();
        Renderer = new PathRenderer();*/
        _current = null;
        _startLoc = new CandidateNode() { Position = start };
        _targetLoc = destination;
        _openList = new List<CandidateNode>();
        _closedList = new List<CandidateNode>();
        _g = 0;
        _openList.Add(_startLoc);
    }

    public List<CandidateNode> Start()
    {
        while (_openList.Count > 0)
        {
            // algorithm's logic goes here
            // get the square with the lowest F score
            var lowest = _openList.Min(l => l.F_Score);
            _current = _openList.First(l => Math.Abs(l.F_Score - lowest) < 0.05);

            // add the current square to the closed list
            _closedList.Add(_current);

            // remove it from the open list
            _openList.Remove(_current);

            //if (closedList.FirstOrDefault(l => l.Position.x == targetLoc.x && l.Position.y == targetLoc.y) != null) //Original
            if (_closedList.FirstOrDefault(l => Math.Abs(l.Position.X - _targetLoc.X) < 0.05 && Math.Abs(l.Position.Y - _targetLoc.Y) < 0.05) != null)
                break;

            var adjacentNodes = GetWalkableAdjacentNodes(_current.Position, _instanceValidMapNodes);
            _g++;

            foreach (var adjacentSquare in adjacentNodes)
            {
                // if this adjacent square is already in the closed list, ignore it
                if (_closedList.FirstOrDefault(l => l.Position == adjacentSquare.Position) != null)
                    continue;

                // if it's not in the open list...
                if (_openList.FirstOrDefault(l => l.Position == adjacentSquare.Position) == null)
                {
                    // compute its score, set the parent
                    adjacentSquare.G_Score = _g;
                    adjacentSquare.H_Score = ComputeHScore(adjacentSquare.Position, _targetLoc);
                    adjacentSquare.F_Score = adjacentSquare.G_Score + adjacentSquare.H_Score;
                    adjacentSquare.Parent = _current;

                    // and add it to the open list
                    _openList.Insert(0, adjacentSquare);
                    /*PathRenderer pathDisplay = new PathRenderer();
                    pathDisplay.DrawGoalLine(current.Position, adjacentSquare.Position);
                    pathRenderers.Add(pathDisplay);*/
                }
                else
                {
                    // test if using the current G score makes the adjacent square's F score
                    // lower, if yes update the parent because it means it's a better path
                    if (_g + adjacentSquare.H_Score < adjacentSquare.F_Score)
                    {
                        adjacentSquare.G_Score = _g;
                        adjacentSquare.F_Score = adjacentSquare.G_Score + adjacentSquare.H_Score;
                        adjacentSquare.Parent = _current;
                    }
                }
            }
            Thread.Sleep(500);
        }

        _closedList.Reverse();
        return _closedList;
    }
    List<CandidateNode> GetWalkableAdjacentNodes(Vector2 currentLoc, List<Vector2> map)
    {
        //draw proximity area around current position
        /*Bounds proximityBounds = new Bounds(new Vector3(currentLoc.x - _proximityRadius, currentLoc.y - _proximityRadius, 0), new Vector3(_proximityRadius * 2, _proximityRadius * 2, 0));*/

        return
            //get map nodes which falls inside the proximity bounds
            map.Where(mapNode => IsInBound(currentLoc,mapNode))
                //create walkable candidate Node
                .Select(node => new CandidateNode() { Position = new Vector2(node.X, node.Y) })
                .ToList();
    }

    bool IsInBound(Vector2 currentPoint,Vector2 targetPoint)
    {
        float left = currentPoint.X - _proximityRadius;
        float right = left + _proximityRadius * 2;
        float top = currentPoint.Y - _proximityRadius;
        float bottom = top + _proximityRadius * 2;

        return targetPoint.X >= left
               && targetPoint.X <= right
               && targetPoint.Y >= top
               && targetPoint.Y <= bottom;


    }
    
    private float ComputeHScore(Vector2 current, Vector2 destination)
    {
        return Math.Abs(destination.X - current.X) + Math.Abs(destination.Y - current.Y);
    }
}