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
	public abstract class TextArray : ArrayValue
	{
		 public abstract string StringValue( int offset );

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return TextValues.CompareTextArrays( this, ( TextArray ) otherValue );
		 }

		 public bool Equals( sbyte[] x )
		 {
			  return false;
		 }

		 public bool Equals( short[] x )
		 {
			  return false;
		 }

		 public bool Equals( int[] x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( long[] x )
		 {
			  return false;
		 }

		 public bool Equals( float[] x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( double[] x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( bool[] x )
		 {
			  return false;
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.TextArray;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }
	}

}