Shader "Grid/Transparent" {
	Properties {
		_MainTex ("Base (RGB) Transparency (A)", 2D) = ""
	}
	SubShader {
		Tags {"Queue"="Transparent"}
				
		Blend SrcAlpha OneMinusSrcAlpha
	    BindChannels
	    {
	        Bind "Vertex", vertex
	        Bind "Color", color
	        Bind "texcoord1", texcoord0
	    }
	    	    
	    Pass
	    {
	    	ZWrite Off   
	        SetTexture[_MainTex] {Combine primary * texture Double}
	    }
	    	    
	}
} 