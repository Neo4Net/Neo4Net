/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.isPowerOfTwo;

	/// <summary>
	/// A set of static convenience methods for checking ctor/method parameters or state.
	/// </summary>
	public sealed class Preconditions
	{
		 private Preconditions()
		 {
			  throw new AssertionError( "no instances" );
		 }

		 /// <summary>
		 /// Ensures that {@code value} is greater than or equal to {@code 1} or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's greater than or equal to {@code 1} </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is less than 1 </exception>
		 public static long RequirePositive( long value )
		 {
			  if ( value < 1 )
			  {
					throw new System.ArgumentException( "Expected positive long value, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code value} is a power of 2 or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's a power of 2 </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is not power of 2 </exception>
		 public static long RequirePowerOfTwo( long value )
		 {
			  if ( !isPowerOfTwo( value ) )
			  {
					throw new System.ArgumentException( "Expected long value to be a non zero power of 2, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code value} is greater than or equal to {@code 1} or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's greater than or equal to {@code 1} </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is less than 1 </exception>
		 public static int RequirePositive( int value )
		 {
			  if ( value < 1 )
			  {
					throw new System.ArgumentException( "Expected positive int value, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code value} is greater than or equal to {@code 0} or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's greater than or equal to {@code 0} </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is less than 0 </exception>
		 public static long RequireNonNegative( long value )
		 {
			  if ( value < 0 )
			  {
					throw new System.ArgumentException( "Expected non-negative long value, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code value} is greater than or equal to {@code 0} or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's greater than or equal to {@code 0} </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is less than 0 </exception>
		 public static int RequireNonNegative( int value )
		 {
			  if ( value < 0 )
			  {
					throw new System.ArgumentException( "Expected non-negative int value, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code value} is exactly zero.
		 /// </summary>
		 /// <param name="value"> a value for check </param>
		 /// <returns> {@code value} if it's equal to {@code 0}. </returns>
		 /// <exception cref="IllegalArgumentException"> if {@code value} is not 0 </exception>
		 public static int RequireExactlyZero( int value )
		 {
			  if ( value != 0 )
			  {
					throw new System.ArgumentException( "Expected int value equal to 0, got " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Ensures that {@code expression} is {@code true} or throws <seealso cref="System.InvalidOperationException"/> otherwise.
		 /// </summary>
		 /// <param name="expression"> an expression for check </param>
		 /// <param name="message"> error message for the exception </param>
		 /// <exception cref="IllegalStateException"> if {@code expression} is {@code false} </exception>
		 public static void CheckState( bool expression, string message )
		 {
			  if ( !expression )
			  {
					throw new System.InvalidOperationException( message );
			  }
		 }

		 /// <summary>
		 /// Ensures that {@code expression} is {@code true} or throws <seealso cref="System.ArgumentException"/> otherwise.
		 /// </summary>
		 /// <param name="expression"> an expression for check </param>
		 /// <param name="message"> error message for the exception </param>
		 /// <exception cref="IllegalArgumentException"> if {@code expression} is {@code false} </exception>
		 public static void CheckArgument( bool expression, string message, params object[] args )
		 {
			  if ( !expression )
			  {
					throw new System.ArgumentException( args.Length > 0 ? format( message, args ) : message );
			  }
		 }

		 public static void RequireBetween( int value, int lowInclusive, int highExclusive )
		 {
			  if ( value < lowInclusive || value >= highExclusive )
			  {
					throw new System.ArgumentException( string.Format( "Expected int value between {0:D} (inclusive) and {1:D} (exclusive), got {2:D}.", lowInclusive, highExclusive, value ) );
			  }
		 }
	}

}