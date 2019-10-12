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

	public abstract class IntegralArray : NumberArray
	{
		 internal abstract long LongValue( int offset );

		 public override int CompareTo( IntegralArray other )
		 {
			  return NumberValues.CompareIntegerArrays( this, other );
		 }

		 public override int CompareTo( FloatingPointArray other )
		 {
			  return NumberValues.CompareIntegerVsFloatArrays( this, other );
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.Integral;
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  int len = Length();
			  hash = hashFunction.Update( hash, len );
			  for ( int i = 0; i < len; i++ )
			  {
					hash = hashFunction.Update( hash, LongValue( i ) );
			  }
			  return hash;
		 }
	}

}