using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Imagine.WebAR.Editor
{
    [CustomEditor(typeof(CurvedTracker))]
    public class CurvedTrackerEditor : UnityEditor.Editor
    {
        private class TargetInfos{
            public string id;
            public string msg;
            public Transform tr;
        }
 
        private CurvedTracker _target;

        private void OnEnable()
        {
            _target = (CurvedTracker)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackerCam"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curvedTargets"));
            CheckTargetProperties();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackerOrigin"));
            EditorGUILayout.Space(20);
            var overrideTrackerSettingsProp = serializedObject.FindProperty("overrideTrackerSettings");
            EditorGUILayout.PropertyField(overrideTrackerSettingsProp);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var overrideTrackerSettingsPropVal = overrideTrackerSettingsProp.boolValue;
            if (!overrideTrackerSettingsPropVal)
            {
                GUI.enabled = false;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackerSettings"), true);
            EditorGUI.indentLevel--;
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnImageFound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnImageLost"));


            DrawEditorDebugger(); 

            serializedObject.ApplyModifiedProperties();
        }

        void CheckTargetProperties(){
			var sp = serializedObject.FindProperty("curvedTargets");

            List<TargetInfos> infos = new List<TargetInfos>();

            for(int i = 0; i < sp.arraySize; i++){
                var obj = sp.GetArrayElementAtIndex(i);
                var tr = (Transform)obj.FindPropertyRelative("transform").objectReferenceValue;
				var id = obj.FindPropertyRelative("id").stringValue;

                var cp = tr.GetComponent<CurvedPlane>();
                var ctInfo = CT_GlobalSettings.Instance.curvedTargetInfos.Find(i=>i.id == id);
                
                if(ctInfo != null){
                    if( cp.bottomRadiusMultiplier != ctInfo.radMul ||
                        cp.arc != ctInfo.arc ||
                        cp.height != ctInfo.height
                    ){
                        infos.Add(new TargetInfos{
                            id = id,
                            tr = tr,
                            msg = "Unsaved Changes!"
                        });
                    }
                    else{
                        //all good
                    }
                }
                else{
                    infos.Add(new TargetInfos{
                            id = id,
                            tr = tr,
                            msg = "Not Found!"
                        });
                }

            }

            if(infos.Count <= 0){
                GUI.backgroundColor = Color.green;
                EditorGUILayout.HelpBox("All curvedTargets validated successfully!", MessageType.None);
            }
            else{
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox("Issues found in curvedTargets", MessageType.Warning);

                foreach(var i in infos){
                    
                    //var idwStyle = GUILayout.Width();
                    if(i.msg == "Not Found!"){
                        GUI.backgroundColor = Color.red;
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(i.id + " : " + i.msg);
                        EditorGUILayout.EndHorizontal();
                    }
                    else{
                        GUI.backgroundColor = Color.yellow;
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(i.id + " : " + i.msg);
                        EditorGUILayout.EndHorizontal();

                        if(GUILayout.Button("Apply All")){
                            if(EditorUtility.DisplayDialog(
                                "Confirm Apply", 
                                "This will overwrite values in the CT_GlobalSettings asset?",
                                "Overwrite",
                                "Cancel"    
                            )){
                                var cp = i.tr.GetComponent<CurvedPlane>();
                                var ct_gs = CT_GlobalSettings.Instance;
                                var ctInfo = ct_gs.curvedTargetInfos.Find(inf=>inf.id == i.id);
                                ctInfo.arc = cp.arc;
                                ctInfo.radMul = cp.bottomRadiusMultiplier;
                                ctInfo.height = cp.height;
                                EditorUtility.SetDirty(ct_gs);
                            }
                        }
                        GUI.backgroundColor = Color.red;
                        if(GUILayout.Button("Revert All")){
                            if(EditorUtility.DisplayDialog(
                                "Confirm Revert", 
                                "Are you sure you want to revert curvedTarget (" + i.id + ") properties to the old values in CT_GlobalSettings asset?",
                                "Revert",
                                "Cancel"    
                            )){
                                var cp = i.tr.GetComponent<CurvedPlane>();
                                var ct_gs = CT_GlobalSettings.Instance;
                                var ctInfo = ct_gs.curvedTargetInfos.Find(inf=>inf.id == i.id);
                                cp.arc = ctInfo.arc;
                                cp.bottomRadiusMultiplier = ctInfo.radMul;
                                cp.height = ctInfo.height;
                                cp.GenerateCylinderMesh();
                                EditorUtility.SetDirty(ct_gs);
                            }  
                        }
                        EditorGUILayout.EndVertical();

                    }

                }
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.white;
    

            GUILayout.Space(20);
        }


        bool showKeyboardCameraControls = false;
        void DrawEditorDebugger(){
            //Editor Runtime Debugger
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Debug Mode");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if(Application.IsPlaying(_target)){
                //Enable Disable
                var imageTargetsProp = serializedObject.FindProperty("curvedTargets");
                var trackedIdsProp = serializedObject.FindProperty("trackedIds");
                var trackedIds = new List<string>();
                if(trackedIdsProp != null){
                    for(var i = 0; i < trackedIdsProp.arraySize; i++){
                        trackedIds.Add(trackedIdsProp.GetArrayElementAtIndex(i).stringValue);
                    }
                }
                
                for(var i = 0; i < imageTargetsProp.arraySize; i++){
                    EditorGUILayout.BeginHorizontal();
                    var imageTargetProp = imageTargetsProp.GetArrayElementAtIndex(i);
                    var id = imageTargetProp.FindPropertyRelative("id").stringValue;
                    EditorGUILayout.LabelField(id);
                    var imageFound = trackedIds.Contains(id);
                    GUI.enabled = !imageFound;
                    if(GUILayout.Button("Found")){
                        _target.SendMessage("OnTrackingFound",id);

                        var imageTargetTransform = ((Transform)imageTargetProp.FindPropertyRelative("transform").objectReferenceValue);
                        var cam = ((ARCamera)serializedObject.FindProperty("trackerCam").objectReferenceValue).transform;

                        cam.transform.position = imageTargetTransform.position + imageTargetTransform.forward * -3;
                        cam.LookAt(imageTargetTransform);
                    }
                    GUI.enabled = imageFound;
                    if(GUILayout.Button("Lost")){
                        _target.SendMessage("OnTrackingLost",id);
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }    

                  
            }
            else{
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Enter Play-mode to Debug In Editor");
                GUI.color = Color.white;
            }

            EditorGUILayout.Space();
            //keyboard camera controls
            showKeyboardCameraControls = EditorGUILayout.Toggle ("Show Keyboard Camera Controls", showKeyboardCameraControls);
            if(showKeyboardCameraControls){
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("W", "Move Forward (Z)");
                EditorGUILayout.LabelField("S", "Move Backward (Z)");
                EditorGUILayout.LabelField("A", "Move Left (X)");
                EditorGUILayout.LabelField("D", "Move Right (X)");
                EditorGUILayout.LabelField("R", "Move Up (Y)");
                EditorGUILayout.LabelField("F", "Move Down (Y)");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Up Arrow", "Tilt Up (along X-Axis)");
                EditorGUILayout.LabelField("Down Arrow", "Tilt Down (along X-Axis)");
                EditorGUILayout.LabelField("Left Arrow", "Tilt Left (along Y-Axis)");
                EditorGUILayout.LabelField("Right Arrow", "Tilt Right (Along Y-Axis)");
                EditorGUILayout.LabelField("Period", "Tilt Clockwise (Along Z-Axis)");
                EditorGUILayout.LabelField("Comma", "Tilt Counter Clockwise (Along Z-Axis)");
                EditorGUILayout.Space(40);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamMoveSensitivity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamTiltSensitivity"));
                EditorGUILayout.EndVertical();
                
            }    

            EditorGUILayout.EndVertical();

            
        }
    }
}

