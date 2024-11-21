Shader "Custom/LargeWhiteSquaresWithPulsingOutline_Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Dummy texture to access UVs
        _MainColor ("Main Color", Color) = (1, 1, 1, 1) // White color for the floor
        _OutlineColor ("Outline Color", Color) = (1, 0, 1, 1) // Neon purple for outline
        _Frequency ("Oscillation Frequency", Float) = 1.0
        _OutlineThickness ("Outline Thickness", Float) = 0.02
        _SquareSize ("Square Size", Float) = 1.0 // Controls the total size of each square
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Use Surface Shader with Standard lighting model
        #pragma surface surf Standard fullforwardshadows

        // Include necessary Unity shader files
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex; // UV coordinates from the mesh
        };

        // Shader properties
        float _Frequency;
        float _SquareSize;
        float _OutlineThickness;
        fixed4 _MainColor;
        fixed4 _OutlineColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate the tiled UV coordinates
            float2 uv = frac(IN.uv_MainTex / _SquareSize);

            // Oscillate brightness of the outline color
            float oscillation = 0.5 + 0.5 * sin(_Time.y * _Frequency);

            // Define the outline area by thickness at the borders of each square
            bool isOutline = (uv.x < _OutlineThickness || uv.x > 1.0 - _OutlineThickness ||
                              uv.y < _OutlineThickness || uv.y > 1.0 - _OutlineThickness);

            // Set color based on whether in outline or main area
            fixed4 color = isOutline ? _OutlineColor * oscillation : _MainColor;

            // Assign the color to the shader outputs
            o.Albedo = color.rgb; // Base color of the surface
            o.Alpha = color.a;    // Alpha value (transparency)
        }
        ENDCG
    }
    FallBack "Diffuse"
}
