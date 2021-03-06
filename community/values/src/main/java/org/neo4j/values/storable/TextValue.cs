﻿using System;

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

	using Org.Neo4j.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.fromArray;

	public abstract class TextValue : ScalarValue
	{
		 internal static readonly ListValue EmptySplit = fromArray( stringArray( "", "" ) );

		 internal TextValue()
		 {
		 }

		 public abstract string StringValue();

		 /// <summary>
		 /// The length of a TextValue is the number of Unicode code points in the text.
		 /// </summary>
		 /// <returns> The number of Unicode code points. </returns>
		 public abstract int Length();

		 public abstract TextValue Substring( int start, int length );

		 public virtual TextValue Substring( int start )
		 {
			  return Substring( start, Math.Max( Length() - start, start ) );
		 }

		 public abstract TextValue Trim();

		 public abstract TextValue Ltrim();

		 public abstract TextValue Rtrim();

		 public abstract TextValue ToLower();

		 public abstract TextValue ToUpper();

		 public abstract ListValue Split( string separator );

		 public abstract TextValue Replace( string find, string replace );

		 public abstract TextValue Reverse();

		 public abstract TextValue Plus( TextValue other );

		 public abstract bool StartsWith( TextValue other );

		 public abstract bool EndsWith( TextValue other );

		 public abstract bool Contains( TextValue other );

		 public abstract int CompareTo( TextValue other );

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return CompareTo( ( TextValue ) otherValue );
		 }

		 public override sealed bool Equals( bool x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( long x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( double x )
		 {
			  return false;
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Text;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapText( this );
		 }

		 internal abstract Matcher Matcher( Pattern pattern );
	}

}