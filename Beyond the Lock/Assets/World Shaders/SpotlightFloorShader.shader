Shader "Custom/SpotlightFloorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FloorColor ("Floor Color", Color) = (0.1, 0.1, 0.2, 1)
        _CustomSpecColor ("Specular Color", Color) = (1,1,1,1) // Renamed variable
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _BumpMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        // Use the Unity Standard Specular Lighting model
        #pragma surface surf StandardSpecular keepalpha noshadow noambient nodynlightmap novertexlights nolightmap nodirlightmap nofog

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _FloorColor;
        fixed4 _CustomSpecColor; // Renamed variable
        half _Glossiness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Sample the textures
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            // Set albedo to the floor color
            o.Albedo = _FloorColor.rgb;

            // Set the specular color and smoothness
            o.Specular = _CustomSpecColor.rgb; // Use the renamed variable
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
