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
	/// <seealso cref="Layout"/> for numbers where numbers doesn't need to be unique.
	/// </summary>
	internal abstract class NumberLayout : IndexLayout<NumberIndexKey, NativeIndexValue>
	{
		 internal NumberLayout( long identifier, int majorVersion, int minorVersion ) : base( identifier, majorVersion, minorVersion )
		 {
		 }

		 public override NumberIndexKey NewKey()
		 {
			  return new NumberIndexKey();
		 }

		 public override NumberIndexKey CopyKey( NumberIndexKey key, NumberIndexKey into )
		 {
			  into.Type = key.Type;
			  into.RawValueBits = key.RawValueBits;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( NumberIndexKey key )
		 {
			  return NumberIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, NumberIndexKey key )
		 {
			  cursor.PutByte( key.Type );
			  cursor.PutLong( key.RawValueBits );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, NumberIndexKey into, int keySize )
		 {
			  into.Type = cursor.Byte;
			  into.RawValueBits = cursor.Long;
			  into.EntityId = cursor.Long;
		 }

		 internal override int CompareValue( NumberIndexKey o1, NumberIndexKey o2 )
		 {
			  return o1.CompareValueTo( o2 );
		 }
	}

}