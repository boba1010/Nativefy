using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefy.Models;

public readonly struct Color(byte a, byte r, byte g, byte b) : IEquatable<Color>
{
    public byte A { get; } = a; public byte R { get; } = r; public byte G { get; } = g; public byte B { get; } = b;

    public static Color FromArgb(byte a, byte r, byte g, byte b) => new(a, r, g, b);

    public string ToHex() => A == 255
        ? $"#{R:X2}{G:X2}{B:X2}"
        : $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    public bool Equals(Color other) => A == other.A && R == other.R && G == other.G && B == other.B;
    public override bool Equals(object? obj) => obj is Color c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(A, R, G, B);
    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public static explicit operator global::Windows.UI.Color(Color v)
    {
        return new global::Windows.UI.Color()
        {
            A = v.A,
            B = v.B,
            G = v.G,
            R = v.R,
        };
    }
}