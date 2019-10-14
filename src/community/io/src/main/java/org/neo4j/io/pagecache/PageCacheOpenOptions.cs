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
namespace Neo4Net.Io.pagecache
{

	/// <summary>
	/// <seealso cref="OpenOption"/>s that are specific to <seealso cref="PageCache.map(File, int, OpenOption...)"/>,
	/// and not normally supported by file systems.
	/// </summary>
	public enum PageCacheOpenOptions
	{
		 /// <summary>
		 /// Map the file even if the specified file page size conflicts with an existing mapping of that file.
		 /// If so, the given file page size will be ignored and a <seealso cref="PagedFile"/> will be returned that uses the
		 /// file page size of the existing mapping.
		 /// </summary>
		 AnyPageSize,

		 /// <summary>
		 /// Mapped file will only use a single channel, overriding the otherwise configured striping amount, e.g. one channel per core.
		 /// </summary>
		 NoChannelStriping
	}

}