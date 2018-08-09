using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour{

    public Tile data;
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;

    private void Start()
    {
       spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
       switch(data.type)
        {
            case "TILE":
                spriteRenderer.sprite = sprites[0];
                this.gameObject.layer = 8;
                Debug.Log("Floor");
                break;
            case "CARPET":
                spriteRenderer.sprite = sprites[1];
                this.gameObject.layer = 8;
                Debug.Log("Floor");
                break;
            case "WOOD":
                spriteRenderer.sprite = sprites[2];
                this.gameObject.layer = 8;
                Debug.Log("Floor");
                break;
            case "WALL":
                spriteRenderer.sprite = sprites[3];
                this.gameObject.layer = 11;
                Debug.Log("Wall");
                break;
        }
    }

}
