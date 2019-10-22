using System.Threading;

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
namespace Neo4Net.metrics.output
{
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using NullLog = Neo4Net.Logging.NullLog;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public class CsvOutputTest
	{
		private bool InstanceFieldsInitialized = false;

		public CsvOutputTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _life );
		}

		 private readonly LifeRule _life = new LifeRule();
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly IJobScheduler _jobScheduler = new OnDemandJobScheduler();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(life);
		 public RuleChain RuleChain;

		 private KernelContext _kernelContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  File storeDir = _directory.directory();
			  _kernelContext = new SimpleKernelContext( storeDir, DatabaseInfo.UNKNOWN, new Dependencies() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveRelativeMetricsCsvPathBeRelativeToNeo4NetHome() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveRelativeMetricsCsvPathBeRelativeToNeo4NetHome()
		 {
			  // GIVEN
			  File home = _directory.absolutePath();
			  Config config = config( MetricsSettings.csvEnabled.name(), "true", MetricsSettings.csvInterval.name(), "10ms", MetricsSettings.csvPath.name(), "the-metrics-dir", GraphDatabaseSettings.Neo4Net_home.name(), home.AbsolutePath );
			  _life.add( CreateCsvOutput( config ) );

			  // WHEN
			  _life.start();

			  // THEN
			  WaitForFileToAppear( new File( home, "the-metrics-dir" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAbsoluteMetricsCsvPathBeAbsolute() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveAbsoluteMetricsCsvPathBeAbsolute()
		 {
			  // GIVEN
			  File outputFPath = Files.createTempDirectory( "output" ).toFile();
			  Config config = config( MetricsSettings.csvEnabled.name(), "true", MetricsSettings.csvInterval.name(), "10ms", MetricsSettings.csvPath.name(), outputFPath.AbsolutePath );
			  _life.add( CreateCsvOutput( config ) );

			  // WHEN
			  _life.start();

			  // THEN
			  WaitForFileToAppear( outputFPath );
		 }

		 private CsvOutput CreateCsvOutput( Config config )
		 {
			  return new CsvOutput( config, new MetricRegistry(), NullLog.Instance, _kernelContext, _fileSystemRule, _jobScheduler );
		 }

		 private Config Config( params string[] keysValues )
		 {
			  return Config.defaults( stringMap( keysValues ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForFileToAppear(java.io.File file) throws InterruptedException
		 private void WaitForFileToAppear( File file )
		 {
			  long end = currentTimeMillis() + SECONDS.toMillis(10);
			  while ( !file.exists() )
			  {
					Thread.Sleep( 10 );
					if ( currentTimeMillis() > end )
					{
						 fail( file + " didn't appear" );
					}
			  }
		 }
	}

}