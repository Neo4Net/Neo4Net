﻿/*
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
	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for absolute times.
	/// </summary>
	internal class ZonedTimeLayout : IndexLayout<ZonedTimeIndexKey, NativeIndexValue>
	{
		 internal ZonedTimeLayout() : base("Tzt", 0, 1)
		 {
		 }

		 public override ZonedTimeIndexKey NewKey()
		 {
			  return new ZonedTimeIndexKey();
		 }

		 public override ZonedTimeIndexKey CopyKey( ZonedTimeIndexKey key, ZonedTimeIndexKey into )
		 {
			  into.NanosOfDayUTC = key.NanosOfDayUTC;
			  into.ZoneOffsetSeconds = key.ZoneOffsetSeconds;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( ZonedTimeIndexKey key )
		 {
			  return ZonedTimeIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, ZonedTimeIndexKey key )
		 {
			  cursor.PutLong( key.NanosOfDayUTC );
			  cursor.PutInt( key.ZoneOffsetSeconds );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, ZonedTimeIndexKey into, int keySize )
		 {
			  into.NanosOfDayUTC = cursor.Long;
			  into.ZoneOffsetSeconds = cursor.Int;
			  into.EntityId = cursor.Long;
		 }
	}

}