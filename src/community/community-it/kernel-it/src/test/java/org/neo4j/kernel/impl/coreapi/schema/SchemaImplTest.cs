using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.coreapi.schema
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexPopulationProgress = Neo4Net.GraphDb.index.IndexPopulationProgress;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

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
//ORIGINAL LINE: @Inject private org.Neo4Net.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

		 private IGraphDatabaseService _db;

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
						 userNode.SetProperty( "username", "user" + id + "@Neo4Net.org" );
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
					Neo4Net.GraphDb.Schema.Schema_IndexState state;

					IndexPopulationProgress progress;
					do
					{
						 state = Schema.getIndexState( indexDefinition );
						 progress = Schema.getIndexPopulationProgress( indexDefinition );

						 assertTrue( progress.CompletedPercentage >= 0 );
						 assertTrue( progress.CompletedPercentage <= 100 );
						 Thread.Sleep( 10 );
					} while ( state == Neo4Net.GraphDb.Schema.Schema_IndexState.Populating );

					assertSame( state, Neo4Net.GraphDb.Schema.Schema_IndexState.Online );
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
					assertThat( index.Name, @is( Neo4Net.Kernel.Api.Internal.IndexReference_Fields.UNNAMED_INDEX ) );
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