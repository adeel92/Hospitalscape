#ifndef CRISPSPRITE_INCLUDED
#define CRISPSPRITE_INCLUDED

half4 tex2D_CS(sampler2D tex, float2 uv)
{			
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);				
	float2 offset = float2(0.125, 0.375);		
	
	half4 c = tex2D(tex, uv + dx * offset.x + dy * offset.y);
	c += tex2D(tex, uv - dx * offset.x - dy * offset.y);
	c += tex2D(tex, uv + dx * offset.y - dy * offset.x);
	c += tex2D(tex, uv - dx * offset.y + dy * offset.x);
	c *= 0.25;
	return c;
}

#endif // CRISPSPRITE_INCLUDED