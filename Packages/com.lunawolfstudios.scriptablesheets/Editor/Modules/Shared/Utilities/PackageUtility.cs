using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using PackageSource = UnityEditor.PackageManager.PackageSource;

namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	public static class PackageUtility
	{
		/// <summary>
		/// Returns true if the provided Object is part of an immutable Package.
		/// </summary>
		public static bool IsAssetImmutable(Object asset)
		{
			return IsAssetImmutable(AssetDatabase.GetAssetPath(asset));
		}

		/// <summary>
		/// Returns true if the provided asset path is part of an immutable Package.
		/// </summary>
		public static bool IsAssetImmutable(string assetPath)
		{
			var assetPackage = PackageInfo.FindForAssetPath(assetPath);
			return assetPackage != null && assetPackage.source != PackageSource.Embedded && assetPackage.source != PackageSource.Local;
		}
	}
}
