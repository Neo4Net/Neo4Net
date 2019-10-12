using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.database
{
	using FileUtils = org.apache.commons.io.FileUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerTestUtils.createTempDir;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class TestLifecycleManagedDatabase
	{
		private bool InstanceFieldsInitialized = false;

		public TestLifecycleManagedDatabase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			DbRule = new ImpermanentDatabaseRule( _logProvider );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule(logProvider);
		 public ImpermanentDatabaseRule DbRule;

		 private File _dataDirectory;
		 private Database _theDatabase;
		 private bool _deletionFailureOk;
		 private GraphFactory _dbFactory;
		 private Config _dbConfig;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _dataDirectory = createTempDir();

			  _dbFactory = new SimpleGraphFactory( ( GraphDatabaseFacade ) DbRule.GraphDatabaseAPI );
			  _dbConfig = Config.defaults( GraphDatabaseSettings.data_directory, _dataDirectory.AbsolutePath );
			  _theDatabase = NewDatabase();
		 }

		 private LifecycleManagingDatabase NewDatabase()
		 {
			  return new LifecycleManagingDatabase( _dbConfig, _dbFactory, GraphDatabaseDependencies.newDependencies().userLogProvider(_logProvider) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdownDatabase() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShutdownDatabase()
		 {
			  this._theDatabase.stop();

			  try
			  {
					FileUtils.forceDelete( _dataDirectory );
			  }
			  catch ( IOException e )
			  {
					// TODO Removed this when EmbeddedGraphDatabase startup failures
					// closes its
					// files properly.
					if ( !_deletionFailureOk )
					{
						 throw e;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogOnSuccessfulStartup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogOnSuccessfulStartup()
		 {
			  _theDatabase.start();

			  _logProvider.assertAtLeastOnce( inLog( typeof( LifecycleManagingDatabase ) ).info( "Started." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShutdownCleanly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShutdownCleanly()
		 {
			  _theDatabase.start();
			  _theDatabase.stop();

			  _logProvider.assertAtLeastOnce( inLog( typeof( LifecycleManagingDatabase ) ).info( "Stopped." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainIfDatabaseLocationIsAlreadyInUse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComplainIfDatabaseLocationIsAlreadyInUse()
		 {
			  _deletionFailureOk = true;
			  _theDatabase.start();

			  LifecycleManagingDatabase db = NewDatabase();

			  try
			  {
					Db.start();
			  }
			  catch ( Exception e )
			  {
					// Wrapped in a lifecycle exception, needs to be dug out
					assertThat( e.InnerException.InnerException, instanceOf( typeof( StoreLockException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetLocation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetLocation()
		 {
			  _theDatabase.start();
			  assertThat( _theDatabase.Location.AbsolutePath, @is( _dbConfig.get( GraphDatabaseSettings.database_path ).AbsolutePath ) );
		 }
	}

}