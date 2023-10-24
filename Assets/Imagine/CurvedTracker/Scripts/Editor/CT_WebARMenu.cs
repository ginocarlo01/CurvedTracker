using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Imagine.WebAR.Editor
{
	public class CT_WebARMenu
	{
		[MenuItem("Assets/Imagine WebAR/Create/Curved Target", false, 1010)]
		public static void CreateImageTarget()
		{
			var texture = (Texture2D)Selection.activeObject;

			var texturePath = AssetDatabase.GetAssetPath(texture);
			Debug.Log(texture.name + ": " + texture.width + "x" + texture.height);

			//var savePath = EditorUtility.SaveFilePanel("Save Imagetarget Prefab", texturePath, texture.name, "prefab");
			
			// Debug.Log("save path: " + savePath);
			var id = texture.name;//Path.GetFileNameWithoutExtension(savePath);
			// Debug.Log(savePath);

			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/CurvedTracker/Prefabs/CurvedTarget.prefab");
			var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			go.name = id;
			Selection.activeGameObject = go;

			var mat = new Material(Shader.Find("Imagine/Double-Sided"));
			mat.mainTexture = texture;

			var matDir = Application.dataPath + "/Imagine/CurvedTracker/Curvedtargets/Materials/";
			if (!Directory.Exists(matDir))
            {
				Directory.CreateDirectory(matDir);
            }
			AssetDatabase.CreateAsset(mat, "Assets/Imagine/CurvedTracker/Curvedtargets/Materials/" + id + " Material.mat");
			var cp = go.GetComponent<CurvedPlane>();
			cp.material = mat;
			cp.GenerateCylinderMesh();

			CT_GlobalSettings.Instance.curvedTargetInfos.Add(new CurvedTargetInfo()
			{
				id = id,
				texture = texture,
				arc = 120,
				radMul = 1,
				height = 1
			});
			EditorUtility.SetDirty(CT_GlobalSettings.Instance);


			var tracker = GameObject.FindObjectOfType<CurvedTracker>();
			if (tracker != null)
			{

				var so = new SerializedObject(tracker);
				var sp = so.FindProperty("curvedTargets");

				go.transform.position = new Vector3((sp.arraySize)*2, 0, 0);
				go.transform.parent = tracker.transform;

				sp.arraySize++;
				so.ApplyModifiedProperties();

				var obj = sp.GetArrayElementAtIndex(sp.arraySize - 1);
				obj.FindPropertyRelative("id").stringValue = id;
				obj.FindPropertyRelative("transform").objectReferenceValue = go.transform;

				so.ApplyModifiedProperties();
			}
		}

		[MenuItem("Assets/Imagine WebAR/Create/Curved Target", true)]
		static bool ValidateCreateImageTarget()
		{
			var asset = Selection.activeObject;
			return (asset != null && asset is Texture2D);
		}

		private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
		private static GetWidthAndHeight getWidthAndHeightDelegate;

		public static void GetActualTextureSize(Texture2D texture, ref int width, ref int height)
		{
			if (texture == null)
				throw new System.NullReferenceException();

			var path = AssetDatabase.GetAssetPath(texture);
			if (string.IsNullOrEmpty(path))
				throw new System.Exception("Texture2D is not an asset texture.");

			var importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null)
				throw new System.Exception("Failed to get Texture importer for " + path);


			if (getWidthAndHeightDelegate == null)
			{
				var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
				getWidthAndHeightDelegate = System.Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method) as GetWidthAndHeight;
			}

			getWidthAndHeightDelegate(importer, ref width, ref height);
		}

		[MenuItem("Assets/Imagine WebAR/Create/Curved Tracker", false, 1100)]
		public static void CreateImageTracker()
		{
			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/CurvedTracker/Prefabs/CurvedTracker.prefab");
			GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			Selection.activeGameObject = gameObject;
			gameObject.name = "CurvedTracker";
		}

		[MenuItem("Assets/Imagine WebAR/Create/AR Camera", false, 1101)]
		public static void CreateImageTrackerCamera()
		{
			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/CurvedTracker/Prefabs/ARCamera.prefab");
			GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			Selection.activeGameObject = gameObject;
			gameObject.name = "ARCamera";
		}

		[MenuItem("Assets/Imagine WebAR/Update Plugin to URP", false, 1200)]
		public static void SetURP()
		{
			if (EditorUtility.DisplayDialog(
				"Update Imagine WebAR Plugin to URP",
				"Please make sure that the Universal RP package is already installed before doing this step.",
				"Proceed",
				"Cancel"))
			{
				string[] files = Directory.GetFiles(Application.dataPath + "/Imagine/CurvedTracker/Demos/Materials", "*.mat", SearchOption.TopDirectoryOnly);
				foreach (var file in files)
				{
					var path = file.Replace(Application.dataPath, "Assets");
					var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
					if (mat.shader.name == "Standard")
					{
						mat.shader = Shader.Find("Universal Render Pipeline/Lit");
					}
					else if (mat.shader.name == "Imagine/ARShadow")
                    {
						mat.shader = Shader.Find("Imagine/ARShadowURP");
					}
				}

				AddDefineSymbol("IMAGINE_URP");
				EditorUtility.DisplayDialog("Completed", "Imagine WebAR Plugin is now set to URP. \n\nSome URP features such as HDR and Post-Processing may be partially/fully unsupported.", "Ok");
			}
		}


		[MenuItem("Assets/Imagine WebAR/Roll-back Plugin to Built-In RP", false, 1201)]
		public static void SetBuiltInRP ()
		{
			if (EditorUtility.DisplayDialog(
				"Roll-back Imagine WebAR Plugin to Built-In RP",
				"Plese confirm.",
				"Proceed",
				"Cancel"))
			{
				string[] files = Directory.GetFiles(Application.dataPath + "/Imagine/CurvedTracker/Demos/Materials", "*.mat", SearchOption.TopDirectoryOnly);
				foreach (var file in files)
				{
					var path = file.Replace(Application.dataPath, "Assets");
					var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
					if (mat.shader.name == "Universal Render Pipeline/Lit" || mat.shader.name == "Hidden/InternalErrorShader")
					{
						mat.shader = Shader.Find("Standard");
					}
					else if (mat.shader.name == "Imagine/ARShadowURP")
					{
						mat.shader = Shader.Find("Imagine/ARShadow");
					}
				}

				RemoveDefineSymbol("IMAGINE_URP");

				EditorUtility.DisplayDialog("Completed", "Imagine WebAR Plugin is now set to Built-In RP. Some edited materials may still require manual shader change","Ok");

			}
		}

		public static void AddDefineSymbol(string symbol)
		{
			string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> allDefines = definesString.Split(';').ToList();
			if (!allDefines.Contains(symbol))
				allDefines.Add(symbol);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(
				 EditorUserBuildSettings.selectedBuildTargetGroup,
				 string.Join(";", allDefines.ToArray()));
			AssetDatabase.RefreshSettings();
		}

		public static void RemoveDefineSymbol(string symbol)
		{
			string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> allDefines = definesString.Split(';').ToList();
			allDefines.RemoveAll(s => s == symbol);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(
				 EditorUserBuildSettings.selectedBuildTargetGroup,
				 string.Join(";", allDefines.ToArray()));
			AssetDatabase.RefreshSettings();

		}
	}
}

