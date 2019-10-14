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
	using Neo4Net.Index.Internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for dates.
	/// </summary>
	internal class DateLayout : IndexLayout<DateIndexKey, NativeIndexValue>
	{
		 internal DateLayout() : base("Tda", 0, 1)
		 {
		 }

		 public override DateIndexKey NewKey()
		 {
			  return new DateIndexKey();
		 }

		 public override DateIndexKey CopyKey( DateIndexKey key, DateIndexKey into )
		 {
			  into.EpochDay = key.EpochDay;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( DateIndexKey key )
		 {
			  return DateIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, DateIndexKey key )
		 {
			  cursor.PutLong( key.EpochDay );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, DateIndexKey into, int keySize )
		 {
			  into.EpochDay = cursor.Long;
			  into.EntityId = cursor.Long;
		 }
	}

}