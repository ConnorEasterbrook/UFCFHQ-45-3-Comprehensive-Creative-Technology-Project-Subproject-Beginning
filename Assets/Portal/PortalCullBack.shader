Shader "Unlit/PortalCullBack"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100
		ztest Less
		cull back
		Offset -1, -1

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct Vertex2Fragment
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};

			sampler2D _MainTex;
			
			Vertex2Fragment vert (appdata v)
			{
				Vertex2Fragment vertex2Fragment;
				vertex2Fragment.vertex = UnityObjectToClipPos(v.vertex);
				vertex2Fragment.screenPos = ComputeScreenPos(vertex2Fragment.vertex);
				return vertex2Fragment;
			}
			
			fixed4 frag (Vertex2Fragment i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				fixed4 col = tex2D(_MainTex, uv);
                return col;
			}
			ENDCG
		}
	}
}
