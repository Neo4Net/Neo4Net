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

	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Org.Neo4j.Values.Storable;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.TRUE;

	// <Boolean> as generic raw type is mostly for show, this class overrides default object:y behaviour to create primitive boolean[] array
	internal class BooleanArrayType : AbstractArrayType<bool>
	{
		 // Affected key state:
		 // long0Array

		 internal BooleanArrayType( sbyte typeId ) : base( ValueGroup.BOOLEAN_ARRAY, typeId, ( o1, o2, i ) -> BooleanType.Compare( o1.long0Array[i], o2.long0Array[i] ), null, ( c, k, i ) -> BooleanType.put( c, k.long0Array[i] ), BooleanType.read, null, org.neo4j.values.storable.ValueWriter_ArrayType.Boolean )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return ArrayKeySize( state, GenericKey.SizeBoolean );
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

		 internal override Value AsValue( GenericKey state )
		 {
			  bool[] array = new bool[state.ArrayLength];
			  for ( int i = 0; i < state.ArrayLength; i++ )
			  {
					array[i] = BooleanType.AsValueRaw( state.Long0Array[i] );
			  }
			  return Values.of( array );
		 }

		 internal virtual void Write( GenericKey state, int offset, bool value )
		 {
			  state.Long0Array[offset] = value ? TRUE : FALSE;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}