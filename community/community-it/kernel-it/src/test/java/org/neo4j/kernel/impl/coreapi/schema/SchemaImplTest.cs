using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexPopulationProgress = Org.Neo4j.Graphdb.index.IndexPopulationProgress;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) public class SchemaImplTest
	public class SchemaImplTest
	{
		 private static readonly Label _userLabel = Label.label( "User" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void createDb()
		 internal virtual void CreateDb()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs).newImpermanentDatabase(_testDirectory.databaseDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void shutdownDb()
		 internal virtual void ShutdownDb()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetIndexPopulationProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestGetIndexPopulationProgress()
		 {
			  assertFalse( IndexExists( _userLabel ) );

			  // Create some nodes
			  using ( Transaction tx = _db.beginTx() )
			  {
					Label label = Label.label( "User" );

					// Create a huge bunch of users so the index takes a while to build
					for ( int id = 0; id < 100000; id++ )
					{
						 Node userNode = _db.createNode( label );
						 userNode.SetProperty( "username", "user" + id + "@neo4j.org" );
					}
					tx.Success();
			  }

			  // Create an index
			  IndexDefinition indexDefinition;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Schema schema = _db.schema();
					indexDefinition = Schema.indexFor( _userLabel ).on( "username" ).create();
					tx.Success();
			  }

			  // Get state and progress
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Schema schema = _db.schema();
					Org.Neo4j.Graphdb.schema.Schema_IndexState state;

					IndexPopulationProgress progress;
					do
					{
						 state = Schema.getIndexState( indexDefinition );
						 progress = Schema.getIndexPopulationProgress( indexDefinition );

						 assertTrue( progress.CompletedPercentage >= 0 );
						 assertTrue( progress.CompletedPercentage <= 100 );
						 Thread.Sleep( 10 );
					} while ( state == Org.Neo4j.Graphdb.schema.Schema_IndexState.Populating );

					assertSame( state, Org.Neo4j.Graphdb.schema.Schema_IndexState.Online );
					assertEquals( 100.0, progress.CompletedPercentage, 0.0001 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createdIndexDefinitionsMustBeUnnamed()
		 internal virtual void CreatedIndexDefinitionsMustBeUnnamed()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().indexFor(_userLabel).on("name").create();
					assertThat( index.Name, @is( Org.Neo4j.@internal.Kernel.Api.IndexReference_Fields.UNNAMED_INDEX ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRememberNamesOfCreatedIndex()
		 internal virtual void MustRememberNamesOfCreatedIndex()
		 {
			  string indexName = "Users index";
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().indexFor(_userLabel).on("name").withName(indexName).create();
					assertThat( index.Name, @is( indexName ) );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().getIndexByName(indexName);
					assertThat( index.Name, @is( indexName ) );
					tx.Success();
			  }
		 }

		 private bool IndexExists( Label label )
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					IEnumerable<IndexDefinition> indexes = _db.schema().getIndexes(label);
					IndexDefinition index = Iterables.firstOrNull( indexes );
					bool exists = index != null;
					transaction.Success();
					return exists;
			  }
		 }
	}

}