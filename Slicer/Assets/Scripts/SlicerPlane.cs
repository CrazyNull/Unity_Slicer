using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerPlane : MonoBehaviour
{
    public const float Epsilon = 0.0001f;

    public Vector3 Point => this.transform.position;
    public Vector3 Normal => this.transform.up;


    void Start()
    {

    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //ISliceable s = other.GetComponent<ISliceable>();
        //if (null != s)
        //{
        //    Sliceable[] ss = s.Slice(this);
        //}
    }

    public void Slice(Sliceable sliceable)
    {
        sliceable.Slice(this);
    }

    public SideOfPlane SideOf(Vector3 pt)
    {
        float result = Vector3.Dot(this.Normal, pt) - Vector3.Dot(this.Normal, this.Point);
        if (result > Epsilon) return SideOfPlane.UP;
        if (result < -Epsilon) return SideOfPlane.DOWN;
        return SideOfPlane.ON;
    }
}
