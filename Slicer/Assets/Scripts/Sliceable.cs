using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sliceable : MonoBehaviour
{
    [Serializable]
    public struct planeData
    {
        public Vector3 point;
        public Vector3 normal;
    }

    public MeshFilter MeshFilter = null;
    public Renderer Renderer = null;
    public MeshCollider Collider = null;

    public List<planeData> _pds = new List<planeData>();
    protected Texture2D _tex = null;

    public static Vector3  MaxV3 => _maxV3;
    private static readonly  Vector3 _maxV3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

    public Sliceable[] Slice(SlicerPlane plane)
    {
        Sliceable otherSliceable = GameObject.Instantiate<GameObject>(this.gameObject, this.transform.parent).GetComponent<Sliceable>();

        if (this._pds.Count < 10)
        {
            Vector3 pos = this.transform.InverseTransformPoint(plane.Point);
            Vector3 normal = this.transform.InverseTransformVector(plane.Normal);
            this._pds.Add(new planeData()
            {
                point = pos,
                normal = normal,
            });
        }
        if (null == this._tex)
        {
            this._tex = new Texture2D(2, 10, TextureFormat.RGBAHalf, false);
            for (int i = 0; i < 10; ++i)
            {
                this._tex.SetPixel(0, i, Color.black);
                this._tex.SetPixel(1, i, Color.black);
            }
        }
        for (int i = 0; i < 10; ++i)
        {
            if (i < this._pds.Count)
            {
                Vector3 p = this._pds[i].point;
                Vector3 n = this._pds[i].normal;
                this._tex.SetPixel(0, i, new Color(p.x, p.y, p.z));
                this._tex.SetPixel(1, i, new Color(n.x, n.y, n.z));
            }
        }
        this._tex.Apply();
        this.Renderer.material.SetTexture("_PlaneTex", _tex);
        this.RefreshCollider(this._pds[this._pds.Count - 1]);


        if (otherSliceable._pds.Count < 10)
        {
            Vector3 pos = otherSliceable.transform.InverseTransformPoint(plane.Point);
            Vector3 normal = otherSliceable.transform.InverseTransformVector(-plane.Normal);
            otherSliceable._pds.Add(new planeData()
            {
                point = pos,
                normal = normal,
            });
        }
        if (null == otherSliceable._tex)
        {
            otherSliceable._tex = new Texture2D(2, 10, TextureFormat.RGBAHalf, false);
            for (int i = 0; i < 10; ++i)
            {
                otherSliceable._tex.SetPixel(0, i, Color.black);
                otherSliceable._tex.SetPixel(1, i, Color.black);
            }
        }
        for (int i = 0; i < 10; ++i)
        {
            if (i < otherSliceable._pds.Count)
            {
                Vector3 p = otherSliceable._pds[i].point;
                Vector3 n = otherSliceable._pds[i].normal;
                otherSliceable._tex.SetPixel(0, i, new Color(p.x, p.y, p.z));
                otherSliceable._tex.SetPixel(1, i, new Color(n.x, n.y, n.z));
            }
        }
        otherSliceable._tex.Apply();
        otherSliceable.Renderer.material.SetTexture("_PlaneTex", otherSliceable._tex);
        otherSliceable.RefreshCollider(otherSliceable._pds[otherSliceable._pds.Count - 1]);

        return new Sliceable[2] { this,otherSliceable};
    }

    public void RefreshCollider(planeData pData)
    {
        Mesh mesh = this.MeshFilter.mesh;

        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(mesh.vertices);

        List<int> triangles = new List<int>();
        triangles.AddRange(mesh.triangles);

        List<int> removeIndexs = new List<int>();
        for (int j = 0; j < vertices.Count; ++j)
        {
            Vector3 v = vertices[j];
            float result = Vector3.Dot(pData.normal, v) - Vector3.Dot(pData.normal, pData.point);
            if (result > 0.0001f)
            {
                removeIndexs.Add(j);
            }
        }

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        for (int j = 0; j < triangles.Count;)
        {
            int t0 = triangles[j];
            int t1 = triangles[j + 1];
            int t2 = triangles[j + 2];

            List<int> closeList = new List<int>();
            List<int> openList = new List<int>();

            if (removeIndexs.Contains(t0)) closeList.Add(t0); else openList.Add(t0);
            if (removeIndexs.Contains(t1)) closeList.Add(t1); else openList.Add(t1);
            if (removeIndexs.Contains(t2)) closeList.Add(t2); else openList.Add(t2);

            if (closeList.Count == 1)
            {
                Vector3? c2o1 = SlicerPlane.CrossPoint(pData.point,pData.normal,vertices[closeList[0]], vertices[openList[0]]);
                Vector3? c2o2 = SlicerPlane.CrossPoint(pData.point, pData.normal,vertices[closeList[0]], vertices[openList[1]]);

                int c2o1Index = vertices.Count + newVertices.Count;
                newVertices.Add(c2o1.Value);
                int c2o2Index = vertices.Count + newVertices.Count;
                newVertices.Add(c2o2.Value);

                Vector3 n = SlicerPlane.TriangleNormal(new Vector3[3] { vertices[t0], vertices[t1], vertices[t2] });

                Vector3[] nt1 = new Vector3[3] { c2o1.Value, vertices[openList[0]], c2o2.Value };
                if (0 <= Vector3.Dot(SlicerPlane.TriangleNormal(nt1), n))
                {
                    newTriangles.Add(c2o1Index);
                    newTriangles.Add(openList[0]);
                    newTriangles.Add(openList[1]);
                }
                else
                {
                    newTriangles.Add(openList[1]);
                    newTriangles.Add(openList[0]);
                    newTriangles.Add(c2o1Index);
                }


                Vector3[] nt2 = new Vector3[3] { c2o1.Value, vertices[openList[1]], c2o2.Value };
                if (0 <= Vector3.Dot(SlicerPlane.TriangleNormal(nt2), n))
                {
                    newTriangles.Add(c2o2Index);
                    newTriangles.Add(c2o1Index);
                    newTriangles.Add(openList[1]);
                }
                else
                {
                    newTriangles.Add(openList[1]);
                    newTriangles.Add(c2o1Index);
                    newTriangles.Add(c2o2Index);
                }

            }
            else if (closeList.Count == 2)
            {
                Vector3? o2c1 = SlicerPlane.CrossPoint(pData.point, pData.normal, vertices[openList[0]], vertices[closeList[0]]);
                Vector3? o2c2 = SlicerPlane.CrossPoint(pData.point, pData.normal, vertices[openList[0]], vertices[closeList[1]]);

                int o2c1Index = vertices.Count + newVertices.Count;
                newVertices.Add(o2c1.Value);
                int o2c2Index = vertices.Count + newVertices.Count;
                newVertices.Add(o2c2.Value);

                Vector3 n = SlicerPlane.TriangleNormal(new Vector3[3] { vertices[t0], vertices[t1], vertices[t2] });
                Vector3[] nt1 = new Vector3[3] { o2c1.Value, vertices[openList[0]], o2c2.Value };
                if (0 <= Vector3.Dot(SlicerPlane.TriangleNormal(nt1),n))
                {
                    newTriangles.Add(o2c1Index);
                    newTriangles.Add(openList[0]);
                    newTriangles.Add(o2c2Index);
                }
                else
                {
                    newTriangles.Add(o2c2Index);
                    newTriangles.Add(openList[0]);
                    newTriangles.Add(o2c1Index);
                }

                //newTriangles.Add(openList[0]);
                //newTriangles.Add(o2c1Index);
                //newTriangles.Add(o2c2Index);
            }

            if (closeList.Count > 0)
            {
                triangles.RemoveAt(j);
                triangles.RemoveAt(j);
                triangles.RemoveAt(j);
            }
            else
            {
                j += 3;
            }
        }

        vertices.AddRange(newVertices);
        triangles.AddRange(newTriangles);

        for (int i = 0; i < removeIndexs.Count; ++i)
        {
            int index = removeIndexs[i];
            vertices[index] = MaxV3;
        }

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (MaxV3 == vertices[i])
            {
                vertices.RemoveAt(i);
                for (int j = 0; j < triangles.Count; ++j)
                {
                    int tIndex = triangles[j];
                    if (tIndex > i)
                    {
                        triangles[j] = --tIndex;
                    }
                }
                --i;
            }
        }

        Mesh colliderMesh = new Mesh();
        colliderMesh.vertices = vertices.ToArray();
        colliderMesh.triangles = triangles.ToArray();
        colliderMesh.RecalculateNormals();
        colliderMesh.RecalculateTangents();

        this.Collider.convex = true;
        this.Collider.sharedMesh = colliderMesh;

    }
}
