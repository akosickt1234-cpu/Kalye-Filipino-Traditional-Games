Shader "Kalye/ui_frame_spin_light"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _LightColor ("Light Color", Color) = (1,0.96,0.8,1)
        _Intensity ("Light Intensity", Range(0, 2)) = 0.9
        _WhiteMix ("White Mix", Range(0, 1)) = 0.12
        _GlowGain ("Glow Gain", Range(1, 3)) = 1.4
        _Power ("Beam Sharpness", Range(1, 16)) = 5
        _Echo ("Opposite Beam", Range(0, 1)) = 0.4
        _AlphaMix ("Alpha Mix", Range(0, 1)) = 0
        _AlphaFloor ("Alpha Floor", Range(0, 1)) = 0.8
        _Angle ("Angle", Float) = 0
        _Size ("Size", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 local_pos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _LightColor;
            float _Intensity;
            float _WhiteMix;
            float _GlowGain;
            float _Power;
            float _Echo;
            float _AlphaMix;
            float _AlphaFloor;
            float _Angle;
            float4 _Size;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                o.local_pos = v.vertex.xy / max(_Size.xy, 0.0001);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                float a = atan2(i.local_pos.y, i.local_pos.x) - _Angle;
                float main_beam = pow(saturate(cos(a) * 0.5 + 0.5), _Power);
                float echo_beam = pow(saturate(cos(a + 3.14159265) * 0.5 + 0.5), _Power) * _Echo;
                float beam = saturate(main_beam + echo_beam);

                float3 lit = col.rgb * (1.0 + beam * _Intensity) + _LightColor.rgb * beam * _Intensity * _WhiteMix;
                col.rgb = lerp(saturate(lit), col.rgb, _AlphaMix);
                col.a = saturate(col.a * lerp(1.0, lerp(_AlphaFloor, _GlowGain, beam), _AlphaMix));
                return col;
            }
            ENDCG
        }
    }
}
