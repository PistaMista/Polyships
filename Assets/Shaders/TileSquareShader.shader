Shader "Unlit/TileSquareShader"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_EffectSlope ("Effect Slope", Vector) = (1, 1, 1, 1)
		_EffectScale("Effect Scale", Float) = 1
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
			float _EffectScale;
			
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
				finalCol = finalCol * (0.9 + sin(_Time * _EffectScale + i.vertex.x * _EffectSlope.x + i.vertex.y * _EffectSlope.y) / 10.0);
				
				return finalCol;
			}
			ENDCG
		}
	}
}
