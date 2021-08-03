using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MST
{
    List<Node> nodes = new List<Node>();
    List<Node> processedNodes = new List<Node>();

    Node rootNode;

    Node currentNode;

    int NodeNumber = 0;

    Graph FinalGraph = new Graph();
    public void InputDataPoints(List<Node> _nodes,Node _rootNode)
    {
        nodes = new List<Node>(_nodes);
        rootNode = _rootNode;
        //Debug.Log("MTG root: " + rootNode.position + "origin node: " + _rootNode.position + "Length of the MTG list: " + nodes.Count + " " + _nodes.Count);
        NodeNumber = nodes.Count;
        if (nodes.Contains(rootNode))
        {
           // Debug.Log("Correct RootNode: " + rootNode.Name);
        }
        else
        {
          //  Debug.Log("ERROR WITH RootNode");
        }
        foreach (var node in nodes)
        {
            //foreach (Node item in _nodes)
            //{
            //    if (node == item)
            //    {
            //        node.Arcs = new List<Arc>(item.Arcs);
            //    }
            //}
            node.value = int.MaxValue;
            if(node == rootNode)
            {
                node.value = 0;
                //FinalGraph.CreateNode(node.Name, node.position);
                FinalGraph.Root = node;
            }
        }

        //GenerateGraph();

    }



   public Graph GenerateGraph()
    {
        currentNode = null;
        processedNodes.Add(rootNode);
      //  Debug.Log("processed NOdes count: " + processedNodes.Count);
        nodes.Remove(rootNode);
      //  Debug.Log("new NOdes count: " + nodes.Count);
        int minValue = int.MaxValue;
        foreach (Arc arc in rootNode.Arcs)
        {
            if (arc.child.value > arc.Weigth)
            {
                arc.child.value = arc.Weigth;
            }
        }
        foreach (var arc in rootNode.Arcs)
        {
            if (arc.child.value < minValue)
            {
                currentNode = arc.child;
                minValue = arc.child.value;
             //   Debug.Log("Current node: " + currentNode.Name);
            }
        }
        processedNodes.Add(currentNode);
      //  Debug.Log("processed Nodes count: " + processedNodes.Count);
        nodes.Remove(currentNode);
    //    Debug.Log("new Nodes count: " + nodes.Count);
        do
        {
            ProcessNode();
        } while (processedNodes.Count < NodeNumber);
        AdjustEdges();

        return FinalGraph;
    }

    public void ProcessNode()
    {
        foreach (Node node in processedNodes)
        {
                foreach (Arc arc in node.Arcs)
                {
                    if (arc.child.value > arc.Weigth && nodes.Contains(arc.child))
                    {
                        arc.child.value = arc.Weigth;
                    }
                }
        }
        int minValue = int.MaxValue;
        foreach (Node node in processedNodes)
        {
            //node.Arcs = new List<Arc>(node.Arcs);
            foreach (var arc in node.Arcs)
            {
                if (arc.child.value < minValue && nodes.Contains(arc.child))
                {
                    currentNode = arc.child;
                    minValue = arc.child.value;
                  //  Debug.Log("Current node: " + currentNode.Name);
                }
            }
        }
        processedNodes.Add(currentNode);
       // Debug.Log("processed Nodes count: " + processedNodes.Count);
        nodes.Remove(currentNode);
       // Debug.Log("new Nodes count: " + nodes.Count);
    }

    void AdjustEdges()
    {
        Node node1 = null;
        foreach (Node node in processedNodes)
        {
            //Debug.Log("START?");
            foreach (Arc arc in node.Arcs)
            {
                if (node.value == arc.Weigth)
                {
                    node1 = arc.child;
                }
            }
            //node.Arcs.Clear();
            //node.AddArc(node1, 69);
            //Node nNode = FinalGraph.CreateNode(node.Name, node.position);
            //nNode.AddArc(node1, 69);
            //FinalGraph.AllNodes.Add(node);
            foreach (var arc in node.Arcs)
            {
                //Debug.Log("Node " + arc.parent.Name + " is connected to " + arc.child.Name);
            }

        }
        FinalGraph.AllNodes = new List<Node>(processedNodes);

    }
    //Get all Nodes from chosen rooms

    //Get root Node of the graph

    //
}
