
namespace KeceK.Utils
{
    /// <summary>
    /// All shader properties used in the game.
    /// </summary>
    public enum ShaderProperty
    {
        _ShineLocation,
        _FadeAmount,
        _HologramBlend,
        _GreyscaleBlend,
    }
    
    public static class UtilsK
    {
        public static string GetShaderProperty(ShaderProperty shaderProperty) => shaderProperty.ToString();
    }
}
