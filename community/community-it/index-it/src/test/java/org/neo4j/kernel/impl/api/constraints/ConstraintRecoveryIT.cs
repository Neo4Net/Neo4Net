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
namespace Org.Neo4j.Kernel.Impl.Api.constraints
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;

	public class ConstraintRecoveryIT
	{
		 private const string KEY = "prop";
		 private static readonly Label _label = Label.label( "label1" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAvailableOrphanedConstraintIndexIfUniqueConstraintCreationFails()
		 public virtual void ShouldHaveAvailableOrphanedConstraintIndexIfUniqueConstraintCreationFails()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs = fileSystemRule.get();
			  EphemeralFileSystemAbstraction fs = FileSystemRule.get();
			  fs.Mkdir( new File( "/tmp" ) );
			  File pathToDb = new File( "/tmp/bar2" );

			  TestGraphDatabaseFactory dbFactory = new TestGraphDatabaseFactory();
			  dbFactory.FileSystem = fs;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction[] storeInNeedOfRecovery = new org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction[1];
			  EphemeralFileSystemAbstraction[] storeInNeedOfRecovery = new EphemeralFileSystemAbstraction[1];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean monitorCalled = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean monitorCalled = new AtomicBoolean( false );

			  Monitors monitors = new Monitors();
			  monitors.AddMonitorListener( new MonitorAdapterAnonymousInnerClass( this, fs, storeInNeedOfRecovery, monitorCalled ) );
			  dbFactory.Monitors = monitors;

			  // This test relies on behaviour that is specific to the Lucene populator, where uniqueness is controlled
			  // after index has been populated, which is why we're using NATIVE20 and index booleans (they end up in Lucene)
			  _db = ( GraphDatabaseAPI ) dbFactory.NewImpermanentDatabaseBuilder( pathToDb ).setConfig( default_schema_provider, NATIVE20.providerName() ).newGraphDatabase();

			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < 2; i++ )
					{
						 _db.createNode( _label ).setProperty( KEY, true );
					}

					tx.Success();
			  }

			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						_db.schema().constraintFor(_label).assertPropertyIsUnique(KEY).create();
						fail( "Should have failed with ConstraintViolationException" );
						tx.Success();
					  }
			  }
			  catch ( ConstraintViolationException )
			  {
			  }

			  _db.shutdown();

			  assertTrue( monitorCalled.get() );

			  // when
			  dbFactory = new TestGraphDatabaseFactory();
			  dbFactory.FileSystem = storeInNeedOfRecovery[0];
			  _db = ( GraphDatabaseAPI ) dbFactory.NewImpermanentDatabase( pathToDb );

			  // then
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					assertEquals( 2, Iterables.count( _db.AllNodes ) );
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					assertEquals( 0, Iterables.count( Iterables.asList( _db.schema().Constraints ) ) );
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					IndexDefinition orphanedConstraintIndex = single( _db.schema().Indexes );
					assertEquals( _label.name(), single(orphanedConstraintIndex.Labels).name() );
					assertEquals( KEY, single( orphanedConstraintIndex.PropertyKeys ) );
			  }

			  _db.shutdown();
		 }

		 private class MonitorAdapterAnonymousInnerClass : IndexingService.MonitorAdapter
		 {
			 private readonly ConstraintRecoveryIT _outerInstance;

			 private EphemeralFileSystemAbstraction _fs;
			 private EphemeralFileSystemAbstraction[] _storeInNeedOfRecovery;
			 private AtomicBoolean _monitorCalled;

			 public MonitorAdapterAnonymousInnerClass( ConstraintRecoveryIT outerInstance, EphemeralFileSystemAbstraction fs, EphemeralFileSystemAbstraction[] storeInNeedOfRecovery, AtomicBoolean monitorCalled )
			 {
				 this.outerInstance = outerInstance;
				 this._fs = fs;
				 this._storeInNeedOfRecovery = storeInNeedOfRecovery;
				 this._monitorCalled = monitorCalled;
			 }

			 public override void indexPopulationScanComplete()
			 {
				  _monitorCalled.set( true );
				  _outerInstance.db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().SchemaStore.flush();
				  _storeInNeedOfRecovery[0] = _fs.snapshot();
			 }
		 }
	}

}