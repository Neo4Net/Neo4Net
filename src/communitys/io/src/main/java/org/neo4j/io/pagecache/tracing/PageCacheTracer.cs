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

	using PageCacheCounters = Neo4Net.Io.pagecache.monitoring.PageCacheCounters;

	/// <summary>
	/// A PageCacheTracer receives a steady stream of events and data about what
	/// whole page cache is doing. Implementations of this interface should be as
	/// efficient as possible, lest they severely slow down the page cache.
	/// </summary>
	public interface PageCacheTracer : PageCacheCounters
	{
		 /// <summary>
		 /// A PageCacheTracer that does nothing other than return the NULL variants of the companion interfaces.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 PageCacheTracer NULL = new PageCacheTracer()
	//	 {
	//		  @@Override public void mappedFile(File file)
	//		  {
	//		  }
	//
	//		  @@Override public void unmappedFile(File file)
	//		  {
	//		  }
	//
	//		  @@Override public EvictionRunEvent beginPageEvictions(int pageCountToEvict)
	//		  {
	//				return EvictionRunEvent.NULL;
	//		  }
	//
	//		  @@Override public MajorFlushEvent beginFileFlush(PageSwapper swapper)
	//		  {
	//				return MajorFlushEvent.NULL;
	//		  }
	//
	//		  @@Override public MajorFlushEvent beginCacheFlush()
	//		  {
	//				return MajorFlushEvent.NULL;
	//		  }
	//
	//		  @@Override public long faults()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long evictions()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long pins()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long unpins()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long hits()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long flushes()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long bytesRead()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long bytesWritten()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long filesMapped()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long filesUnmapped()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long evictionExceptions()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public double hitRatio()
	//		  {
	//				return 0d;
	//		  }
	//
	//		  @@Override public double usageRatio()
	//		  {
	//				return 0d;
	//		  }
	//
	//		  @@Override public void pins(long pins)
	//		  {
	//		  }
	//
	//		  @@Override public void unpins(long unpins)
	//		  {
	//		  }
	//
	//		  @@Override public void hits(long hits)
	//		  {
	//		  }
	//
	//		  @@Override public void faults(long faults)
	//		  {
	//		  }
	//
	//		  @@Override public void bytesRead(long bytesRead)
	//		  {
	//		  }
	//
	//		  @@Override public void evictions(long evictions)
	//		  {
	//		  }
	//
	//		  @@Override public void evictionExceptions(long evictionExceptions)
	//		  {
	//		  }
	//
	//		  @@Override public void bytesWritten(long bytesWritten)
	//		  {
	//		  }
	//
	//		  @@Override public void flushes(long flushes)
	//		  {
	//		  }
	//
	//		  @@Override public void maxPages(long maxPages)
	//		  {
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return PageCacheTracer.class.getName() + ".NULL";
	//		  }
	//	 };

		 /// <summary>
		 /// The given file has been mapped, where no existing mapping for that file existed.
		 /// </summary>
		 void MappedFile( File file );

		 /// <summary>
		 /// The last reference to the given file has been unmapped.
		 /// </summary>
		 void UnmappedFile( File file );

		 /// <summary>
		 /// A background eviction has begun. Called from the background eviction thread.
		 /// 
		 /// This call will be paired with a following PageCacheTracer#endPageEviction call.
		 /// 
		 /// The method returns an EvictionRunEvent to represent the event of this eviction run.
		 /// 
		 /// </summary>
		 EvictionRunEvent BeginPageEvictions( int pageCountToEvict );

		 /// <summary>
		 /// A PagedFile wants to flush all its bound pages.
		 /// </summary>
		 MajorFlushEvent BeginFileFlush( PageSwapper swapper );

		 /// <summary>
		 /// The PageCache wants to flush all its bound pages.
		 /// </summary>
		 MajorFlushEvent BeginCacheFlush();

		 /// <summary>
		 /// Report number of observed pins </summary>
		 /// <param name="pins"> number of pins </param>
		 void Pins( long pins );

		 /// <summary>
		 /// Report number of observed unpins </summary>
		 /// <param name="unpins"> number of unpins </param>
		 void Unpins( long unpins );

		 /// <summary>
		 /// Report number of observer hits </summary>
		 /// <param name="hits"> number of hits </param>
		 void Hits( long hits );

		 /// <summary>
		 /// Report number of observed faults </summary>
		 /// <param name="faults"> number of faults </param>
		 void Faults( long faults );

		 /// <summary>
		 /// Report number of bytes read </summary>
		 /// <param name="bytesRead"> number of read bytes </param>
		 void BytesRead( long bytesRead );

		 /// <summary>
		 /// Report number of observed evictions </summary>
		 /// <param name="evictions"> number of evictions </param>
		 void Evictions( long evictions );

		 /// <summary>
		 /// Report number of eviction exceptions </summary>
		 /// <param name="evictionExceptions"> number of eviction exceptions </param>
		 void EvictionExceptions( long evictionExceptions );

		 /// <summary>
		 /// Report number of bytes written </summary>
		 /// <param name="bytesWritten"> number of written bytes </param>
		 void BytesWritten( long bytesWritten );

		 /// <summary>
		 /// Report number of flushes </summary>
		 /// <param name="flushes"> number of flushes </param>
		 void Flushes( long flushes );

		 /// <summary>
		 /// Sets the number of available pages. </summary>
		 /// <param name="maxPages"> the total number of available pages. </param>
		 void MaxPages( long maxPages );
	}

}