﻿/*
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
namespace Neo4Net.Kernel.impl.store
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;

	public class NoStoreHeaderFormat : StoreHeaderFormat<NoStoreHeader>
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static readonly NoStoreHeaderFormat NoStoreHeaderFormatConflict = new NoStoreHeaderFormat();

		 private NoStoreHeaderFormat()
		 {
		 }

		 public override int NumberOfReservedRecords()
		 {
			  return 0;
		 }

		 public override void WriteHeader( PageCursor cursor )
		 {
			  throw new System.NotSupportedException( "Should not be called" );
		 }

		 public override NoStoreHeader ReadHeader( PageCursor cursor )
		 {
			  return NO_STORE_HEADER;
		 }
	}

}