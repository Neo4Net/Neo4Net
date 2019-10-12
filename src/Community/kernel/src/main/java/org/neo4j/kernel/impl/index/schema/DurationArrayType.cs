using System;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	// Raw type is DurationValue, but that's because DurationValue doesn't wrap a TemporalValue or derivative, rather extracts fields from it
	internal class DurationArrayType : AbstractArrayType<DurationValue>
	{
		 // Affected key state:
		 // long0Array (totalAvgSeconds)
		 // long1Array (nanosOfSecond)
		 // long2Array (months)
		 // long3Array (days)

		 internal DurationArrayType( sbyte typeId ) : base( ValueGroup.DURATION_ARRAY, typeId, ( o1, o2, i ) -> DurationType.Compare( o1.long0Array[i], o1.long1Array[i], o1.long2Array[i], o1.long3Array[i], o2.long0Array[i], o2.long1Array[i], o2.long2Array[i], o2.long3Array[i] ), ( k, i ) -> DurationType.AsValue( k.long0Array[i], k.long1Array[i], k.long2Array[i], k.long3Array[i] ), ( c, k, i ) -> DurationType.put( c, k.long0Array[i], k.long1Array[i], k.long2Array[i], k.long3Array[i] ), DurationType.read, DurationValue[]::new, org.neo4j.values.storable.ValueWriter_ArrayType.Duration )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, GenericKey.SizeDuration );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
			  Array.Copy( from.Long1Array, 0, to.Long1Array, 0, length );
			  Array.Copy( from.Long2Array, 0, to.Long2Array, 0, length );
			  Array.Copy( from.Long3Array, 0, to.Long3Array, 0, length );
		 }

		 internal override void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
			  key.Long1Array = EnsureBigEnough( key.Long1Array, length );
			  key.Long2Array = EnsureBigEnough( key.Long2Array, length );
			  key.Long3Array = EnsureBigEnough( key.Long3Array, length );
		 }

		 internal virtual void Write( GenericKey state, int offset, long months, long days, long totalAvgSeconds, int nanos )
		 {
			  state.Long0Array[offset] = totalAvgSeconds;
			  state.Long1Array[offset] = nanos;
			  state.Long2Array[offset] = months;
			  state.Long3Array[offset] = days;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  joiner.add( "long1Array=" + Arrays.ToString( state.Long1Array ) );
			  joiner.add( "long2Array=" + Arrays.ToString( state.Long2Array ) );
			  joiner.add( "long3Array=" + Arrays.ToString( state.Long3Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}