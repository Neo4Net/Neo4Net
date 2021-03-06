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
	using UpdateMode = Org.Neo4j.Kernel.Impl.Api.index.UpdateMode;

	public class IndexUpdateEntry
	{
		 private IndexUpdateEntry()
		 {
			  // Static utility class
		 }

		 public static void Read<KEY, VALUE>( PageCursor cursor, Layout<KEY, VALUE> layout, UpdateMode updateMode, KEY key1, KEY key2, VALUE value )
		 {
			  switch ( updateMode.innerEnumValue )
			  {
			  case UpdateMode.InnerEnum.ADDED:
					BlockEntry.Read( cursor, layout, key1, value );
					break;
			  case UpdateMode.InnerEnum.REMOVED:
					BlockEntry.Read( cursor, layout, key1 );
					break;
			  case UpdateMode.InnerEnum.CHANGED:
					BlockEntry.Read( cursor, layout, key1 );
					BlockEntry.Read( cursor, layout, key2, value );
					break;
			  default:
					throw new System.ArgumentException( "Unknown update mode " + updateMode );
			  }
		 }

		 public static void Write<KEY, VALUE>( PageCursor cursor, Layout<KEY, VALUE> layout, UpdateMode updateMode, KEY key1, KEY key2, VALUE value )
		 {
			  switch ( updateMode.innerEnumValue )
			  {
			  case UpdateMode.InnerEnum.ADDED:
					BlockEntry.Write( cursor, layout, key1, value );
					break;
			  case UpdateMode.InnerEnum.REMOVED:
					BlockEntry.Write( cursor, layout, key1 );
					break;
			  case UpdateMode.InnerEnum.CHANGED:
					BlockEntry.Write( cursor, layout, key1 );
					BlockEntry.Write( cursor, layout, key2, value );
					break;
			  default:
					throw new System.ArgumentException( "Unknown update mode " + updateMode );
			  }
		 }
	}

}