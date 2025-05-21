Shader "UI/BlurEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurStrength ("Blur Strength", Range(0, 10)) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _BlurStrength;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

half4 frag(v2f i) : COLOR
{
half4 color = tex2D(_MainTex, i.uv);
    float weightTotal = 1.0;

    for (float range = 0.1f; range <= _BlurStrength; range += 0.1f)
    {
        half4 sampleColor = tex2D(_MainTex, i.uv + range);
        color += sampleColor * 0.8;
        weightTotal += 0.8;
    }

    color.a = tex2D(_MainTex, i.uv).a; // Preserve transparency
    return color / weightTotal;

}

            ENDCG
        }
    }
}