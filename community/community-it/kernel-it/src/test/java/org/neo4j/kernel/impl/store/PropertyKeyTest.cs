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
namespace Org.Neo4j.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using BatchInserter = Org.Neo4j.@unsafe.Batchinsert.BatchInserter;
	using BatchInserters = Org.Neo4j.@unsafe.Batchinsert.BatchInserters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PropertyKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lazyLoadWithinWriteTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LazyLoadWithinWriteTransaction()
		 {
			  // Given
			  FileSystemAbstraction fileSystem = Fs.get();
			  BatchInserter inserter = BatchInserters.inserter( TestDirectory.databaseDir(), fileSystem );
			  int count = 3000;
			  long nodeId = inserter.CreateNode( MapWithManyProperties( count ) );
			  inserter.Shutdown();

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fileSystem).newImpermanentDatabase(TestDirectory.databaseDir());

			  // When
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode();
						Node node = Db.getNodeById( nodeId );
      
						// Then
						assertEquals( count, Iterables.count( node.PropertyKeys ) );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static IDictionary<string, object> MapWithManyProperties( int count )
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  for ( int i = 0; i < count; i++ )
			  {
					properties["key:" + i] = "value";
			  }
			  return properties;
		 }
	}

}