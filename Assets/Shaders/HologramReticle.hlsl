#ifndef HOLOGRAM_RETICLE_INCLUDED
#define HOLOGRAM_RETICLE_INCLUDED

// Hologram.fbx is centered at the mesh origin. Work in its local space so the
// rings follow the sphere's surface, including its existing transform/scale.
void HologramReticle_float(float3 PositionOS, float3 BaseColor,
    float3 BaseEmission, float BaseAlpha, float Scanline,
    float ArcDegrees, float Brightness, float3 ReticleColor, float Enabled,
    float LineWidthPixels,
    out float3 DisplayColor, out float3 DisplayEmission, out float DisplayAlpha)
{
    float3 cameraOS = TransformWorldToObject(TransformViewToWorld(float3(0, 0, 0)));
    float3x3 viewToObject = mul((float3x3)GetWorldToObjectMatrix(), (float3x3)GetViewToWorldMatrix());
    float3 forwardOS = normalize(mul(viewToObject, float3(0, 0, -1)));
    float3 rightOS = normalize(mul(viewToObject, float3(1, 0, 0)));
    float3 upOS = normalize(mul(viewToObject, float3(0, 1, 0)));

    // Intersect the center-of-view ray with the sphere. Use this fragment's
    // radius to accommodate the original mesh's slightly varying radius;
    // the fragment on that ray maps exactly to the midpoint between the rings.
    float radius = max(length(PositionOS), 0.000001);
    float b = dot(cameraOS, forwardOS);
    float discriminant = b * b - dot(cameraOS, cameraOS) + radius * radius;
    float root = sqrt(max(discriminant, 0.0));
    float nearHit = -b - root;
    float farHit = -b + root;
    float hit = nearHit > radius * 0.00001 ? nearHit : farHit;
    float3 anchor = cameraOS + forwardOS * hit;
    float anchorLength = length(anchor);
    float3 normal = anchorLength > 0.000001 ? anchor / anchorLength : forwardOS;

    // Orient the spherical patch with the camera, without flattening its UVs
    // into screen space. Longitude/latitude measure distance along curved arcs.
    float3 tangent = rightOS - normal * dot(rightOS, normal);
    if (dot(tangent, tangent) < 0.000001)
        tangent = cross(upOS, normal);
    tangent /= max(length(tangent), 0.000001);
    float3 bitangent = cross(normal, tangent);
    bitangent *= dot(bitangent, upOS) < 0.0 ? -1.0 : 1.0;
    float3 radial = PositionOS / radius;
    float facing = dot(radial, normal);
    float horizontal = dot(radial, tangent);
    float longitudeZ = abs(horizontal) + abs(facing) > 0.000001 ? facing : 0.000001;
    float2 arc = float2(atan2(horizontal, longitudeZ),
        asin(clamp(dot(radial, bitangent), -0.999999, 0.999999)));
    float2 p = arc / radians(clamp(ArcDegrees, 1.0, 60.0));

    // Mirror one circle to form two equal rings, with a clear gap in the middle.
    // Derivatives convert surface distance to pixels for smooth, stable edges
    // as the sphere curves away from the camera or the render resolution changes.
    const float ringRadius = 0.15;
    const float centerOffset = 0.20;
    float distanceToCircle = length(float2(abs(p.x) - centerOffset, p.y)) - ringRadius;
    float2 gradient = float2(ddx(distanceToCircle), ddy(distanceToCircle));
    float pixelSize = max(length(gradient), 0.00001);
    float halfStroke = 0.5 * clamp(LineWidthPixels, 1.0, 6.0) * pixelSize;
    float aa = max(fwidth(distanceToCircle), 0.00001) * 0.5;
    float mask = 1.0 - smoothstep(max(halfStroke - aa, 0.0), halfStroke + aa,
        abs(distanceToCircle));
    mask *= saturate(Enabled) * step(0.0, discriminant) * step(radius * 0.00001, hit);
    mask *= step(0.00001, facing) * step(0.0, dot(PositionOS - cameraOS, forwardOS));

    float scan = lerp(0.85, 1.0, saturate(Scanline));
    float3 ink = ReticleColor * max(Brightness, 0.0) * scan;
    // Composite the rings without applying edge coverage twice. Outside them,
    // all three original graph outputs remain unchanged.
    float combinedAlpha = mask + saturate(BaseAlpha) * (1.0 - mask);
    float weight = mask / max(combinedAlpha, 0.00001);
    // Attenuate the bright blue hologram underneath the antialiased strokes
    // while preserving the original scanlines and Fresnel everywhere else.
    float3 baseLight = max(BaseColor, 0.0) + max(BaseEmission, 0.0);
    float peak = max(1.0, max(baseLight.r, max(baseLight.g, baseLight.b)));
    float background = pow(1.0 - mask, 8.0) / lerp(1.0, peak, saturate(mask * 4.0));
    DisplayColor = BaseColor * (1.0 - weight) * background;
    DisplayEmission = BaseEmission * (1.0 - weight) * background + ink * weight;
    DisplayAlpha = max(BaseAlpha, combinedAlpha);
}

#endif
