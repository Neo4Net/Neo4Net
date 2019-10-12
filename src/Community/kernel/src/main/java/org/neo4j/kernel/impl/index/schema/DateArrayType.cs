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

	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	internal class DateArrayType : AbstractArrayType<LocalDate>
	{
		 // Affected key state:
		 // long0Array (epochDay)

		 internal DateArrayType( sbyte typeId ) : base( ValueGroup.DATE_ARRAY, typeId, ( o1, o2, i ) -> DateType.Compare( o1.long0Array[i], o2.long0Array[i] ), ( k, i ) -> DateType.AsValueRaw( k.long0Array[i] ), ( c, k, i ) -> DateType.put( c, k.long0Array[i] ), DateType.read, LocalDate[]::new, org.neo4j.values.storable.ValueWriter_ArrayType.Date )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, GenericKey.SizeDate );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
		 }

		 internal override void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
		 }

		 public virtual void Write( GenericKey state, int offset, long epochDay )
		 {
			  state.Long0Array[offset] = epochDay;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}