using System;

namespace SQ2.Core;

internal static class Ease
{
    public static double InOutSine(double x) => -(Math.Cos(Math.PI * x) - 1) / 2;
    public static double InSine(double x) => 1 - Math.Cos(x * Math.PI / 2);
}