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
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	internal class LocalTimeType : Type
	{
		 // Affected key state:
		 // long0 (nanoOfDay)

		 internal LocalTimeType( sbyte typeId ) : base( ValueGroup.LOCAL_TIME, typeId, LocalTimeValue.MIN_VALUE, LocalTimeValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeLocalTime;
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

		 internal static LocalTimeValue AsValue( long long0 )
		 {
			  return LocalTimeValue.localTime( AsValueRaw( long0 ) );
		 }

		 internal static LocalTime AsValueRaw( long long0 )
		 {
			  return LocalTimeValue.localTimeRaw( long0 );
		 }

		 internal static int Compare( long thisLong0, long thatLong0 )
		 {
			  return Long.compare( thisLong0, thatLong0 );
		 }

		 internal static void Put( PageCursor cursor, long long0 )
		 {
			  cursor.PutLong( long0 );
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  into.WriteLocalTime( cursor.Long );
			  return true;
		 }

		 internal virtual void Write( GenericKey state, long nanoOfDay )
		 {
			  state.Long0 = nanoOfDay;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
		 }
	}

}