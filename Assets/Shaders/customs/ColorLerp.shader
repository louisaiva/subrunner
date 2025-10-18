// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// modified to create a color lerp as described in https://gamedev.stackexchange.com/questions/205192/how-to-briefly-turn-an-entire-sprite-another-color-in-unity

Shader "Sprites/ColorLerp"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment ColorLerpFrag // Use our version instead of the default.
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"


            fixed4 ColorLerpFrag(v2f IN) : SV_Target
            {
                // Sample sprite texture.
                fixed4 c = SampleSpriteTexture (IN.texcoord);
                // Blend toward sprite component color depending on its alpha.
                c.rgb = lerp(IN.color.rgb, c.rgb, IN.color.a);
                // Pre-multiply alpha to work with the blending function.
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}