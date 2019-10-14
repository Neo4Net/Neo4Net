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
	using Neo4Net.Index.@internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for local date times.
	/// </summary>
	internal class LocalDateTimeLayout : IndexLayout<LocalDateTimeIndexKey, NativeIndexValue>
	{
		 internal LocalDateTimeLayout() : base("Tld", 0, 1)
		 {
		 }

		 public override LocalDateTimeIndexKey NewKey()
		 {
			  return new LocalDateTimeIndexKey();
		 }

		 public override LocalDateTimeIndexKey CopyKey( LocalDateTimeIndexKey key, LocalDateTimeIndexKey into )
		 {
			  into.EpochSecond = key.EpochSecond;
			  into.NanoOfSecond = key.NanoOfSecond;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( LocalDateTimeIndexKey key )
		 {
			  return LocalDateTimeIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, LocalDateTimeIndexKey key )
		 {
			  cursor.PutLong( key.EpochSecond );
			  cursor.PutInt( key.NanoOfSecond );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, LocalDateTimeIndexKey into, int keySize )
		 {
			  into.EpochSecond = cursor.Long;
			  into.NanoOfSecond = cursor.Int;
			  into.EntityId = cursor.Long;
		 }
	}

}