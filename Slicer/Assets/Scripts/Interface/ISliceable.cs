using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISliceable 
{
    Sliceable[] Slice(SlicerPlane plane);
}
