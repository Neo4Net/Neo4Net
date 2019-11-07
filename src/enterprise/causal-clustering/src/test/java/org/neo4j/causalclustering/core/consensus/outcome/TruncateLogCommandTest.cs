/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.outcome
{
	using Test = org.junit.Test;

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.ReplicatedInteger.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;

	public class TruncateLogCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applyTo()
		 public virtual void ApplyTo()
		 {
			  //Test that truncate commands correctly remove entries from the cache.

			  //given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  long fromIndex = 2L;
			  TruncateLogCommand truncateLogCommand = new TruncateLogCommand( fromIndex );
			  InFlightCache inFlightCache = new ConsecutiveInFlightCache();

			  inFlightCache.Put( 0L, new RaftLogEntry( 0L, ValueOf( 0 ) ) );
			  inFlightCache.Put( 1L, new RaftLogEntry( 1L, ValueOf( 1 ) ) );
			  inFlightCache.Put( 2L, new RaftLogEntry( 2L, ValueOf( 2 ) ) );
			  inFlightCache.Put( 3L, new RaftLogEntry( 3L, ValueOf( 3 ) ) );

			  //when
			  truncateLogCommand.ApplyTo( inFlightCache, log );

			  //then
			  assertNotNull( inFlightCache.Get( 0L ) );
			  assertNotNull( inFlightCache.Get( 1L ) );
			  assertNull( inFlightCache.Get( 2L ) );
			  assertNull( inFlightCache.Get( 3L ) );

			  logProvider.AssertAtLeastOnce( inLog( this.GetType() ).debug("Start truncating in-flight-map from index %d. Current map:%n%s", fromIndex, inFlightCache) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateWithGaps()
		 public virtual void ShouldTruncateWithGaps()
		 {
			  //given
			  long fromIndex = 1L;
			  TruncateLogCommand truncateLogCommand = new TruncateLogCommand( fromIndex );

			  InFlightCache inFlightCache = new ConsecutiveInFlightCache();

			  inFlightCache.Put( 0L, new RaftLogEntry( 0L, ValueOf( 0 ) ) );
			  inFlightCache.Put( 2L, new RaftLogEntry( 1L, ValueOf( 1 ) ) );
			  inFlightCache.Put( 4L, new RaftLogEntry( 2L, ValueOf( 2 ) ) );

			  truncateLogCommand.ApplyTo( inFlightCache, NullLog.Instance );

			  inFlightCache.Put( 1L, new RaftLogEntry( 3L, ValueOf( 1 ) ) );
			  inFlightCache.Put( 2L, new RaftLogEntry( 4L, ValueOf( 2 ) ) );

			  assertNotNull( inFlightCache.Get( 1L ) );
			  assertNotNull( inFlightCache.Get( 2L ) );
		 }
	}

}