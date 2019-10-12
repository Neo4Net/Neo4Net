using System.Threading;

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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	/// <summary>
	/// A base class for page cache background tasks.
	/// </summary>
	internal abstract class BackgroundTask : ThreadStart
	{
		 private readonly MuninnPageCache _pageCache;

		 internal BackgroundTask( MuninnPageCache pageCache )
		 {
			  this._pageCache = pageCache;
		 }

		 public override void Run()
		 {
			  int pageCacheId = _pageCache.PageCacheId;
			  string taskName = this.GetType().Name;
			  string threadName = "MuninnPageCache[" + pageCacheId + "]-" + taskName;
			  Thread thread = Thread.CurrentThread;
			  string previousName = thread.Name;
			  try
			  {
					thread.Name = threadName;
					Run( _pageCache );
			  }
			  finally
			  {
					thread.Name = previousName;
			  }
		 }

		 protected internal abstract void Run( MuninnPageCache pageCache );
	}

}