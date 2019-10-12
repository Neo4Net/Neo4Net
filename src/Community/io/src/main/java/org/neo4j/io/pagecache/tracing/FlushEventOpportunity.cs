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
namespace Neo4Net.Io.pagecache.tracing
{

	/// <summary>
	/// Represents the opportunity to flush a page.
	/// 
	/// The flushing might not happen, though, because only dirty pages are flushed.
	/// </summary>
	public interface FlushEventOpportunity
	{
		 /// <summary>
		 /// A FlushEventOpportunity that only returns the FlushEvent.NULL.
		 /// </summary>

		 /// <summary>
		 /// Begin flushing the given page.
		 /// </summary>
		 FlushEvent BeginFlush( long filePageId, long cachePageId, PageSwapper swapper );
	}

	public static class FlushEventOpportunity_Fields
	{
		 public static readonly FlushEventOpportunity Null = ( filePageId, cachePageId, swapper ) => FlushEvent.NULL;
	}

}