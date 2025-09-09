using System.Collections.Generic;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Scanning
{
	[System.Flags]
	public enum SheetAsset
	{
		Default = 0,
		ScriptableObject = 1 << 0,
		AnimationClip = 1 << 1,
		AnimatorController = 1 << 2,
		AudioClip = 1 << 3,
		AudioMixer = 1 << 4,
		AvatarMask = 1 << 5,
		Flare = 1 << 6,
		Font = 1 << 7,
		Material = 1 << 8,
		Mesh = 1 << 9,
#if UNITY_6000_0_OR_NEWER
		PhysicsMaterial = 1 << 10,
#else
		PhysicMaterial = 1 << 10,
#endif
		PhysicsMaterial2D = 1 << 11,
		Prefab = 1 << 12,
		Scene = 1 << 13,
		Shader = 1 << 14,
		Sprite = 1 << 15,
		TerrainData = 1 << 16,
		TextAsset = 1 << 17,
		Texture = 1 << 18,
		VideoClip = 1 << 19,
	}

	public static class SheetAssetExtensions
	{
		private static readonly Dictionary<SheetAsset, string> s_SheetAssetToType = new Dictionary<SheetAsset, string>()
		{
			{ SheetAsset.AnimationClip, "UnityEditor.AnimationClip" },
			{ SheetAsset.AnimatorController, "UnityEditor.Animations.AnimatorController" },
			{ SheetAsset.AudioClip, "UnityEditor.AudioClip" },
			{ SheetAsset.AudioMixer, "UnityEditor.Audio.AudioMixerController" },
			{ SheetAsset.AvatarMask, "UnityEngine.AvatarMask" },
			{ SheetAsset.Flare, "UnityEngine.Flare" },
			{ SheetAsset.Font, "UnityEngine.Font" },
			{ SheetAsset.Material, "UnityEngine.Material" },
			{ SheetAsset.Mesh, "UnityEngine.Mesh" },
#if UNITY_6000_0_OR_NEWER
			{ SheetAsset.PhysicsMaterial, "UnityEngine.PhysicsMaterial" },
#else
			{ SheetAsset.PhysicMaterial, "UnityEngine.PhysicMaterial" },
#endif
			{ SheetAsset.PhysicsMaterial2D, "UnityEngine.PhysicsMaterial2D" },
			{ SheetAsset.Prefab, "UnityEngine.GameObject" },
			{ SheetAsset.Scene, "UnityEngine.SceneAsset" },
			{ SheetAsset.ScriptableObject, "UnityEngine.ScriptableObject" },
			{ SheetAsset.Shader, "UnityEngine.Shader" },
			{ SheetAsset.Sprite, "UnityEngine.Sprite" },
			{ SheetAsset.TerrainData, "UnityEngine.TerrainData" },
			{ SheetAsset.TextAsset, "UnityEngine.TextAsset" },
			{ SheetAsset.Texture, "UnityEngine.Texture2D" },
			{ SheetAsset.VideoClip, "UnityEngine.Video.VideoClip" }
		};

		public static string GetDefaultType(this SheetAsset sheetAsset)
		{
			if (s_SheetAssetToType.TryGetValue(sheetAsset, out string type))
			{
				return type;
			}
			else
			{
				Debug.LogWarning($"{nameof(SheetAsset)} '{sheetAsset}' not found in dictionary {nameof(s_SheetAssetToType)}.");
				return string.Empty;
			}
		}
	}
}
