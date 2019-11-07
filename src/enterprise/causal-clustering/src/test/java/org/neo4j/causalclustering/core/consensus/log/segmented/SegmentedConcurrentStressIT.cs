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

	using Neo4Net.causalclustering.core.consensus.log;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.NullLogProvider.getInstance;

	public class SegmentedConcurrentStressIT : ConcurrentStressIT<SegmentedRaftLog>
	{
		 public override SegmentedRaftLog CreateRaftLog( FileSystemAbstraction fsa, File dir )
		 {
			  long rotateAtSize = ByteUnit.mebiBytes( 8 );
			  LogProvider logProvider = Instance;
			  int readerPoolSize = 8;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( raft_log_pruning_strategy.DefaultValue, logProvider ) ).newInstance();
			  return new SegmentedRaftLog( fsa, dir, rotateAtSize, new DummyRaftableContentSerializer(), logProvider, readerPoolSize, Clocks.fakeClock(), new OnDemandJobScheduler(), pruningStrategy );
		 }
	}

}