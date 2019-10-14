using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Index.recovery
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using SchemaIndex = Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex;
	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UniqueIndexRecoveryTest
	public class UniqueIndexRecoveryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory storeDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory StoreDir = TestDirectory.testDirectory();

		 private const string PROPERTY_KEY = "key";
		 private const string PROPERTY_VALUE = "value";
		 private static readonly Label _label = label( "label" );

		 private readonly TestGraphDatabaseFactory _factory = new TestGraphDatabaseFactory();
		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex[] parameters()
		 public static GraphDatabaseSettings.SchemaIndex[] Parameters()
		 {
			  return GraphDatabaseSettings.SchemaIndex.values();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex;
		 public GraphDatabaseSettings.SchemaIndex SchemaIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _db = ( GraphDatabaseAPI ) NewDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverCreationOfUniquenessConstraintFollowedByDeletionOfThatSameConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverCreationOfUniquenessConstraintFollowedByDeletionOfThatSameConstraint()
		 {
			  // given
			  CreateUniqueConstraint();
			  DropConstraints();

			  // when - perform recovery
			  Restart( Snapshot( StoreDir.absolutePath() ) );

			  // then - just make sure the constraint is gone
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertFalse( _db.schema().getConstraints(_label).GetEnumerator().hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverWhenCommandsTemporarilyViolateConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverWhenCommandsTemporarilyViolateConstraints()
		 {
			  // GIVEN
			  Node unLabeledNode = CreateUnLabeledNodeWithProperty();
			  Node labeledNode = CreateLabeledNode();
			  CreateUniqueConstraint();
			  RotateLogAndCheckPoint(); // snapshot
			  PropertyOnLabeledNode = labeledNode;
			  DeletePropertyOnLabeledNode( labeledNode );
			  AddLabelToUnLabeledNode( unLabeledNode );
			  FlushAll(); // persist - recovery will do everything since last log rotate

			  // WHEN recovery is triggered
			  Restart( Snapshot( StoreDir.absolutePath() ) );

			  // THEN
			  // it should just not blow up!
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertThat( _db.findNode( _label, PROPERTY_KEY, PROPERTY_VALUE ), equalTo( unLabeledNode ) );
					tx.Success();
			  }
		 }

		 private void Restart( File newStore )
		 {
			  _db.shutdown();
			  _db = ( GraphDatabaseAPI ) NewDb();
		 }

		 private GraphDatabaseService NewDb()
		 {
			  return _factory.newEmbeddedDatabaseBuilder( StoreDir.absolutePath() ).setConfig(GraphDatabaseSettings.default_schema_provider, SchemaIndex.providerName()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File snapshot(final java.io.File path) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static File Snapshot( File path )
		 {
			  File snapshotDir = new File( path, "snapshot-" + ( new Random() ).Next() );
			  FileUtils.copyRecursively(path, snapshotDir, pathName =>
			  {
				string subPath = pathName.AbsolutePath.substring( path.Path.length() + 1 );
				// since the db is running, exclude the lock files
				return !"store_lock".Equals( subPath ) && !subPath.EndsWith( "write.lock", StringComparison.Ordinal );
			  });
			  return snapshotDir;
		 }

		 private void AddLabelToUnLabeledNode( Node unLabeledNode )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					unLabeledNode.AddLabel( _label );
					tx.Success();
			  }
		 }

		 private Node PropertyOnLabeledNode
		 {
			 set
			 {
				  using ( Transaction tx = _db.beginTx() )
				  {
						value.SetProperty( PROPERTY_KEY, PROPERTY_VALUE );
						tx.Success();
				  }
			 }
		 }

		 private void DeletePropertyOnLabeledNode( Node labeledNode )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					labeledNode.RemoveProperty( PROPERTY_KEY );
					tx.Success();
			  }
		 }

		 private void CreateUniqueConstraint()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
					tx.Success();
			  }
		 }

		 private Node CreateLabeledNode()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					tx.Success();
					return node;
			  }
		 }

		 private Node CreateUnLabeledNodeWithProperty()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					node.SetProperty( PROPERTY_KEY, PROPERTY_VALUE );
					tx.Success();
					return node;
			  }
		 }

		 private void DropConstraints()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					foreach ( ConstraintDefinition constraint in _db.schema().getConstraints(_label) )
					{
						 constraint.Drop();
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rotateLogAndCheckPoint() throws java.io.IOException
		 private void RotateLogAndCheckPoint()
		 {
			  _db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );
		 }

		 private void FlushAll()
		 {
			  _db.DependencyResolver.resolveDependency( typeof( StorageEngine ) ).flushAndForce( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }
	}

}