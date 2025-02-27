﻿using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : PathFindingAlgorithm
{
    public override List<string> PseudoCode => new List<string>()
    {
        "<style=\"sortingTitle\">A*:</style>",
        "fringe = <style=\"sortingKeyword\">PriorityQueue(0, start)</style>",
        "closedList = <style=\"sortingKeyword\">List()</style>",
        "<style=\"sortingKeyword\">while</style> fringe <style=\"sortingKeyword\">not empty</style>:",
        "    node = fringe.<style=\"sortingFunction\">Get</style>()",
        "    <style=\"sortingKeyword\">if</style> node == goal:",
        "         <style=\"sortingKeyword\">return</style> node",
        "    closedList.<style=\"sortingFunction\">Add</style>(node)",
        "    <style=\"sortingKeyword\">foreach</style> n in <style=\"sortingFunction\">Neighbours(node)</style>:",
        "        <style=\"sortingKeyword\">if</style> closedList <style=\"sortingKeyword\">not contains</style> n:",
        "            g = node.g + 1",
        "            h = <style=\"sortingFunction\">EstDist(n.pos, goal)</style>",
        "            f = g + h",
        "            fringe.<style=\"sortingFunction\">Add</style>(f, n)",
    };


    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float g;
        public float h;
        public float f { get { return g + h; } }
        public Node(Vector2Int pos)
        {
            position = pos;
            parent = null;
            g = 0;
            h = 0;
        }
    }

    public AStarPathFinding()
        : base("A*")
    { }

    public override List<PathFindingStep> Run(MazeElement[,] layout)
    {
        List<Node> _openList = new List<Node>();
        List<Node> _closedList = new List<Node>();
        List<PathFindingStep> steps = new List<PathFindingStep>();
        Node _currentNode;
        Node initialNode = new Node(_playerPosition);
        _openList.Add(initialNode);
        PathFindingStep initialStep = new PathFindingStep();
        initialStep.Layout = new MazeElement.MazeElementType[_mazeSize, _mazeSize];
        initialStep.MazeInfos = new string[_mazeSize, _mazeSize];
        initialStep.Parents = new Vector2Int[_mazeSize, _mazeSize];
        for (int x = 0; x < _mazeSize; ++x)
        {
            for (int y = 0; y < _mazeSize; ++y)
            {
                initialStep.Layout[x, y] = layout[x, y].ElementType;
                initialStep.MazeInfos[x, y] = FormatNodeText(new Node(new Vector2Int(x, y)));
                initialStep.Parents[x, y] = new Vector2Int(-1, -1);
            }
        }
        steps.Add(initialStep);
        PathFindingStep lastStep = initialStep;
        while (_openList.Count > 0)
        {
            PathFindingStep searchStep = new PathFindingStep(lastStep);
            _currentNode = _openList[0];
            foreach (Node node in _openList)
            {
                if (node.f < _currentNode.f)
                {
                    _currentNode = node;
                }
            }
            _openList.Remove(_currentNode);
            _closedList.Add(_currentNode);
            searchStep.Layout[_currentNode.position.x, _currentNode.position.y] = MazeElement.MazeElementType.IGNORED;
            searchStep.MazeInfos[_currentNode.position.x, _currentNode.position.y] = FormatNodeText(_currentNode);
            if (_currentNode.parent != null)
            {
                searchStep.Parents[_currentNode.position.x, _currentNode.position.y] = _currentNode.parent.position;
            }
            searchStep.NextStepDelay = 1.0f;
            searchStep.Complete = false;
            steps.Add(searchStep);
            lastStep = searchStep;
            foreach (Vector2Int item in AdjacentPaths(_currentNode.position, searchStep.Layout, _mazeSize))
            {
                PathFindingStep expandStep = new PathFindingStep(lastStep);
                if (item == _goalPosition)
                {
                    expandStep.Layout[_goalPosition.x, _goalPosition.y] = MazeElement.MazeElementType.CORRECT;
                    MarkCorrect(expandStep, _currentNode);

                    expandStep.Complete = true;
                    expandStep.NextStepDelay = -1.0f;
                    steps.Add(expandStep);
                    return steps;
                }
                if (expandStep.Layout[item.x, item.y] == MazeElement.MazeElementType.PATH)
                {
                    Node newNode = new Node(item);
                    newNode.parent = _currentNode;
                    newNode.g = _currentNode.g + 1;
                    newNode.h = DistanceHeuristic(item);
                    _openList.Add(newNode);
                    expandStep.Layout[item.x, item.y] = MazeElement.MazeElementType.WALKED;
                    expandStep.MazeInfos[item.x, item.y] = FormatNodeText(newNode);
                    expandStep.Parents[item.x, item.y] = _currentNode.position;
                    expandStep.NextStepDelay = 1.0f;
                    steps.Add(expandStep);
                    lastStep = expandStep;
                }
                else if (expandStep.Layout[item.x, item.y] == MazeElement.MazeElementType.WALKED)
                {
                    Node found = _openList.Find((node) => node.position == item);
                    Debug.Assert(found.position == item); // Should be found, assert if not
                    expandStep.NextStepDelay = 1.0f;
                    if (found.g > _currentNode.g + 1)
                    {
                        found.g = _currentNode.g + 1;
                        found.parent = _currentNode;
                        expandStep.MazeInfos[item.x, item.y] = FormatNodeText(found);
                        expandStep.Parents[item.x, item.y] = _currentNode.position;
                        steps.Add(expandStep);
                        lastStep = expandStep;
                    }
                }
            }
        }
        return steps;
    }

    private void MarkCorrect(PathFindingStep step, Node node)
    {
        step.Layout[node.position.x, node.position.y] = MazeElement.MazeElementType.CORRECT;
        if (node.parent != null)
        {
            MarkCorrect(step, node.parent);
        }
    }

    private float DistanceHeuristic(Vector2Int point)
    {
        Vector2Int goalVector = _goalPosition - point;
        return Mathf.Pow(goalVector.x, 2) + Mathf.Pow(goalVector.y, 2);
    }

    private static string FormatNodeText(Node node)
    {
        return $"g: {node.g}\nh: {node.h}\nf: {node.f}";
    }

}
