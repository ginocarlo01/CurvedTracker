Shader "Imagine/PaperCupUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _arc ("Arc", Range(0,360)) = 360
        _r1 ("Upper Radius", Range(1, 1)) = 1
        _r2 ("Lower Radius", Range(0, 0.999)) = 0.8
        _w ("Width (UV)", float) = 1
        _h ("Height (UV)", float) = 1
        _h2 ("Height (Scale)", Range(0, 10)) = 1
        _o ("Outline", Range(.005, 0.01)) = 0.005
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _r1;
            float _r2;
            float _w;
            float _h;
            float _h2;
            float _arc;
            float _o;

            v2f vert (appdata v)
            {
                v2f _o;
                _o.vertex = UnityObjectToClipPos(v.vertex);
                _o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(_o,_o.vertex);
                return _o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float hoffset = (_h2/(2 * 3.14159*_r1))*((_r1+_r2)/2) - 1;

                // float l = sqrt((_r1-_r2)*(_r1-_r2) + _h*_h);// + hoffset;
                // float R1 = _r1 * l/(_r1 - _r2);
                // float R2 = R1 * (_r2/_r1);//R1 - l;
                float ThetaSector = (360 * (_r1-_r2)) / sqrt((_r1-_r2)*(_r1-_r2) + _h2*_h2);
                float R1 = 360 * _r1 / ThetaSector;
                float R2 = 360 * _r2 / ThetaSector;

                float chord = 2 * R1 * sin(ThetaSector/2/180*3.14159);
                if(ThetaSector > 180)
                    chord = 2*R1;
                R1/=chord;
                R2/=chord;

                // if(_w < 2 * R1){
                //     ThetaSector = 2 * asin(_w / (2*R1)) * 180 / 3.14159;
                // }
                // else{
                //     ThetaSector = 360 - 2 * asin((2*R1)/_w) * 180 / 3.14159;
                // }
                
                // Calculate the center of the curve
                float2 center = float2(0.5, _h-R1);

                // Calculate the distance from the center
                float distance = length(i.uv - center);

                // Calculate the radius based on the distance
                float radius = lerp(R1, R1, distance);

                // Calculate the angle based on the UV coordinate
                float angle = atan2(i.uv.x - center.x, i.uv.y - center.y) * 360 / ThetaSector / (_arc/360);
                float angle2 = atan2(i.uv.x - center.x, i.uv.y - center.y) * 360 / ThetaSector;


                // Map the UV coordinates to the curved position
                float2 curvedUV = float2(angle / (2 * 3.14159) + 0.5, (distance - R2) / (radius - R2));
                float2 curvedUV2 = float2(angle2 / (2 * 3.14159) + 0.5, (distance - R2) / (radius - R2));


                // Sample the texture at the curved UV coordinates
                fixed4 c = tex2D(_MainTex, curvedUV);

                if(curvedUV2.x < 0 || curvedUV2.x > 1 ||curvedUV2.y < 0 || curvedUV2.y > 1 ){
                    if( (curvedUV2.x > -_o  && curvedUV2.x < _o)   ||
                        (curvedUV2.x > 1-_o && curvedUV2.x < 1+_o) ||
                        (curvedUV2.y > -_o  && curvedUV2.y < _o)   ||
                        (curvedUV2.y > 1-_o && curvedUV2.y < 1+_o) ){
                        c = fixed4(0.8,0.8,0.8,1);
                    }
                    else{
                        c = fixed4(1,1,1,1);
                        //c = ThetaSector < 180 ? fixed4(1,1,1,1) : fixed4(1,0.5,0.5,1);
                    }
                }
                else if(curvedUV.x < 0 || curvedUV.x > 1 ||curvedUV.y < 0 || curvedUV.y > 1 ){
                    c = fixed4(1,1,1,1);
                }

                //c *= (ThetaSector/360);


                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return c;
            }
            ENDCG
        }
    }
}
