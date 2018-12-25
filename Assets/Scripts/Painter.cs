using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Painter : MonoBehaviour {

    int width = 1000;
    int height = 500;
    int brushWidth = 20;
    int brushHeight = 20;
    Color brushColor = Color.red;

    int oldX, oldY;

    private Texture2D transTex;
    private Color[] brush;

    // Use this for initialization
    void Start () {
        transTex = new Texture2D(width, height);
        GameObject paint = GameObject.Find("Paint");
        paint.GetComponent<RawImage>().texture = transTex;
        Clear(transTex, Color.gray);

        brush = new Color[brushWidth * brushHeight];

        for (int i = 0; i < brush.Length; i++)
        {
            brush[i] = brushColor;
        }
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;

        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(camRay, out hit)/* && Input.GetMouseButton(0)*/) {
            int x = (int)(hit.textureCoord.x * width);
            int y = (int)(hit.textureCoord.y * height);

            Debug.Log("Clicked! x = " + x + ", y = " + y);

            if ((x != oldX) || (y != oldY))
            {
                transTex.SetPixels(x, y, brushWidth, brushHeight, brush);
                transTex.Apply();
                oldX = x;
                oldY = y;
            }
        }
    }

    void Clear(Texture2D tex, Color clearCol)
    {
        Color[] col = new Color[tex.width * tex.height];

        for (int i = 0; i < col.Length; i++)
        {
            col[i] = clearCol;
        }

        tex.SetPixels(col);
        tex.Apply();
    }
}
