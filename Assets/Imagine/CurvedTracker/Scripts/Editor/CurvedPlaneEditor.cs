using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Imagine.WebAR.Editor{
    [CustomEditor(typeof(CurvedPlane))]
    public class CurvedPlaneEditor : UnityEditor.Editor
    {
        CurvedPlane _target;

        void OnEnable(){
            _target = (CurvedPlane)target;
            if( _target.GetComponent<MeshFilter>().sharedMesh == null || //for newly created targets
                _target.name != _target.GetComponent<MeshFilter>().sharedMesh.name //if you copy paste target
                ){
                _target.GenerateCylinderMesh();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            //_target.numSegments = EditorGUILayout.IntSlider("Num Segments", _target.numSegments, 1, 36);
            _target.preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", _target.preserveAspect);
            
            _target.bottomRadiusMultiplier = EditorGUILayout.Slider("Bottom Radius Multiplier", _target.bottomRadiusMultiplier, 0, 2);
            _target.arc = EditorGUILayout.Slider("Arc (degrees)", _target.arc, 1, 360);
            //we auto-set height
            //if(_target.preserveAspect){
                int w = 1;
                int h = 1;
                CT_WebARMenu.GetActualTextureSize((Texture2D)_target.material.mainTexture, ref w, ref h);
                //Debug.Log(w + "x" + h);
                var ar = (float)w/h;
                _target.height =  2 * Mathf.PI * _target.arc / 360 / ar;
                EditorGUILayout.LabelField("Height (from Aspect Ratio) = " + _target.height);
            // }
            // else{
            //     _target.height = EditorGUILayout.FloatField("Height", _target.height);
            // }

            if(EditorUtility.IsPersistent(_target)){
                //don't generate mesh for prefab files
                //we only do this for scene objects
                return;
            }

            if(EditorGUI.EndChangeCheck()){
                _target.GenerateCylinderMesh();
                EditorUtility.SetDirty(_target);
            }

            if(GUILayout.Button("Regenerate Mesh")){
                _target.GenerateCylinderMesh();
            }
            if(GUILayout.Button("Export Mesh")){
                var mesh = _target.GetComponent<MeshFilter>().mesh;
                SaveMesh(mesh, _target.name + " mesh", false, true);
            }
            if(GUILayout.Button("Export Papercup Cutout")){
                _target.ExportPapercupCutout();
            }
        }

        public void OnSceneGUI(){
            Handles.color = Color.white;
            var tr = _target.transform;
            var topPos = tr.position + tr.up * _target.height/2;
            var botPos = tr.position - tr.up * _target.height/2;
            Handles.DrawWireDisc(topPos, tr.up, 1);
            Handles.DrawWireDisc(botPos, tr.up, _target.bottomRadiusMultiplier);
            Handles.DrawLine(topPos, botPos);

            var vec1 = Quaternion.AngleAxis(_target.arc/2, tr.up) * -tr.forward;
            var vec2 = Quaternion.AngleAxis(-_target.arc/2, tr.up) * -tr.forward;
            Handles.DrawLine(topPos, topPos + vec1, 2);
            Handles.DrawLine(topPos, topPos + vec2, 2);
            Handles.DrawLine(botPos, botPos + vec1 * _target.bottomRadiusMultiplier, 2);
            Handles.DrawLine(botPos, botPos + vec2 * _target.bottomRadiusMultiplier, 2);
        }

        public static void SaveMesh (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh) {
            string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
            if (string.IsNullOrEmpty(path)) return;
            
            path = FileUtil.GetProjectRelativePath(path);

            Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
            
            if (optimizeMesh)
                MeshUtility.Optimize(meshToSave);
            
            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
	    }

        
    }
}

