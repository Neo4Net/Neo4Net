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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

	/// <summary>
	/// <seealso cref="Tracker"/> capable of keeping {@code int} range values, using <seealso cref="IntArray"/>.
	/// Will fail in <seealso cref="set(long, long)"/> with <seealso cref="ArithmeticException"/> if trying to put a too big value.
	/// </summary>
	public class IntTracker : AbstractTracker<IntArray>
	{
		 internal static readonly int Size = Integer.BYTES;
		 internal static readonly int IdBits = ( sizeof( sbyte ) * 8 ) * Size - 1;
		 internal static readonly long MaxId = ( 1 << IdBits ) - 1;
		 internal const int DEFAULT_VALUE = -1;
		 private static readonly LongBitsManipulator _bits = new LongBitsManipulator( IdBits, 1 );

		 public IntTracker( IntArray array ) : base( array )
		 {
		 }

		 public override long Get( long index )
		 {
			  return _bits.get( Array.get( index ), 0 );
		 }

		 /// <exception cref="ArithmeticException"> if value is bigger than <seealso cref="Integer.MAX_VALUE"/>. </exception>
		 public override void Set( long index, long value )
		 {
			  long field = Array.get( index );
			  field = _bits.set( field, 0, value );
			  Array.set( index, ( int ) field );
		 }

		 public override void MarkAsDuplicate( long index )
		 {
			  long field = Array.get( index );
			  // Since the default value for the whole field is -1 (i.e. all 1s) then this mark will have to be 0.
			  field = _bits.set( field, 1, 0 );
			  Array.set( index, ( int ) field );
		 }

		 public override bool IsMarkedAsDuplicate( long index )
		 {
			  long field = Array.get( index );
			  return _bits.get( field, 1 ) == 0;
		 }
	}

}