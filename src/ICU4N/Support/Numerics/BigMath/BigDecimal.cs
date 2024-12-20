// 
//  Copyright 2009-2017  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using ICU4N.Support.Numerics.BigMath;
using System;
using System.Globalization;
using System.Text;
using J2N.Numerics;
using ICU4N.Support;
#if !PORTABLE
using System.Runtime.Serialization;
#endif
using SR = ICU4N.Support.Numerics.BigMath.Messages;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// This class represents immutable arbitrary precision decimal numbers.
    /// </summary>
    /// <remarks>
    /// Each <see cref="BigDecimal"/> instance is represented with a unscaled 
    /// arbitrary precision mantissa (the unscaled value) and a scale. The value 
    /// of the <see cref="BigDecimal"/> is <see cref="UnscaledValue"/> 10^(-<see cref="Scale"/>).
    /// </remarks>
#if !PORTABLE
    [Serializable]
#endif
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]

#if FEATURE_BIGMATH
    public
#else
    internal
#endif
        sealed partial class BigDecimal : J2N.Numerics.Number, IComparable<BigDecimal>, IEquatable<BigDecimal> // ICU4N TODO: Clean up and make public
#if !PORTABLE
        , ISerializable
#endif
    {

        /// <summary>
        /// The constant zero as a <see cref="BigDecimal"/>.
        /// </summary>
        public static readonly BigDecimal Zero = new BigDecimal(0, 0);

        /// <summary>
        /// The constant one as a <see cref="BigDecimal"/>.
        /// </summary>
        public static readonly BigDecimal One = new BigDecimal(1, 0);

        /// <summary>
        /// The constant ten as a <see cref="BigDecimal"/>.
        /// </summary>
        public static readonly BigDecimal Ten = new BigDecimal(10, 0);

        /// <summary>
        /// The double closer to <c>Log10(2)</c>.
        /// </summary>
        private const double Log10Of2 = 0.3010299956639812;

        /// <summary>
        /// The <see cref="string"/> representation is cached.
        /// </summary>
#if !PORTABLE
        [NonSerialized]
#endif
        internal string toStringImage;

        /// <summary>
        /// An array with powers of five that fit in the type <see cref="long"/>
        /// (<c>5^0,5^1,...,5^27</c>).
        /// </summary>
        internal static readonly BigInteger[] FivePow;

        /// <summary>
        /// An array with powers of ten that fit in the type <see cref="long"/> 
        /// (<c>10^0,10^1,...,10^18</c>).
        /// </summary>
        internal static readonly BigInteger[] TenPow;

        /// <summary>
        /// An array with powers of ten that fit in the type <see cref="long"/> 
        /// (<c>10^0,10^1,...,10^18</c>).
        /// </summary>
        internal static readonly long[] LongTenPow = new long[] {
            1L,
            10L,
            100L,
            1000L,
            10000L,
            100000L,
            1000000L,
            10000000L,
            100000000L,
            1000000000L,
            10000000000L,
            100000000000L,
            1000000000000L,
            10000000000000L,
            100000000000000L,
            1000000000000000L,
            10000000000000000L,
            100000000000000000L,
            1000000000000000000L,
        };


        private static readonly long[] LongFivePow = new long[] {
            1L,
            5L,
            25L,
            125L,
            625L,
            3125L,
            15625L,
            78125L,
            390625L,
            1953125L,
            9765625L,
            48828125L,
            244140625L,
            1220703125L,
            6103515625L,
            30517578125L,
            152587890625L,
            762939453125L,
            3814697265625L,
            19073486328125L,
            95367431640625L,
            476837158203125L,
            2384185791015625L,
            11920928955078125L,
            59604644775390625L,
            298023223876953125L,
            1490116119384765625L,
            7450580596923828125L,
        };

        private static readonly int[] LongFivePowBitLength = new int[LongFivePow.Length];
        internal static readonly int[] LongTenPowBitLength = new int[LongTenPow.Length];

        private const int BiScaledByZeroLength = 11;

        /// <summary>
        /// An array with the first <see cref="BigInteger"/> scaled by zero.
        /// (<c>[0,0],[1,0],...,[10,0]</c>).
        /// </summary>
        private static readonly BigDecimal[] BiScaledByZero = new BigDecimal[BiScaledByZeroLength];

        /// <summary>
        /// An array with the zero number scaled by the first positive scales.
        /// (<c>0*10^0, 0*10^1, ..., 0*10^10</c>).
        /// </summary>
        internal static readonly BigDecimal[] ZeroScaledBy = new BigDecimal[11];

        static BigDecimal()
        {
            // To fill all static arrays.
            int i = 0;

            for (; i < ZeroScaledBy.Length; i++)
            {
                BiScaledByZero[i] = new BigDecimal(i, 0);
                ZeroScaledBy[i] = new BigDecimal(0, i);
            }

            for (int j = 0; j < LongFivePowBitLength.Length; j++)
            {
                LongFivePowBitLength[j] = CalcBitLength(LongFivePow[j]);
            }
            for (int j = 0; j < LongTenPowBitLength.Length; j++)
            {
                LongTenPowBitLength[j] = CalcBitLength(LongTenPow[j]);
            }

            // Taking the references of useful powers.
            TenPow = Multiplication.BigTenPows;
            FivePow = Multiplication.BigFivePows;
        }

        /// <summary>
        /// The arbitrary precision integer (unscaled value) in the internal
        /// representation of <see cref="BigDecimal"/>.
        /// </summary>
        private BigInteger intVal;

#if !PORTABLE
        [NonSerialized]
#endif
        private int _bitLength;

#if !PORTABLE
        [NonSerialized]
#endif
        private long smallValue;

        /// <summary>
        /// The 32-bit integer scale in the internal representation 
        /// of <see cref="BigDecimal"/>.
        /// </summary>
        private int _scale;

        /// <summary>
        /// Represent the number of decimal digits in the unscaled value.
        /// </summary>
        /// <remarks>
        /// This precision is calculated the first time, and used in the following 
        /// calls of method <see cref="Precision"/>. Note that some call to the private 
        /// method <see cref="InplaceRound"/> could update this field.
        /// </remarks>
        /// <seealso cref="Precision"/>
        /// <seealso cref="InplaceRound"/>
#if !PORTABLE
        [NonSerialized]
#endif
        private int _precision = 0;

        #region .ctor

        // ICU4N: Overload used by Parse/TryParse
        internal BigDecimal(BigInteger unscaledValue, int scale, int precision, MathContext mc)
        {
            _scale = scale;
            SetUnscaledValue(unscaledValue);
            _precision = precision;
            if (mc != null)
                InplaceRound(mc); // ICU4N TODO: After rounding is debugged, refactor to use precision and roundingMode as input parameters instead so we can factor out MathContext.
        }

        // ICU4N: Overload used by Parse/TryParse
        internal BigDecimal(long smallValue, int scale, int precision, MathContext mc)
        {
            this.smallValue = smallValue;
            _scale = scale;
            _bitLength = CalcBitLength(smallValue);
            _precision = precision;
            if (mc != null)
                InplaceRound(mc); // ICU4N TODO: After rounding is debugged, refactor to use precision and roundingMode as input parameters instead so we can factor out MathContext.
        }

        private BigDecimal(long smallValue, int scale)
        {
            this.smallValue = smallValue;
            _scale = scale;
            _bitLength = CalcBitLength(smallValue);
        }

        private BigDecimal(int smallValue, int scale)
        {
            this.smallValue = smallValue;
            _scale = scale;
            _bitLength = CalcBitLength(smallValue);
        }



        internal BigDecimal()
        {

        }


        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the 64bit 
        /// double <paramref name="val"/>. The constructed big decimal is 
        /// equivalent to the given double.
        /// </summary>
        /// <param name="val">The double value to be converted to a 
        /// <see cref="BigDecimal"/> instance.</param>
        /// <remarks>
        /// For example, <c>new BigDecimal(0.1)</c> is equal to <c>0.1000000000000000055511151231257827021181583404541015625</c>. 
        /// This happens as <c>0.1</c> cannot be represented exactly in binary.
        /// <para>
        /// To generate a big decimal instance which is equivalent to <c>0.1</c> use the
        /// <see cref="BigDecimal.Parse(string)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="FormatException">
        /// If <paramref name="val"/> is infinity or not a number.
        /// </exception>
        public BigDecimal(double val)
        {
            if (double.IsInfinity(val) || double.IsNaN(val))
            {
                // math.03=Infinity or NaN
                throw new OverflowException(Messages.math03); //$NON-NLS-1$ // ICU4N: Changed to OverflowException to match .NET
            }

            long bits = J2N.BitConversion.DoubleToInt64Bits(val); // IEEE-754

            // Extracting the exponent, note that the bias is 1023
            _scale = 1075 - (int)((bits >> 52) & 0x7FFL);
            // Extracting the 52 bits of the mantisa.
            long mantisa = (_scale == 1075) ? (bits & 0xFFFFFFFFFFFFFL) << 1 : (bits & 0xFFFFFFFFFFFFFL) | 0x10000000000000L;
            if (mantisa == 0)
            {
                _scale = 0;
                _precision = 1;
            }
            // To simplify all factors '2' in the mantisa 
            if (_scale > 0)
            {
                int trailingZeros = System.Math.Min(_scale, mantisa.TrailingZeroCount());
                //long mantisa2 = (long)(((ulong)mantisa) >> trailingZeros);
                mantisa >>>= trailingZeros;
                _scale -= trailingZeros;
            }
            // Calculating the new unscaled value and the new scale
            if ((bits >> 63) != 0)
            {
                mantisa = -mantisa;
            }
            int mantisaBits = CalcBitLength(mantisa);
            if (_scale < 0)
            {
                _bitLength = mantisaBits == 0 ? 0 : mantisaBits - _scale;
                if (_bitLength < 64)
                {
                    smallValue = mantisa << (-_scale);
                }
                else
                {
                    intVal = BigInteger.GetInstance(mantisa) << (-_scale);
                }
                _scale = 0;
            }
            else if (_scale > 0)
            {
                // m * 2^e =  (m * 5^(-e)) * 10^e
                if (_scale < LongFivePow.Length && mantisaBits + LongFivePowBitLength[_scale] < 64)
                {
                    smallValue = mantisa * LongFivePow[_scale];
                    _bitLength = CalcBitLength(smallValue);
                }
                else
                {
                    SetUnscaledValue(Multiplication.MultiplyByFivePow(BigInteger.GetInstance(mantisa), _scale));
                }
            }
            else
            {
                // scale == 0
                smallValue = mantisa;
                _bitLength = mantisaBits;
            }
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the 64bit 
        /// double <paramref name="val"/>. The constructed big decimal is 
        /// equivalent to the given double.
        /// </summary>
        /// <param name="val">The double value to be converted to a 
        /// <see cref="BigDecimal"/> instance.</param>
        /// <param name="context">The rounding mode and precision for the result of 
        /// this operation.</param>
        /// <remarks>
        /// For example, <c>new BigDecimal(0.1)</c> is equal to <c>0.1000000000000000055511151231257827021181583404541015625</c>. 
        /// This happens as <c>0.1</c> cannot be represented exactly in binary.
        /// <para>
        /// To generate a big decimal instance which is equivalent to <c>0.1</c> use the
        /// <see cref="BigDecimal.Parse(string, IFormatProvider?)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="FormatException">
        /// If <paramref name="val"/> is infinity or not a number.
        /// </exception>
        /// <exception cref="ArithmeticException">
        /// if <see cref="MathContext.Precision"/> of <paramref name="context"/> is greater than 0
        /// and <see cref="MathContext.RoundingMode"/> is equal to <see cref="RoundingMode.Unnecessary"/>
        /// and the new big decimal cannot be represented within the given precision without rounding.
        /// </exception>
        public BigDecimal(double val, MathContext context)
            : this(val)
        {
            InplaceRound(context);
        }


        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the
        /// <paramref name="val">given big integer</paramref>.
        /// </summary>
        /// <param name="val">The value to be converted to a <see cref="BigDecimal"/> instance.</param>
        /// <remarks>
        /// The <see cref="Scale"/> of the result is <c>0</c>.
        /// </remarks>
        public BigDecimal(BigInteger val)
            : this(val, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the
        /// <paramref name="val">given big integer</paramref>.
        /// </summary>
        /// <param name="val">The value to be converted to a <see cref="BigDecimal"/> instance.</param>
        /// <param name="context">The rounding mode and precision for the result of this operation.</param>
        /// <remarks>
        /// The <see cref="Scale"/> of the result is <c>0</c>.
        /// </remarks>
        /// <exception cref="ArithmeticException">
        /// If <see cref="MathContext.Precision"/> is greater than 0 and <see cref="MathContext.RoundingMode"/> is
        /// equal to <see cref="RoundingMode.Unnecessary"/> and the new big decimal cannot be represented  within the 
        /// given precision without rounding.
        /// </exception>
        public BigDecimal(BigInteger val, MathContext context)
            : this(val)
        {
            InplaceRound(context);
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from a <paramref name="unscaledValue">given unscaled value</paramref> 
        /// and a given scale.
        /// </summary>
        /// <param name="unscaledValue">Represents the unscaled value of the decimal.</param>
        /// <param name="scale">The scale of this <see cref="BigDecimal"/></param>
        /// <remarks>
        /// The value of this instance is <c><paramref name="unscaledValue"/> x 10^(-<paramref name="scale"/>)</c>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="unscaledValue"/> is <b>null</b>.
        /// </exception>
        public BigDecimal(BigInteger unscaledValue, int scale)
        {
            if (unscaledValue is null)
                throw new ArgumentNullException(nameof(unscaledValue));

            _scale = scale;
            SetUnscaledValue(unscaledValue);
        }

        

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from a <paramref name="unscaledValue">given unscaled 
        /// value</paramref> and a given scale.
        /// </summary>
        /// <param name="unscaledValue">Represents the unscaled value of this big decimal.</param>
        /// <param name="scale">The scale factor of the decimal.</param>
        /// <param name="context">The context used to round the result of the operations.</param>
        /// <remarks>
        /// The value of this instance is <c><paramref name="unscaledValue"/> 10^(-<paramref name="scale"/>)</c>. 
        /// The result is rounded according to the specified math context.
        /// </remarks>
        /// <exception cref="ArithmeticException">
        /// If <see cref="MathContext.Precision"/> is greater than zero, the
        /// <see cref="MathContext.RoundingMode"/> is set to <see cref="RoundingMode.Unnecessary"/>
        /// and the decimal cannot be represented within the given precision without rounding.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the given <paramref name="unscaledValue"/> is null.
        /// </exception>
        public BigDecimal(BigInteger unscaledValue, int scale, MathContext context)
            : this(unscaledValue, scale)
        {
            InplaceRound(context);
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the given int 
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The integer value to convert to a decimal.</param>
        /// <remarks>
        /// The scale factor of the result is zero.
        /// </remarks>
        public BigDecimal(int value)
            : this(value, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the given <paramref name="value">integer value</paramref>. 
        /// </summary>
        /// <param name="value">Integer value to be converted to a <see cref="BigDecimal"/> instance.</param>
        /// <param name="context">The rounding mode and precision for the result of this operation.</param>
        /// <remarks>
        /// The scale of the result is {@code 0}. The result is rounded according to the specified math context.
        /// </remarks>
        /// <exception cref="ArithmeticException">
        /// Thrown if precision is greater than 0 and <see cref="RoundingMode"/> is
        /// <see cref="RoundingMode.Unnecessary"/> and the new big decimal cannot be represented
        /// within the given precision without rounding. 
        /// </exception>
        public BigDecimal(int value, MathContext context)
            : this(value, 0)
        {
            InplaceRound(context);
        }

        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the given long <paramref name="value"/>,
        /// with a scale of <c>0</c>.
        /// </summary>
        /// <param name="value">The long value to be converted to a <see cref="BigDecimal"/></param>
        public BigDecimal(long value)
            : this(value, 0)
        {
        }


        /// <summary>
        /// Constructs a new <see cref="BigDecimal"/> instance from the given long <paramref name="value"/>,
        /// with a scale of <c>0</c> and the value rounded according to the specified context.
        /// </summary>
        /// <param name="value">The long value to be converted to a <see cref="BigDecimal"/></param>
        /// <param name="context">The context that defines the rounding mode and precision to apply to the
        /// value obtained from the given integer.</param>
        /// <exception cref="ArithmeticException">
        /// If the <see cref="MathContext.Precision"/> value specified is greater than <c>0</c> and
        /// the <see cref="MathContext.RoundingMode"/> is <see cref="RoundingMode.Unnecessary"/> and
        /// the new <see cref="BigDecimal"/> cannot be represented within the given precision
        /// without rounding.
        /// </exception>
        public BigDecimal(long value, MathContext context)
            : this(value)
        {
            InplaceRound(context);
        }

        #endregion

        /// <summary>
        /// Gets the sign of the decimal, where <c>-1</c> if the value is less than 0,
        /// <c>0</c> if the value is 0 and <c>1</c> if the value is greater than 0.
        /// </summary>
        /// <seealso cref="Sign"/>
        public int Sign
        {
            get
            {
                if (_bitLength < 64)
                    return Math.Sign(smallValue);
                return GetUnscaledValue().Sign;
            }
        }

        internal int BitLength
        {
            get { return _bitLength; }
            set { _bitLength = value; }
        }

        internal long SmallValue
        {
            get { return smallValue; }
            set { smallValue = value; }
        }

        /// <summary>
        /// Gets a value indicating if the decimal is equivalent to zero.
        /// </summary>
        public bool IsZero
        {
            get
            {
                //Watch out: -1 has a bitLength=0
                return _bitLength == 0 && smallValue != -1;
            }
        }

        /// <summary>
        /// Gets the scale value of this <see cref="BigDecimal"/> instance
        /// </summary>
        /// <remarks>
        /// The scale is the number of digits behind the decimal point. The value of 
        /// this <see cref="BigDecimal"/> is the <c>unsignedValue * 10^(-scale)</c>. 
        /// If the scale is negative, then this <see cref="BigDecimal"/> represents a big integer.
        /// </remarks>
        public int Scale
        {
            get { return _scale; }
            internal set { _scale = value; }
        }

        /// <summary>
        /// Gets the precision value of this <see cref="BigDecimal"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The precision is the number of decimal digits used to represent this decimal.
        /// It is equivalent to the number of digits of the unscaled value.
        /// </para>
        /// <para>
        ///  The precision of <c>0</c> is <c>1</c> (independent of the scale).
        /// </para>
        /// </remarks>
        public int Precision
        {
            get
            {
                // Checking if the precision already was calculated
                if (_precision > 0)
                {
                    return _precision;
                }
                int bitLength = this._bitLength;
                int decimalDigits = 1; // the precision to be calculated
                double doubleUnsc = 1; // intVal in 'double'

                if (bitLength < 1024)
                {
                    // To calculate the precision for small numbers
                    if (bitLength >= 64)
                    {
                        doubleUnsc = GetUnscaledValue().ToDouble();
                    }
                    else if (bitLength >= 1)
                    {
                        doubleUnsc = smallValue;
                    }
                    decimalDigits += (int)Math.Log10(Math.Abs(doubleUnsc));
                }
                else
                {
                    // (bitLength >= 1024)
                    /* To calculate the precision for large numbers
                     * Note that: 2 ^(bitlength() - 1) <= intVal < 10 ^(precision()) */
                    decimalDigits += (int)((bitLength - 1) * Log10Of2);
                    // If after division the number isn't zero, exists an aditional digit
                    if ((GetUnscaledValue() / Multiplication.PowerOf10(decimalDigits)).Sign != 0)
                    {
                        decimalDigits++;
                    }
                }
                _precision = decimalDigits;
                return _precision;
            }
            internal set { _precision = value; }
        }

        /// <summary>
        /// Gets the unscaled value (mantissa) of this <see cref="BigDecimal"/> instance as 
        /// a <see cref="BigInteger"/>.
        /// </summary>
        /// <remarks>
        /// The unscaled value can be computed as <c>(this * 10^(scale))</c>.
        /// </remarks>
        public BigInteger UnscaledValue
        {
            get { return GetUnscaledValue(); }
        }

        #region Public Methods

        /// <summary>
        /// Returns a new <see cref="BigDecimal"/> instance whose value is equal to 
        /// <paramref name="unscaledVal"/> 10^(-<paramref name="scale"/>). The scale 
        /// of the result is <paramref name="scale"/>, and its unscaled value is <paramref name="unscaledVal"/>.
        /// </summary>
        /// <param name="unscaledVal">The unscaled value to be used to construct 
        /// the new <see cref="BigDecimal"/>.</param>
        /// <param name="scale">The scale to be used to construct the new <see cref="BigDecimal"/>.</param>
        /// <returns>
        /// Returns a <see cref="BigDecimal"/> instance with the value <paramref name="unscaledVal"/> 
        /// * 10^(-<paramref name="scale"/>).
        /// </returns>
        public static BigDecimal GetInstance(long unscaledVal, int scale) // ICU4N TODO: API - Ideally, this would be made into a constructor if/when BigDecimal is made into a struct.
        {
            if (scale == 0)
                return GetInstance(unscaledVal);
            if ((unscaledVal == 0) && (scale >= 0) &&
                (scale < ZeroScaledBy.Length))
            {
                return ZeroScaledBy[scale];
            }

            return new BigDecimal(unscaledVal, scale);
        }

        /// <summary>
        /// Returns a new <see cref="BigDecimal"/> instance whose value is equal 
        /// to <paramref name="unscaledVal"/>. The scale of the result is <c>0</c>, 
        /// and its unscaled value is <paramref name="unscaledVal"/>.
        /// </summary>
        /// <param name="unscaledVal">The value to be converted to a <see cref="BigDecimal"/>.</param>
        /// <returns>
        /// Returns a <see cref="BigDecimal"/> instance with the value <paramref name="unscaledVal"/>.
        /// </returns>
        public static BigDecimal GetInstance(long unscaledVal) // ICU4N TODO: API - Ideally, this would be made into a constructor if/when BigDecimal is made into a struct.
        {
            if ((unscaledVal >= 0) && (unscaledVal < BiScaledByZeroLength))
            {
                return BiScaledByZero[(int)unscaledVal];
            }
            return new BigDecimal(unscaledVal, 0);
        }

        /// <summary>
        /// Returns a new <see cref="BigDecimal"/> instance whose value is equal to 
        /// <see cref="J2N.Numerics.Double.ToString(double, IFormatProvider?)"/> of the value
        /// in the invariant culture. For example, <c>GetInstance(0.1)</c> is converted to
        /// (unscaled=1, scale=1), although the double <c>0.1</c> cannot be
        /// represented exactly as a double value. In contrast to that, a new
        /// <c>BigDecimal(0.1)</c> instance has the value 
        /// <c>0.1000000000000000055511151231257827021181583404541015625</c> with an
        /// unscaled value <c>1000000000000000055511151231257827021181583404541015625</c>
        /// and the scale <c>55</c>.
        /// </summary>
        /// <param name="value"><see cref="double"/> value to be converted to a <see cref="BigDecimal"/>.</param>
        /// <returns></returns>
        /// <exception cref="OverflowException"><paramref name="value"/> is infinite or not a number.</exception>
        public static BigDecimal GetInstance(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                // math.03=Infinity or NaN
                throw new OverflowException(Messages.math03); //$NON-NLS-1$ // ICU4N: Changed to OverflowException to match .NET
            }
            // ICU4N TODO: Use the logic from Singulink BigDecimal FromFloat() to do a binary conversion here.
            return Parse(J2N.Numerics.Double.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }


        #endregion

        /**
        * Compares this {@code BigDecimal} with {@code val}. Returns one of the
        * three values {@code 1}, {@code 0}, or {@code -1}. The method behaves as
        * if {@code this.subtract(val)} is computed. If this difference is > 0 then
        * 1 is returned, if the difference is &lt; 0 then -1 is returned, and if the
        * difference is 0 then 0 is returned. This means, that if two decimal
        * instances are compared which are equal in value but differ in scale, then
        * these two instances are considered as equal.
        *
        * @param val
        *            value to be compared with {@code this}.
        * @return {@code 1} if {@code this > val}, {@code -1} if {@code this &lt; val},
        *         {@code 0} if {@code this == val}.
        * @throws NullPointerException
        *             if {@code val == null}.
        */
        public int CompareTo(BigDecimal val)
        {
            if (val is null) return 1; // Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

            int thisSign = Sign;
            int valueSign = val.Sign;

            if (thisSign == valueSign)
            {
                if (this._scale == val._scale && this._bitLength < 64 && val._bitLength < 64)
                {
                    return (smallValue < val.smallValue) ? -1 : (smallValue > val.smallValue) ? 1 : 0;
                }
                long diffScale = (long)this._scale - val._scale;
                int diffPrecision = this.AproxPrecision() - val.AproxPrecision();
                if (diffPrecision > diffScale + 1)
                {
                    return thisSign;
                }
                else if (diffPrecision < diffScale - 1)
                {
                    return -thisSign;
                }
                else
                {
                    // thisSign == val.signum()  and  diffPrecision is aprox. diffScale
                    BigInteger thisUnscaled = this.GetUnscaledValue();
                    BigInteger valUnscaled = val.GetUnscaledValue();
                    // If any of both precision is bigger, append zeros to the shorter one
                    if (diffScale < 0)
                    {
                        thisUnscaled = thisUnscaled * Multiplication.PowerOf10(-diffScale);
                    }
                    else if (diffScale > 0)
                    {
                        valUnscaled = valUnscaled * Multiplication.PowerOf10(diffScale);
                    }
                    return thisUnscaled.CompareTo(valUnscaled);
                }
            }
            else if (thisSign < valueSign)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Compares two <see cref="BigDecimal"/> values and returns an integer that indicates whether the first
        /// value is less than, equal to, or greater than the second value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>, as shown in the following table.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero </term>
        ///         <term><paramref name="left"/> precedes y in the sort order. -or- <paramref name="left"/> is <c>null</c> and <paramref name="right"/> is not <c>null</c>.</term>
        ///     </item>
        ///     <item>
        ///         <term>Zero </term>
        ///         <term><paramref name="left"/> is equal to <paramref name="right"/>. -or- <paramref name="left"/> and <paramref name="right"/> are both <c>null</c>.</term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero </term>
        ///         <term><paramref name="left"/> follows <paramref name="right"/> in the sort order. -or- <paramref name="right"/> is <c>null</c> and <paramref name="left"/> is not <c>null</c>.</term>
        ///     </item>
        /// </list>
        /// </returns>
        public static int Compare(BigDecimal left, BigDecimal right) // ICU4N: Added to match .NET BigInteger
        {
            if (!(left is null))
                return left.CompareTo(right);
            return -1;
        }

        /**
         * Returns the minimum of this {@code BigDecimal} and {@code val}.
         *
         * @param val
         *            value to be used to compute the minimum with this.
         * @return {@code min(this, val}.
         * @throws NullPointerException
         *             if {@code val == null}.
         */
        public static BigDecimal Min(BigDecimal a, BigDecimal val)
        {
            return ((Compare(a, val) <= 0) ? a : val);
        }

        /**
        * Returns the maximum of this {@code BigDecimal} and {@code val}.
        *
        * @param val
        *            value to be used to compute the maximum with this.
        * @return {@code max(this, val}.
        * @throws NullPointerException
        *             if {@code val == null}.
        */
        public static BigDecimal Max(BigDecimal a, BigDecimal val)
        {
            return ((Compare(a, val) >= 0) ? a : val);
        }

        /**
        * Returns {@code true} if {@code x} is a {@code BigDecimal} instance and if
        * this instance is equal to this big decimal. Two big decimals are equal if
        * their unscaled value and their scale is equal. For example, 1.0
        * (10*10^(-1)) is not equal to 1.00 (100*10^(-2)). Similarly, zero
        * instances are not equal if their scale differs.
        *
        * @param x
        *            object to be compared with {@code this}.
        * @return true if {@code x} is a {@code BigDecimal} and {@code this == x}.
        */
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (!(obj is BigDecimal bigDecimal))
                return false;

            return Equals(bigDecimal);
        }

        public bool Equals(BigDecimal other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return other._scale == _scale
                   && (_bitLength < 64
                       ? (other.smallValue == smallValue)
                       : intVal.Equals(other.intVal));
        }

        /**
        * Returns a hash code for this {@code BigDecimal}.
        *
        * @return hash code for {@code this}.
        */
        public override int GetHashCode()
        {
            int hashCode;
            if (_bitLength < 64)
            {
                hashCode = (int)(smallValue & 0xffffffff);
                hashCode = 33 * hashCode + (int)((smallValue >> 32) & 0xffffffff);
                hashCode = 17 * hashCode + _scale;
                return hashCode;
            }

            hashCode = 17 * intVal.GetHashCode() + _scale;
            return hashCode;
        }

        /* Private Methods */

        /**
        * It does all rounding work of the public method
        * {@code round(MathContext)}, performing an inplace rounding
        * without creating a new object.
        *
        * @param mc
        *            the {@code MathContext} for perform the rounding.
        * @see #round(MathContext)
        */
        internal void InplaceRound(MathContext mc)
        {
            int mcPrecision = mc.Precision;
            if (AproxPrecision() - mcPrecision <= 0 || mcPrecision == 0)
            {
                return;
            }
            int discardedPrecision = Precision - mcPrecision;
            // If no rounding is necessary it returns immediately
            if ((discardedPrecision <= 0))
            {
                return;
            }
            // When the number is small perform an efficient rounding
            if (this._bitLength < 64)
            {
                SmallRound(mc, discardedPrecision);
                return;
            }
            // Getting the integer part and the discarded fraction
            BigInteger sizeOfFraction = Multiplication.PowerOf10(discardedPrecision);
            BigInteger fraction;
            BigInteger integer = BigInteger.DivideAndRemainder(GetUnscaledValue(), sizeOfFraction, out fraction);
            long newScale = (long)_scale - discardedPrecision;
            int compRem;
            BigDecimal tempBD;
            // If the discarded fraction is non-zero, perform rounding
            if (fraction.Sign != 0)
            {
                // To check if the discarded fraction >= 0.5
                compRem = (BigInteger.Abs(fraction).ShiftLeftOneBit().CompareTo(sizeOfFraction));
                // To look if there is a carry
                compRem = RoundingBehavior(BigInteger.TestBit(integer, 0) ? 1 : 0, fraction.Sign * (5 + compRem),
                    mc.RoundingMode);
                if (compRem != 0)
                {
                    integer += BigInteger.GetInstance(compRem);
                }
                tempBD = new BigDecimal(integer);
                // If after to add the increment the precision changed, we normalize the size
                if (tempBD.Precision > mcPrecision)
                {
                    integer = integer / BigInteger.Ten;
                    newScale--;
                }
            }
            // To update all internal fields
            _scale = ToInt32Scale(newScale);
            _precision = mcPrecision;
            SetUnscaledValue(integer);
        }

        internal static int Int64CompareTo(long value1, long value2)
        {
            return value1 > value2 ? 1 : (value1 < value2 ? -1 : 0);
        }

        /**
        * This method implements an efficient rounding for numbers which unscaled
        * value fits in the type {@code long}.
        *
        * @param mc
        *            the context to use
        * @param discardedPrecision
        *            the number of decimal digits that are discarded
        * @see #round(MathContext)
        */
        private void SmallRound(MathContext mc, int discardedPrecision)
        {
            long sizeOfFraction = LongTenPow[discardedPrecision];
            long newScale = (long)_scale - discardedPrecision;
            long unscaledVal = smallValue;
            // Getting the integer part and the discarded fraction
            long integer = unscaledVal / sizeOfFraction;
            long fraction = unscaledVal % sizeOfFraction;
            int compRem;
            // If the discarded fraction is non-zero perform rounding
            if (fraction != 0)
            {
                // To check if the discarded fraction >= 0.5
                compRem = Int64CompareTo(Math.Abs(fraction) << 1, sizeOfFraction);
                // To look if there is a carry
                integer += RoundingBehavior(((int)integer) & 1, Math.Sign(fraction) * (5 + compRem),
                    mc.RoundingMode);
                // If after to add the increment the precision changed, we normalize the size
                if (Math.Log10(Math.Abs(integer)) >= mc.Precision)
                {
                    integer /= 10;
                    newScale--;
                }
            }
            // To update all internal fields
            _scale = ToInt32Scale(newScale);
            _precision = mc.Precision;
            smallValue = integer;
            _bitLength = CalcBitLength(integer);
            intVal = null;
        }

        /**
        * Return an increment that can be -1,0 or 1, depending of
        * {@code roundingMode}.
        *
        * @param parityBit
        *            can be 0 or 1, it's only used in the case
        *            {@code HALF_EVEN}
        * @param fraction
        *            the mantisa to be analyzed
        * @param roundingMode
        *            the type of rounding
        * @return the carry propagated after rounding
        */
        internal static int RoundingBehavior(int parityBit, int fraction, RoundingMode roundingMode)
        {
            int increment = 0; // the carry after rounding

            switch (roundingMode)
            {
                case RoundingMode.Unnecessary:
                    if (fraction != 0)
                    {
                        // math.08=Rounding necessary
                        throw new ArithmeticException(Messages.math08); //$NON-NLS-1$
                    }
                    break;
                case RoundingMode.Up:
                    increment = Math.Sign(fraction);
                    break;
                case RoundingMode.Down:
                    break;
                case RoundingMode.Ceiling:
                    increment = Math.Max(Math.Sign(fraction), 0);
                    break;
                case RoundingMode.Floor:
                    increment = Math.Min(Math.Sign(fraction), 0);
                    break;
                case RoundingMode.HalfUp:
                    if (Math.Abs(fraction) >= 5)
                    {
                        increment = Math.Sign(fraction);
                    }
                    break;
                case RoundingMode.HalfDown:
                    if (Math.Abs(fraction) > 5)
                    {
                        increment = Math.Sign(fraction);
                    }
                    break;
                case RoundingMode.HalfEven:
                    if (Math.Abs(fraction) + parityBit > 5)
                    {
                        increment = Math.Sign(fraction);
                    }
                    break;
            }
            return increment;
        }

        /**
        * If {@code intVal} has a fractional part throws an exception,
        * otherwise it counts the number of bits of value and checks if it's out of
        * the range of the primitive type. If the number fits in the primitive type
        * returns this number as {@code long}, otherwise throws an
        * exception.
        *
        * @param bitLengthOfType
        *            number of bits of the type whose value will be calculated
        *            exactly
        * @return the exact value of the integer part of {@code BigDecimal}
        *         when is possible
        * @throws ArithmeticException when rounding is necessary or the
        *             number don't fit in the primitive type
        */
        private long ValueExact(int bitLengthOfType)
        {
            BigInteger bigInteger = ToBigIntegerExact();

            if (bigInteger.BitLength < bitLengthOfType)
            {
                // It fits in the primitive type
                return bigInteger.ToInt64();
            }
            // math.08=Rounding necessary
            throw new ArithmeticException(Messages.math08); //$NON-NLS-1$
        }

        /**
        * If the precision already was calculated it returns that value, otherwise
        * it calculates a very good approximation efficiently . Note that this
        * value will be {@code precision()} or {@code precision()-1}
        * in the worst case.
        *
        * @return an approximation of {@code precision()} value
        */
        internal int AproxPrecision()
        {
            return ((_precision > 0) ? _precision : (int)((_bitLength - 1) * Log10Of2)) + 1;
        }

        /**
        * It tests if a scale of type {@code long} fits in 32 bits. It
        * returns the same scale being casted to {@code int} type when is
        * possible, otherwise throws an exception.
        *
        * @param longScale
        *            a 64 bit scale
        * @return a 32 bit scale when is possible
        * @throws ArithmeticException when {@code scale} doesn't
        *             fit in {@code int} type
        * @see #scale
        */
        internal static int ToInt32Scale(long longScale)
        {
            if (longScale < int.MinValue)
                // math.09=Overflow
                throw new OverflowException(Messages.math09); //$NON-NLS-1$

            if (longScale > int.MaxValue)
                // math.0A=Underflow
                throw new OverflowException(Messages.math0A); //$NON-NLS-1$

            return (int)longScale;
        }

        /**
        * It returns the value 0 with the most approximated scale of type
        * {@code int}. if {@code longScale > Integer.MAX_VALUE} the
        * scale will be {@code Integer.MAX_VALUE}; if
        * {@code longScale &lt; Integer.MIN_VALUE} the scale will be
        * {@code Integer.MIN_VALUE}; otherwise {@code longScale} is
        * casted to the type {@code int}.
        *
        * @param longScale
        *            the scale to which the value 0 will be scaled.
        * @return the value 0 scaled by the closer scale of type {@code int}.
        * @see #scale
        */
        internal static BigDecimal GetZeroScaledBy(long longScale)
        {
            if (longScale == (int)longScale)
            {
                return GetInstance(0, (int)longScale);
            }
            if (longScale >= 0)
            {
                return new BigDecimal(0, int.MaxValue);
            }
            return new BigDecimal(0, int.MinValue);
        }

        private BigInteger GetUnscaledValue()
        {
            if (intVal is null)
                intVal = BigInteger.GetInstance(smallValue);
            return intVal;
        }

        internal void SetUnscaledValue(BigInteger unscaledValue)
        {
            intVal = unscaledValue;
            _bitLength = unscaledValue.BitLength;
            if (_bitLength < 64)
            {
                smallValue = unscaledValue.ToInt64();
            }
        }

        internal static int CalcBitLength(long smallValue)
        {
            if (smallValue < 0)
            {
                smallValue = ~smallValue;
            }
            return 64 - smallValue.LeadingZeroCount();
        }

        internal static int CalcBitLength(int smallValue)
        {
            if (smallValue < 0)
            {
                smallValue = ~smallValue;
            }
            return 32 - smallValue.LeadingZeroCount();
        }

        /*
        [OnSerializing]
        internal void BeforeSerialization(StreamingContext context) {
        GetUnscaledValue();
        }

        [OnDeserialized]
        internal void AfterDeserialization(StreamingContext context) {
        intVal.AfterDeserialization(context);
        _bitLength = intVal.BitLength;
        if (_bitLength < 64) {
            smallValue = intVal.ToInt64();
        }
        }
        */

#if !PORTABLE
        private BigDecimal(SerializationInfo info, StreamingContext context)
        {
            intVal = (BigInteger)info.GetValue("intVal", typeof(BigInteger));
            _scale = info.GetInt32("scale");
            _bitLength = intVal.BitLength;
            if (_bitLength < 64)
            {
                smallValue = intVal.ToInt64();
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetUnscaledValue();
            info.AddValue("intVal", intVal, typeof(BigInteger));
            info.AddValue("scale", _scale);
        }

#endif

        #region Parse

        public static bool TryParse(char[] chars, int offset, int length, out BigDecimal value)
        {
            return TryParse(chars, offset, length, NumberFormatInfo.InvariantInfo, out value);
        }

        public static bool TryParse(char[] chars, int offset, int length, IFormatProvider provider, out BigDecimal value)
        {
            return TryParse(chars, offset, length, null, provider, out value);
        }

        public static bool TryParse(char[] chars, int offset, int length, MathContext context, out BigDecimal value)
        {
            return TryParse(chars, offset, length, context, null, out value);
        }

        public static bool TryParse(char[] chars, int offset, int length, MathContext context, IFormatProvider provider,
            out BigDecimal value)
        {
            Exception error;
            if (!DecimalString.TryParse(chars, offset, length, provider, out value, out error))
                return false;

            if (context != null)
                value.InplaceRound(context);

            return true;
        }

        public static bool TryParse(char[] chars, out BigDecimal value)
        {
            return TryParse(chars, (MathContext)null, out value);
        }

        public static bool TryParse(char[] chars, MathContext context, out BigDecimal value)
        {
            return TryParse(chars, context, NumberFormatInfo.InvariantInfo, out value);
        }

        public static bool TryParse(char[] chars, IFormatProvider provider, out BigDecimal value)
        {
            return TryParse(chars, null, provider, out value);
        }

        public static bool TryParse(char[] chars, MathContext context, IFormatProvider provider, out BigDecimal value)
        {
            if (chars == null)
            {
                value = null;
                return false;
            }

            return TryParse(chars, 0, chars.Length, context, provider, out value);
        }

        public static BigDecimal Parse(char[] chars, int offset, int length, IFormatProvider provider)
        {
            return Parse(chars, offset, length, null, provider);
        }

        public static BigDecimal Parse(char[] chars, int offset, int length)
        {
            return Parse(chars, offset, length, (MathContext)null);
        }

        public static BigDecimal Parse(char[] chars, int offset, int length, MathContext context)
        {
            return Parse(chars, offset, length, context, NumberFormatInfo.InvariantInfo);
        }

        public static BigDecimal Parse(char[] chars, int offset, int length, MathContext context, IFormatProvider provider)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            return Parse(new string(chars, offset, length), context, provider);

            //Exception error;
            //BigDecimal value;
            //if (!DecimalString.TryParse(chars, offset, length, provider, out value, out error))
            //    throw error;

            //if (context != null)
            //    value.InplaceRound(context);

            //return value;
        }

        public static BigDecimal Parse(char[] chars, IFormatProvider provider)
        {
            return Parse(chars, null, provider);
        }

        public static BigDecimal Parse(char[] chars)
        {
            return Parse(chars, (MathContext)null);
        }

        public static BigDecimal Parse(char[] chars, MathContext context)
        {
            return Parse(chars, context, NumberFormatInfo.InvariantInfo);
        }

        public static BigDecimal Parse(char[] chars, MathContext context, IFormatProvider provider)
        {
            if (chars is null)
                throw new ArgumentNullException(nameof(chars));
            if (chars.Length == 0)
                throw new FormatException(string.Format(SR.Format_EmptyInputString, chars));

            return Parse(chars, 0, chars.Length, context, provider);
            //return Parse(new string(chars, 0, chars.Length), )
        }

        public static bool TryParse(string s, out BigDecimal value)
        {
            return TryParse(s, (MathContext)null, out value);
        }

        public static bool TryParse(string s, MathContext context, out BigDecimal value)
        {
            return TryParse(s, context, NumberFormatInfo.InvariantInfo, out value);
        }

        public static bool TryParse(string s, IFormatProvider provider, out BigDecimal value)
        {
            return TryParse(s, null, provider, out value);
        }

        public static bool TryParse(string s, MathContext context, IFormatProvider provider, out BigDecimal value)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            if (string.IsNullOrWhiteSpace(s))
            {
                value = null;
                return false;
            }

            var data = s.ToCharArray(); // ICU4N TODO: Eliminate allocation

            Exception error;
            if (!DecimalString.TryParse(data, 0, data.Length, provider, out value, out error)) // ICU4N TODO: Don't allocate error within Try method
                return false;

            if (context != null)
                value.InplaceRound(context);

            return true;
        }

        public static BigDecimal Parse(string s, IFormatProvider provider)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
                throw new FormatException(string.Format(SR.Format_EmptyInputString, s));

            //return Parse(s, null, provider);
            return ParseNumbers.StringToBigDecimal(s, startIndex: 0, length: s.Length);
        }

        public static BigDecimal Parse(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
                throw new FormatException(string.Format(SR.Format_EmptyInputString, s));

            //return Parse(s, (MathContext)null);
            return ParseNumbers.StringToBigDecimal(s, startIndex: 0, length: s.Length);
        }

        public static BigDecimal Parse(string s, MathContext context)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
                throw new FormatException(string.Format(SR.Format_EmptyInputString, s));

            //return Parse(s, context, NumberFormatInfo.InvariantInfo);
            return ParseNumbers.StringToBigDecimal(s, startIndex: 0, length: s.Length, mathContext: context);
        }

        public static BigDecimal Parse(string s, MathContext context, IFormatProvider provider)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
                throw new FormatException(string.Format(SR.Format_EmptyInputString, s));

            var data = s.ToCharArray();

            Exception error;
            BigDecimal value;
            if (!DecimalString.TryParse(data, 0, data.Length, provider, out value, out error)) // ICU4N TODO: Don't allocate error within Try method
                throw error;

            if (context != null)
                value.InplaceRound(context);

            return value;
        }


        #endregion

        #region Arithmetic Operators

        public static BigDecimal operator +(BigDecimal a, BigDecimal b)
        {
            // In case of implicit operators apply the precision of the dividend
            return Add(a, b);
        }

        public static BigDecimal operator -(BigDecimal a, BigDecimal b)
        {
            // In case of implicit operators apply the precision of the dividend
            return Subtract(a, b);
        }

        public static BigDecimal operator /(BigDecimal a, BigDecimal b)
        {
            // In case of implicit operators apply the precision of the dividend
            return Divide(a, b);
        }

        public static BigDecimal operator %(BigDecimal a, BigDecimal b)
        {
            // In case of implicit operators apply the precision of the dividend
            return Remainder(a, b); // ICU4N TODO: Shouldn't this be Mod? It is on BigInteger.
        }

        public static BigDecimal operator *(BigDecimal a, BigDecimal b)
        {
            // In case of implicit operators apply the precision of the dividend
            return Multiply(a, b);
        }

        public static BigDecimal operator +(BigDecimal a)
        {
            return Plus(a);
        }

        public static BigDecimal operator -(BigDecimal a)
        {
            return Negate(a);
        }

        public static bool operator ==(BigDecimal a, BigDecimal b)
        {
            if ((object)a == null && (object)b == null)
                return true;
            if ((object)a == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(BigDecimal a, BigDecimal b)
        {
            return !(a == b);
        }

        public static bool operator >(BigDecimal a, BigDecimal b)
        {
            return Compare(a, b) < 0;
        }

        public static bool operator <(BigDecimal a, BigDecimal b)
        {
            return Compare(a, b) > 0;
        }

        public static bool operator >=(BigDecimal a, BigDecimal b)
        {
            return a == b || a > b;
        }

        public static bool operator <=(BigDecimal a, BigDecimal b)
        {
            return a == b || a < b;
        }

        public static BigDecimal operator >>(BigDecimal a, int b)
        {
            return BigInteger.ShiftRight((BigInteger)a, b); // ICU4N TODO: Need to verify this
        }

        public static BigDecimal operator <<(BigDecimal a, int b)
        {
            return BigInteger.ShiftLeft((BigInteger)a, b); // ICU4N TODO: Need to verify this
        }

        #endregion

        #region Explicit Operators

        public static explicit operator byte(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToByte();
        }

        public static explicit operator sbyte(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToSByte();
        }

        public static explicit operator short(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToInt16Exact(); // ICU4N TODO: Is "Exact" right?
        }

        public static explicit operator int(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToInt32();
        }

        public static explicit operator long(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToInt64();
        }

        public static explicit operator float(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToSingle();
        }

        public static explicit operator double(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToDouble();
        }

        public static explicit operator BigInteger(BigDecimal d)
        {
            if (d is null) return default;
            return d.ToBigInteger();
        }

        #endregion

        #region Implicit Operators

        // ICU4N TODO: Conversion to/from decimal

        public static implicit operator BigDecimal(long value)
        {
            return new BigDecimal(value);
        }

        public static implicit operator BigDecimal(double value)
        {
            return new BigDecimal(value);
        }

        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value);
        }

        public static implicit operator BigDecimal(BigInteger value)
        {
            return new BigDecimal(value);
        }

        #endregion
    }
}
