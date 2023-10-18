
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RadialFill"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
        _Angle("Angle", Range(0, 360)) = 0
        _Fill("Fill angle", Range(0, 360)) = 15
        _UvOffset("UV offset", Vector) = (0, 0, 0, 0)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex        : SV_POSITION;
				float4 localVertex   : TEXCOORD1;
				fixed4 color         : COLOR;
				float2 texcoord      : TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.localVertex = IN.vertex;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float _Angle;
			float _Fill;
			float2 _UvOffset;

			fixed4 frag(v2f IN) : SV_Target
			{
                //-------- Creating arc --------//
                // sector start/end angles
                float fill = 360 - _Fill;
                float halfFill = fill / 2;
                float startAngle = _Angle - halfFill;
                float endAngle = _Angle + halfFill;

                // check offsets
                float offset0 = clamp(0, 360, startAngle + 360);
                float offset360 = clamp(0, 360, endAngle - 360);

                // convert uv to atan coordinates
                float2 atan2Coord = float2(
                    lerp(-1, 1, IN.localVertex.x + _UvOffset.x),
                    lerp(-1, 1, IN.localVertex.y + _UvOffset.y)
                );
                float atanAngle = atan2(atan2Coord.y, atan2Coord.x) * 57.3; // angle in degrees

                // convert angle to 360 system
                if(atanAngle < 0) atanAngle = 360 + atanAngle;

                if(atanAngle >= startAngle && atanAngle <= endAngle) discard;
                if(atanAngle <= offset360) discard;
                if(atanAngle >= offset0) discard;

				return _Color;
			}
		ENDCG
		}
	}
}
