Shader "Kalye/ui_shine_sweep"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineIntensity ("Shine Intensity", Range(0, 1)) = 0.55
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.08
        _ShineSlant ("Shine Slant", Range(-1, 1)) = 0.35
        _SweepFrom ("Sweep From", Float) = 0.2
        _SweepTo ("Sweep To", Float) = 1.15
        _SweepDuration ("Sweep Duration", Float) = 0.9
        _SweepInterval ("Sweep Interval", Float) = 3.5
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
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float _ShineIntensity;
            float _ShineWidth;
            float _ShineSlant;
            float _SweepFrom;
            float _SweepTo;
            float _SweepDuration;
            float _SweepInterval;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                float cycle = fmod(_Time.y, _SweepInterval);
                float is_sweeping = step(cycle, _SweepDuration);
                float progress = saturate(cycle / _SweepDuration);
                float position = lerp(_SweepFrom, _SweepTo, progress);
                float coord = i.texcoord.x + i.texcoord.y * _ShineSlant;
                float band = 1.0 - smoothstep(0.0, _ShineWidth, abs(coord - position));

                col.rgb = saturate(col.rgb + _ShineColor.rgb * band * _ShineIntensity * is_sweeping);
                return col;
            }
            ENDCG
        }
    }
}
