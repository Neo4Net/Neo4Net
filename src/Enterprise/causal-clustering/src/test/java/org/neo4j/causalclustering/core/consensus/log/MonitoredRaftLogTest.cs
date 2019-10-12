/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.consensus.log
{
	using Test = org.junit.Test;

	using RaftLogAppendIndexMonitor = Neo4Net.causalclustering.core.consensus.log.monitoring.RaftLogAppendIndexMonitor;
	using RaftLogCommitIndexMonitor = Neo4Net.causalclustering.core.consensus.log.monitoring.RaftLogCommitIndexMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class MonitoredRaftLogTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMonitorAppendIndexAndCommitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorAppendIndexAndCommitIndex()
		 {
			  // Given
			  Monitors monitors = new Monitors();
			  StubRaftLogAppendIndexMonitor appendMonitor = new StubRaftLogAppendIndexMonitor();
			  monitors.AddMonitorListener( appendMonitor );

			  StubRaftLogCommitIndexMonitor commitMonitor = new StubRaftLogCommitIndexMonitor();
			  monitors.AddMonitorListener( commitMonitor );

			  MonitoredRaftLog log = new MonitoredRaftLog( new InMemoryRaftLog(), monitors );

			  // When
			  log.Append( new RaftLogEntry( 0, ReplicatedInteger.valueOf( 1 ) ) );
			  log.Append( new RaftLogEntry( 0, ReplicatedInteger.valueOf( 1 ) ) );

			  assertEquals( 1, appendMonitor.AppendIndex() );
			  assertEquals( 0, commitMonitor.CommitIndex() );

			  log.Truncate( 1 );
			  assertEquals( 0, appendMonitor.AppendIndex() );
		 }

		 private class StubRaftLogCommitIndexMonitor : RaftLogCommitIndexMonitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CommitIndexConflict;

			  public override long CommitIndex()
			  {
					return CommitIndexConflict;
			  }

			  public override void CommitIndex( long commitIndex )
			  {
					this.CommitIndexConflict = commitIndex;
			  }
		 }

		 private class StubRaftLogAppendIndexMonitor : RaftLogAppendIndexMonitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long AppendIndexConflict;

			  public override long AppendIndex()
			  {
					return AppendIndexConflict;
			  }

			  public override void AppendIndex( long appendIndex )
			  {
					this.AppendIndexConflict = appendIndex;
			  }
		 }
	}

}