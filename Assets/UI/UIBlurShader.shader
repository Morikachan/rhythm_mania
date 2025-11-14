Shader "UI/UIBlurShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        // New property for blur intensity
        _BlurSize ("Blur Size", Range(0, 5)) = 0.5
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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _BlurSize; // Our blur intensity

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                fixed4 col = fixed4(0,0,0,0);
                
                // Simple Box Blur (can be optimized or improved for better quality)
                float pixelSizeX = _BlurSize / _ScreenParams.x; // Adjust for screen resolution
                float pixelSizeY = _BlurSize / _ScreenParams.y; // Adjust for screen resolution

                // --- Updated 5x5 Box Blur ---
                for (int x = -2; x <= 2; x++) // Range changed from -1 to 1 to -2 to 2
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        // Sample the texture at the offset UV
                        col += tex2D(_MainTex, uv + float2(x * pixelSizeX, y * pixelSizeY));
                    }
                }
                col /= 25.0; // Dividing by 5*5 = 25 total samples
                // --------------------------

                col *= i.color;
                return col;
            }
            ENDCG
        }
    }
}
