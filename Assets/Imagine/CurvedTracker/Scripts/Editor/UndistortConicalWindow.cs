using UnityEngine;
using UnityEditor;

using Imagine.WebAR.Editor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Imagine.WebAR.Editor{
    public class UndistortConicalWindow : EditorWindow
    {
        private Texture2D texture;

        private float upperRadius = 1, lowerRadius = 1;
        private int outlineWidth = 5;

        float WIDTH = 0;
        float HEIGHT = 0;
        float MAX_LENGTH = 400;
        float theta = 0;

        [MenuItem("Assets/Imagine WebAR/Texture/Undistort Conical Image", false, 3000)]
        public static void ShowWindow()
        {
            GetWindow<UndistortConicalWindow>("Undistort Conical Image");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Conical Target Setup", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false) as Texture2D;
            if(EditorGUI.EndChangeCheck()){
                if(texture != null){
                    var w = 0;
                    var h = 0;
                    CT_WebARMenu.GetActualTextureSize(texture, ref w, ref h);
                    var max = Mathf.Max(w, h);
                    Debug.Log(w + "x" + h + " max: " + max);

                    WIDTH = MAX_LENGTH * w / max;
                    HEIGHT = MAX_LENGTH * h / max;
                    Debug.Log(WIDTH + "x" + HEIGHT);

                    minSize = new Vector2(MAX_LENGTH, MAX_LENGTH + 250);
                    maxSize = minSize;
                }
            }

            

            //EditorGUILayout.MinMaxSlider(ref lowerRadius, ref upperRadius, 1, 400);


            if (texture != null)
            {
                EditorGUILayout.LabelField("Width = " + WIDTH);
                EditorGUILayout.LabelField("Height = " + HEIGHT);


                //Rect previewRect = GUILayoutUtility.GetAspectRect(WIDTH / (float)HEIGHT);
                Rect bgRect = GUILayoutUtility.GetAspectRect(1);
                //bgRect.y = previewRect.y;
                EditorGUI.DrawRect(bgRect, Color.gray);
                var previewRect = new Rect(
                    (MAX_LENGTH - WIDTH)/2,
                    bgRect.y + (MAX_LENGTH - HEIGHT)/2,
                    WIDTH,
                    HEIGHT
                    );

                EditorGUI.DrawTextureTransparent(previewRect, texture);

                
                Vector2 circleCenter = new Vector2(200, upperRadius + previewRect.y - bgRect.y) + bgRect.position;

            

                Handles.BeginGUI();

                Handles.color = new Color(1,1,1,1);
                Handles.DrawLine(new Vector2(200,0) + bgRect.position, new Vector2(200,400) + bgRect.position);
                Handles.DrawLine(new Vector2(0,200) + bgRect.position, new Vector2(400,200) + bgRect.position);

                Handles.color = Color.red;
                Handles.DrawSolidDisc(circleCenter, Vector3.forward, 3);

                int numCircles = outlineWidth;//5; // Number of concentric circles
                float space = 0.4f;
                var checkTheta = 0.5f * MAX_LENGTH / upperRadius;
                if(checkTheta < 1){
                    theta = 2 * Mathf.Asin(checkTheta) * Mathf.Rad2Deg;
                }
                else{
                    theta = 360 - 2 * Mathf.Acos(HEIGHT/upperRadius - 1) * Mathf.Rad2Deg;
                }
                Debug.Log("theta = " + theta);
                EditorGUILayout.LabelField("Sector Angle = " + theta);

                var numLineSegments = 48;//Mathf.Floor(theta);
                var segmentAngle = theta / numLineSegments;
                var a1 = Vector2.zero;
                var a2 = Vector2.zero;


                List<Vector3> points = new List<Vector3>();
                for (int i = 0; i < numLineSegments; i++)
                {
                    for(int j = 0; j < numCircles; j++){
                        float angle = i * segmentAngle + 180 - theta/2;
                        float rad = upperRadius - j * space;
                        Vector3 startPoint = (Vector3)circleCenter + Quaternion.Euler(0f, 0f, angle) * Vector3.up * rad;
                        Vector3 endPoint = (Vector3)circleCenter + Quaternion.Euler(0f, 0f, angle + segmentAngle) * Vector3.up * rad;
                        Handles.DrawLine(startPoint, endPoint);

                        // if(j == 0){
                        //     points.Add(startPoint);
                        //     if(i == numLineSegments - 1){
                        //         points.Add(endPoint);
                        //     }
                        // }
                        if(j==0){
                            if(i == 0)
                                a1 = startPoint;
                            else if (i == numLineSegments - 1)
                                a2 = endPoint;
                        }
                        
                    }
                }
                
                var b1 = Vector2.zero;
                var b2 = Vector2.zero;
                Handles.color = Color.yellow;
                for (int i = 0; i < numLineSegments; i++)
                //for (var i = numLineSegments - 1; i >= 0; i--)
                {
                    for(int j = 0; j < numCircles; j++){
                        float angle = i * segmentAngle + 180 - theta/2;
                        float rad = lowerRadius + j * space;
                        Vector3 startPoint = (Vector3)circleCenter + Quaternion.Euler(0f, 0f, angle) * Vector3.up * rad;
                        Vector3 endPoint = (Vector3)circleCenter + Quaternion.Euler(0f, 0f, angle + segmentAngle) * Vector3.up * rad;
                        Handles.DrawLine(startPoint, endPoint);

                        // if(j == 0 ){
                        //     if(i == numLineSegments-1){
                        //         points.Add(endPoint);
                        //     }
                        //     points.Add(startPoint);
                        // }

                        if(j==0){
                            if(i == 0)
                                b1 = startPoint;
                            else if (i == numLineSegments - 1)
                                b2 = endPoint;
                        }
                    }
                }
                Handles.color = new Color(1,0.75f, 0, 1);
                //Handles.DrawAAConvexPolygon(points.ToArray());
                Handles.DrawLine(a1, b1);
                Handles.DrawLine(a2, b2);
                Handles.EndGUI();

                
            }
            GUI.color = Color.red;
            upperRadius = EditorGUILayout.Slider("Upper Radius Slider", upperRadius, (float)Mathf.Min(WIDTH, HEIGHT)/2, 2000 );
            GUI.color = Color.yellow;
            lowerRadius = lowerRadius > upperRadius ? upperRadius : lowerRadius;
            lowerRadius = EditorGUILayout.Slider("Lower Radius Slider", lowerRadius, 1, upperRadius );

            

            GUI.color = Color.white;

            outlineWidth = EditorGUILayout.IntSlider("Outline Width", outlineWidth, 1, 10 );

            EditorGUILayout.Space();


            if(GUILayout.Button("Export Undistorted Image")){
                
                //Polar Remapping
                var polarRemapMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Imagine/CurvedTracker/Materials/PolarRemapMat.mat");
                polarRemapMat.SetTexture("_MainTex", texture);
                polarRemapMat.SetFloat("_W", WIDTH);
                polarRemapMat.SetFloat("_H", HEIGHT);
                polarRemapMat.SetFloat("_R1", upperRadius);
                polarRemapMat.SetFloat("_R2", lowerRadius);
                polarRemapMat.SetFloat("_Angle", theta);

                //Debug.Log("texname = " + texture.name);

                RenderTexture renderTexture = new RenderTexture(8192, 8192, 0);
                RenderTexture.active = renderTexture;
                Graphics.Blit(null, renderTexture, polarRemapMat);


                Texture2D remapTexture = new Texture2D(renderTexture.width, renderTexture.height);
                remapTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                remapTexture.Apply();


                //Polar to rectangular
                var polarToRectMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Imagine/CurvedTracker/Materials/PolarToRectMat.mat");
                polarToRectMat.SetTexture("_MainTex", remapTexture);
                polarToRectMat.SetFloat("_Rf", lowerRadius/upperRadius);
                polarToRectMat.SetFloat("_Angle", theta);


                var ar = (2 * Mathf.PI * upperRadius * theta / 360) / (upperRadius - lowerRadius);
                RenderTexture renderTexture2 = new RenderTexture(
                    2048, 
                    (int)(2048 / ar), 0);
                RenderTexture.active = renderTexture2;
                Graphics.Blit(null, renderTexture2, polarToRectMat);

                Texture2D polarToRectTexture = new Texture2D(renderTexture2.width, renderTexture2.height);
                polarToRectTexture.ReadPixels(new Rect(0, 0, renderTexture2.width, renderTexture2.height), 0, 0);
                polarToRectTexture.Apply();

                var path = EditorUtility.SaveFilePanel("Save Undistorted Image", "", texture.name + "_undistorted.png",".png");
                if(!string.IsNullOrEmpty(path)){
        
                    byte[] bytes = polarToRectTexture.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    // bytes = remapTexture.EncodeToPNG();
                    // File.WriteAllBytes(path + "remapped.png", bytes);

                    EditorUtility.RevealInFinder(path);
                }
                else{
                    EditorUtility.DisplayDialog("Image not Saved", "User pressed cancel", "Ok");
                }

                RenderTexture.active = null;
                renderTexture.Release();
                renderTexture2.Release();
                DestroyImmediate(remapTexture);
                DestroyImmediate(polarToRectTexture);

            }
        }
    }

}
