Shader "Custom/RippleSurfaceShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _RippleSpeed ("Ripple Speed", Float) = 1.0
        _RippleAmplitude ("Ripple Amplitude", Float) = 0.05
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float _RippleSpeed;
        float _RippleAmplitude;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Calculate the ripple effect
            float2 uv = IN.uv_MainTex;
            float ripple = sin(_Time.y * _RippleSpeed + (uv.x + uv.y) * 20.0) * _RippleAmplitude;
            uv.y += ripple;

            fixed4 c = tex2D(_MainTex, uv);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
