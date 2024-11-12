Shader "Custom/LargeWhiteSquaresWithPulsingOutline"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1) // White color for the floor
        _OutlineColor ("Outline Color", Color) = (1, 0, 1, 1) // Neon purple for outline
        _Frequency ("Oscillation Frequency", Float) = 1.0
        _OutlineThickness ("Outline Thickness", Float) = 0.02
        _SquareSize ("Square Size", Float) = 1.0 // Controls the total size of each square
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float _Frequency;
            float _SquareSize;
            float _OutlineThickness;
            float4 _MainColor;
            float4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the tiled UV coordinates
                float2 uv = frac(i.uv / _SquareSize);

                // Oscillate brightness of the outline color
                float oscillation = 0.5 + 0.5 * sin(_Time.y * _Frequency);

                // Define the outline area by thickness at the borders of each square
                bool isOutline = (uv.x < _OutlineThickness || uv.x > 1.0 - _OutlineThickness ||
                                  uv.y < _OutlineThickness || uv.y > 1.0 - _OutlineThickness);

                // Set color based on whether in outline or main area
                float4 color = isOutline ? _OutlineColor * oscillation : _MainColor;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
