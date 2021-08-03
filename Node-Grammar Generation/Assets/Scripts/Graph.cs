using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Meshing;
using TriangleNet.Geometry;
using System;

[System.Serializable]
public class Graph
{
    public Node Root = null;
    public List<Node> AllNodes = new List<Node>();
    public TriangleNet.Mesh mesh = null;
    public Polygon polygon = new Polygon();

    private Node node0;
    private Node node1;
    private bool hasRootNode = false;

    public Node CreateNode(string name, Vector2 position)
    {
        var n = new Node(name);
        n.position = position;
        n.vertex = new TriangleNet.Geometry.Vertex(position.x, position.y);
        //Debug.Log("Created Node");
        AllNodes.Add(n);
        if (!hasRootNode)
        {
            //Debug.Log("created root node");
            Root = n;
            hasRootNode = true;
        }
        return n;
    }

    public void CreateMesh()
    {
        foreach (var node in AllNodes)
        {
            polygon.Add(node.vertex);
        }
        TriangleNet.Meshing.ConstraintOptions options =
        new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = false };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);

        ApplyDelaunayMeshToMST(mesh);
    }

    private void ApplyDelaunayMeshToMST(TriangleNet.Mesh mesh)
    {
        int iterator = 0;
        foreach (var edge in mesh.Edges)
        {
            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            node0 = null;
            node1 = null;
            foreach (var node in AllNodes)
            {
                if (node.vertex == v0)
                {
                    node0 = node;
                }
                if (node.vertex == v1)
                {
                    node1 = node;
                }
            }
            if (node0 != null && node1 != null)
            {
                int weight = Mathf.RoundToInt(Vector2.Distance(node0.position, node1.position));
                node0.AddArc(node1, weight);
                iterator++;
            }
        }
        Debug.Log("Number of connections generated: " + iterator);
    }
}
//                if (node.position == new Vector2((float)v0.x,(float)v0.y) || (node.position == new Vector2((float)v1.x, (float)v1.y)))