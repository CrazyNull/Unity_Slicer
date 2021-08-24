Shader "Sliceable/Fragment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _SpecularColor ("Specular Color", Color) = (0,0,0,1)
        _Gloss ("Gloss",Range(0.5,10.0)) = 1.0

        _PlaneTex ("Texture", 2D) = "black" {}

        _InsideColor ("Inside Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : TEXCOORD1;
                float4 pos : TEXCOORD2;
            };

            float4 _InsideColor;

            sampler2D _MainTex;
            sampler2D _PlaneTex;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.pos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                for(int j = 0; j < 10 ; j++)
                {
                    float3 p = tex2Dlod(_PlaneTex, fixed4(0.25,j * 0.1 + 0.05,0,0));
                    float3 n = tex2Dlod(_PlaneTex, fixed4(0.75,j * 0.1 + 0.05,0,0));
                    float result = dot(n, i.pos) - dot(n,p);
                    if (result > 0.0001) discard;
                }
                return _InsideColor;
            }
            ENDCG
        }

        Pass
        {
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc" 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                fixed4 color : TEXCOORD3;
                float3 pos : TEXCOORD5;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _SpecularColor;
            float _Gloss;

            sampler2D _PlaneTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld,v.normal));
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                
                o.color = v.color;
                o.pos = v.vertex;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                for(int j = 0; j < 10 ; j++)
                {
                    float3 p = tex2Dlod(_PlaneTex, fixed4(0.25,j * 0.1 + 0.05,0,0));
                    float3 n = tex2Dlod(_PlaneTex, fixed4(0.75,j * 0.1 + 0.05,0,0));
                    float result = dot(n, i.pos) - dot(n,p);
                    if (result > 0.0001) discard;
                }
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed3 worldNormal = normalize(i.worldNormal);
                fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 diffuse = _LightColor0.rbg * col.rbg * saturate(dot(worldNormal,worldLightDir)) * 0.5 + 0.5;

                fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                fixed3 halfDir = normalize(viewDir + worldLightDir);
                fixed3 specular = _LightColor0.rgb * _SpecularColor.rgb * pow(max(0,dot(halfDir,worldNormal)),_Gloss);

                col.rgb = diffuse * _Color + specular.rgb;
                return col;
            }
            ENDCG
        }


    }

    Fallback "Mobile/Diffuse"
}
