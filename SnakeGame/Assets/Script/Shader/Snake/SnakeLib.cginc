#ifndef SNAKE_FUNC_CGINC
#define SNAKE_FUNC_CGINC

float rand(float x) {
    return frac(sin(x + 231.23 * 2451.71) * 5629.643);
}
float rand(float2 st) {
    return frac(sin(dot(st.xy + 75.3, float2(2561, 6922))) * 7623);
}
float rand(float3 st) {
    return frac(sin(dot(st.xyz + 92.6, float3(2561, 6922, 9813))) * 5223);
}
float rand(float4 st) {
    return frac(sin(dot(st.xyzw + 93.645, float4(2561, 6922, 9813, 6412))) * 5223);
}

float when_eq(float x, float y) {
  return 1.0 - abs(sign(x - y));
}
float when_neq(float x, float y) {
  return abs(sign(x - y));
}
float when_gt(float x, float y) {
  return max(sign(x - y), 0.0);
}
float when_lt(float x, float y) {
  return max(sign(y - x), 0.0);
}
float when_ge(float x, float y) {
  return 1.0 - when_lt(x, y);
}
float when_le(float x, float y) {
  return 1.0 - when_gt(x, y);
}

float2 when_eq(float2 x, float2 y) {
  return 1.0 - abs(sign(x - y));
}
float2 when_neq(float2 x, float2 y) {
  return abs(sign(x - y));
}
float2 when_gt(float2 x, float2 y) {
  return max(sign(x - y), 0.0);
}
float2 when_lt(float2 x, float2 y) {
  return max(sign(y - x), 0.0);
}
float2 when_ge(float2 x, float2 y) {
  return 1.0 - when_lt(x, y);
}
float2 when_le(float2 x, float2 y) {
  return 1.0 - when_gt(x, y);
}

float3 when_eq(float3 x, float3 y) {
  return 1.0 - abs(sign(x - y));
}
float3 when_neq(float3 x, float3 y) {
  return abs(sign(x - y));
}
float3 when_gt(float3 x, float3 y) {
  return max(sign(x - y), 0.0);
}
float3 when_lt(float3 x, float3 y) {
  return max(sign(y - x), 0.0);
}
float3 when_ge(float3 x, float3 y) {
  return 1.0 - when_lt(x, y);
}
float3 when_le(float3 x, float3 y) {
  return 1.0 - when_gt(x, y);
}

#endif