Shader "Unlit/TileSquareShader"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_EffectSlope ("Effect Slope", Vector) = (1, 1, 1, 1)
		_EffectSpeed("Effect Speed", Float) = 1
		_EffectDip("Effect Dip", Float) = 0.1
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			float2 _EffectSlope;
			float _EffectSpeed;
			float _EffectDip;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 finalCol = _Color;
				finalCol = finalCol * ((1 - _EffectDip) + sin(_Time * _EffectSpeed + i.vertex.x * _EffectSlope.x + i.vertex.y * _EffectSlope.y) * _EffectDip);
				
				return finalCol;
			}
			ENDCG
		}
	}
}
