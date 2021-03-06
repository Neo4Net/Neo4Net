﻿/*
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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using OnDemandJobScheduler = Org.Neo4j.Test.OnDemandJobScheduler;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogContractTest : RaftLogContractTest
	{
		private bool InstanceFieldsInitialized = false;

		public SegmentedRaftLogContractTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( _fsRule ).around( _life );
		}

		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly LifeRule _life = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(fsRule).around(life);
		 public RuleChain Chain;

		 public override RaftLog CreateRaftLog()
		 {
			  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
			  FileSystemAbstraction fileSystem = _fsRule.get();
			  fileSystem.Mkdir( directory );

			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( "1 entries", logProvider ) ).NewInstance();
			  return _life.add( new SegmentedRaftLog( fileSystem, directory, 1024, new DummyRaftableContentSerializer(), logProvider, 8, Clocks.fakeClock(), new OnDemandJobScheduler(), pruningStrategy ) );
		 }
	}

}