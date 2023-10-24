Shader "Custom/PolarToRectWarp" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Rf ("Radius Multiplier", Range(0,1)) = 0.5
        _Angle ("Angle", Range(0.001, 360)) = 10
        /*[MaterialToggle] _ShowRemap ("Show Remapped", float) = 0
        _TestUF ("Test UF",float) = 1
        _TestVF ("Test UF",float) = 1*/

    }

    SubShader {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

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

            float _Rf;
            float _Angle;
            //float _ShowRemap;
            //float _TestUF;
            //float _TestVF;


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

                //u = -1*(0.5 - u)*_TestUF - 0.5;
                //v = 1 - (1 - v) * _TestVF;


                // Convert polar coordinates (UV) to rectangular coordinates (XY)
                float offset = -0.5 * _Angle + 90;
                float angle = (u * _Angle + offset) * (3.14159 / 180); // Map U to an angle between 0 and 2*pi
                float radius = _Rf + v * (1 - _Rf);

                // Calculate rectangular coordinates
                float x = radius * cos(angle);
                float y = radius * sin(angle);

                
                // Map the rectangular coordinates to the [0, 1] range
                x = (x + 1.0) * 0.5 * -1;
                y = (y + 1.0) * 0.5;

                // Sample the texture
                half4 col = tex2D(_MainTex, float2(x, y));

                //if(_ShowRemap) col = tex2D(_MainTex, float2(u, v));

                return col;
            }
            ENDCG
        }
    }
}
