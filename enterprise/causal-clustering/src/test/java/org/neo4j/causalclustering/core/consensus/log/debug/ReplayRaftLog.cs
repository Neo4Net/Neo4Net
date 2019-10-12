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
namespace Org.Neo4j.causalclustering.core.consensus.log.debug
{

	using CoreLogPruningStrategy = Org.Neo4j.causalclustering.core.consensus.log.segmented.CoreLogPruningStrategy;
	using CoreLogPruningStrategyFactory = Org.Neo4j.causalclustering.core.consensus.log.segmented.CoreLogPruningStrategyFactory;
	using SegmentedRaftLog = Org.Neo4j.causalclustering.core.consensus.log.segmented.SegmentedRaftLog;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using ReplicatedTransaction = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionFactory = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransactionFactory;
	using CoreReplicatedContentMarshal = Org.Neo4j.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using Args = Org.Neo4j.Helpers.Args;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_reader_pool_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_rotation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class ReplayRaftLog
	{
		 private ReplayRaftLog()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  Args arg = Args.parse( args );

			  string from = arg.Get( "from" );
			  Console.WriteLine( "From is " + from );
			  string to = arg.Get( "to" );
			  Console.WriteLine( "to is " + to );

			  File logDirectory = new File( from );
			  Console.WriteLine( "logDirectory = " + logDirectory );
			  Config config = Config.defaults( stringMap() );

			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					LogProvider logProvider = Instance;
					CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( config.Get( raft_log_pruning_strategy ), logProvider ) ).newInstance();
					SegmentedRaftLog log = new SegmentedRaftLog( fileSystem, logDirectory, config.Get( raft_log_rotation_size ), CoreReplicatedContentMarshal.marshaller(), logProvider, config.Get(raft_log_reader_pool_size), Clocks.systemClock(), new ThreadPoolJobScheduler(), pruningStrategy );

					long totalCommittedEntries = log.AppendIndex(); // Not really, but we need to have a way to pass in the commit index
					for ( int i = 0; i <= totalCommittedEntries; i++ )
					{
						 ReplicatedContent content = readLogEntry( log, i ).content();
						 if ( content is ReplicatedTransaction )
						 {
							  ReplicatedTransaction tx = ( ReplicatedTransaction ) content;
							  ReplicatedTransactionFactory.extractTransactionRepresentation( tx, new sbyte[0] ).accept(element =>
							  {
								Console.WriteLine( element );
								return false;
							  });
						 }
					}
			  }
		 }
	}

}