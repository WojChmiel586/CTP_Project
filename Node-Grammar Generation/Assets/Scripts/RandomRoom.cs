using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class RandomRoom : MonoBehaviour
{
    public enum GridSpace
    {
        Empty,
        Wall,
        Floor
    }

    public int ID;
    //public Vector2 position;
    public Vector2 offset;
    public int width;
    public int height;
    public GridSpace[,] layout;
    public List<GameObject> roomTiles;
    [Space]
    public GameObject floorParent;
    public GameObject wallParent;
    [Space]
    public GameObject wallObj;
    public GameObject floorObj;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        width = Random.Range(6, 16);
        height = Random.Range(6, 16);
        if (width % 2 != 0)
        {
            width += 1;
        }
        if (height % 2 != 0)
        {
            height += 1;
        }
        boxCollider.size = new Vector2(width + 2, height + 2);
        //boxCollider.offset = new Vector2(-0.5f, -0.5f);
        offset = new Vector2(((float)width / 2), ((float)height / 2));

        BasicLayout();
    }

    private void BasicLayout()
    {
        layout = new GridSpace[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                layout[i, j] = GridSpace.Floor;
            }
        }
        for (int i = 0; i < width; i++)
        {
            layout[0, i] = GridSpace.Wall;
            layout[height-1, i] = GridSpace.Wall;
        }
        for (int i = 0; i < height; i++)
        {
            layout[i, 0] = GridSpace.Wall;
            layout[i, width-1] = GridSpace.Wall;
        }
        CreateDoor();
        SpawnRoom();
    }

    private void CreateDoor()
    {
        int side = Random.Range(0, 4);
        int x;
        if (transform.position.x > 0)
        {
            side = 3;
        }
        else
        {
            side = 1;
        }
        if (transform.position.y > 0)
        {
            side = 0;
        }
        else
        {
            side = 2;
        }
        switch (side)
        {
            //top
            case 0:
                x = Random.Range(1, width - 1);
                layout[0, x] = GridSpace.Floor;
                break;
            //right
            case 1:
                x = Random.Range(1, height - 1);
                layout[x, width - 1] = GridSpace.Floor;
                break;
            //bottom
            case 2:
                x = Random.Range(1, width - 1);
                layout[height - 1, x] = GridSpace.Floor;
                break;
            //left
            case 3:
                x = Random.Range(1, height - 1);
                layout[x, 0] = GridSpace.Floor;
                break;

        }
    }

    public void SpawnRoom()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                switch (layout[i,j])
                {
                    case GridSpace.Empty:
                        break;
                    case GridSpace.Wall:
                        {

                            Spawn(j,i, wallObj);
                        }
                        break;
                    case GridSpace.Floor:
                        {

                            Spawn(j, i, floorObj);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    void Spawn(int x, int y, GameObject tileToSpawn)
    {
        Vector2 spawnPos = new Vector2(x + transform.position.x - offset.x, y + transform.position.y - offset.y);
        GameObject obj = Instantiate(tileToSpawn, spawnPos, Quaternion.identity);
        roomTiles.Add(obj);
        if (tileToSpawn == wallObj)
        {
            obj.transform.parent = wallParent.transform;
        }
        if (tileToSpawn == floorObj)
        {
            obj.transform.parent = floorParent.transform;
        }
    }

     public void SnapToGrid()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);
        Vector3 vector = new Vector3(x, y, 0);
        transform.position = vector;
    }

    public Vector2 GetSize()
    {
        return new Vector2(height, width);
    }

    public bool AdequateRoomRatio(float minimumRatio)
    {
        float ratio = 0.0f;
        if (height >= width)
        {
            ratio = (float)width / (float)height;
            //Debug.Log("height > width" + height + "  " + width + "   " + ratio);
        }
        else
        {
            ratio = (float)height / (float)width;
            //Debug.Log("height < width" + height + "  " + width + "   " + ratio);
        }
        //Debug.Log("Room ratio = " + ratio);
        return ratio >= minimumRatio;
    }

    public void OnDrawGizmos()
    {
        Handles.Label(transform.position, ID.ToString());
    }

    public float TopEdge()
    {
        return transform.position.y + (height / 2);
    }

    public float BottomEdge()
    {
        return transform.position.y - (height / 2);
    }

    public float LeftEdge()
    {
        return transform.position.x - (width / 2);
    }

    public float RightEdge()
    {
        return transform.position.x + (width / 2);
    }
    public bool isRoomInside(RandomRoom otherRoom)
    {
        if ((otherRoom.TopEdge() > this.BottomEdge() && otherRoom.TopEdge() < TopEdge()) ||
            (otherRoom.BottomEdge() < TopEdge() && otherRoom.BottomEdge() > BottomEdge()) ||
            (otherRoom.LeftEdge() < RightEdge() && otherRoom.LeftEdge() > LeftEdge()) ||
            (otherRoom.RightEdge() > LeftEdge() && otherRoom.RightEdge() < RightEdge()))
        {
            return true;
        }


        return false;
    }
    public bool RoomOverlapVertically(RandomRoom otherRoom)
    {
        //Debug.Log("This room left and right edge: " + LeftEdge() + " ; " + RightEdge());
        Debug.Log("This room left and other room left edge: " + LeftEdge() + " ; " + otherRoom.LeftEdge());
        //Debug.Log("This ther room left and right edge: " + otherRoom.LeftEdge() + " ; " + otherRoom.RightEdge());
        Debug.Log("This room right and other room right edge: " + RightEdge() + " ; " + otherRoom.RightEdge());
        if ((otherRoom.LeftEdge() < RightEdge() && otherRoom.LeftEdge() >= LeftEdge() + 1) ||
            (otherRoom.RightEdge() > LeftEdge() && otherRoom.RightEdge() <= RightEdge() + 1)||
            (otherRoom.RightEdge() >= RightEdge() && otherRoom.LeftEdge() <= LeftEdge()))
        {
            return true;
        }
        return false;
    }

    public bool RoomOverlapHorizontally(RandomRoom otherRoom)
    {
        //Debug.Log("This room bottom and top edge: " + BottomEdge() + " ; " + TopEdge());
        Debug.Log("This room bottom and other room bottom edge: " + BottomEdge() + " ; " + otherRoom.BottomEdge());
        //Debug.Log("This Other room bottom and top edge: " + otherRoom.BottomEdge() + " ; " + otherRoom.TopEdge());
        Debug.Log("This room top and other room top edge: " + TopEdge() + " ; " + otherRoom.TopEdge());
        if ((otherRoom.TopEdge() > BottomEdge() && otherRoom.TopEdge() <= TopEdge() + 1) ||
            (otherRoom.BottomEdge() < TopEdge() && otherRoom.BottomEdge() >= BottomEdge() + 1)||
            (otherRoom.BottomEdge() <= BottomEdge() && otherRoom.TopEdge() >= TopEdge()))
        {
            return true;
        }
        return false;
    }

    public Vector3 RandomPointInRoom()
    {
        Transform obj = floorParent.transform.GetChild(Random.Range(0, floorParent.transform.childCount));

        return obj.position;
    }

}