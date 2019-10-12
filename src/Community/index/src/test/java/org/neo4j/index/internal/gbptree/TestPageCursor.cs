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
namespace Neo4Net.Index.@internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using DelegatingPageCursor = Neo4Net.Io.pagecache.impl.DelegatingPageCursor;

	internal class TestPageCursor : DelegatingPageCursor
	{
		 private bool _shouldRetry;

		 internal TestPageCursor( PageCursor @delegate ) : base( @delegate )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  // Always call delegate to reset state
			  bool toReturn = base.ShouldRetry() || _shouldRetry;
			  _shouldRetry = false;
			  return toReturn;
		 }

		 internal virtual void Changed()
		 {
			  _shouldRetry = true;
		 }
	}

}