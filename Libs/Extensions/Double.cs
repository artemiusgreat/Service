using System;

namespace Service.Extensions
{
  public static class DoubleExtensions
  {
    public static bool IsEqual(this double input, double num, double epsilon = double.Epsilon)
    {
      return Math.Abs(input - num) < epsilon;
    }
  }
}
