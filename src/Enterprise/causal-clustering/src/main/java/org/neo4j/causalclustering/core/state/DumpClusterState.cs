using System;

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
namespace Neo4Net.causalclustering.core.state
{

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
	using Marshal = Neo4Net.causalclustering.identity.MemberId.Marshal;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.ReplicationModule.SESSION_TRACKER_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.server.CoreServerModule.LAST_FLUSHED_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.IdentityModule.CORE_MEMBER_ID_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ConsensusModule.RAFT_MEMBERSHIP_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ConsensusModule.RAFT_TERM_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ConsensusModule.RAFT_VOTE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.CoreStateMachinesModule.ID_ALLOCATION_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.CoreStateMachinesModule.LOCK_TOKEN_NAME;

	public class DumpClusterState
	{
		 private readonly FileSystemAbstraction _fs;
		 private readonly File _clusterStateDirectory;
		 private readonly PrintStream @out;

		 /// <param name="args"> [0] = data directory </param>
		 /// <exception cref="IOException"> When IO exception occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException, ClusterStateException
		 public static void Main( string[] args )
		 {
			  if ( args.Length != 1 )
			  {
					Console.WriteLine( "usage: DumpClusterState <data directory>" );
					Environment.Exit( 1 );
			  }

			  File dataDirectory = new File( args[0] );

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					DumpClusterState dumpTool = new DumpClusterState( fileSystem, dataDirectory, System.out );
					dumpTool.Dump();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DumpClusterState(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File dataDirectory, java.io.PrintStream out) throws ClusterStateException
		 internal DumpClusterState( FileSystemAbstraction fs, File dataDirectory, PrintStream @out )
		 {
			  this._fs = fs;
			  this._clusterStateDirectory = ( new ClusterStateDirectory( dataDirectory ) ).Initialize( fs ).get();
			  this.@out = @out;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dump() throws java.io.IOException
		 internal virtual void Dump()
		 {
			  SimpleStorage<MemberId> memberIdStorage = new SimpleFileStorage<MemberId>( _fs, _clusterStateDirectory, CORE_MEMBER_ID_NAME, new MemberId.Marshal(), NullLogProvider.Instance );
			  if ( memberIdStorage.Exists() )
			  {
					MemberId memberId = memberIdStorage.ReadState();
					@out.println( CORE_MEMBER_ID_NAME + ": " + memberId );
			  }

			  DumpState( LAST_FLUSHED_NAME, new LongIndexMarshal() );
			  DumpState( LOCK_TOKEN_NAME, new ReplicatedLockTokenState.Marshal( new MemberId.Marshal() ) );
			  DumpState( ID_ALLOCATION_NAME, new IdAllocationState.Marshal() );
			  DumpState( SESSION_TRACKER_NAME, new GlobalSessionTrackerState.Marshal( new MemberId.Marshal() ) );

			  /* raft state */
			  DumpState( RAFT_MEMBERSHIP_NAME, new RaftMembershipState.Marshal() );
			  DumpState( RAFT_TERM_NAME, new TermState.Marshal() );
			  DumpState( RAFT_VOTE_NAME, new VoteState.Marshal( new MemberId.Marshal() ) );
		 }

		 private void DumpState<T1>( string name, StateMarshal<T1> marshal )
		 {
			  int rotationSize = Config.defaults().get(CausalClusteringSettings.replicated_lock_token_state_size);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.core.state.storage.DurableStateStorage<?> storage = new org.neo4j.causalclustering.core.state.storage.DurableStateStorage<>(fs, clusterStateDirectory, name, marshal, rotationSize, org.neo4j.logging.NullLogProvider.getInstance());
			  DurableStateStorage<object> storage = new DurableStateStorage<object>( _fs, _clusterStateDirectory, name, marshal, rotationSize, NullLogProvider.Instance );

			  if ( storage.Exists() )
			  {
					using ( Lifespan ignored = new Lifespan( storage ) )
					{
						 @out.println( name + ": " + storage.InitialState );
					}
			  }
		 }
	}

}