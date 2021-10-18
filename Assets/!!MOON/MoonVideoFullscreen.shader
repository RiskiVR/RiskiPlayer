Shader "!M.O.O.N/MoonVideoFullscreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_OverlayTex("Overlay", 2D) = "white"{}
		_Debug("debug", Vector) = (0,0,0.08,0)
		_Scale("Scale", Vector) = (1.41,0.74,1,1)
		_Fade("Overlay Fade", Range(0,1)) = 0
		[HideInInspector]_RotateX("Rotate X", Float) = 0
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ZWrite Off
		ZTest Always
		Cull Off
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _OverlayTex;
			fixed4 _MainTex_ST; 
			fixed4 _Overlay_ST;
			float4 _Debug;
			float4 _Scale;
			float _Fade;
			float _RotateX;

			 struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

			 float2x2 rotate(float deg) {
				 float s = sin(deg);
				 float c = cos(deg);
				 return float2x2(c, -s, s, c);
			 }
	

            v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
            {
				v2f o;
				//pos.xz = mul(pos.xz, rotate(_RotateX));
				pos.y *= -1;
				pos.x *= -1;
				pos.xyz *= _Scale;
				o.uv = TRANSFORM_TEX(uv, _MainTex);
				float4 abspos = pos - _Debug;
				abspos.z = abspos.z;
				o.pos = mul(UNITY_MATRIX_P, abspos);
				return o;
            }


 
 
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 FinalImage;

				float4 OverlayTexture = tex2D(_OverlayTex, i.uv);
				float4 MainTexture = tex2D(_MainTex, i.uv);
				FinalImage = lerp(MainTexture+OverlayTexture,MainTexture, _Fade);
				return FinalImage;
            }
            ENDCG
        }
    }
}
