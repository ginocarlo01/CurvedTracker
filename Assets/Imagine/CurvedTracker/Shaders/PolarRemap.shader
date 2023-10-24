Shader "Custom/PolarRemap" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _W ("Width", float) = 100
        _H ("Height", float) = 100
        _R1 ("Top Radius", float) = 50
        _R2 ("Bottom Radius", float) = 0
        _Angle ("Angle", Range(0.001, 360)) = 10
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _W;
            float _H;
            float _R1;
            float _R2;
            float _Angle;



            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target {

                float u = i.uv.x;
                float v = i.uv.y;


                //map sector to full circle
                float a = _Angle > 180 ? 180 : _Angle;
                u = -1 * (0.5 - u) / sin(a * 3.14159 / 180 / 2) + 0.5;

                float rf = _R2/_R1;
                //v = 1 - ((1-v) / (0.5 - 0.5 * rf * cos(_Angle * 3.14159 / 180 / 2)));
                if(_Angle < 180){
                    v = 1 - ((1-v) / (0.5 - 0.5 * rf * cos(_Angle * 3.14159 / 180 / 2)));
                }
                else{
                    v = 1 - ((1-v) / (0.5 - 0.5 * cos(_Angle * 3.14159 / 180 / 2)));
                }

                half4 col = tex2D(_MainTex, float2(u, v));

                return col;
            }
            ENDCG
        }
    }
}
