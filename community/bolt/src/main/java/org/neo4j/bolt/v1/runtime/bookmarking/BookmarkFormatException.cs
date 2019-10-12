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
namespace Org.Neo4j.Bolt.v1.runtime.bookmarking
{
	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.bookmarking.Bookmark.BOOKMARK_TX_PREFIX;

	internal class BookmarkFormatException : BoltIOException
	{
		 internal BookmarkFormatException( string bookmarkString, System.FormatException cause ) : base( org.neo4j.kernel.api.exceptions.Status_Transaction.InvalidBookmark, string.Format( "Supplied bookmark [{0}] does not conform to pattern {1}; unable to parse transaction id", bookmarkString, BOOKMARK_TX_PREFIX ), cause )
		 {
		 }

		 internal BookmarkFormatException( object bookmarkObject ) : base( org.neo4j.kernel.api.exceptions.Status_Transaction.InvalidBookmark, string.Format( "Supplied bookmark [{0}] does not conform to pattern {1}", bookmarkObject, BOOKMARK_TX_PREFIX ) )
		 {
		 }
	}

}