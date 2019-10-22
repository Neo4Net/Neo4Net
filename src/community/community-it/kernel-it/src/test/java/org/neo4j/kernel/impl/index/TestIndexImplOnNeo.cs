using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.index
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.index.IndexManager_Fields.PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class TestIndexImplOnNeo
	internal class TestIndexImplOnNeo
	{
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