using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setCorridorSize(int sizeX, int sizeY)
    {
        var go = Instantiate(floorPrefab,transform.position,Quaternion.identity,transform);
        go.GetComponent<SpriteRenderer>().size = new Vector2(sizeX, sizeY);
    }
}
