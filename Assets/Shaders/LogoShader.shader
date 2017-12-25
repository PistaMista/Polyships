
Shader "UI/Logo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_EffectColor("Effect Color", Color) = (1, 1, 1, 1)
		_EffectSpeed("Effect Speed", Float) = 1
        _Frequency("Effect Frequency", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
			fixed4 _EffectColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
			float _EffectSpeed;
            float _Frequency;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 initialColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 finalColor = initialColor;
				//float colorMod = (1 - ((_Time * _EffectSpeed) % 2 + IN.texcoord.x ));
                float colorMod = sin(IN.texcoord.x * _Frequency + _Time * _EffectSpeed - IN.texcoord.y);
				finalColor.r += _EffectColor.r * colorMod;
				finalColor.g += _EffectColor.g * colorMod;
				finalColor.b += _EffectColor.b * colorMod;

                //color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				//color.a += UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) * 0.5;
				
				finalColor.a = initialColor.a;

                // #ifdef UNITY_UI_ALPHACLIP
                // clip (color.a - 0.001);
                // #endif

                return finalColor;
            }
        ENDCG
        }
    }
}
