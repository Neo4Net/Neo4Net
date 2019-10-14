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
namespace Neo4Net.Index.@internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Keeps id and generation of root of the tree. Can move <seealso cref="PageCursor"/> to root id and return its generation,
	/// both read atomically.
	/// </summary>
	internal class Root
	{
		 /// <summary>
		 /// Current page id which contains the root of the tree.
		 /// </summary>
		 private readonly long _rootId;

		 /// <summary>
		 /// Generation of current <seealso cref="rootId"/>.
		 /// </summary>
		 private readonly long _rootGeneration;

		 internal Root( long rootId, long rootGeneration )
		 {
			  this._rootId = rootId;
			  this._rootGeneration = rootGeneration;
		 }

		 /// <summary>
		 /// Moves the provided {@code cursor} to the current root id and returning the generation where
		 /// that root id was assigned.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to place at the current root id. </param>
		 /// <returns> the generation where the current root was assigned. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long goTo(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 internal virtual long GoTo( PageCursor cursor )
		 {
			  PageCursorUtil.GoTo( cursor, "root", _rootId );
			  return _rootGeneration;
		 }

		 internal virtual long Id()
		 {
			  return _rootId;
		 }

		 internal virtual long Generation()
		 {
			  return _rootGeneration;
		 }
	}

}