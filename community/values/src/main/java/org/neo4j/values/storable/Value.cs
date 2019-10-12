using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Values.Storable
{

	using Geometry = Org.Neo4j.Graphdb.spatial.Geometry;
	using HashFunction = Org.Neo4j.Hashing.HashFunction;
	using Org.Neo4j.Values;
	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	public abstract class Value : AnyValue
	{
		 internal static readonly Pattern MapPattern = Pattern.compile( "\\{(.*)\\}" );

		 internal static readonly Pattern KeyValuePattern = Pattern.compile( "(?:\\A|,)\\s*+(?<k>[a-z_A-Z]\\w*+)\\s*:\\s*(?<v>[^\\s,]+)" );

		 internal static readonly Pattern QuotesPattern = Pattern.compile( "^[\"']|[\"']$" );

		 public override bool Eq( object other )
		 {
			  return other is Value && Equals( ( Value ) other );
		 }

		 public abstract boolean ( Value other );

		 public virtual bool Equals( sbyte[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( short[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( int[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( long[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( float[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( double[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( bool x )
		 {
			  return false;
		 }

		 public virtual bool Equals( bool[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( long x )
		 {
			  return false;
		 }

		 public virtual bool Equals( double x )
		 {
			  return false;
		 }

		 public virtual bool Equals( char x )
		 {
			  return false;
		 }

		 public virtual bool Equals( string x )
		 {
			  return false;
		 }

		 public virtual bool Equals( char[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( string[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( Geometry[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( ZonedDateTime[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( LocalDate[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( DurationValue[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( DateTime[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( LocalTime[] x )
		 {
			  return false;
		 }

		 public virtual bool Equals( OffsetTime[] x )
		 {
			  return false;
		 }

		 public override bool? TernaryEquals( AnyValue other )
		 {
			  if ( other == null || other == NO_VALUE )
			  {
					return null;
			  }
			  if ( other.SequenceValue && this.SequenceValue )
			  {
					return ( ( SequenceValue ) this ).ternaryEquality( ( SequenceValue ) other );
			  }
			  if ( other is Value && ( ( Value ) other ).ValueGroup() == ValueGroup() )
			  {
					Value otherValue = ( Value ) other;
					if ( this.NaN || otherValue.NaN )
					{
						 return null;
					}
					return Equals( otherValue );
			  }
			  return false;
		 }

		 internal abstract int UnsafeCompareTo( Value other );

		 /// <summary>
		 /// Should return {@code null} for values that cannot be compared
		 /// under Comparability semantics.
		 /// </summary>
		 internal virtual Comparison UnsafeTernaryCompareTo( Value other )
		 {
			  return Comparison.from( UnsafeCompareTo( other ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.neo4j.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  WriteTo( ( ValueWriter<E> )writer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract <E extends Exception> void writeTo(ValueWriter<E> writer) throws E;
		 public abstract void writeTo<E>( ValueWriter<E> writer ) where E : Exception;

		 /// <summary>
		 /// Return this value as a regular java boxed primitive, String or primitive array. This method performs defensive
		 /// copying when needed, so the returned value is safe to modify.
		 /// </summary>
		 /// <returns> the object version of the current value </returns>
		 public abstract object AsObjectCopy();

		 /// <summary>
		 /// Return this value as a regular java boxed primitive, String or primitive array. This method does not clone
		 /// primitive arrays.
		 /// </summary>
		 /// <returns> the object version of the current value </returns>
		 public virtual object AsObject()
		 {
			  return AsObjectCopy();
		 }

		 /// <summary>
		 /// Returns a json-like string representation of the current value.
		 /// </summary>
		 public abstract string PrettyPrint();

		 public abstract ValueGroup ValueGroup();

		 public abstract NumberType NumberType();

		 public long HashCode64()
		 {
			  HashFunction xxh64 = HashFunction.incrementalXXH64();
			  long seed = 1; // Arbitrary seed, but it must always be the same or hash values will change.
			  return xxh64.Finalise( UpdateHash( xxh64, xxh64.Initialise( seed ) ) );
		 }

		 public abstract long UpdateHash( HashFunction hashFunction, long hash );

		 public virtual bool NaN
		 {
			 get
			 {
				  return false;
			 }
		 }

		 internal static void ParseHeaderInformation( CharSequence text, string type, CSVHeaderInformation info )
		 {
			  Matcher mapMatcher = MapPattern.matcher( text );
			  string errorMessage = format( "Failed to parse %s value: '%s'", type, text );
			  if ( !( mapMatcher.find() && mapMatcher.groupCount() == 1 ) )
			  {
					throw new InvalidValuesArgumentException( errorMessage );
			  }

			  string mapContents = mapMatcher.group( 1 );
			  if ( mapContents.Length == 0 )
			  {
					throw new InvalidValuesArgumentException( errorMessage );
			  }

			  Matcher matcher = KeyValuePattern.matcher( mapContents );
			  if ( !( matcher.find() ) )
			  {
					throw new InvalidValuesArgumentException( errorMessage );
			  }

			  do
			  {
					string key = matcher.group( "k" );
					if ( !string.ReferenceEquals( key, null ) )
					{
						 string value = matcher.group( "v" );
						 if ( !string.ReferenceEquals( value, null ) )
						 {
							  info.Assign( key, value );
						 }
					}
			  } while ( matcher.find() );
		 }
	}

}