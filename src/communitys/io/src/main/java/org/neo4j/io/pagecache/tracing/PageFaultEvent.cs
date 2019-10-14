using System;

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
	/// Begin a page fault as part of a pin event.
	/// </summary>
	public interface PageFaultEvent : EvictionEventOpportunity
	{
		 /// <summary>
		 /// A PageFaultEvent that does nothing.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 PageFaultEvent NULL = new PageFaultEvent()
	//	 {
	//		  @@Override public void addBytesRead(long bytes)
	//		  {
	//		  }
	//
	//		  @@Override public void done()
	//		  {
	//		  }
	//
	//		  @@Override public void done(Throwable throwable)
	//		  {
	//		  }
	//
	//		  @@Override public EvictionEvent beginEviction()
	//		  {
	//				return EvictionEvent.NULL;
	//		  }
	//
	//		  @@Override public void setCachePageId(long cachePageId)
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// Add up a number of bytes that has been read from the backing file into the free page being bound.
		 /// </summary>
		 void AddBytesRead( long bytes );

		 /// <summary>
		 /// The id of the cache page that is being faulted into.
		 /// </summary>
		 long CachePageId { set; }

		 /// <summary>
		 /// The page fault completed successfully.
		 /// </summary>
		 void Done();

		 /// <summary>
		 /// The page fault did not complete successfully, but instead caused the given Throwable to be thrown.
		 /// </summary>
		 void Done( Exception throwable );
	}

}