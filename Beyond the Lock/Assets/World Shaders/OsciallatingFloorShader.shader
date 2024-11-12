Shader "Custom/OscillatingFloorShader"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1) // White
        _SquareColor ("Square Color", Color) = (1, 0, 1, 1) // Neon purple
        _Frequency ("Oscillation Frequency", Float) = 1.0
        _SquareSize ("Square Size", Float) = 0.2
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
            float4 _MainColor;
            float4 _SquareColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get the fractional part of the UVs for tiling
                float2 uv = frac(i.uv / _SquareSize);

                // Calculate brightness oscillation
                float oscillation = 0.5 + 0.5 * sin(_Time.y * _Frequency);
                float4 color = _MainColor;

                // Check if within square and apply oscillating color
                if (uv.x < 0.5 && uv.y < 0.5)
                {
                    color = lerp(_MainColor, _SquareColor, oscillation);
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
