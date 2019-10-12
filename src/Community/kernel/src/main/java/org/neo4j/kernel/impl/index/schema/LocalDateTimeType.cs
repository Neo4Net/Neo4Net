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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	internal class LocalDateTimeType : Type
	{
		 // Affected key state:
		 // long0 (nanoOfSecond)
		 // long1 (epochSecond)

		 internal LocalDateTimeType( sbyte typeId ) : base( ValueGroup.LOCAL_DATE_TIME, typeId, LocalDateTimeValue.MIN_VALUE, LocalDateTimeValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeLocalDateTime;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  to.Long1 = from.Long1;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  return AsValue( state.Long0, state.Long1 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, left.Long1, right.Long0, right.Long1 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  Put( cursor, state.Long0, state.Long1 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return Read( cursor, into );
		 }

		 internal static LocalDateTimeValue AsValue( long long0, long long1 )
		 {
			  return LocalDateTimeValue.localDateTime( AsValueRaw( long0, long1 ) );
		 }

		 internal static DateTime AsValueRaw( long long0, long long1 )
		 {
			  return LocalDateTimeValue.localDateTimeRaw( long1, long0 );
		 }

		 internal static void Put( PageCursor cursor, long long0, long long1 )
		 {
			  cursor.PutLong( long1 );
			  cursor.PutInt( ( int ) long0 );
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  into.WriteLocalDateTime( cursor.Long, cursor.Int );
			  return true;
		 }

		 internal static int Compare( long thisLong0, long thisLong1, long thatLong0, long thatLong1 )
		 {
			  int compare = Long.compare( thisLong1, thatLong1 );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( ( int ) thisLong0, ( int ) thatLong0 );
			  }
			  return compare;
		 }

		 internal virtual void Write( GenericKey state, long epochSecond, int nano )
		 {
			  state.Long0 = nano;
			  state.Long1 = epochSecond;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
		 }
	}

}