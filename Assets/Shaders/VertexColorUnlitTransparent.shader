Shader "Custom/VertexColorUnlitTransparent" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    Category {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Lighting Off
		ZWrite Off
		Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        
		BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }
        
        SubShader {
            Pass {
                SetTexture [_MainTex] {
                    Combine texture * primary
                }
            }
        }
    }
}
