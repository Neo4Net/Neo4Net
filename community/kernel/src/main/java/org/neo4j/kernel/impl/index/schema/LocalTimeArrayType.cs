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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Org.Neo4j.Values.Storable;

	internal class LocalTimeArrayType : AbstractArrayType<LocalTime>
	{
		 // Affected key state:
		 // long0Array (nanoOfDay)

		 internal LocalTimeArrayType( sbyte typeId ) : base( ValueGroup.LOCAL_TIME_ARRAY, typeId, ( o1, o2, i ) -> LocalTimeType.Compare( o1.long0Array[i], o2.long0Array[i] ), ( k, i ) -> LocalTimeType.AsValueRaw( k.long0Array[i] ), ( c, k, i ) -> LocalTimeType.put( c, k.long0Array[i] ), LocalTimeType.read, LocalTime[]::new, org.neo4j.values.storable.ValueWriter_ArrayType.LocalTime )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, GenericKey.SizeLocalTime );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
		 }

		 internal override void InitializeArray( GenericKey key, int length, Org.Neo4j.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
		 }

		 internal virtual void Write( GenericKey state, int offset, long nanoOfDay )
		 {
			  state.Long0Array[offset] = nanoOfDay;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}