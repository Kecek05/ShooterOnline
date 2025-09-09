using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.Samples.ComponentExplorer
{
	public class ComponentExplorerGenerator
	{
		[MenuItem("Assets/Create/Scriptable Sheets/Component Explorer")]
		private static void CreatePrefabWithUnityEngineComponents()
		{
			var unityEngineTypes = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("UnityEngine")).SelectMany(a => a.GetTypes())
				.Where(t => typeof(Component).IsAssignableFrom(t)
							&& !t.IsAbstract
							&& t != typeof(Cloth)
							&& t != typeof(NavMeshObstacle)
							&& t != typeof(SkinnedMeshRenderer)
							&& t != typeof(Transform)
							&& t != typeof(VideoPlayer)
							&& !typeof(MonoBehaviour).IsAssignableFrom(t)
							&& !t.Name.Contains("NetworkView")
							&& t.GetConstructor(Type.EmptyTypes) != null).ToArray();

			var obj = new GameObject("ComponentExplorer");
			foreach (var type in unityEngineTypes)
			{
				try
				{
					obj.AddComponent(type);
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"Could not add {type.Name}\n{ex.Message}.");
				}
			}

			PrefabUtility.SaveAsPrefabAsset(obj, "Assets/ComponentExplorer.prefab");

			Object.DestroyImmediate(obj);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
