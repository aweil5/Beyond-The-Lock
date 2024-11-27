Shader "Custom/GlitchShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.5
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GlitchIntensity;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 glitchUV = i.uv;
                glitchUV.x += (frac(sin(dot(i.uv.xy ,float2(12.9898,78.233))) * 43758.5453) - 0.5) * _GlitchIntensity;
                glitchUV.y += (frac(sin(dot(i.uv.xy ,float2(63.7264,10.873))) * 24652.3453) - 0.5) * _GlitchIntensity;

                fixed4 col = tex2D(_MainTex, glitchUV);
                col.rgb += _GlitchIntensity * float3(0.1, -0.1, 0.1); // Color shift
                col.a *= (1.0 - _GlitchIntensity); // Adjust transparency
                return col;
            }
            ENDCG
        }
    }
}
