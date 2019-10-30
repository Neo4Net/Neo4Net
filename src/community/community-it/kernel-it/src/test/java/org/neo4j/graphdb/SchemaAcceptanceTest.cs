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
namespace Neo4Net.GraphDb
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.createIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.findNodesByLabelAndProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getConstraints;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getIndexes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.isEmpty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.waitForIndex;

	public class SchemaAcceptanceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.ImpermanentDatabaseRule dbRule = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

		 private IGraphDatabaseService _db;
		 private Label _label = Labels.MyLabel;
		 private string _propertyKey = "my_property_key";
		 private string _secondPropertyKey = "my_second_property_key";

		 private enum Labels
		 {
			  MyLabel,
			  MyOtherLabel
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _db = DbRule.GraphDatabaseAPI;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingAnIndexingRuleShouldSucceed()
		 public virtual void AddingAnIndexingRuleShouldSucceed()
		 {
			  // WHEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );

			  // THEN
			  assertThat( getIndexes( _db, _label ), containsOnly( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingACompositeIndexingRuleShouldSucceed()
		 public virtual void AddingACompositeIndexingRuleShouldSucceed()
		 {
			  // WHEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey, _secondPropertyKey );

			  // THEN
			  assertThat( getIndexes( _db, _label ), containsOnly( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingAnIndexingRuleInNestedTxShouldSucceed()
		 public virtual void AddingAnIndexingRuleInNestedTxShouldSucceed()
		 {
			  IndexDefinition index;

			  // WHEN
			  IndexDefinition indexDef;
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( Transaction nestedTransaction = _db.beginTx() )
					{
						 indexDef = _db.schema().indexFor(_label).on(_propertyKey).create();
						 nestedTransaction.Success();
					}

					index = indexDef;
					tx.Success();
			  }
			  waitForIndex( _db, indexDef );

			  // THEN
			  assertThat( getIndexes( _db, _label ), containsOnly( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowConstraintViolationIfAskedToIndexSamePropertyAndLabelTwiceInSameTx()
		 public virtual void ShouldThrowConstraintViolationIfAskedToIndexSamePropertyAndLabelTwiceInSameTx()
		 {
			  // WHEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					Schema schema = _db.schema();
					Schema.indexFor( _label ).on( _propertyKey ).create();
					try
					{
						 Schema.indexFor( _label ).on( _propertyKey ).create();
						 fail( "Should not have validated" );
					}
					catch ( ConstraintViolationException e )
					{
						 assertEquals( "There already exists an index :MY_LABEL(my_property_key).", e.Message );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowConstraintViolationIfAskedToIndexPropertyThatIsAlreadyIndexed()
		 public virtual void ShouldThrowConstraintViolationIfAskedToIndexPropertyThatIsAlreadyIndexed()
		 {
			  // GIVEN
			  Schema schema;
			  using ( Transaction tx = _db.beginTx() )
			  {
					schema = _db.schema();
					Schema.indexFor( _label ).on( _propertyKey ).create();
					tx.Success();
			  }

			  // WHEN
			  ConstraintViolationException caught = null;
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						Schema.indexFor( _label ).on( _propertyKey ).create();
						tx.Success();
					  }
			  }
			  catch ( ConstraintViolationException e )
			  {
					caught = e;
			  }

			  // THEN
			  assertThat( caught, not( nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowConstraintViolationIfAskedToCreateCompoundConstraint()
		 public virtual void ShouldThrowConstraintViolationIfAskedToCreateCompoundConstraint()
		 {
			  // WHEN
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						Schema schema = _db.schema();
						Schema.constraintFor( _label ).assertPropertyIsUnique( "my_property_key" ).assertPropertyIsUnique( "other_property" ).create();
						tx.Success();
						fail( "Should not be able to create constraint on multiple propertyKey keys" );
					  }
			  }
			  catch ( System.NotSupportedException e )
			  {
					assertThat( e.Message, containsString( "can only create one unique constraint" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void droppingExistingIndexRuleShouldSucceed()
		 public virtual void DroppingExistingIndexRuleShouldSucceed()
		 {
			  // GIVEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );

			  // WHEN
			  DropIndex( index );

			  // THEN
			  assertThat( getIndexes( _db, _label ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void droppingAnUnexistingIndexShouldGiveHelpfulExceptionInSameTransaction()
		 public virtual void DroppingAnUnexistingIndexShouldGiveHelpfulExceptionInSameTransaction()
		 {
			  // GIVEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );

			  // WHEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					index.Drop();
					try
					{
						 index.Drop();
						 fail( "Should not be able to drop index twice" );
					}
					catch ( ConstraintViolationException e )
					{
						 assertThat( e.Message, containsString( "No such INDEX ON :MY_LABEL(my_property_key)." ) );
					}
					tx.Success();
			  }

			  // THEN
			  assertThat( "Index should have been deleted", getIndexes( _db, _label ), not( contains( index ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void droppingAnUnexistingIndexShouldGiveHelpfulExceptionInSeparateTransactions()
		 public virtual void DroppingAnUnexistingIndexShouldGiveHelpfulExceptionInSeparateTransactions()
		 {
			  // GIVEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );
			  DropIndex( index );

			  // WHEN
			  try
			  {
					DropIndex( index );
					fail( "Should not be able to drop index twice" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertThat( e.Message, containsString( "No such INDEX ON :MY_LABEL(my_property_key)." ) );
			  }

			  // THEN
			  assertThat( "Index should have been deleted", getIndexes( _db, _label ), not( contains( index ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void awaitingIndexComingOnlineWorks()
		 public virtual void AwaitingIndexComingOnlineWorks()
		 {
			  // GIVEN

			  // WHEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );

			  // PASS
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexOnline(index, 1L, TimeUnit.MINUTES);

					// THEN
					assertEquals( Neo4Net.GraphDb.Schema.Schema_IndexState.Online, _db.schema().getIndexState(index) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void awaitingAllIndexesComingOnlineWorks()
		 public virtual void AwaitingAllIndexesComingOnlineWorks()
		 {
			  // GIVEN

			  // WHEN
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );
			  createIndex( _db, _label, "other_property" );

			  // PASS
			  waitForIndex( _db, index );
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1L, TimeUnit.MINUTES);

					// THEN
					assertEquals( Neo4Net.GraphDb.Schema.Schema_IndexState.Online, _db.schema().getIndexState(index) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndex()
		 public virtual void ShouldPopulateIndex()
		 {
			  // GIVEN
			  Node node = CreateNode( _db, _propertyKey, "Neo", _label );

			  // create an index
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );
			  waitForIndex( _db, index );

			  // THEN
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, "Neo", _db ), containsOnly( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecreateDroppedIndex()
		 public virtual void ShouldRecreateDroppedIndex()
		 {
			  // GIVEN
			  Node node = CreateNode( _db, _propertyKey, "Neo", _label );

			  // create an index
			  IndexDefinition index = createIndex( _db, _label, _propertyKey );
			  waitForIndex( _db, index );

			  // delete the index right away
			  DropIndex( index );

			  // WHEN recreating that index
			  createIndex( _db, _label, _propertyKey );
			  waitForIndex( _db, index );

			  // THEN it should exist and be usable
			  assertThat( getIndexes( _db, _label ), contains( index ) );
			  assertThat( findNodesByLabelAndProperty( _label, _propertyKey, "Neo", _db ), containsOnly( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquenessConstraint()
		 public virtual void ShouldCreateUniquenessConstraint()
		 {
			  // WHEN
			  ConstraintDefinition constraint = CreateUniquenessConstraint( _label, _propertyKey );

			  // THEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( ConstraintType.UNIQUENESS, constraint.ConstraintType );

					assertEquals( _label.name(), constraint.Label.name() );
					assertEquals( asSet( _propertyKey ), Iterables.asSet( constraint.PropertyKeys ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedConstraintsByLabel()
		 public virtual void ShouldListAddedConstraintsByLabel()
		 {
			  // GIVEN
			  ConstraintDefinition constraint1 = CreateUniquenessConstraint( _label, _propertyKey );
			  CreateUniquenessConstraint( Labels.MyOtherLabel, _propertyKey );

			  // WHEN THEN
			  assertThat( getConstraints( _db, _label ), containsOnly( constraint1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedConstraints()
		 public virtual void ShouldListAddedConstraints()
		 {
			  // GIVEN
			  ConstraintDefinition constraint1 = CreateUniquenessConstraint( Labels.MyLabel, _propertyKey );
			  ConstraintDefinition constraint2 = CreateUniquenessConstraint( Labels.MyOtherLabel, _propertyKey );

			  // WHEN THEN
			  assertThat( getConstraints( _db ), containsOnly( constraint1, constraint2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquenessConstraint()
		 public virtual void ShouldDropUniquenessConstraint()
		 {
			  // GIVEN
			  ConstraintDefinition constraint = CreateUniquenessConstraint( _label, _propertyKey );

			  // WHEN
			  DropConstraint( _db, constraint );

			  // THEN
			  assertThat( getConstraints( _db, _label ), Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingConstraintWhenIndexAlreadyExistsGivesNiceError()
		 public virtual void AddingConstraintWhenIndexAlreadyExistsGivesNiceError()
		 {
			  // GIVEN
			  createIndex( _db, _label, _propertyKey );

			  // WHEN
			  try
			  {
					CreateUniquenessConstraint( _label, _propertyKey );
					fail( "Expected exception to be thrown" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertEquals( "There already exists an index :MY_LABEL(my_property_key). A constraint cannot be created " + "until the index has been dropped.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingUniquenessConstraintWhenDuplicateDataExistsGivesNiceError()
		 public virtual void AddingUniquenessConstraintWhenDuplicateDataExistsGivesNiceError()
		 {
			  // GIVEN
			  using ( Transaction transaction = _db.beginTx() )
			  {
					_db.createNode( _label ).setProperty( _propertyKey, "value1" );
					_db.createNode( _label ).setProperty( _propertyKey, "value1" );
					transaction.Success();
			  }

			  // WHEN
			  try
			  {
					CreateUniquenessConstraint( _label, _propertyKey );
					fail( "Expected exception to be thrown" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertThat( e.Message, containsString( "Unable to create CONSTRAINT ON ( my_label:MY_LABEL ) ASSERT my_label.my_property_key IS UNIQUE" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingConstraintWhenAlreadyConstrainedGivesNiceError()
		 public virtual void AddingConstraintWhenAlreadyConstrainedGivesNiceError()
		 {
			  // GIVEN
			  CreateUniquenessConstraint( _label, _propertyKey );

			  // WHEN
			  try
			  {
					CreateUniquenessConstraint( _label, _propertyKey );
					fail( "Expected exception to be thrown" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertEquals( "Constraint already exists: CONSTRAINT ON ( my_label:MY_LABEL ) ASSERT my_label.my_property_key " + "IS UNIQUE", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingIndexWhenAlreadyConstrained()
		 public virtual void AddingIndexWhenAlreadyConstrained()
		 {
			  // GIVEN
			  CreateUniquenessConstraint( _label, _propertyKey );

			  // WHEN
			  try
			  {
					createIndex( _db, _label, _propertyKey );
					fail( "Expected exception to be thrown" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertEquals( "There is a uniqueness constraint on :MY_LABEL(my_property_key), so an index is already " + "created that matches this.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingIndexWhenAlreadyIndexed()
		 public virtual void AddingIndexWhenAlreadyIndexed()
		 {
			  // GIVEN
			  createIndex( _db, _label, _propertyKey );

			  // WHEN
			  try
			  {
					createIndex( _db, _label, _propertyKey );
					fail( "Expected exception to be thrown" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertEquals( "There already exists an index :MY_LABEL(my_property_key).", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addedUncommittedIndexesShouldBeVisibleWithinTheTransaction()
		 public virtual void AddedUncommittedIndexesShouldBeVisibleWithinTheTransaction()
		 {
			  // GIVEN
			  IndexDefinition indexA = createIndex( _db, _label, "a" );
			  CreateUniquenessConstraint( _label, "b" );

			  // WHEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertThat( count( _db.schema().getIndexes(_label) ), @is(2L) );
					IndexDefinition indexC = _db.schema().indexFor(_label).on("c").create();
					// THEN
					assertThat( count( _db.schema().getIndexes(_label) ), @is(3L) );
					assertThat( _db.schema().getIndexState(indexA), @is(Neo4Net.GraphDb.Schema.Schema_IndexState.Online) );
					assertThat( _db.schema().getIndexState(indexC), @is(Neo4Net.GraphDb.Schema.Schema_IndexState.Populating) );
			  }
		 }

		 private void DropConstraint( IGraphDatabaseService db, ConstraintDefinition constraint )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					constraint.Drop();
					tx.Success();
			  }
		 }

		 private ConstraintDefinition CreateUniquenessConstraint( Label label, string prop )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					ConstraintDefinition constraint = _db.schema().constraintFor(label).assertPropertyIsUnique(prop).create();
					tx.Success();
					return constraint;
			  }
		 }

		 private void DropIndex( IndexDefinition index )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					index.Drop();
					tx.Success();
			  }
		 }

		 private Node CreateNode( IGraphDatabaseService db, string key, object value, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label );
					node.SetProperty( key, value );
					tx.Success();
					return node;
			  }
		 }
	}

}