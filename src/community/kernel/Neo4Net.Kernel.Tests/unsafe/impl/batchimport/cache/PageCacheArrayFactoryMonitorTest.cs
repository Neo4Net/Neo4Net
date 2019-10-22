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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using Test = org.junit.Test;

	using PageCache = Neo4Net.Io.pagecache.PageCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;

	public class PageCacheArrayFactoryMonitorTest
	{
		 private readonly PageCachedNumberArrayFactory _factory = new PageCachedNumberArrayFactory( mock( typeof( PageCache ) ), new File( "storeDir" ) );
		 private readonly PageCacheArrayFactoryMonitor _monitor = new PageCacheArrayFactoryMonitor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComposeFailureDescriptionForFailedCandidates()
		 public virtual void ShouldComposeFailureDescriptionForFailedCandidates()
		 {
			  // given
			  _monitor.allocationSuccessful( 123, _factory, asList( new NumberArrayFactory_AllocationFailure( new System.OutOfMemoryException( "OOM1" ), HEAP ), new NumberArrayFactory_AllocationFailure( new System.OutOfMemoryException( "OOM2" ), OFF_HEAP ) ) );

			  // when
			  string failure = _monitor.pageCacheAllocationOrNull();

			  // then
			  assertThat( failure, containsString( "OOM1" ) );
			  assertThat( failure, containsString( "OOM2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearFailureStateAfterAccessorCall()
		 public virtual void ShouldClearFailureStateAfterAccessorCall()
		 {
			  // given
			  _monitor.allocationSuccessful( 123, _factory, asList( new NumberArrayFactory_AllocationFailure( new System.OutOfMemoryException( "OOM1" ), HEAP ), new NumberArrayFactory_AllocationFailure( new System.OutOfMemoryException( "OOM2" ), OFF_HEAP ) ) );

			  // when
			  string failure = _monitor.pageCacheAllocationOrNull();
			  string secondCall = _monitor.pageCacheAllocationOrNull();

			  // then
			  assertNotNull( failure );
			  assertNull( secondCall );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullFailureOnNoFailure()
		 public virtual void ShouldReturnNullFailureOnNoFailure()
		 {
			  // then
			  assertNull( _monitor.pageCacheAllocationOrNull() );
		 }
	}

}