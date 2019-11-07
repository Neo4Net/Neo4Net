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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogVerificationIT : RaftLogVerificationIT
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.causalclustering.core.consensus.log.RaftLog createRaftLog() throws Throwable
		 protected internal override RaftLog CreateRaftLog()
		 {
			  FileSystemAbstraction fsa = FsRule.get();

			  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
			  fsa.Mkdir( directory );

			  long rotateAtSizeBytes = 128;
			  int readerPoolSize = 8;

			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( raft_log_pruning_strategy.DefaultValue, logProvider ) ).newInstance();
			  SegmentedRaftLog newRaftLog = new SegmentedRaftLog( fsa, directory, rotateAtSizeBytes, new DummyRaftableContentSerializer(), logProvider, readerPoolSize, Clocks.systemClock(), new OnDemandJobScheduler(), pruningStrategy );

			  newRaftLog.Init();
			  newRaftLog.Start();

			  return newRaftLog;
		 }

		 protected internal override long Operations()
		 {
			  return 500;
		 }
	}

}