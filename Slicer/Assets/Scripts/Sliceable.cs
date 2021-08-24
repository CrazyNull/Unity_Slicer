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
        this.RefreshCollider();


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
        otherSliceable.RefreshCollider();

        return new Sliceable[2] { this,otherSliceable};
    }

    public void RefreshCollider()
    {
        Mesh mesh = this.MeshFilter.mesh;

        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(mesh.vertices);

        List<int> triangles = new List<int>();
        triangles.AddRange(mesh.triangles);

        List<int> removeIndexs = new List<int>();
        for (int i = 0; i < this._pds.Count; ++i)
        {
            planeData pData = this._pds[i];
            for (int j = 0; j < vertices.Count; ++j)
            {
                Vector3 v = vertices[j];
                float result = Vector3.Dot(pData.normal, v) - Vector3.Dot(pData.normal, pData.point);
                if (result > 0.0001f)
                {
                    removeIndexs.Add(j);
                    vertices[j] = Vector3.zero;
                }
            }
        }


        for (int i = 0; i < removeIndexs.Count; ++i)
        {
            int index = removeIndexs[i];
            for (int j = 0; j < triangles.Count;)
            {
                int t0 = triangles[j];
                int t1 = triangles[j + 1];
                int t2 = triangles[j + 2];

                if (t0 == index || t1 == index || t2 == index)
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
        }

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (Vector3.zero == vertices[i])
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
