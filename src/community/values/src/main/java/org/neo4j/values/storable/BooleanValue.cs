using System;

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

	/// <summary>
	/// This does not extend AbstractProperty since the JVM can take advantage of the 4 byte initial field alignment if
	/// we don't extend a class that has fields.
	/// </summary>
	public abstract class BooleanValue : ScalarValue
	{

		 private BooleanValue()
		 {
		 }

		 public override bool Eq( object other )
		 {
			  return other is Value && Equals( ( Value ) other );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapBoolean( this );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Boolean;
		 }

		 public abstract bool BooleanValue();

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  return hashFunction.Update( hash, GetHashCode() );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Boolean";
			 }
		 }

		 public static readonly BooleanValue TRUE = new BooleanValueAnonymousInnerClass();

		 private class BooleanValueAnonymousInnerClass : BooleanValue
		 {
			 public override bool Equals( Value other )
			 {
				  return this == other;
			 }

			 public override bool Equals( bool x )
			 {
				  return x;
			 }

			 public override int computeHash()
			 {
				  //Use same as Boolean.TRUE.hashCode
				  return 1231;
			 }

			 public override bool booleanValue()
			 {
				  return true;
			 }

			 internal override int unsafeCompareTo( Value otherValue )
			 {
				  BooleanValue other = ( BooleanValue ) otherValue;
				  return other.BooleanValueConflict() ? 0 : 1;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
			 public override void writeTo<E>( ValueWriter<E> writer ) where E : Exception
			 {
				  writer.WriteBoolean( true );
			 }

			 public override object asObjectCopy()
			 {
				  return true;
			 }

			 public override string prettyPrint()
			 {
				  return Convert.ToString( true );
			 }

			 public override string ToString()
			 {
				  return format( "%s('%s')", outerInstance.TypeName, Convert.ToString( true ) );
			 }
		 }

		 public static readonly BooleanValue FALSE = new BooleanValueAnonymousInnerClass2();

		 private class BooleanValueAnonymousInnerClass2 : BooleanValue
		 {
			 public override bool Equals( Value other )
			 {
				  return this == other;
			 }

			 public override bool Equals( bool x )
			 {
				  return !x;
			 }

			 public override int computeHash()
			 {
				  //Use same as Boolean.FALSE.hashCode
				  return 1237;
			 }

			 public override bool booleanValue()
			 {
				  return false;
			 }

			 internal override int unsafeCompareTo( Value otherValue )
			 {
				  BooleanValue other = ( BooleanValue ) otherValue;
				  return !other.BooleanValueConflict() ? 0 : -1;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
			 public override void writeTo<E>( ValueWriter<E> writer ) where E : Exception
			 {
				  writer.WriteBoolean( false );
			 }

			 public override object asObjectCopy()
			 {
				  return false;
			 }

			 public override string prettyPrint()
			 {
				  return Convert.ToString( false );
			 }

			 public override string ToString()
			 {
				  return format( "%s('%s')", outerInstance.TypeName, Convert.ToString( false ) );
			 }
		 }
	}

}