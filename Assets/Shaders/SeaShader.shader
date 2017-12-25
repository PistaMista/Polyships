// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SeaShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
		_MaxElevation("Max Elevation", Float) = 10
		_Speed("Speed", Float) = 1
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
		
		sampler2D _Noise;
		fixed4 _Color;
		float4 _LightColor0;
		float _MaxElevation;
		float _Speed;


		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			//UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
			half4 color : COLOR;
		};

			
		v2f vert (appdata v)
		{
			//float sine = (sin(_Time * 12 + v.vertex.x) + 1) / 2.0;
			float sine = tex2Dlod(_Noise, v.vertex);
			v.vertex.y = v.vertex.y + sine * _MaxElevation;
			
			 v2f o;

			// float4 normal = float4(v.normal, 0.0);
			// float3 n = normalize(mul(normal, unity_WorldToObject));
			// float3 l = normalize(_WorldSpaceLightPos0);
			
			// float3 NdotL = max(0.0, dot(n, l));
			
 
			// float3 d = NdotL * _Color * _LightColor0;
			o.color = _Color * (0.7 + sine * 0.3);
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
