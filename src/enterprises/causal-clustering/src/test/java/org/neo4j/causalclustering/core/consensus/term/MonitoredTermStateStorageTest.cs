/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.term
{
	using Test = org.junit.Test;

	using RaftTermMonitor = Neo4Net.causalclustering.core.consensus.log.monitoring.RaftTermMonitor;
	using Neo4Net.causalclustering.core.state.storage;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class MonitoredTermStateStorageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMonitorTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorTerm()
		 {
			  // given
			  Monitors monitors = new Monitors();
			  StubRaftTermMonitor raftTermMonitor = new StubRaftTermMonitor();
			  monitors.AddMonitorListener( raftTermMonitor );
			  TermState state = new TermState();
			  MonitoredTermStateStorage monitoredTermStateStorage = new MonitoredTermStateStorage( new InMemoryStateStorage<TermState>( new TermState() ), monitors );

			  // when
			  state.Update( 7 );
			  monitoredTermStateStorage.PersistStoreData( state );

			  // then
			  assertEquals( 7, raftTermMonitor.Term() );
		 }

		 private class StubRaftTermMonitor : RaftTermMonitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long TermConflict;

			  public override long Term()
			  {
					return TermConflict;
			  }

			  public override void Term( long term )
			  {
					this.TermConflict = term;
			  }
		 }
	}

}