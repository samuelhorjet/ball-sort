import { ImageResponse } from "next/og";

export const runtime = "edge";

export const size = {
  width: 32,
  height: 32,
};
export const contentType = "image/png";

export default function Icon() {
  const brand = "#ff2d8b";
  const blue = "#4d9fff";
  const green = "#3dd9a0";
  const yellow = "#ffd93d";
  const bg = "#0d0d1e"; 

  return new ImageResponse(
    (
      <div
        style={{
          background: bg,
          width: "100%",
          height: "100%",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          borderRadius: "6px",
        }}
      >
        <div
          style={{
            display: "flex", 
            position: "relative",
            width: 22,
            height: 22,
          }}
        >
          {/* Top (pink) */}
          <div
            style={{
              position: "absolute",
              left: 7,
              top: 0,
              width: 8,
              height: 8,
              borderRadius: "50%",
              backgroundColor: brand,
            }}
          />
          {/* Left (blue) */}
          <div
            style={{
              position: "absolute",
              left: 0,
              top: 7,
              width: 8,
              height: 8,
              borderRadius: "50%",
              backgroundColor: blue,
            }}
          />
          {/* Right (yellow) */}
          <div
            style={{
              position: "absolute",
              left: 14,
              top: 7,
              width: 8,
              height: 8,
              borderRadius: "50%",
              backgroundColor: yellow,
            }}
          />
          {/* Bottom (green) */}
          <div
            style={{
              position: "absolute",
              left: 7,
              top: 14,
              width: 8,
              height: 8,
              borderRadius: "50%",
              backgroundColor: green,
            }}
          />
          {/* Center (white) */}
          <div
            style={{
              position: "absolute",
              left: 5,
              top: 5,
              width: 12,
              height: 12,
              borderRadius: "50%",
              backgroundColor: "white",
            }}
          />
        </div>
      </div>
    ),
    {
      ...size,
    }
  );
}
