using UnityEngine;
using System.Collections;
using Vectrosity;
using System.Collections.Generic;
using System;

public class DrawLine : MonoBehaviour
{
    #region public fields
    //The parent object of all lines
    public Canvas VectorCanvas;
    //Base texture for all lines
    public Texture2D lineTexture;
    //End cap texture for all lines (so the ends will be round)
    public Texture2D endCapTexture;
    public float distanceFromCamera = 100.0f;

    public bool mouseInputEnabled;
    public bool touchInputEnabled;
    #endregion

    #region private fields
    private VectorLine currentLine;
    private List<VectorLine> lines;

    //Current line's attributes
    private Color32 drawColor;
    private Vector3 previousPoint;
    private float previousWidth;
    private bool canDraw = false;
    
    //"Global" constants for lines
    private float minWidth = 5.0f;
    private float maxWidth = 40.0f;
    private int maxPointsNumber = 5000;
    private int minPixelMove = 1;
    private float speedScale = 3.0f;

    //Used for Vector3.sqrMagnitude, which is faster than .magnitude
    private int sqrMinPixelMove;
    private int lineCounter;

    #endregion

    #region monobehaviour methods
    void Start()
    {
        lines = new List<VectorLine>();
        drawColor = new Color32(255, 255, 255, 255);

        sqrMinPixelMove = minPixelMove * minPixelMove;
        lineCounter = -1;

        //Needed for *currentLine.endCap = "RoundCap";*
        VectorLine.SetEndCap("RoundCap", EndCap.Mirror, lineTexture, endCapTexture);
    }

    void Update()
    {
        if (mouseInputEnabled)
        {
            drawLineWithMouse();
        }

        if (touchInputEnabled)
        {
            drawLineWithTouch();
        }
    }
    #endregion

    #region private methods
    /// <summary>
    /// Start or continue drawing a VectorLine based on mouse input
    /// </summary>
    private void drawLineWithMouse()
    {
        Vector3 newPoint = GetMousePos();

        // Mouse button clicked, so start a new line
        if (Input.GetMouseButtonDown(0))
        {
            StartNewLine(newPoint);
        }
        // Mouse button held down and mouse has moved far enough to make a new point
        else if (Input.GetMouseButton(0) &&
                 (newPoint - previousPoint).sqrMagnitude > sqrMinPixelMove &&
                 canDraw)
        {
            AddPointToLine(newPoint);
        }
    }
    /// <summary>
    /// Start or continue drawing a VectorLine based on touch input
    /// </summary>
    private void drawLineWithTouch()
    {
        Vector3 newPoint = GetTouchPos();

        //New touch has started, so start a new line
        if (Input.touchCount != 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartNewLine(newPoint);
        }
        //Touch has moved (and moved enough), so add a new point
        else if (Input.touchCount == 1 && 
                 Input.GetTouch(0).phase == TouchPhase.Moved &&
                 (Input.GetTouch(0).position - (Vector2)previousPoint).sqrMagnitude > sqrMinPixelMove &&
                 canDraw)
        {
            AddPointToLine(newPoint);
        }
    }

    /// <summary>
    /// Starts a new VectorLine
    /// </summary>
    /// <param name="firstPoint">The first point of the VectorLine (in world coordinates)</param>
    private void StartNewLine(Vector3 firstPoint)
    {
        currentLine = new VectorLine("DrawnLine3D " + ++lineCounter, new List<Vector3>(), lineTexture, minWidth, LineType.Continuous, Joins.Fill);

        //Setting line attributes
        currentLine.endCap = "RoundCap";
        currentLine.smoothWidth = true;
        currentLine.endPointsUpdate = 2;
        currentLine.color = drawColor;

        //Adding things
        lines.Add(currentLine);
        currentLine.points3.Add(firstPoint);

        //Drawing the line
        currentLine.Draw3D();
        //For some reason .Draw3D() messes this up, needs to be set afterwards
        currentLine.rectTransform.SetParent(VectorCanvas.transform);

        currentLine.rectTransform.gameObject.GetComponent<MeshRenderer>().sortingOrder = lineCounter;

        previousPoint = firstPoint;
        canDraw = true;
    }

    /// <summary>
    /// Adds a new point to the line's end
    /// </summary>
    /// <param name="pointToAdd">The point to be added (in world coordinates)</param>
    private void AddPointToLine(Vector3 pointToAdd)
    {
        //Getting the speed of drawing gesture from the difference of the new and previous points' magnitude
        float dragSpeed = (pointToAdd - previousPoint).magnitude * speedScale;

        //Adding the new point
        currentLine.points3.Add(pointToAdd);

        //Setting the width
        currentLine.SetWidth(Mathf.Clamp(dragSpeed, minWidth, maxWidth), currentLine.GetSegmentNumber());

        //Drawing the line, and setting parent
        currentLine.Draw3D();
        currentLine.rectTransform.SetParent(VectorCanvas.transform);

        previousPoint = pointToAdd;

        //If we have too many points in the current line, end it
        if (currentLine.points3.Count >= maxPointsNumber)
        {
            canDraw = false;
        }
    }

    /// <summary>
    /// Gets the position of the drawing touch gesture in world coordinates
    /// </summary>
    /// <returns>Position of drawing touch</returns>
    private Vector3 GetTouchPos()
    {
        if (Input.touchCount == 0) return Vector3.zero;
        Vector3 p = Input.GetTouch(0).position;
        p.z = distanceFromCamera;
        return Camera.main.ScreenToWorldPoint(p);
    }

    /// <summary>
    /// Gets the position of the drawing mouse gesture in world coordinates
    /// </summary>
    /// <returns>Position of mouse</returns>
    Vector3 GetMousePos()
    {
        var p = Input.mousePosition;
        p.z = distanceFromCamera;
        return Camera.main.ScreenToWorldPoint(p);
    }
    #endregion

    #region public methods
    /// <summary>
    /// Deletes all drawn lines on the screen
    /// </summary>
    public void OnReset()
    {
        foreach (VectorLine line in lines)
        {
            line.points3.Clear();
            line.Draw3D();
            GameObject.Destroy(line.rectTransform.gameObject);
        }
        lines.Clear();
        lineCounter = -1;
    }

    /// <summary>
    /// Sets the color of the next line drawn
    /// </summary>
    /// <param name="colorID">ID of color to be set</param>
    public void OnSetcolor(int colorID)
    {
        switch (colorID)
        {
            case 0:
                //White
                drawColor = new Color32(255, 255, 255, 255);
                break;
            case 1:
                //Black
                drawColor = new Color32(0, 0, 0, 255);
                break;
            case 2:
                //Red
                drawColor = new Color32(255, 0, 0, 255);
                break;
            case 3:
                //Green
                drawColor = new Color32(0, 255, 0, 255);
                break;
            case 4:
                //Yellow
                drawColor = new Color32(255, 255, 0, 255);
                break;
            default:
                break;
        }
    }
    #endregion
}
