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
namespace Neo4Net.Io.pagecache.tracing
{
	/// <summary>
	/// An eviction run is started when the page cache has determined that it
	/// needs to evict a batch of pages. The dedicated eviction thread is
	/// mostly sleeping when it is not performing an eviction run.
	/// </summary>
	public interface EvictionRunEvent : AutoCloseablePageCacheTracerEvent, EvictionEventOpportunity
	{
		 /// <summary>
		 /// An EvictionRunEvent that does nothing other than return the EvictionEvent.NULL.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 EvictionRunEvent NULL = new EvictionRunEvent()
	//	 {
	//		  @@Override public EvictionEvent beginEviction()
	//		  {
	//				return EvictionEvent.NULL;
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//	 };
	}

}