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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{


	/// <summary>
	/// <seealso cref="Tracker"/> capable of keeping 6B range values, using <seealso cref="ByteArray"/>.
	/// </summary>
	public class BigIdTracker : AbstractTracker<ByteArray>
	{
		 internal const int SIZE = 5;
		 internal static readonly int IdBits = ( ( sizeof( sbyte ) * 8 ) * SIZE ) - 1;
		 internal static readonly sbyte[] DefaultValue;
		 public static readonly long MaxId = 1L << IdBits - 1;
		 private static readonly LongBitsManipulator _bits = new LongBitsManipulator( IdBits, 1 );
		 static BigIdTracker()
		 {
			  DefaultValue = new sbyte[SIZE];
			  Arrays.fill( DefaultValue, ( sbyte ) - 1 );
		 }

		 public BigIdTracker( ByteArray array ) : base( array )
		 {
		 }

		 public override long Get( long index )
		 {
			  return _bits.get( Array.get5ByteLong( index, 0 ), 0 );
		 }

		 public override void Set( long index, long value )
		 {
			  long field = Array.get5ByteLong( index, 0 );
			  field = _bits.set( field, 0, value );
			  Array.set5ByteLong( index, 0, field );
		 }

		 public override void MarkAsDuplicate( long index )
		 {
			  long field = Array.get5ByteLong( index, 0 );
			  field = _bits.set( field, 1, 0 );
			  Array.set5ByteLong( index, 0, field );
		 }

		 public override bool IsMarkedAsDuplicate( long index )
		 {
			  long field = Array.get5ByteLong( index, 0 );
			  return _bits.get( field, 1 ) == 0;
		 }
	}

}