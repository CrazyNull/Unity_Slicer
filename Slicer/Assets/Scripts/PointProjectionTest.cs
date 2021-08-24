using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointProjectionTest : MonoBehaviour
{
    public Transform Point;
    public SlicerPlane Plane;

    public bool Show = false;
    protected Vector3? _projection = null;

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
            this.UpdateProjection();
        }
    }

    protected void UpdateProjection()
    {
        this._projection = SlicerPlane.PointProjection(Plane.Point, Plane.Normal, Point.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Point.position, Plane.transform.position);
        if (null != this._projection)
        {
            Gizmos.DrawSphere(this._projection.Value, 0.25f);
        }
    }
}
