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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using ReservedIdException = Neo4Net.Kernel.impl.store.id.validation.ReservedIdException;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserters = Neo4Net.@unsafe.Batchinsert.BatchInserters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class BatchInsertionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EmbeddedDatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexNodesWithMultipleLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexNodesWithMultipleLabels()
		 {
			  // Given
			  File path = DbRule.databaseLayout().databaseDirectory();
			  BatchInserter inserter = BatchInserters.inserter( path, FileSystemRule.get() );

			  inserter.createNode( map( "name", "Bob" ), label( "User" ), label( "Admin" ) );

			  inserter.CreateDeferredSchemaIndex( label( "User" ) ).on( "name" ).create();
			  inserter.CreateDeferredSchemaIndex( label( "Admin" ) ).on( "name" ).create();

			  // When
			  inserter.Shutdown();

			  // Then
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						assertThat( count( Db.findNodes( label( "User" ), "name", "Bob" ) ), equalTo( 1L ) );
						assertThat( count( Db.findNodes( label( "Admin" ), "name", "Bob" ) ), equalTo( 1L ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIndexNodesWithWrongLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIndexNodesWithWrongLabel()
		 {
			  // Given
			  File file = new File( DbRule.DatabaseDirAbsolutePath );
			  BatchInserter inserter = BatchInserters.inserter( file, FileSystemRule.get() );

			  inserter.createNode( map( "name", "Bob" ), label( "User" ), label( "Admin" ) );

			  inserter.CreateDeferredSchemaIndex( label( "Banana" ) ).on( "name" ).create();

			  // When
			  inserter.Shutdown();

			  // Then
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						assertThat( count( Db.findNodes( label( "Banana" ), "name", "Bob" ) ), equalTo( 0L ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToMakeRepeatedCallsToSetNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToMakeRepeatedCallsToSetNodeProperty()
		 {
			  File file = DbRule.databaseLayout().databaseDirectory();
			  BatchInserter inserter = BatchInserters.inserter( file, FileSystemRule.get() );
			  long nodeId = inserter.createNode( Collections.emptyMap() );

			  const object finalValue = 87;
			  inserter.SetNodeProperty( nodeId, "a", "some property value" );
			  inserter.SetNodeProperty( nodeId, "a", 42 );
			  inserter.SetNodeProperty( nodeId, "a", 3.14 );
			  inserter.SetNodeProperty( nodeId, "a", true );
			  inserter.SetNodeProperty( nodeId, "a", finalValue );
			  inserter.Shutdown();

			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						assertThat( Db.getNodeById( nodeId ).getProperty( "a" ), equalTo( finalValue ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToMakeRepeatedCallsToSetNodePropertyWithMultiplePropertiesPerBlock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToMakeRepeatedCallsToSetNodePropertyWithMultiplePropertiesPerBlock()
		 {
			  File file = DbRule.databaseLayout().databaseDirectory();
			  BatchInserter inserter = BatchInserters.inserter( file, FileSystemRule.get() );
			  long nodeId = inserter.createNode( Collections.emptyMap() );

			  const object finalValue1 = 87;
			  const object finalValue2 = 3.14;
			  inserter.SetNodeProperty( nodeId, "a", "some property value" );
			  inserter.SetNodeProperty( nodeId, "a", 42 );
			  inserter.SetNodeProperty( nodeId, "b", finalValue2 );
			  inserter.SetNodeProperty( nodeId, "a", finalValue2 );
			  inserter.SetNodeProperty( nodeId, "a", true );
			  inserter.SetNodeProperty( nodeId, "a", finalValue1 );
			  inserter.Shutdown();

			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						assertThat( Db.getNodeById( nodeId ).getProperty( "a" ), equalTo( finalValue1 ) );
						assertThat( Db.getNodeById( nodeId ).getProperty( "b" ), equalTo( finalValue2 ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.kernel.impl.store.id.validation.ReservedIdException.class) public void makeSureCantCreateNodeWithMagicNumber() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureCantCreateNodeWithMagicNumber()
		 {
			  // given
			  File path = DbRule.databaseLayout().databaseDirectory();
			  BatchInserter inserter = BatchInserters.inserter( path, FileSystemRule.get() );

			  try
			  {
					// when
					long id = IdGeneratorImpl.INTEGER_MINUS_ONE;
					inserter.CreateNode( id, null );

					// then throws
			  }
			  finally
			  {
					inserter.Shutdown();
			  }
		 }
	}

}