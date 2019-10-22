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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	internal class ZonedTimeArrayType : AbstractArrayType<OffsetTime>
	{
		 // Affected key state:
		 // long0Array (nanosOfDayUTC)
		 // long1Array (zoneOffsetSeconds)

		 internal ZonedTimeArrayType( sbyte typeId ) : base( ValueGroup.ZONED_TIME_ARRAY, typeId, ( o1, o2, i ) -> ZonedTimeType.Compare( o1.long0Array[i], o1.long1Array[i], o2.long0Array[i], o2.long1Array[i] ), ( k, i ) -> ZonedTimeType.AsValueRaw( k.long0Array[i], k.long1Array[i] ), ( c, k, i ) -> ZonedTimeType.put( c, k.long0Array[i], k.long1Array[i] ), ZonedTimeType.read, OffsetTime[]::new, org.Neo4Net.values.storable.ValueWriter_ArrayType.ZonedTime )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, GenericKey.SizeZonedTime );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
			  Array.Copy( from.Long1Array, 0, to.Long1Array, 0, length );
		 }

		 internal override void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
			  key.Long1Array = EnsureBigEnough( key.Long1Array, length );
		 }

		 internal virtual void Write( GenericKey state, int offset, long nanosOfDayUTC, int offsetSeconds )
		 {
			  state.Long0Array[offset] = nanosOfDayUTC;
			  state.Long1Array[offset] = offsetSeconds;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  joiner.add( "long1Array=" + Arrays.ToString( state.Long1Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}