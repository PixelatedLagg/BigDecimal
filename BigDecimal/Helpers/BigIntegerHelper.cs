﻿namespace ExtendedNumerics.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Exceptions;

public static class BigIntegerHelper
{
	public static BigInteger GCD( this IEnumerable<BigInteger> numbers ) => numbers.Aggregate( GCD );

	public static BigInteger GCD( BigInteger value1, BigInteger value2 )
	{
		var absValue1 = BigInteger.Abs( value1 );
		var absValue2 = BigInteger.Abs( value2 );

		while ( absValue1 != 0 && absValue2 != 0 )
		{
			if ( absValue1 > absValue2 )
			{
				absValue1 %= absValue2;
			}
			else
			{
				absValue2 %= absValue1;
			}
		}

		return BigInteger.Max( absValue1, absValue2 );
	}

	public static Int32 GetLength( this BigInteger source )
	{
		var result = 0;
		var copy = BigInteger.Abs( source );
		while ( copy > 0 )
		{
			copy /= 10;
			result++;
		}

		return result;
	}

	public static IEnumerable<BigInteger> GetRange( BigInteger min, BigInteger max )
	{
		while ( min < max )
		{
			yield return min;
			min++;
		}
	}

	public static Int32 GetSignifigantDigits( this BigInteger value )
	{
		if ( value.IsZero )
		{
			return 0;
		}

		var valueString = value.ToString().TrimEnd( '0' );

		if ( String.IsNullOrEmpty( valueString ) )
		{
			return 0;
		}

		if ( value < BigInteger.Zero )
		{
			return valueString.Length - 1;
		}

		return valueString.Length;
	}

	public static Boolean IsCoprime( BigInteger value1, BigInteger value2 ) => GCD( value1, value2 ) == 1;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	[Pure]
	public static BigInteger LCM( IEnumerable<BigInteger> numbers ) => numbers.Aggregate( LCM );

	public static BigInteger LCM( BigInteger num1, BigInteger num2 )
	{
		var absValue1 = BigInteger.Abs( num1 );
		var absValue2 = BigInteger.Abs( num2 );
		return absValue1 * absValue2 / GCD( absValue1, absValue2 );
	}

	// Returns the NTHs root of a BigInteger with Remainder. The root must be greater than or equal to 1 or value must be a
	// positive integer.
	public static BigInteger NthRoot( this BigInteger value, Int32 root, out BigInteger remainder )
	{
		if ( root < 1 )
		{
			throw new ArgumentException( "root must be greater than or equal to 1", nameof( root ) );
		}

		if ( value.Sign == -1 )
		{
			throw new ArgumentException( "value must be a positive integer", nameof( value ) );
		}

		if ( value == BigInteger.One )
		{
			remainder = 0;
			return BigInteger.One;
		}

		if ( value == BigInteger.Zero )
		{
			remainder = 0;
			return BigInteger.Zero;
		}

		if ( root == 1 )
		{
			remainder = 0;
			return value;
		}

		var upperbound = value;
		var lowerbound = BigInteger.Zero;

		while ( true )
		{
			var nval = ( upperbound + lowerbound ) >> 1;
			var tstsq = BigInteger.Pow( nval, root );
			if ( tstsq > value )
			{
				upperbound = nval;
			}

			if ( tstsq < value )
			{
				lowerbound = nval;
			}

			if ( tstsq == value )
			{
				lowerbound = nval;
				break;
			}

			if ( lowerbound == upperbound - 1 )
			{
				break;
			}
		}

		remainder = value - BigInteger.Pow( lowerbound, root );
		return lowerbound;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	[Pure]
	public static BigInteger Square( this BigInteger input ) => input * input;

	[NeedsTesting]
	public static BigInteger SquareRoot( this BigInteger input )
	{
		if ( input.IsZero )
		{
			return BigInteger.Zero;
		}

		var n = BigInteger.Zero;
		var p = BigInteger.Zero;
		var low = BigInteger.Zero;
		var high = BigInteger.Abs( input );

		while ( high > low + 1 )
		{
			n = ( high + low ) >> 1;
			p = n * n;
			if ( input < p )
			{
				high = n;
			}
			else if ( input > p )
			{
				low = n;
			}
			else
			{
				break;
			}
		}

		return input == p ? n : low;
	}

	/// <summary>
	///     <para>Attempt to parse a fraction from a String.</para>
	/// </summary>
	/// <example>" 1234.45 / 346.456 "</example>
	/// <param name="numberString"></param>
	/// <param name="result"></param>
	/// <exception cref="OutOfRangeException">Uncomment this if you want an exception instead of a Boolean.</exception>
	[NeedsTesting]
	public static Boolean TryParseFraction( this String numberString, out BigDecimal? result )
	{
		result = default( BigDecimal? );

		if ( String.IsNullOrWhiteSpace( numberString ) )
		{
			return false;
		}

		var parts = numberString.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() ).ToList();

		if ( parts.Count != 2 )
		{
			return false;
		}

		try
		{
			var numerator = BigDecimal.Parse( parts[0] );
			var denominator = BigDecimal.Parse( parts[1] );

			result = BigDecimal.Divide( numerator, denominator );
			return true;
		}
		catch ( Exception )
		{

			//throw new OutOfRangeException( "Couldn't parse numerator or denominator." );
			return false;
		}
	}


	/// <summary>
	/// Calculates a factorial by the divide and conquer method.
	/// This is faster than repeatedly multiplying the next value by a running product
	/// by not repeatedly multiplying by large values.
	/// Essentially, this multiplies every number in the array with its neighbor, 
	/// returning an array half as long of products of two numbers.
	/// We then take that array and multiply each pair of values in the array
	/// with its neighbor, resulting in another array half the length of the previous one, and so on...
	/// This results in many multiplications of small, equally sized operands 
	/// and only a few multiplications of larger operands.
	/// In the limit, this is more efficient.
	/// 
	/// The factorial function is used during the calculation of trigonometric functions to arbitrary precision.
	/// </summary>
	public static class FastFactorial
	{
		public static BigInteger Factorial(BigInteger value)
		{
			if (value == 0 || value == 1) { return 1; }
			return MultiplyRange(2, value);
		}

		/// <summary>Divide the range of numbers to multiply in half recursively.</summary>
		private static BigInteger MultiplyRange(BigInteger from, BigInteger to)
		{
			var diff = to - from;
			if (diff == 1) { return from * to; }
			if (diff == 0) { return from; }

			BigInteger half = (from + to) / 2;
			return BigInteger.Multiply(MultiplyRange(from, half), MultiplyRange(half + 1, to));
		}
	}
}