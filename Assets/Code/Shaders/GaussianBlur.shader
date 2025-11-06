Shader "UI/GaussianBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
        
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
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
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
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            
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
            
            // 9-tap Gaussian kernel weights
            static const float weights[9] = {
                0.0162162162,
                0.0540540541,
                0.1216216216,
                0.1945945946,
                0.2270270270,
                0.1945945946,
                0.1216216216,
                0.0540540541,
                0.0162162162
            };
            
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                
                // Horizontal pass
                fixed4 col = fixed4(0, 0, 0, 0);
                for (int i = -4; i <= 4; i++)
                {
                    float2 offset = float2(texelSize.x * i, 0);
                    col += tex2D(_MainTex, IN.texcoord + offset) * weights[i + 4];
                }
                
                // Vertical pass
                fixed4 finalCol = fixed4(0, 0, 0, 0);
                for (int j = -4; j <= 4; j++)
                {
                    float2 offset = float2(0, texelSize.y * j);
                    finalCol += tex2D(_MainTex, IN.texcoord + offset) * weights[j + 4];
                }
                
                // Average horizontal and vertical for single-pass approximation
                col = (col + finalCol) * 0.5;
                
                col *= IN.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}