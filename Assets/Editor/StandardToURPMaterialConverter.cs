#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace KeceK.Editor
{
    public class StandardToURPMaterialConverter : EditorWindow
    {
        [MenuItem("KeceK/Convert to URP Lit", false, 20)]
        static void ConvertToURPLit()
        {
            var selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets);

            if (selectedMaterials.Length == 0)
            {
                EditorUtility.DisplayDialog("Material Converter", "Please select at least one material to convert.",
                    "OK");
                return;
            }

            int convertedCount = 0;
            foreach (var material in selectedMaterials)
            {
                if (material.shader.name == "Standard")
                {
                    ConvertStandardToURPLit(material);
                    convertedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Material Converter", $"Converted {convertedCount} materials to URP Lit.",
                "OK");
        }

        private static void ConvertStandardToURPLit(Material standardMaterial)
{
    // Backup original values before changing the shader
    Color color = standardMaterial.HasProperty("_Color") ? standardMaterial.GetColor("_Color") : Color.white;
    Texture mainTex = standardMaterial.HasProperty("_MainTex") ? standardMaterial.GetTexture("_MainTex") : null;
    Vector2 mainTexOffset = standardMaterial.HasProperty("_MainTex") ? standardMaterial.GetTextureOffset("_MainTex") : Vector2.zero;
    Vector2 mainTexScale = standardMaterial.HasProperty("_MainTex") ? standardMaterial.GetTextureScale("_MainTex") : Vector2.one;
    
    float smoothness = standardMaterial.HasProperty("_Glossiness") ? standardMaterial.GetFloat("_Glossiness") : 0.5f;
    float metallic = standardMaterial.HasProperty("_Metallic") ? standardMaterial.GetFloat("_Metallic") : 0.0f;
    
    Texture bumpMap = standardMaterial.HasProperty("_BumpMap") ? standardMaterial.GetTexture("_BumpMap") : null;
    float bumpScale = standardMaterial.HasProperty("_BumpScale") ? standardMaterial.GetFloat("_BumpScale") : 1.0f;
    
    Texture occlusionMap = standardMaterial.HasProperty("_OcclusionMap") ? standardMaterial.GetTexture("_OcclusionMap") : null;
    float occlusionStrength = standardMaterial.HasProperty("_OcclusionStrength") ? standardMaterial.GetFloat("_OcclusionStrength") : 1.0f;
    
    Texture emissionMap = standardMaterial.HasProperty("_EmissionMap") ? standardMaterial.GetTexture("_EmissionMap") : null;
    Color emissionColor = standardMaterial.HasProperty("_EmissionColor") ? standardMaterial.GetColor("_EmissionColor") : Color.black;
    bool isEmissionEnabled = standardMaterial.HasProperty("_EmissionColor") && !standardMaterial.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.EmissiveIsBlack);
    
    Texture metallicGlossMap = standardMaterial.HasProperty("_MetallicGlossMap") ? standardMaterial.GetTexture("_MetallicGlossMap") : null;
    
    // Handle transparency
    bool isTransparent = standardMaterial.HasProperty("_Mode") && standardMaterial.GetFloat("_Mode") > 0.5f;

    // Change shader to URP Lit
    standardMaterial.shader = Shader.Find("Universal Render Pipeline/Lit");

    // Apply values to URP shader properties
    standardMaterial.SetColor("_BaseColor", color);
    if (mainTex != null)
    {
        standardMaterial.SetTexture("_BaseMap", mainTex);
        standardMaterial.SetTextureOffset("_BaseMap", mainTexOffset);
        standardMaterial.SetTextureScale("_BaseMap", mainTexScale);
    }
    
    standardMaterial.SetFloat("_Smoothness", smoothness);
    standardMaterial.SetFloat("_Metallic", metallic);
    
    if (bumpMap != null)
    {
        standardMaterial.SetTexture("_BumpMap", bumpMap);
        standardMaterial.SetFloat("_BumpScale", bumpScale);
        standardMaterial.EnableKeyword("_NORMALMAP");
    }
    
    if (occlusionMap != null)
    {
        standardMaterial.SetTexture("_OcclusionMap", occlusionMap);
        standardMaterial.SetFloat("_OcclusionStrength", occlusionStrength);
    }
    
    if (emissionMap != null || emissionColor != Color.black)
    {
        if (emissionMap != null)
            standardMaterial.SetTexture("_EmissionMap", emissionMap);
        
        standardMaterial.SetColor("_EmissionColor", emissionColor);
        
        if (isEmissionEnabled)
        {
            standardMaterial.EnableKeyword("_EMISSION");
            standardMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }
    
    if (metallicGlossMap != null)
    {
        standardMaterial.SetTexture("_MetallicGlossMap", metallicGlossMap);
        standardMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
    }

    // Handle transparency settings
    if (isTransparent)
    {
        standardMaterial.SetFloat("_Surface", 1); // 1 = transparent
        standardMaterial.SetFloat("_Blend", 0);   // 0 = alpha blend
        standardMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        standardMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        standardMaterial.SetFloat("_ZWrite", 0);
        standardMaterial.renderQueue = (int)RenderQueue.Transparent;
        standardMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        standardMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }
    else
    {
        standardMaterial.SetFloat("_Surface", 0); // 0 = opaque
        standardMaterial.SetFloat("_SrcBlend", (float)BlendMode.One);
        standardMaterial.SetFloat("_DstBlend", (float)BlendMode.Zero);
        standardMaterial.SetFloat("_ZWrite", 1);
        standardMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        standardMaterial.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }
}
    }
}

#endif
