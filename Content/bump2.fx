
cbuffer data :register(b0)
{
	float4x4 world;
	float4x4 worldViewProj;
	float4 lightDirection;
	float4 viewDirection;
	float bias;
};

struct VS_IN
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float3 tangent: TANGENT;
	float3 binormal: BINORMAL;
	float2 texcoord : TEXCOORD;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD;
	float3 lightDirection:LIGHT;
	float3 viewDirection:VIEW;
};

//texture
Texture2D textureMap:register(t0);
Texture2D normalMap:register(t1);
SamplerState textureSampler;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	float3 B = mul(world, input.binormal);
	float3 T = mul(world, input.tangent);
	float3 N = mul(world, input.normal);

	output.position = mul(worldViewProj, input.position);
	output.normal = N;
	output.texcoord = input.texcoord;



	float3x3 Tangent = { T,B,N };
	output.lightDirection = mul(Tangent, lightDirection.xyz);
	output.viewDirection = mul(Tangent, viewDirection.xyz);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float2 parallax = input.viewDirection.xy * normalMap.Sample(textureSampler, input.texcoord).a*bias;

	float4 D = textureMap.Sample(textureSampler, input.texcoord + parallax);
	float4 N = normalMap.Sample(textureSampler, input.texcoord + parallax)*2.0f - 1.0f;

	return saturate(dot(N,input.lightDirection))*D + 0.2F;
}

technique BumpMap
{
	pass Pass1
	{
		Profile = 9.1;
		VertexShader = VS;
		PixelShader = PS;
	}
}