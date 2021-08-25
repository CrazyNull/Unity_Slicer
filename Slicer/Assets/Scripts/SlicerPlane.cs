using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerPlane : MonoBehaviour
{
    public const float Epsilon = 0.0001f;

    public Vector3 Point => this.transform.position;
    public Vector3 Normal => this.transform.up;

    public void Slice(Sliceable sliceable)
    {
        sliceable.Slice(this);
    }

    public static SideOfPlane SideOf(Vector3 Point, Vector3 Normal, Vector3 pt)
    {
        float result = Vector3.Dot(Normal, pt) - Vector3.Dot(Normal, Point);
        if (result > Epsilon) return SideOfPlane.UP;
        if (result < -Epsilon) return SideOfPlane.DOWN;
        return SideOfPlane.ON;
    }

    public Vector3? CrossPoint(Vector3 point1, Vector3 point2)
    {
        return CrossPoint(this.Point, this.Normal, point1, point2);
    }

    public static Vector3? CrossPoint(Vector3 planePoint, Vector3 planeNormal, Vector3 point1, Vector3 point2)
    {
        SideOfPlane result1 = SlicerPlane.SideOf(planePoint, planeNormal, point1);
        SideOfPlane result2 = SlicerPlane.SideOf(planePoint, planeNormal, point2);

        if (result1 != result2)
        {
            if (result1 == SideOfPlane.ON) return point1;
            if (result2 == SideOfPlane.ON) return point2;
            Vector3 upPoint;
            Vector3 downPoint;
            if (result1 == SideOfPlane.UP)
            {
                upPoint = point1;
                downPoint = point2;
            }
            else
            {
                upPoint = point2;
                downPoint = point1;
            }

            Vector3 vectorpp2upp = upPoint - planePoint;
            float angle = 90f - Mathf.Acos(Vector3.Dot(vectorpp2upp.normalized, planeNormal.normalized)) * Mathf.Rad2Deg;
            float dis = vectorpp2upp.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 vectorDown2Up = upPoint - downPoint;
            float angle2 = 90f - Mathf.Acos(Vector3.Dot(vectorDown2Up.normalized, planeNormal.normalized)) * Mathf.Rad2Deg;
            float dis2 = dis / Mathf.Sin(angle2 * Mathf.Deg2Rad);

            Vector3 crossPoint = (vectorDown2Up.magnitude - dis2) * vectorDown2Up.normalized + downPoint;

            return crossPoint;
        }
        else
        {
            if (result1 == SideOfPlane.ON || result2 == SideOfPlane.ON)
            {
                return (point1 + point2) * 0.5f;
            }
        }
        return null;
    }

    public Vector3? PointProjection(Vector3 point)
    {
        return PointProjection(this.Point, this.Normal, point);
    }

    public static Vector3? PointProjection(Vector3 planePoint, Vector3 planeNormal, Vector3 point)
    {
        Vector3 vectorpp2p = point - planePoint;
        float angle = 90f - Mathf.Acos(Vector3.Dot(vectorpp2p.normalized, planeNormal.normalized)) * Mathf.Rad2Deg;
        float dis = vectorpp2p.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);
        return point - dis * planeNormal.normalized;
    }


    public static Vector3 TriangleNormal(Vector3[] t)
    {
        return Vector3.Cross(t[1] - t[0],t[2] - t[1]);
    }
}
