#ifndef VOLUME_FUNC_CGINC
#define VOLUME_FUNC_CGINC
struct Ray {
  float3 origin;
  float3 dir;
};

struct AABB {
  float3 min;
  float3 max;
};

struct SphericalCoord {
  float r;
  float omega;
  float tetha;
};

#ifndef ITERATIONS
    #define ITERATIONS 100
#endif

bool intersect(Ray r, AABB aabb, out float t0, out float t1)
{
  float3 invR = 1.0 / r.dir;
  float3 tbot = invR * (aabb.min - r.origin);
  float3 ttop = invR * (aabb.max - r.origin);
  float3 tmin = min(ttop, tbot);
  float3 tmax = max(ttop, tbot);
  float2 t = max(tmin.xx, tmin.yz);
  t0 = max(t.x, t.y);
  t = min(tmax.xx, tmax.yz);
  t1 = min(t.x, t.y);
  return t0 <= t1;
}

float3 get_uv(float3 p) {
  return (p);
}

// {r, Omega, Theta}
SphericalCoord GetSphereCoord(float3 coord) {
  SphericalCoord o;
  o.r = sqrt(coord.x * coord.x + coord.y * coord.y + coord.z * coord.z);
  float xyDist = sqrt(coord.x * coord.x + coord.y * coord.y);
  o.omega = atan2(coord.z, xyDist);
  o.tetha = atan2(coord.y, coord.x);

  return o;
}
#endif