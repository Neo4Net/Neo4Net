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
	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for durations.
	/// </summary>
	internal class DurationLayout : IndexLayout<DurationIndexKey, NativeIndexValue>
	{
		 internal DurationLayout() : base("Tdu", 0, 1)
		 {
		 }

		 public override DurationIndexKey NewKey()
		 {
			  return new DurationIndexKey();
		 }

		 public override DurationIndexKey CopyKey( DurationIndexKey key, DurationIndexKey into )
		 {
			  into.TotalAvgSeconds = key.TotalAvgSeconds;
			  into.NanosOfSecond = key.NanosOfSecond;
			  into.Months = key.Months;
			  into.Days = key.Days;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( DurationIndexKey key )
		 {
			  return DurationIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, DurationIndexKey key )
		 {
			  cursor.PutLong( key.TotalAvgSeconds );
			  cursor.PutInt( key.NanosOfSecond );
			  cursor.PutLong( key.Months );
			  cursor.PutLong( key.Days );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, DurationIndexKey into, int keySize )
		 {
			  into.TotalAvgSeconds = cursor.Long;
			  into.NanosOfSecond = cursor.Int;
			  into.Months = cursor.Long;
			  into.Days = cursor.Long;
			  into.EntityId = cursor.Long;
		 }
	}

}