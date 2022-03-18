Shader "Custom/InnerSphericalMaskShader"
{
    Properties
    {
        _Colour ("Colour", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB)", 2D) = "white" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _MaskRadius("Mask Radius", Range(0, 100)) = 5
        _MaskSoftness("Mask Softness", Range(0, 100)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows 
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Colour;

        float _Glossiness;
        float _Metallic;

        float _MaskRadius;
        float _MaskSoftness;

        uniform float4 _PlayerPos;

        void surf (Input IN, inout SurfaceOutputStandard output)
        {
            fixed4 colourFloat = tex2D (_MainTex, IN.uv_MainTex) * _Colour;

            float4 transparentColour = (0, 0, 0, 0);

            // Compute the distance between the player position and the vertices of the dissolving object
            float distanceFloat = distance(_PlayerPos, IN.worldPos);

            // Compute the effect amount based on the distance of the player relative to the _MaskRadius and compared to _MaskSoftness percentage-wise
            float effectAmount = saturate ((distanceFloat - _MaskRadius) / -_MaskSoftness);

            clip(effectAmount - 1);

            // Lerp between gray texture to the color texture based on the dissolve amount
            float4 lerpColor = lerp (transparentColour, colourFloat, effectAmount);

            float4 lerpTransparency = lerp (0, 1, effectAmount);

            output.Albedo = lerpColor.rgb;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
            output.Alpha = lerpTransparency;
        }
        ENDCG
    }
    FallBack "Diffuse" // For shadows
}
