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
namespace Org.Neo4j.Io.pagecache.tracing
{
	/// <summary>
	/// Begin pinning a page.
	/// </summary>
	public interface PinEvent
	{
		 /// <summary>
		 /// A PinEvent that does nothing other than return the PageFaultEvent.NULL.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 PinEvent NULL = new PinEvent()
	//	 {
	//		  @@Override public void setCachePageId(long cachePageId)
	//		  {
	//		  }
	//
	//		  @@Override public PageFaultEvent beginPageFault()
	//		  {
	//				return PageFaultEvent.NULL;
	//		  }
	//
	//		  @@Override public void hit()
	//		  {
	//		  }
	//
	//		  @@Override public void done()
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// The id of the cache page that holds the file page we pinned.
		 /// </summary>
		 long CachePageId { set; }

		 /// <summary>
		 /// The page we want to pin is not in memory, so being a page fault to load it in.
		 /// </summary>
		 PageFaultEvent BeginPageFault();

		 /// <summary>
		 /// Page found and bounded.
		 /// </summary>
		 void Hit();

		 /// <summary>
		 /// The pinning has completed and the page is now unpinned.
		 /// </summary>
		 void Done();
	}

}