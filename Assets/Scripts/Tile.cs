using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;
    public bool isSelect;

    public bool isEmpty
    {
        get { return spriteRenderer.sprite == null ? true : false; }
    }
}
