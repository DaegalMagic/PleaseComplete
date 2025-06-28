using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileType type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"blocks/{_type.ToString()}");
        }
    }
    public TileType _type;
    public bool isMatched;
    public Position position;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}