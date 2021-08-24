using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossPointTest : MonoBehaviour
{
    public Transform Point1;
    public Transform Point2;
    public SlicerPlane Plane;

    public bool Show = false;

    protected Vector3? _cross = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Show)
        {
            Show = false;
            this.UpdateCrossPoint();
        }
    }


    protected void UpdateCrossPoint()
    {
        this._cross = SlicerPlane.CrossPoint(Plane.Point, Plane.Normal,Point1.position,Point2.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Point1.position, Point2.position);
        if (null != this._cross)
        {
            Gizmos.DrawSphere(this._cross.Value,0.25f);
        }
    }
}
