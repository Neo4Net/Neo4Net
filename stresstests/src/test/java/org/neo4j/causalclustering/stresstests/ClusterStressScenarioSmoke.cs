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
namespace Org.Neo4j.causalclustering.stresstests
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.stresstests.ClusterStressTesting.stressTest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.findCauseOrSuppressed;

	public class ClusterStressScenarioSmoke
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterStressScenarioSmoke()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _fileSystem ).around( _pageCacheRule );
		}

		 private readonly DefaultFileSystemRule _fileSystem = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fileSystem).around(pageCacheRule);
		 public RuleChain Rules;

		 private Config _config;
		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  this._pageCache = _pageCacheRule.getPageCache( _fileSystem );

			  _config = new Config();
			  _config.workDurationMinutes( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressBackupRandomMemberAndStartStop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressBackupRandomMemberAndStartStop()
		 {
			  _config.workloads( Workloads.CreateNodesWithProperties, Workloads.BackupRandomMember, Workloads.StartStopRandomMember );
			  stressTest( _config, _fileSystem, _pageCache );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressCatchupNewReadReplica() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressCatchupNewReadReplica()
		 {
			  _config.workloads( Workloads.CreateNodesWithProperties, Workloads.CatchupNewReadReplica, Workloads.StartStopRandomCore );
			  stressTest( _config, _fileSystem, _pageCache );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressReplaceRandomMember() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressReplaceRandomMember()
		 {
			  _config.workloads( Workloads.CreateNodesWithProperties, Workloads.ReplaceRandomMember );
			  stressTest( _config, _fileSystem, _pageCache );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void simulateFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SimulateFailure()
		 {
			  try
			  {
					_config.workloads( Workloads.FailingWorkload, Workloads.StartStopRandomCore );
					stressTest( _config, _fileSystem, _pageCache );
					fail( "Should throw" );
			  }
			  catch ( Exception rte )
			  {
					assertTrue( findCauseOrSuppressed( rte, e => e.Message.Equals( FailingWorkload.MESSAGE ) ).Present );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressIdReuse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressIdReuse()
		 {
			  _config.numberOfEdges( 0 );
			  _config.reelectIntervalSeconds( 20 );

			  _config.preparations( Preparations.IdReuseSetup );

			  // having two deletion workers is on purpose
			  _config.workloads( Workloads.IdReuseInsertion, Workloads.IdReuseDeletion, Workloads.IdReuseDeletion, Workloads.IdReuseReelection );
			  _config.validations( Validations.ConsistencyCheck, Validations.IdReuseUniqueFreeIds );

			  stressTest( _config, _fileSystem, _pageCache );
		 }
	}

}