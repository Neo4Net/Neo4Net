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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.closeTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class DefaultPageCacheTracerTest
	public class DefaultPageCacheTracerTest
	{
		 private PageCacheTracer _tracer;
		 private PageSwapper _swapper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach public void setUp()
		 public virtual void SetUp()
		 {
			  _tracer = new DefaultPageCacheTracer();
			  _swapper = new DummyPageSwapper( "filename", ( int ) ByteUnit.kibiBytes( 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCountEvictions()
		 internal virtual void MustCountEvictions()
		 {
			  using ( EvictionRunEvent evictionRunEvent = _tracer.beginPageEvictions( 2 ) )
			  {
					using ( EvictionEvent evictionEvent = evictionRunEvent.BeginEviction() )
					{
						 FlushEvent flushEvent = evictionEvent.FlushEventOpportunity().beginFlush(0, 0, _swapper);
						 flushEvent.AddBytesWritten( 12 );
						 flushEvent.Done();
					}

					using ( EvictionEvent evictionEvent = evictionRunEvent.BeginEviction() )
					{
						 FlushEvent flushEvent = evictionEvent.FlushEventOpportunity().beginFlush(0, 0, _swapper);
						 flushEvent.AddBytesWritten( 12 );
						 flushEvent.Done();
						 evictionEvent.ThrewException( new IOException() );
					}

					using ( EvictionEvent evictionEvent = evictionRunEvent.BeginEviction() )
					{
						 FlushEvent flushEvent = evictionEvent.FlushEventOpportunity().beginFlush(0, 0, _swapper);
						 flushEvent.AddBytesWritten( 12 );
						 flushEvent.Done();
						 evictionEvent.ThrewException( new IOException() );
					}

					evictionRunEvent.BeginEviction().close();
			  }

			  AssertCounts( 0, 0, 0, 0, 4, 2, 3, 0, 36, 0, 0, 0d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCountFileMappingAndUnmapping()
		 internal virtual void MustCountFileMappingAndUnmapping()
		 {
			  _tracer.mappedFile( new File( "a" ) );

			  AssertCounts( 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0d );

			  _tracer.unmappedFile( new File( "a" ) );

			  AssertCounts( 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCountFlushes()
		 internal virtual void MustCountFlushes()
		 {
			  using ( MajorFlushEvent cacheFlush = _tracer.beginCacheFlush() )
			  {
					cacheFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
					cacheFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
					cacheFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
			  }

			  AssertCounts( 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0d );

			  using ( MajorFlushEvent fileFlush = _tracer.beginFileFlush( _swapper ) )
			  {
					fileFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
					fileFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
					fileFlush.FlushEventOpportunity().beginFlush(0, 0, _swapper).done();
			  }

			  AssertCounts( 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateHitRatio()
		 internal virtual void ShouldCalculateHitRatio()
		 {
			  assertThat( "hitRation", _tracer.hitRatio(), closeTo(0d, 0.0001) );
			  _tracer.hits( 3 );
			  _tracer.faults( 7 );
			  assertThat( "hitRation", _tracer.hitRatio(), closeTo(3.0 / 10, 0.0001) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void usageRatio()
		 internal virtual void UsageRatio()
		 {
			  assertThat( _tracer.usageRatio(), @is(Double.NaN) );
			  _tracer.maxPages( 10 );
			  assertThat( _tracer.usageRatio(), closeTo(0d, 0.0001) );
			  _tracer.faults( 5 );
			  assertThat( _tracer.usageRatio(), closeTo(0.5, 0.0001) );
			  _tracer.faults( 5 );
			  _tracer.evictions( 5 );
			  assertThat( _tracer.usageRatio(), closeTo(0.5, 0.0001) );
			  _tracer.faults( 5 );
			  assertThat( _tracer.usageRatio(), closeTo(1d, 0.0001) );
		 }

		 private void AssertCounts( long pins, long unpins, long hits, long faults, long evictions, long evictionExceptions, long flushes, long bytesRead, long bytesWritten, long filesMapped, long filesUnmapped, double hitRatio )
		 {
			  assertThat( "pins", _tracer.pins(), @is(pins) );
			  assertThat( "unpins", _tracer.unpins(), @is(unpins) );
			  assertThat( "hits", _tracer.hits(), @is(hits) );
			  assertThat( "faults", _tracer.faults(), @is(faults) );
			  assertThat( "evictions", _tracer.evictions(), @is(evictions) );
			  assertThat( "evictionExceptions", _tracer.evictionExceptions(), @is(evictionExceptions) );
			  assertThat( "flushes", _tracer.flushes(), @is(flushes) );
			  assertThat( "bytesRead", _tracer.bytesRead(), @is(bytesRead) );
			  assertThat( "bytesWritten", _tracer.bytesWritten(), @is(bytesWritten) );
			  assertThat( "filesMapped", _tracer.filesMapped(), @is(filesMapped) );
			  assertThat( "filesUnmapped", _tracer.filesUnmapped(), @is(filesUnmapped) );
			  assertThat( "hitRatio", _tracer.hitRatio(), closeTo(hitRatio, 0.0001) );
		 }
	}

}