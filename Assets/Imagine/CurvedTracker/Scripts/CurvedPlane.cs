using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Imagine.WebAR
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CurvedPlane : MonoBehaviour
    {
        private const int DEGREES_PER_SEGMENT = 5;
        //public int numSegments = 16;  // Number of segments around the cylinder
        public float radius = 1f;    // Radius of the cylinder
        [HideInInspector]
        public bool preserveAspect = true;
        [HideInInspector]
        public float height = 2f;    // Height of the cylinder
        [Range(0f,2f)] 
        [HideInInspector]
        public float bottomRadiusMultiplier = 1;

        [Range(1f,360f)] 
        [HideInInspector]
        public float arc = 90f;
        public Material material;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public Material papercupMaterial;

        private void Awake()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.material = material;
        }
        public void GenerateCylinderMesh()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.material = material;

            if(meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == name){
                DestroyImmediate(meshFilter.sharedMesh);
            }


            // Create a new mesh
            Mesh mesh = new Mesh();

            float startAngle = 0.25f * 2 * Mathf.PI - 2 * Mathf.PI * arc / 360 / 2;
            int numSegments = Mathf.CeilToInt(arc/DEGREES_PER_SEGMENT);
            
            // Calculate the angle between segments
            float angleIncrement = arc / 360 * 2f * Mathf.PI / numSegments;

            // Calculate the height half
            float halfHeight = height * 0.5f;

            // Generate vertices and UVs
            Vector3[] vertices = new Vector3[(numSegments + 1) * 2];
            Vector2[] uv = new Vector2[(numSegments + 1) * 2];
            for (int i = 0; i <= numSegments; i++)
            {
                float angle = startAngle + i * angleIncrement;
                float x1 = -Mathf.Cos(angle) * radius;
                float z1 = -Mathf.Sin(angle) * radius;
                float x2 = -Mathf.Cos(angle) * radius * bottomRadiusMultiplier;
                float z2 = -Mathf.Sin(angle) * radius * bottomRadiusMultiplier;

                vertices[i] = new Vector3(x2, -halfHeight, z2);     // Bottom vertex
                vertices[i + numSegments + 1] = new Vector3(x1, halfHeight, z1);   // Top vertex

                // Calculate UV coordinates
                float u = (float)i / numSegments;    // U coordinate
                uv[i] = new Vector2(u, 0f);                // Bottom UV
                uv[i + numSegments + 1] = new Vector2(u, 1f);  // Top UV
            }

            // Generate triangles
            int[] triangles = new int[numSegments * 6];
            for (int i = 0; i < numSegments; i++)
            {
                int index = i * 6;

                triangles[index] = i;
                triangles[index + 1] = i + numSegments + 1;
                triangles[index + 2] = i + 1;

                triangles[index + 3] = i + numSegments + 1;
                triangles[index + 4] = i + numSegments + 2;
                triangles[index + 5] = i + 1;
            }

            // Assign vertices, UVs, and triangles to the mesh
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            // Recalculate normals to ensure proper lighting
            mesh.RecalculateNormals();

            // Assign the mesh to the MeshFilter component
            meshFilter.sharedMesh = mesh;
            mesh.name = name;
        }

        public Texture2D tempTex;
        public void ExportPapercupCutout(){
#if UNITY_EDITOR
            RenderTexture renderTexture = new RenderTexture(2048, 2048, 0);
            RenderTexture.active = renderTexture;

            papercupMaterial.SetTexture("_MainTex", material.GetTexture("_MainTex"));
            papercupMaterial.SetFloat("_arc", arc);
            papercupMaterial.SetFloat("_r1", radius);
            papercupMaterial.SetFloat("_r2", radius * bottomRadiusMultiplier);
            papercupMaterial.SetFloat("_h2", height);

            Graphics.Blit(null, renderTexture, papercupMaterial);
            //Graphics.Blit(renderTexture, null as RenderTexture);

            Debug.Log(RenderTexture.active.width + "x" + RenderTexture.active.height);

            var path = EditorUtility.SaveFilePanel("Save Papercup Cutout", "", name + "_papercup_cutout.png",".png");
            if(!string.IsNullOrEmpty(path)){
                Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                byte[] bytes = texture.EncodeToPNG();

                File.WriteAllBytes(path, bytes);

                //DestroyImmediate(texture);
                tempTex = texture;
                EditorUtility.RevealInFinder(path);
            }

            RenderTexture.active = null;
            renderTexture.Release();
#endif
        }
    }
}
