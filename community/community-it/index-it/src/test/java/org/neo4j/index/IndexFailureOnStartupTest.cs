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
namespace Org.Neo4j.Index
{
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.schema.Schema_IndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.SabotageNativeIndex.nativeIndexDirectoryStructure;

	public class IndexFailureOnStartupTest
	{
		 private static readonly Label _person = Label.label( "Person" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule().startLazily();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedIndexShouldRepairAutomatically() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedIndexShouldRepairAutomatically()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_person).on("name").create();
					tx.Success();
			  }
			  AwaitIndexesOnline( 5, SECONDS );
			  CreateNamed( _person, "Johan" );
			  // when - we restart the database in a state where the index is not operational
			  Db.restartDatabase( new SabotageNativeIndex( Random.random() ) );
			  // then - the database should still be operational
			  CreateNamed( _person, "Lars" );
			  AwaitIndexesOnline( 5, SECONDS );
			  IndexStateShouldBe( equalTo( ONLINE ) );
			  AssertFindsNamed( _person, "Lars" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToViolateConstraintWhenBackingIndexFailsToOpen() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToViolateConstraintWhenBackingIndexFailsToOpen()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_person).assertPropertyIsUnique("name").create();
					tx.Success();
			  }
			  CreateNamed( _person, "Lars" );
			  // when - we restart the database in a state where the index is not operational
			  Db.restartDatabase( new SabotageNativeIndex( Random.random() ) );
			  // then - we must not be able to violate the constraint
			  CreateNamed( _person, "Johan" );
			  Exception failure = null;
			  try
			  {
					CreateNamed( _person, "Lars" );
			  }
			  catch ( Exception e )
			  {
					// this must fail, otherwise we have violated the constraint
					failure = e;
			  }
			  assertNotNull( failure );
			  IndexStateShouldBe( equalTo( ONLINE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldArchiveFailedIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldArchiveFailedIndex()
		 {
			  // given
			  Db.withSetting( GraphDatabaseSettings.archive_failed_index, "true" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( _person );
					node.SetProperty( "name", "Fry" );
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( _person );
					node.SetProperty( "name", Values.pointValue( CoordinateReferenceSystem.WGS84, 1, 2 ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_person).assertPropertyIsUnique("name").create();
					tx.Success();
			  }
			  assertThat( ArchiveFile(), nullValue() );

			  // when
			  Db.restartDatabase( new SabotageNativeIndex( Random.random() ) );

			  // then
			  IndexStateShouldBe( equalTo( ONLINE ) );
			  assertThat( ArchiveFile(), notNullValue() );
		 }

		 private File ArchiveFile()
		 {
			  File indexDir = nativeIndexDirectoryStructure( Db.databaseLayout() ).rootDirectory();
			  File[] files = indexDir.listFiles( pathname => pathname.File && pathname.Name.StartsWith( "archive-" ) );
			  if ( files == null || Files.Length == 0 )
			  {
					return null;
			  }
			  assertEquals( 1, Files.Length );
			  return files[0];
		 }

		 private void AwaitIndexesOnline( int timeout, TimeUnit unit )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(timeout, unit);
					tx.Success();
			  }
		 }

		 private void AssertFindsNamed( Label label, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertNotNull( "Must be able to find node created while index was offline", Db.findNode( label, "name", name ) );
					tx.Success();
			  }
		 }

		 private void IndexStateShouldBe( Matcher<Org.Neo4j.Graphdb.schema.Schema_IndexState> matchesExpectation )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( IndexDefinition index in Db.schema().Indexes )
					{
						 assertThat( Db.schema().getIndexState(index), matchesExpectation );
					}
					tx.Success();
			  }
		 }

		 private void CreateNamed( Label label, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label ).setProperty( "name", name );
					tx.Success();
			  }
		 }
	}

}