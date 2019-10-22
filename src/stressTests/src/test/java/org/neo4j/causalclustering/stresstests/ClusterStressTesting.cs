using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.stresstests
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Workload = Neo4Net.helper.Workload;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Log = Neo4Net.Logging.Log;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

	public class ClusterStressTesting
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterStressTesting()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(pageCacheRule);
		 public RuleChain Rules;

		 private FileSystemAbstraction _fileSystem;
		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fileSystem = _fileSystemRule.get();
			  _pageCache = _pageCacheRule.getPageCache( _fileSystem );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBehaveCorrectlyUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBehaveCorrectlyUnderStress()
		 {
			  StressTest( new Config(), _fileSystem, _pageCache );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void stressTest(Config config, org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, org.Neo4Net.io.pagecache.PageCache pageCache) throws Exception
		 internal static void StressTest( Config config, FileSystemAbstraction fileSystem, PageCache pageCache )
		 {
			  Resources resources = new Resources( fileSystem, pageCache, config );
			  Control control = new Control( config );
			  Log log = config.LogProvider().getLog(typeof(ClusterStressTesting));

			  log.Info( config.ToString() );

			  IList<Preparation> preparations = config.Preparations().Select(preparation => preparation.create(resources)).ToList();

			  IList<Workload> workloads = config.Workloads().Select(workload => workload.create(control, resources, config)).ToList();

			  IList<Validation> validations = config.Validations().Select(validator => validator.create(resources)).ToList();

			  if ( workloads.Count == 0 )
			  {
					throw new System.ArgumentException( "No workloads." );
			  }

			  ExecutorService executor = Executors.newCachedThreadPool();

			  try
			  {
					log.Info( "Starting resources" );
					resources.Start();

					log.Info( "Preparing scenario" );
					foreach ( Preparation preparation in preparations )
					{
						 preparation.Prepare();
					}

					log.Info( "Preparing workloads" );
					foreach ( Workload workload in workloads )
					{
						 workload.Prepare();
					}

					log.Info( "Starting workloads" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> completions = new java.util.ArrayList<>();
					IList<Future<object>> completions = new List<Future<object>>();
					foreach ( Workload workload in workloads )
					{
						 completions.Add( executor.submit( workload ) );
					}

					control.AwaitEnd( completions );

					foreach ( Workload workload in workloads )
					{
						 workload.Validate();
					}
			  }
			  catch ( Exception cause )
			  {
					control.OnFailure( cause );
			  }

			  log.Info( "Shutting down executor" );
			  executor.shutdownNow();
			  executor.awaitTermination( 5, TimeUnit.MINUTES );

			  log.Info( "Stopping resources" );
			  resources.Stop();

			  control.AssertNoFailure();

			  log.Info( "Validating results" );
			  foreach ( Validation validation in validations )
			  {
					validation.Validate();
			  }

			  // let us only cleanup resources when everything went well, and otherwise leave them for post-mortem
			  log.Info( "Cleaning up" );
			  resources.Cleanup();
		 }
	}

}