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
namespace Neo4Net.Collections.Helpers
{

	/// <summary>
	/// Utility to handle pairs of objects.
	/// </summary>
	/// @param <T1> the type of the <seealso cref="first() first value"/> of the pair. </param>
	/// @param <T2> the type of the <seealso cref="other() other value"/> of the pair. </param>
	public abstract class Pair<T1, T2>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final Pair EMPTY = Pair.of(null, null);
		 private static readonly Pair _empty = Pair.Of( null, null );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T1, T2> Pair<T1,T2> empty()
		 public static Pair<T1, T2> Empty<T1, T2>()
		 {
			  return _empty;
		 }

		 /// <summary>
		 /// Create a new pair of objects.
		 /// </summary>
		 /// <param name="first"> the first object in the pair. </param>
		 /// <param name="other"> the other object in the pair. </param>
		 /// @param <T1> the type of the first object in the pair </param>
		 /// @param <T2> the type of the second object in the pair </param>
		 /// <returns> a new pair of the two parameters. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T1, T2> Pair<T1,T2> pair(final T1 first, final T2 other)
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Pair<T1, T2> PairConflict<T1, T2>( T1 first, T2 other )
		 {
			  return new PairAnonymousInnerClass( first, other );
		 }

		 private class PairAnonymousInnerClass : Pair<T1, T2>
		 {
			 private T1 _first;
			 private T2 _other;

			 public PairAnonymousInnerClass( T1 first, T2 other )
			 {
				 this._first = first;
				 this._other = other;
			 }

			 public override T1 first()
			 {
				  return _first;
			 }

			 public override T2 other()
			 {
				  return _other;
			 }
		 }

		 /// <summary>
		 /// Alias of <seealso cref="pair(object, object)"/>. </summary>
		 /// <param name="first"> the first object in the pair. </param>
		 /// <param name="other"> the other object in the pair. </param>
		 /// @param <T1> the type of the first object in the pair </param>
		 /// @param <T2> the type of the second object in the pair </param>
		 /// <returns> a new pair of the two parameters. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T1, T2> Pair<T1, T2> of(final T1 first, final T2 other)
		 public static Pair<T1, T2> Of<T1, T2>( T1 first, T2 other )
		 {
			  return PairConflict( first, other );
		 }

		 internal Pair()
		 {
			  // package private, limited number of subclasses
		 }

		 /// <returns> the first object in the pair. </returns>
		 public abstract T1 First();

		 /// <returns> the other object in the pair. </returns>
		 public abstract T2 Other();

		 public override string ToString()
		 {
			  return "(" + First() + ", " + Other() + ")";
		 }

		 public override int GetHashCode()
		 {
			  return ( 31 * GetHashCode( First() ) ) | GetHashCode(Other());
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj is Pair )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") Pair that = (Pair) obj;
					Pair that = ( Pair ) obj;
					return Equals( this.Other(), that.Other() ) && Equals(this.First(), that.First());
			  }
			  return false;
		 }

		 internal static int GetHashCode( object obj )
		 {
			  return obj == null ? 0 : obj.GetHashCode();
		 }

		 internal static bool Equals( object obj1, object obj2 )
		 {
			  return ( obj1 == obj2 ) || ( obj1 != null && obj1.Equals( obj2 ) );
		 }
	}

}