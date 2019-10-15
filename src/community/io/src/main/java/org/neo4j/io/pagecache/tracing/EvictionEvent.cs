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
	/// The eviction of a page has begun.
	/// </summary>
	public interface EvictionEvent : IDisposablePageCacheTracerEvent
	{
		 /// <summary>
		 /// An EvictionEvent that does nothing other than return the FlushEventOpportunity.NULL.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 EvictionEvent NULL = new EvictionEvent()
	//	 {
	//		  @@Override public void setFilePageId(long filePageId)
	//		  {
	//		  }
	//
	//		  @@Override public void setSwapper(PageSwapper swapper)
	//		  {
	//		  }
	//
	//		  @@Override public FlushEventOpportunity flushEventOpportunity()
	//		  {
	//				return FlushEventOpportunity.NULL;
	//		  }
	//
	//		  @@Override public void threwException(IOException exception)
	//		  {
	//		  }
	//
	//		  @@Override public void setCachePageId(long cachePageId)
	//		  {
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// The file page id the evicted page was bound to.
		 /// </summary>
		 long FilePageId { set; }

		 /// <summary>
		 /// The swapper the evicted page was bound to.
		 /// </summary>
		 PageSwapper Swapper { set; }

		 /// <summary>
		 /// Eviction implies an opportunity to flush.
		 /// </summary>
		 FlushEventOpportunity FlushEventOpportunity();

		 /// <summary>
		 /// Indicates that the eviction caused an exception to be thrown.
		 /// This can happen if some kind of IO error occurs.
		 /// </summary>
		 void ThrewException( IOException exception );

		 /// <summary>
		 /// The cache page id of the evicted page.
		 /// </summary>
		 long CachePageId { set; }
	}

}