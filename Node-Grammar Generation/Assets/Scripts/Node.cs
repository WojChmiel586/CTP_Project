using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

[System.Serializable]
public class Node
{
    public string Name;
    public Vector2 position;
    public Vertex vertex;
    public int value;
    public List<Arc> Arcs = new List<Arc>();
    public Node parent = null;

    public Node(string _name)
    {
        Name = _name;
    }

    public Node AddArc(Node _child, int w)
    {
        Arcs.Add(new Arc
        {
            parent = this,
            child = _child,
            Weigth = w
        });
        if (!_child.Arcs.Exists(a => a.parent == _child && a.child == this))
        {
            _child.AddArc(this, w);
        }

        return this;
    }
}
