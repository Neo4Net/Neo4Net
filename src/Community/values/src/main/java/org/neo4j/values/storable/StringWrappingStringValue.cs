using System;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.Storable
{

	using HashFunction = Neo4Net.Hashing.HashFunction;

	/// <summary>
	/// Implementation of StringValue that wraps a `java.lang.String` and
	/// delegates methods to that instance.
	/// </summary>
	internal sealed class StringWrappingStringValue : StringValue
	{
		 private readonly string _value;

		 internal StringWrappingStringValue( string value )
		 {
			  Debug.Assert( !string.ReferenceEquals( value, null ) );
			  this._value = value;
		 }

		 internal override string Value()
		 {
			  return _value;
		 }

		 public override int Length()
		 {
			  return _value.codePointCount( 0, _value.Length );
		 }

		 public override int ComputeHash()
		 {
			  //NOTE that we are basing the hash code on code points instead of char[] values.
			  if ( _value.Length == 0 )
			  {
					return 0;
			  }
			  int h = 1, length = _value.Length;
			  for ( int offset = 0, codePoint; offset < length; offset += Character.charCount( codePoint ) )
			  {
					codePoint = char.ConvertToUtf32( _value, offset );
					h = 31 * h + codePoint;
			  }
			  return h;
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  return UpdateHash( hashFunction, hash, _value );
		 }

		 public static long UpdateHash( HashFunction hashFunction, long hash, string value )
		 {
			  //NOTE that we are basing the hash code on code points instead of char[] values.
			  int length = value.Length;
			  int codePointCount = 0;
			  for ( int offset = 0; offset < length; )
			  {
					int codePointA = char.ConvertToUtf32( value, offset );
					int codePointB = 0;
					offset += Character.charCount( codePointA );
					codePointCount++;
					if ( offset < length )
					{
						 codePointB = char.ConvertToUtf32( value, offset );
						 offset += Character.charCount( codePointB );
						 codePointCount++;
					}
					hash = hashFunction.Update( hash, ( ( long ) codePointA << 32 ) + codePointB );
			  }
			  return hashFunction.Update( hash, codePointCount );
		 }

		 public override TextValue Substring( int start, int length )
		 {
			  int s = Math.Min( start, length() );
			  int e = Math.Min( s + length, length() );
			  int codePointStart = _value.offsetByCodePoints( 0, s );
			  int codePointEnd = _value.offsetByCodePoints( 0, e );

			  return Values.StringValue( _value.Substring( codePointStart, codePointEnd - codePointStart ) );
		 }

		 public override TextValue Trim()
		 {
			  int start = LtrimIndex( _value );
			  int end = RtrimIndex( _value );
			  return Values.StringValue( _value.Substring( start, Math.Max( end, start ) - start ) );
		 }

		 public override TextValue Ltrim()
		 {
			  int start = LtrimIndex( _value );
			  return Values.StringValue( _value.Substring( start, _value.Length - start ) );
		 }

		 public override TextValue Rtrim()
		 {
			  int end = RtrimIndex( _value );
			  return Values.StringValue( _value.Substring( 0, end ) );
		 }

		 public override TextValue Reverse()
		 {
			  StringBuilder stringBuilder = new StringBuilder( Value() );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET StringBuilder equivalent to the Java 'reverse' method:
			  return Values.StringValue( stringBuilder.reverse().ToString() );
		 }

		 public override TextValue Plus( TextValue other )
		 {
			  return new StringWrappingStringValue( _value + other.StringValue() );
		 }

		 public override bool StartsWith( TextValue other )
		 {
			  return _value.StartsWith( other.StringValue(), StringComparison.Ordinal );
		 }

		 public override bool EndsWith( TextValue other )
		 {
			  return _value.EndsWith( other.StringValue(), StringComparison.Ordinal );
		 }

		 public override bool Contains( TextValue other )
		 {
			  return _value.Contains( other.StringValue() );
		 }

		 internal override Matcher Matcher( Pattern pattern )
		 {
			  return pattern.matcher( _value );
		 }

		 private int LtrimIndex( string value )
		 {
			  int start = 0, length = value.Length;
			  while ( start < length )
			  {
					int codePoint = char.ConvertToUtf32( value, start );
					if ( !char.IsWhiteSpace( codePoint ) )
					{
						 break;
					}
					start += Character.charCount( codePoint );
			  }

			  return start;
		 }

		 private int RtrimIndex( string value )
		 {
			  int end = value.Length;
			  while ( end > 0 )
			  {
					int codePoint = value.codePointBefore( end );
					if ( !char.IsWhiteSpace( codePoint ) )
					{
						 break;
					}
					end--;
			  }
			  return end;
		 }
	}

}