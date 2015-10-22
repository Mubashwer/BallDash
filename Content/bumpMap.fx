// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Adapted for COMP30019 by Jeremy Nicholson, 10 Sep 2012
// Adapted further by Chris Ewin, 23 Sep 2013

// these won't change in a given iteration of the shader
float4x4 World;
float4x4 View;
float4x4 Projection;
float4 cameraPos;
///float4 lightAmbCol = float4(0.4f, 0.4f, 0.4f, 1.0f);
//float4 lightPntPos = float4(0.0f, 0.0f, -2.0f, 1.0f);
//float4 lightPntCol = float4(1.0f, 1.0f, 1.0f, 1.0f);
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);    
float SpecularIntensity = 1;
float3 ViewVector = float3(1,0,0);
float4x4 worldInvTrp;

Texture2D shaderTexture;
SamplerState SampleType;
float BumpConstant = 1;
Texture2D texture1;
SamplerState g_1
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

//

struct VS_IN
{
	float4 pos : SV_POSITION;
	float4 nrm : NORMAL;
	float2 tex : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	// Other vertex properties, e.g. texture co-ords, surface Kd, Ks, etc
};

struct PS_IN
{
	float4 pos : SV_POSITION; //Position in camera co-ords
	float2 tex : TEXCOORD0;
	float4 wpos : TEXCOORD1; //Position in world co-ords
	float3 wnrm : TEXCOORD2; //Normal in world co-ords 
	float3 Tangent : TEXCOORD3;
	float3 Binormal : TEXCOORD4;
};


PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	// Convert Vertex position and corresponding normal into world coords
	// Note that we have to multiply the normal by the transposed inverse of the world 
	// transformation matrix (for cases where we have non-uniform scaling; we also don't
	// care about the "fourth" dimension, because translations don't affect the normal)
	output.wpos = mul(input.pos, World);
	output.wnrm = mul(input.nrm.xyz, (float3x3)worldInvTrp);
	output.Tangent = mul(input.Tangent.xyz, (float3x3)worldInvTrp);
	output.Binormal = mul(input.Binormal.xyz, (float3x3)worldInvTrp);

	// Transform vertex in world coordinates to camera coordinates
	float4 viewPos = mul(output.wpos, View);
	output.pos = mul(viewPos, Projection);

	// Just pass along the colour at the vertex
	output.tex = input.tex;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{

	float3 bump = BumpConstant * (texture1.Sample(g_1, input.tex) - (0.5, 0.5, 0.5));
	float3 bumpNormal = input.wnrm + (bump.x * input.Tangent + bump.y * input.Binormal);
	bumpNormal = normalize(bumpNormal);

	// Calculate the diffuse light component with the bump map normal
    float diffuseIntensity = dot(normalize(DiffuseLightDirection), bumpNormal);
    if(diffuseIntensity < 0)
        diffuseIntensity = 0;

    // Calculate the specular light component with the bump map normal
    float3 light = normalize(DiffuseLightDirection);
	float3 z = 2 * dot(light, bumpNormal) * bumpNormal - light;
    float3 r = normalize(z);
	float3 k = mul(normalize(ViewVector), World);
    float3 v = normalize(k);
    float dotProduct = dot(r, v);

    float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

    // Calculate the texture color
	//float3 bump = BumpConstant * (texture1.Sample(g_1, input.tex) - (0.5, 0.5, 0.5));
    float4 textureColor = texture1.Sample(g_1, input.tex);
    textureColor.a = 1;

    // Combine all of these values into one (including the ambient light)
    return saturate(textureColor * (diffuseIntensity) + AmbientColor * AmbientIntensity + specular);



















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