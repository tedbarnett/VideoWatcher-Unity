using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSizeSetup : MonoBehaviour
{

    private float width;
    private float height;
    private int cellsX;
    private int cellsY;
    private int totalChildrenCount;

    void Start()
    {
        totalChildrenCount = this.gameObject.transform.childCount;
        cellsX = Mathf.CeilToInt(Mathf.Sqrt(totalChildrenCount)); //Convert.ToInt32(myFloat);
        cellsY = cellsX;
        if ((cellsX * (cellsX - 1)) >= totalChildrenCount) cellsY = cellsX - 1;

        width = this.gameObject.GetComponent<RectTransform>().rect.width;
        height = this.gameObject.GetComponent<RectTransform>().rect.height;
        Vector2 newSize = new Vector2(width / cellsX, height / cellsY);
        this.gameObject.GetComponent<GridLayoutGroup>().cellSize = newSize;
    }

}
