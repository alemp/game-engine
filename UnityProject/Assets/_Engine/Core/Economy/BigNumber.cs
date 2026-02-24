using System;
using System.Globalization;

namespace GameEngine.Core.Economy
{
    /// <summary>
    /// Arbitrary-precision number for idle game economy (exponential scaling).
    /// Uses mantissa + exponent representation: value = mantissa * 10^exponent.
    /// </summary>
    public readonly struct BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>
    {
        private const double Epsilon = 1e-10;

        public double Mantissa { get; }
        public int Exponent { get; }

        public BigNumber(double mantissa, int exponent = 0)
        {
            if (Math.Abs(mantissa) < Epsilon)
            {
                Mantissa = 0;
                Exponent = 0;
                return;
            }

            while (Math.Abs(mantissa) >= 10)
            {
                mantissa /= 10;
                exponent++;
            }
            while (Math.Abs(mantissa) < 1 && Math.Abs(mantissa) > Epsilon)
            {
                mantissa *= 10;
                exponent--;
            }

            Mantissa = mantissa;
            Exponent = exponent;
        }

        public static BigNumber Zero => new(0, 0);
        public static BigNumber One => new(1, 0);

        public static BigNumber FromDouble(double value)
        {
            if (Math.Abs(value) < Epsilon)
                return Zero;
            var exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            var mant = value / Math.Pow(10, exp);
            return new BigNumber(mant, exp);
        }

        public double ToDouble()
        {
            if (Math.Abs(Mantissa) < Epsilon)
                return 0;
            return Mantissa * Math.Pow(10, Exponent);
        }

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            var (m1, e1, m2, e2) = AlignExponents(a, b);
            return new BigNumber(m1 + m2, Math.Max(a.Exponent, b.Exponent));
        }

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            var (m1, e1, m2, e2) = AlignExponents(a, b);
            return new BigNumber(m1 - m2, Math.Max(a.Exponent, b.Exponent));
        }

        public static BigNumber operator *(BigNumber a, BigNumber b) =>
            new(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);

        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (Math.Abs(b.Mantissa) < Epsilon)
                throw new DivideByZeroException();
            return new BigNumber(a.Mantissa / b.Mantissa, a.Exponent - b.Exponent);
        }

        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;
        public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);
        public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);

        private static (double m1, int e1, double m2, int e2) AlignExponents(BigNumber a, BigNumber b)
        {
            var diff = a.Exponent - b.Exponent;
            if (diff >= 0)
                return (a.Mantissa, a.Exponent, b.Mantissa * Math.Pow(10, -diff), b.Exponent);
            return (a.Mantissa * Math.Pow(10, diff), a.Exponent, b.Mantissa, b.Exponent);
        }

        public int CompareTo(BigNumber other)
        {
            if (Exponent != other.Exponent)
                return Exponent.CompareTo(other.Exponent);
            return Mantissa.CompareTo(other.Mantissa);
        }

        public bool Equals(BigNumber other) =>
            Math.Abs(Mantissa - other.Mantissa) < Epsilon && Exponent == other.Exponent;

        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Mantissa, Exponent);

        public override string ToString()
        {
            if (Math.Abs(Mantissa) < Epsilon)
                return "0";
            if (Exponent >= -2 && Exponent <= 2)
                return ToDouble().ToString("G4", CultureInfo.InvariantCulture);
            return $"{Mantissa:G2}e{Exponent}";
        }
    }
}
