using System;

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
namespace Neo4Net.causalclustering.core.state
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RaftMembershipState = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipState;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using GlobalSessionTrackerState = Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using IdAllocationState = Neo4Net.causalclustering.core.state.machines.id.IdAllocationState;
	using ReplicatedLockTokenState = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.ReplicationModule.SESSION_TRACKER_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.IdentityModule.CORE_MEMBER_ID_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ConsensusModule.RAFT_MEMBERSHIP_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ConsensusModule.RAFT_TERM_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ConsensusModule.RAFT_VOTE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.server.CoreServerModule.LAST_FLUSHED_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule.ID_ALLOCATION_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule.LOCK_TOKEN_NAME;

	public class DumpClusterStateTest
	{
		private bool InstanceFieldsInitialized = false;

		public DumpClusterStateTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_clusterStateDirectory = new ClusterStateDirectory( _dataDir, false );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsa = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fsa = new EphemeralFileSystemRule();
		 private File _dataDir = new File( "data" );
		 private ClusterStateDirectory _clusterStateDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws ClusterStateException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _clusterStateDirectory.initialize( Fsa.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpClusterState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpClusterState()
		 {
			  // given
			  CreateStates();
			  MemoryStream @out = new MemoryStream();
			  DumpClusterState dumpTool = new DumpClusterState( Fsa.get(), _dataDir, new PrintStream(@out) );

			  // when
			  dumpTool.Dump();

			  // then
			  int lineCount = @out.ToString().Split(Environment.NewLine, true).length;
			  assertEquals( 8, lineCount );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createStates() throws java.io.IOException
		 private void CreateStates()
		 {
			  SimpleStorage<MemberId> memberIdStorage = new SimpleFileStorage<MemberId>( Fsa.get(), _clusterStateDirectory.get(), CORE_MEMBER_ID_NAME, new MemberId.Marshal(), NullLogProvider.Instance );
			  memberIdStorage.WriteState( new MemberId( System.Guid.randomUUID() ) );

			  CreateDurableState( LAST_FLUSHED_NAME, new LongIndexMarshal() );
			  CreateDurableState( LOCK_TOKEN_NAME, new ReplicatedLockTokenState.Marshal( new MemberId.Marshal() ) );
			  CreateDurableState( ID_ALLOCATION_NAME, new IdAllocationState.Marshal() );
			  CreateDurableState( SESSION_TRACKER_NAME, new GlobalSessionTrackerState.Marshal( new MemberId.Marshal() ) );

			  /* raft state */
			  CreateDurableState( RAFT_MEMBERSHIP_NAME, new RaftMembershipState.Marshal() );
			  CreateDurableState( RAFT_TERM_NAME, new TermState.Marshal() );
			  CreateDurableState( RAFT_VOTE_NAME, new VoteState.Marshal( new MemberId.Marshal() ) );
		 }

		 private void CreateDurableState<T>( string name, StateMarshal<T> marshal )
		 {
			  DurableStateStorage<T> storage = new DurableStateStorage<T>( Fsa.get(), _clusterStateDirectory.get(), name, marshal, 1024, NullLogProvider.Instance );

			  //noinspection EmptyTryBlock: Will create initial state.
			  using ( Lifespan ignored = new Lifespan( storage ) )
			  {
			  }
		 }
	}

}