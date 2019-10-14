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

	using HashFunction = Neo4Net.Hashing.HashFunction;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public abstract class NonPrimitiveArray<T extends Comparable<? super T>> extends ArrayValue
	public abstract class NonPrimitiveArray<T> : ArrayValue
	{
		 protected internal abstract T[] Value();

		 public override sealed bool Equals( bool[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( char[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( string[] x )
		 {
			  return false;
		 }

		 public override bool Equals( sbyte[] x )
		 {
			  return false;
		 }

		 public override bool Equals( short[] x )
		 {
			  return false;
		 }

		 public override bool Equals( int[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( long[] x )
		 {
			  return false;
		 }

		 public override bool Equals( float[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( double[] x )
		 {
			  return false;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 protected internal int CompareToNonPrimitiveArray( NonPrimitiveArray<T> other )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( this.Length(), other.Length() );

			  while ( x == 0 && i < length )
			  {
					x = this.Value()[i].CompareTo(other.Value()[i]);
					i++;
			  }
			  if ( x == 0 )
			  {
					x = this.Length() - other.Length();
			  }
			  return x;
		 }

		 public override int ComputeHash()
		 {
			  return Arrays.GetHashCode( Value() );
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, Length() );
			  foreach ( T obj in Value() )
			  {
					hash = hashFunction.Update( hash, obj.GetHashCode() );
			  }
			  return hash;
		 }

		 public override int Length()
		 {
			  return Value().Length;
		 }

		 public override T[] AsObjectCopy()
		 {
			  return Value().Clone();
		 }

		 [Obsolete]
		 public override T[] AsObject()
		 {
			  return Value();
		 }

		 public override string PrettyPrint()
		 {
			  return Arrays.ToString( Value() );
		 }

		 public override sealed string ToString()
		 {
			  return this.GetType().Name + Arrays.ToString(Value());
		 }
	}

}