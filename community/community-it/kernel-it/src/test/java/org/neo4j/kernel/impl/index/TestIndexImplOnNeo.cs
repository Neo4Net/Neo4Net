using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.index
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.index.IndexManager_Fields.PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class TestIndexImplOnNeo
	internal class TestIndexImplOnNeo
	{
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
			  _db = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fs)).newImpermanentDatabase(_testDirectory.databaseDir());
		 }

		 private void RestartDb()
		 {
			  ShutdownDb();
			  CreateDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void shutdownDb()
		 internal virtual void ShutdownDb()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createIndexWithProviderThatUsesNeoAsDataSource()
		 internal virtual void CreateIndexWithProviderThatUsesNeoAsDataSource()
		 {
			  string indexName = "inneo";
			  assertFalse( IndexExists( indexName ) );
			  IDictionary<string, string> config = stringMap( PROVIDER, "test-dummy-neo-index", "config1", "A value", "another config", "Another value" );

			  Index<Node> index;
			  using ( Transaction transaction = _db.beginTx() )
			  {
					index = _db.index().forNodes(indexName, config);
					transaction.Success();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					assertTrue( IndexExists( indexName ) );
					assertEquals( config, _db.index().getConfiguration(index) );
					using ( IndexHits<Node> indexHits = index.get( "key", "something else" ) )
					{
						 assertEquals( 0, Iterables.count( indexHits ) );
					}
					tx.Success();
			  }

			  RestartDb();

			  using ( Transaction tx = _db.beginTx() )
			  {
					assertTrue( IndexExists( indexName ) );
					assertEquals( config, _db.index().getConfiguration(index) );
					tx.Success();
			  }
		 }

		 private bool IndexExists( string indexName )
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					bool exists = _db.index().existsForNodes(indexName);
					transaction.Success();
					return exists;
			  }
		 }
	}

}