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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.TRUE;

	internal class BooleanType : Type
	{
		 // Affected key state:
		 // long0

		 internal BooleanType( sbyte typeId ) : base( ValueGroup.BOOLEAN, typeId, Values.of( false ), Values.of( true ) )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeBoolean;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  return AsValue( state.Long0 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, right.Long0 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  Put( cursor, state.Long0 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return Read( cursor, into );
		 }

		 internal static BooleanValue AsValue( long long0 )
		 {
			  return Values.booleanValue( AsValueRaw( long0 ) );
		 }

		 internal static bool AsValueRaw( long long0 )
		 {
			  return BooleanOf( long0 );
		 }

		 internal static int Compare( long thisLong0, long thatLong0 )
		 {
			  return Long.compare( thisLong0, thatLong0 );
		 }

		 internal static void Put( PageCursor cursor, long long0 )
		 {
			  cursor.PutByte( ( sbyte ) long0 );
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  into.WriteBoolean( cursor.Byte == TRUE );
			  return true;
		 }

		 internal virtual void Write( GenericKey state, bool value )
		 {
			  state.Long0 = value ? TRUE : FALSE;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
		 }
	}

}