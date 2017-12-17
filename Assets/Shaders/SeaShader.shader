// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SeaShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {} 
	}
	SubShader {
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
		LOD 200

		Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog
			
		#include "UnityCG.cginc"
		

		fixed4 _Color;
		float4 _LightColor0;


		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			//UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
			half4 color : COLOR;
		};

			
		v2f vert (appdata v)
		{
			v.vertex.y = v.vertex.y + sin(_Time * 30 + v.vertex.x) * 1;
			
			v2f o;

			float4 normal = float4(v.normal, 0.0);
				float3 n = normalize(mul(normal, unity_WorldToObject));
				float3 l = normalize(_WorldSpaceLightPos0);
 
				float3 NdotL = max(0.0, dot(n, l));
 
				float3 d = NdotL * _Color * _LightColor0;
				o.color = float4(d, 1.0);
				o.vertex = UnityObjectToClipPos(v.vertex);

			
			//UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			return i.color;
		}
		ENDCG
		}
	}
	FallBack "Diffuse"
}
