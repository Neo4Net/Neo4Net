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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using DefaultPageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracer;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class DefaultPageCursorTracerTest
	{
		 private PageSwapper _swapper;
		 private PageCursorTracer _pageCursorTracer;
		 private DefaultPageCacheTracer _cacheTracer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _cacheTracer = new DefaultPageCacheTracer();
			  _pageCursorTracer = CreateTracer();
			  _swapper = new DummyPageSwapper( "filename", ( int ) ByteUnit.kibiBytes( 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countPinsAndUnpins()
		 internal virtual void CountPinsAndUnpins()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  pinEvent.Done();
			  pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );

			  assertEquals( 2, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.unpins() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noHitForPinEventWithPageFault()
		 internal virtual void NoHitForPinEventWithPageFault()
		 {
			  PinFaultAndHit();

			  assertEquals( 1, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.faults() );
			  assertEquals( 0, _pageCursorTracer.hits() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void hitForPinEventWithoutPageFault()
		 internal virtual void HitForPinEventWithoutPageFault()
		 {
			  PinAndHit();

			  assertEquals( 1, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.hits() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void accumulateHitsReporting()
		 internal virtual void AccumulateHitsReporting()
		 {
			  PinAndHit();
			  PinAndHit();

			  assertEquals( 2, _pageCursorTracer.hits() );
			  assertEquals( 2, _pageCursorTracer.accumulatedHits() );

			  _pageCursorTracer.reportEvents();
			  PinAndHit();

			  assertEquals( 1, _pageCursorTracer.hits() );
			  assertEquals( 3, _pageCursorTracer.accumulatedHits() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void accumulatedFaultsReporting()
		 internal virtual void AccumulatedFaultsReporting()
		 {
			  PinFaultAndHit();
			  PinFaultAndHit();

			  assertEquals( 2, _pageCursorTracer.faults() );
			  assertEquals( 2, _pageCursorTracer.accumulatedFaults() );

			  _pageCursorTracer.reportEvents();
			  PinFaultAndHit();
			  PinFaultAndHit();

			  assertEquals( 2, _pageCursorTracer.faults() );
			  assertEquals( 4, _pageCursorTracer.accumulatedFaults() );
			  assertEquals( 0, _pageCursorTracer.accumulatedHits() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countHitsOnlyForPinEventsWithoutPageFaults()
		 internal virtual void CountHitsOnlyForPinEventsWithoutPageFaults()
		 {
			  PinAndHit();
			  PinAndHit();
			  PinAndHit();
			  PinFaultAndHit();
			  PinFaultAndHit();
			  PinAndHit();
			  PinAndHit();

			  assertEquals( 7, _pageCursorTracer.pins() );
			  assertEquals( 5, _pageCursorTracer.hits() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countPageFaultsAndBytesRead()
		 internal virtual void CountPageFaultsAndBytesRead()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  {
					PageFaultEvent pageFaultEvent = pinEvent.BeginPageFault();
					{
						 pageFaultEvent.AddBytesRead( 42 );
					}
					pageFaultEvent.Done();
					pageFaultEvent = pinEvent.BeginPageFault();
					{
						 pageFaultEvent.AddBytesRead( 42 );
					}
					pageFaultEvent.Done();
			  }
			  pinEvent.Done();

			  assertEquals( 1, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.unpins() );
			  assertEquals( 2, _pageCursorTracer.faults() );
			  assertEquals( 84, _pageCursorTracer.bytesRead() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countPageEvictions()
		 internal virtual void CountPageEvictions()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  {
					PageFaultEvent faultEvent = pinEvent.BeginPageFault();
					{
						 EvictionEvent evictionEvent = faultEvent.BeginEviction();
						 evictionEvent.FilePageId = 0;
						 evictionEvent.CachePageId = 0;
						 evictionEvent.ThrewException( new IOException( "exception" ) );
						 evictionEvent.Close();
					}
					faultEvent.Done();
			  }
			  pinEvent.Done();

			  assertEquals( 1, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.unpins() );
			  assertEquals( 1, _pageCursorTracer.faults() );
			  assertEquals( 1, _pageCursorTracer.evictions() );
			  assertEquals( 1, _pageCursorTracer.evictionExceptions() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countFlushesAndBytesWritten()
		 internal virtual void CountFlushesAndBytesWritten()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  {
					PageFaultEvent faultEvent = pinEvent.BeginPageFault();
					{
						 EvictionEvent evictionEvent = faultEvent.BeginEviction();
						 {
							  FlushEventOpportunity flushEventOpportunity = evictionEvent.FlushEventOpportunity();
							  {
									FlushEvent flushEvent = flushEventOpportunity.BeginFlush( 0, 0, _swapper );
									flushEvent.AddBytesWritten( 27 );
									flushEvent.Done();
									FlushEvent flushEvent1 = flushEventOpportunity.BeginFlush( 0, 1, _swapper );
									flushEvent1.AddBytesWritten( 13 );
									flushEvent1.Done();
							  }
						 }
						 evictionEvent.Close();
					}
					faultEvent.Done();
			  }
			  pinEvent.Done();

			  assertEquals( 1, _pageCursorTracer.pins() );
			  assertEquals( 1, _pageCursorTracer.unpins() );
			  assertEquals( 1, _pageCursorTracer.faults() );
			  assertEquals( 1, _pageCursorTracer.evictions() );
			  assertEquals( 2, _pageCursorTracer.flushes() );
			  assertEquals( 40, _pageCursorTracer.bytesWritten() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void reportCountersToPageCursorTracer()
		 internal virtual void ReportCountersToPageCursorTracer()
		 {
			  GenerateEventSet();
			  _pageCursorTracer.reportEvents();

			  assertEquals( 1, _cacheTracer.pins() );
			  assertEquals( 1, _cacheTracer.unpins() );
			  assertEquals( 1, _cacheTracer.faults() );
			  assertEquals( 1, _cacheTracer.evictions() );
			  assertEquals( 1, _cacheTracer.evictionExceptions() );
			  assertEquals( 1, _cacheTracer.flushes() );
			  assertEquals( 10, _cacheTracer.bytesWritten() );
			  assertEquals( 150, _cacheTracer.bytesRead() );

			  GenerateEventSet();
			  GenerateEventSet();
			  _pageCursorTracer.reportEvents();

			  assertEquals( 3, _cacheTracer.pins() );
			  assertEquals( 3, _cacheTracer.unpins() );
			  assertEquals( 3, _cacheTracer.faults() );
			  assertEquals( 3, _cacheTracer.evictions() );
			  assertEquals( 3, _cacheTracer.evictionExceptions() );
			  assertEquals( 3, _cacheTracer.flushes() );
			  assertEquals( 30, _cacheTracer.bytesWritten() );
			  assertEquals( 450, _cacheTracer.bytesRead() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateHitRatio()
		 internal virtual void ShouldCalculateHitRatio()
		 {
			  assertEquals( 0d, _pageCursorTracer.hitRatio(), 0.0001 );

			  PinFaultAndHit();

			  assertEquals( 0.0 / 1, _pageCursorTracer.hitRatio(), 0.0001 );

			  PinAndHit();

			  assertEquals( 1.0 / 2, _pageCursorTracer.hitRatio(), 0.0001 );

			  PinFaultAndHit();
			  PinFaultAndHit();
			  PinFaultAndHit();
			  PinAndHit();
			  PinAndHit();

			  assertEquals( 3.0 / 7, _pageCursorTracer.hitRatio(), 0.0001 );

			  _pageCursorTracer.reportEvents();

			  assertEquals( 3.0 / 7, _cacheTracer.hitRatio(), 0.0001 );
		 }

		 private void GenerateEventSet()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( false, 0, _swapper );
			  {
					PageFaultEvent pageFaultEvent = pinEvent.BeginPageFault();
					pageFaultEvent.AddBytesRead( 150 );
					{
						 EvictionEvent evictionEvent = pageFaultEvent.BeginEviction();
						 {
							  FlushEventOpportunity flushEventOpportunity = evictionEvent.FlushEventOpportunity();
							  FlushEvent flushEvent = flushEventOpportunity.BeginFlush( 0, 0, _swapper );
							  flushEvent.AddBytesWritten( 10 );
							  flushEvent.Done();
						 }
						 evictionEvent.ThrewException( new IOException( "eviction exception" ) );
						 evictionEvent.Close();
					}
					pageFaultEvent.Done();
			  }
			  pinEvent.Done();
		 }

		 private PageCursorTracer CreateTracer()
		 {
			  DefaultPageCursorTracer pageCursorTracer = new DefaultPageCursorTracer();
			  pageCursorTracer.Init( _cacheTracer );
			  return pageCursorTracer;
		 }

		 private void PinAndHit()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  pinEvent.Hit();
			  pinEvent.Done();
		 }

		 private void PinFaultAndHit()
		 {
			  PinEvent pinEvent = _pageCursorTracer.beginPin( true, 0, _swapper );
			  PageFaultEvent pageFaultEvent = pinEvent.BeginPageFault();
			  pinEvent.Hit();
			  pageFaultEvent.Done();
			  pinEvent.Done();
		 }
	}

}