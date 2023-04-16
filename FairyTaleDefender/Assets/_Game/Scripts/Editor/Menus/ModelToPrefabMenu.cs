using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BoundfoxStudios.FairyTaleDefender.Editor.Menus
{
	public static class ModelToPrefabMenu
	{
		private static readonly AssetLocator<GameObject> AssetLocator =
			new("Prefabs/Environment/EnvironmentAsset_Base.prefab");

		[MenuItem("Assets/" + Constants.MenuNames.MenuName + "/Create Prefab", priority = 0)]
		private static void ConvertToPrefab()
		{
			AssetLocator.SafeInvokeAsync(environmentAssetPrefab =>
					ConvertToPrefab(GetFilteredAssets(), environmentAssetPrefab))
				.Forget();
		}

		[MenuItem("Assets/" + Constants.MenuNames.MenuName + "/Create Prefab", true)]
		private static bool ConvertToPrefabValidate()
		{
			if (Selection.count == 0)
			{
				return false;
			}

			return GetFilteredAssets().Length > 0;
		}

		private static GameObject[] GetFilteredAssets()
		{
			var filteredSelection =
				Selection.GetFiltered<GameObject>(SelectionMode.Assets)
					.Where(EditorUtility.IsPersistent)
					.Where(selection =>
					{
						var assetPath = AssetDatabase.GetAssetPath(selection);
						return assetPath.Contains(".fbx");
					})
					.ToArray();
			return filteredSelection;
		}

		private static void ConvertToPrefab(GameObject[] gameObjects, GameObject basePrefab)
		{
			foreach (var gameObject in gameObjects)
			{
				ConvertToPrefab(gameObject, basePrefab);
			}
		}

		private static void ConvertToPrefab(GameObject gameObject, GameObject basePrefab)
		{
			var originalAssetPath = AssetDatabase.GetAssetPath(gameObject);
			var originalAssetFolder = Path.GetDirectoryName(originalAssetPath)!;
			var prefabAssetPath = Path.Combine(originalAssetFolder, $"{gameObject.name}.prefab");
			prefabAssetPath = AssetDatabase.GenerateUniqueAssetPath(prefabAssetPath);

			var basePrefabVariant = (GameObject) PrefabUtility.InstantiatePrefab(basePrefab);

			var gfxChild = basePrefabVariant.transform.Find("GFX");

			if (!gfxChild)
			{
				Debug.LogError(
					$"Can not create a prefab from {gameObject.name}. No child named \"GFX\" found in {basePrefab.name}", basePrefab);
				return;
			}

			var staticFlags = GameObjectUtility.GetStaticEditorFlags(gfxChild.gameObject);

			var gameObjectInstance = (GameObject) PrefabUtility.InstantiatePrefab(gameObject, gfxChild);
			GameObjectUtility.SetStaticEditorFlags(gameObjectInstance, staticFlags);

			var savedPrefab = PrefabUtility.SaveAsPrefabAsset(basePrefabVariant, prefabAssetPath, out var success);
			GameObjectUtility.SetStaticEditorFlags(savedPrefab, staticFlags);

			if (!success)
			{
				Debug.Log($"Creating a prefab from {gameObject.name} was not successful", gameObject);
			}

			if (basePrefabVariant)
			{
				Object.DestroyImmediate(basePrefabVariant);
			}
		}
	}
}