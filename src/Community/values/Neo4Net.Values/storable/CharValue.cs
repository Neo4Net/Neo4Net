using System;
using System.Diagnostics;

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
namespace Neo4Net.Values.Storable
{

	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.list;

	public sealed class CharValue : TextValue
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly char ValueConflict;

		 internal CharValue( char value )
		 {
			  this.ValueConflict = value;
		 }

		 public override bool Eq( object other )
		 {
			  return other is Value && Equals( ( Value ) other );
		 }

		 public override bool Equals( Value other )
		 {
			  return other.Equals( ValueConflict );
		 }

		 public override bool Equals( char x )
		 {
			  return ValueConflict == x;
		 }

		 public override bool Equals( string x )
		 {
			  return x.Length == 1 && x[0] == ValueConflict;
		 }

		 public override int ComputeHash()
		 {
			  //The 31 is there to give it the same hash as the string equivalent
			  return 31 + ValueConflict;
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  return UpdateHash( hashFunction, hash, ValueConflict );
		 }

		 public static long UpdateHash( IHashFunction hashFunction, long hash, char value )
		 {
			  hash = hashFunction.Update( hash, value );
			  return hashFunction.Update( hash, 1 ); // Pretend we're a string of length 1.
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void WriteTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteString( ValueConflict );
		 }

		 public override object AsObjectCopy()
		 {
			  return ValueConflict;
		 }

		 public override string PrettyPrint()
		 {
			  return format( "'%s'", ValueConflict );
		 }

		 public override string StringValue()
		 {
			  return Convert.ToString( ValueConflict );
		 }

		 public override int Length()
		 {
			  return 1;
		 }

		 public override TextValue Substring( int start, int length )
		 {
			  if ( length != 1 && start != 0 )
			  {
					return StringValue.EMPTY;
			  }

			  return this;
		 }

		 public override TextValue Trim()
		 {
			  if ( char.IsWhiteSpace( ValueConflict ) )
			  {
					return StringValue.EMPTY;
			  }
			  else
			  {
					return this;
			  }
		 }

		 public override TextValue Ltrim()
		 {
			  return Trim();
		 }

		 public override TextValue Rtrim()
		 {
			  return Trim();
		 }

		 public override TextValue ToLower()
		 {
			  return new CharValue( char.ToLower( ValueConflict ) );
		 }

		 public override TextValue ToUpper()
		 {
			  return new CharValue( char.ToUpper( ValueConflict ) );
		 }

		 public override ListValue Split( string separator )
		 {
			  if ( separator.Equals( StringValue() ) )
			  {
					return EmptySplit;
			  }
			  else
			  {
					return list( Values.StringValue( StringValue() ) );
			  }
		 }

		 public override TextValue Replace( string find, string replace )
		 {
			  Debug.Assert( !string.ReferenceEquals( find, null ) );
			  Debug.Assert( !string.ReferenceEquals( replace, null ) );
			  if ( StringValue().Equals(find) )
			  {
					return Values.StringValue( replace );
			  }
			  else
			  {
					return this;
			  }
		 }

		 public override TextValue Reverse()
		 {
			  return this;
		 }

		 public override TextValue Plus( TextValue other )
		 {
			  return Values.StringValue( ValueConflict + other.StringValue() );
		 }

		 public override bool StartsWith( TextValue other )
		 {
			  return other.Length() == 1 && other.StringValue()[0] == ValueConflict;
		 }

		 public override bool EndsWith( TextValue other )
		 {
			  return StartsWith( other );
		 }

		 public override bool Contains( TextValue other )
		 {
			  return StartsWith( other );
		 }

		 public char Value()
		 {
			  return ValueConflict;
		 }

		 public override int CompareTo( TextValue other )
		 {
			  return TextValues.CompareCharToString( ValueConflict, other.StringValue() );
		 }

		 public override T Map<T>( IValueMapper<T> mapper )
		 {
			  return mapper.MapChar( this );
		 }

		 internal override Matcher Matcher( Pattern pattern )
		 {
			  return pattern.matcher( "" + ValueConflict ); // TODO: we should be able to do this without allocation
		 }

		 public override string ToString()
		 {
			  return format( "%s('%s')", TypeName, ValueConflict );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Char";
			 }
		 }
	}

}