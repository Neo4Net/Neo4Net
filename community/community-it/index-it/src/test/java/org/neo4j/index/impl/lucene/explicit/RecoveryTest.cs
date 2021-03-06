﻿/*
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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	/// <summary>
	/// Don't extend Neo4jTestCase since these tests restarts the db in the tests.
	/// </summary>
	public class RecoveryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRecovery()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Node otherNode = Db.createNode();
					Relationship rel = node.CreateRelationshipTo( otherNode, withName( "recovery" ) );
					Db.index().forNodes("node-index").add(node, "key1", "string value");
					Db.index().forNodes("node-index").add(node, "key2", 12345);
					Db.index().forRelationships("rel-index").add(rel, "key1", "string value");
					Db.index().forRelationships("rel-index").add(rel, "key2", 12345);
					tx.Success();
			  }

			  ForceRecover();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptValuesWithNullToString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptValuesWithNullToString()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Node otherNode = Db.createNode();
					Relationship rel = node.CreateRelationshipTo( otherNode, RelationshipType.withName( "recovery" ) );
					Index<Node> nodeIndex = Db.index().forNodes("node-index");
					RelationshipIndex relationshipIndex = Db.index().forRelationships("rel-index");

					// Add
					AssertAddFailsWithIllegalArgument( nodeIndex, node, "key1", new ClassWithToStringAlwaysNull() );
					AssertAddFailsWithIllegalArgument( relationshipIndex, rel, "key1", new ClassWithToStringAlwaysNull() );

					// Remove
					AssertRemoveFailsWithIllegalArgument( nodeIndex, node, "key1", new ClassWithToStringAlwaysNull() );
					AssertRemoveFailsWithIllegalArgument( relationshipIndex, rel, "key1", new ClassWithToStringAlwaysNull() );
					tx.Success();
			  }

			  ForceRecover();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAsLittleAsPossibleRecoveryScenario() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestAsLittleAsPossibleRecoveryScenario()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes("my-index").add(Db.createNode(), "key", "value");
					tx.Success();
			  }

			  ForceRecover();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexDeleteIssue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexDeleteIssue()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes("index");
					tx.Success();
			  }
			  ShutdownDB();

			  Db.ensureStarted();
			  Index<Node> index;
			  Index<Node> index2;
			  using ( Transaction tx = Db.beginTx() )
			  {
					index = Db.index().forNodes("index");
					index2 = Db.index().forNodes("index2");
					Node node = Db.createNode();
					index.Add( node, "key", "value" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					index.Delete();
					index2.Add( Db.createNode(), "key", "value" );
					tx.Success();
			  }
			  Db.shutdown();

			  Db.ensureStarted();
			  ForceRecover();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoveryForRelationshipCommandsOnly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoveryForRelationshipCommandsOnly()
		 {
			  // shutdown db here
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  ShutdownDB();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Relationship> index = Db.index().forRelationships("myIndex");
					Node node = Db.createNode();
					Relationship relationship = Db.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));

					index.Add( relationship, "key", "value" );
					tx.Success();
			  }

			  Db.shutdown();

			  Config config = Config.defaults();
			  IndexConfigStore indexStore = new IndexConfigStore( databaseLayout, FileSystemRule.get() );
			  LuceneDataSource ds = new LuceneDataSource( databaseLayout, config, indexStore, FileSystemRule.get(), OperationalMode.single );
			  ds.Start();
			  ds.Stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoveryOnDeletedIndex()
		 public virtual void RecoveryOnDeletedIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes("index");
					tx.Success();
			  }

			  // shutdown db here
			  ShutdownDB();

			  Index<Node> index;
			  Index<Node> index2;
			  using ( Transaction tx = Db.beginTx() )
			  {
					index = Db.index().forNodes("index");
					index2 = Db.index().forNodes("index2");
					Node node = Db.createNode();
					index.Add( node, "key", "value" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					index.Delete();
					index2.Add( Db.createNode(), "key", "value" );
					tx.Success();
			  }

			  Db.shutdownAndKeepStore();

			  Db.ensureStarted();

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertFalse( Db.index().existsForNodes("index") );
					assertNotNull( Db.index().forNodes("index2").get("key", "value").Single );
			  }
		 }

		 private void ShutdownDB()
		 {
			  Db.shutdownAndKeepStore();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forceRecover() throws java.io.IOException
		 private void ForceRecover()
		 {
			  Db.restartDatabase();
		 }

		 internal class ClassWithToStringAlwaysNull
		 {
			  public override string ToString()
			  {
					return null;
			  }

		 }

		 private void AssertAddFailsWithIllegalArgument<ENTITY>( Index<ENTITY> index, ENTITY entity, string key, object value ) where ENTITY : Org.Neo4j.Graphdb.PropertyContainer
		 {
			  try
			  {
					index.Add( entity, key, value );
					fail( "Should not accept value with null toString" );
			  }
			  catch ( System.ArgumentException )
			  {
					// Good
			  }
		 }

		 private void AssertRemoveFailsWithIllegalArgument<ENTITY>( Index<ENTITY> index, ENTITY entity, string key, object value ) where ENTITY : Org.Neo4j.Graphdb.PropertyContainer
		 {
			  try
			  {
					index.Remove( entity, key, value );
					fail( "Should not accept value with null toString" );
			  }
			  catch ( System.ArgumentException )
			  {
					// Good
			  }
		 }
	}

}