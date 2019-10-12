using System;
using System.Collections.Generic;
using System.Diagnostics;

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

	using HashFunction = Org.Neo4j.Hashing.HashFunction;
	using Org.Neo4j.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

	public abstract class StringValue : TextValue
	{
		 internal abstract string Value();

		 public override bool Equals( Value value )
		 {
			  return value.Equals( value() );
		 }

		 public override bool Equals( char x )
		 {
			  return Value().Length == 1 && Value()[0] == x;
		 }

		 public override bool Equals( string x )
		 {
			  return Value().Equals(x);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteString( Value() );
		 }

		 public override TextValue ToLower()
		 {
			  return new StringWrappingStringValue( Value().ToLower() );
		 }

		 public override TextValue ToUpper()
		 {
			  return new StringWrappingStringValue( Value().ToUpper() );
		 }

		 public override ListValue Split( string separator )
		 {
			  Debug.Assert( !string.ReferenceEquals( separator, null ) );
			  string asString = Value();
			  //Cypher has different semantics for the case where the separator
			  //is exactly the value, in cypher we expect two empty arrays
			  //where as java returns an empty array
			  if ( separator.Equals( asString ) )
			  {
					return EmptySplit;
			  }
			  else if ( separator.Length == 0 )
			  {
					return VirtualValues.fromArray( Values.CharArray( asString.ToCharArray() ) );
			  }

			  IList<AnyValue> split = SplitNonRegex( asString, separator );
			  return VirtualValues.fromList( split );
		 }

		 /// <summary>
		 /// Splits a string.
		 /// </summary>
		 /// <param name="input"> String to be split </param>
		 /// <param name="delim"> delimiter, must not be not empty </param>
		 /// <returns> the split string as a List of TextValues </returns>
		 private static IList<AnyValue> SplitNonRegex( string input, string delim )
		 {
			  IList<AnyValue> l = new List<AnyValue>();
			  int offset = 0;

			  while ( true )
			  {
					int index = input.IndexOf( delim, offset, StringComparison.Ordinal );
					if ( index == -1 )
					{
						 string substring = input.Substring( offset );
						 l.Add( Values.StringValue( substring ) );
						 return l;
					}
					else
					{
						 string substring = input.Substring( offset, index - offset );
						 l.Add( Values.StringValue( substring ) );
						 offset = index + delim.Length;
					}
			  }
		 }

		 public override TextValue Replace( string find, string replace )
		 {
			  Debug.Assert( !string.ReferenceEquals( find, null ) );
			  Debug.Assert( !string.ReferenceEquals( replace, null ) );

			  return Values.StringValue( Value().Replace(find, replace) );
		 }

		 public override object AsObjectCopy()
		 {
			  return Value();
		 }

		 public override string ToString()
		 {
			  return format( "%s(\"%s\")", TypeName, Value() );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "String";
			 }
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public override string StringValueConflict()
		 {
			  return Value();
		 }

		 public override string PrettyPrint()
		 {
			  return format( "'%s'", Value() );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapString( this );
		 }

		 //NOTE: this doesn't respect code point order for code points that doesn't fit 16bits
		 public override int CompareTo( TextValue other )
		 {
			  string thisString = Value();
			  string thatString = other.StringValue();
			  return thisString.CompareTo( thatString );
		 }

		 internal static TextValue EMPTY = new StringValueAnonymousInnerClass();

		 private class StringValueAnonymousInnerClass : StringValue
		 {
			 protected internal override int computeHash()
			 {
				  return 0;
			 }

			 public override long updateHash( HashFunction hashFunction, long hash )
			 {
				  return hashFunction.Update( hash, 0 ); // Mix in our length; a single zero.
			 }

			 public override int length()
			 {
				  return 0;
			 }

			 public override TextValue substring( int start, int end )
			 {
				  return this;
			 }

			 public override TextValue trim()
			 {
				  return this;
			 }

			 public override TextValue ltrim()
			 {
				  return this;
			 }

			 public override TextValue rtrim()
			 {
				  return this;
			 }

			 public override TextValue reverse()
			 {
				  return this;
			 }

			 public override TextValue plus( TextValue other )
			 {
				  return other;
			 }

			 public override bool startsWith( TextValue other )
			 {
				  return other.Length() == 0;
			 }

			 public override bool endsWith( TextValue other )
			 {
				  return other.Length() == 0;
			 }

			 public override bool contains( TextValue other )
			 {
				  return other.Length() == 0;
			 }

			 public override TextValue toLower()
			 {
				  return this;
			 }

			 public override TextValue toUpper()
			 {
				  return this;
			 }

			 public override TextValue replace( string find, string replace )
			 {
				  if ( find.Length == 0 )
				  {
						return Values.StringValue( replace );
				  }
				  else
				  {
						return this;
				  }
			 }

			 public override int compareTo( TextValue other )
			 {
				  return -other.Length();
			 }

			 internal override Matcher matcher( Pattern pattern )
			 {
				  return pattern.matcher( "" );
			 }

			 internal override string value()
			 {
				  return "";
			 }
		 }
	}


}