Shader "Grid/Transparent Texture Blend" {
	Properties {
		_Blend ("Blend", Range (0, 1) ) = 0.5
		_MainTex ("Base (RGB) Transparency (A)", 2D) = ""
		_Texture2 ("Texture 2",2D) = ""
	}
	SubShader {
		Tags {"Queue"="Transparent"}
		
		Pass {
			SetTexture[_MainTex]
			SetTexture[_Texture2] { 
				ConstantColor (0,0,0, [_Blend]) 
				Combine texture Lerp(constant) previous
			}		
		}
		
		Blend SrcAlpha OneMinusSrcAlpha
	    BindChannels
	    {
	        Bind "Vertex", vertex
	        Bind "Color", color
	    }	    
	    
	    Pass
	    {      
	        SetTexture[_MainTex] {Combine primary * texture Double}
	    }
	    
	    
	}
} 