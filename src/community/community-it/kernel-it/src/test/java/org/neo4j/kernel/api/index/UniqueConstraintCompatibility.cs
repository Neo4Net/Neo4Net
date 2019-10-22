using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.Api.Index
{
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Assert = org.junit.Assert;
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Lock = Neo4Net.Kernel.impl.locking.Lock;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.Kernel.impl.locking.LockService_LockType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public class UniqueConstraintCompatibility extends IndexProviderCompatibilityTestSuite.Compatibility
	public class UniqueConstraintCompatibility : IndexProviderCompatibilityTestSuite.Compatibility
	{
		 public UniqueConstraintCompatibility( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.uniqueForLabel( 1, 2 ) )
		 {
		 }

		 /*
		  * There are a quite a number of permutations to consider, when it comes to unique
		  * constraints.
		  *
		  * We have two supported providers:
		  *  - InMemoryIndexProvider
		  *  - LuceneIndexProvider
		  *
		  * An index can be in a number of states, two of which are interesting:
		  *  - ONLINE: the index is in active duty
		  *  - POPULATING: the index is in the process of being created and filled with data
		  *
		  * Further more, indexes that are POPULATING have two ways of ingesting data:
		  *  - Through add()'ing existing data
		  *  - Through NodePropertyUpdates sent to a "populating updater"
		  *
		  * Then, when we add data to an index, two outcomes are possible, depending on the
		  * data:
		  *  - The index does not contain an equivalent value, and the IEntity id is added to
		  *    the index.
		  *  - The index already contains an equivalent value, and the addition is rejected.
		  *
		  * And when it comes to observing these outcomes, there are a whole bunch of
		  * interesting transaction states that are worth exploring:
		  *  - Adding a label to a node
		  *  - Removing a label from a node
		  *  - Combinations of adding and removing a label, ultimately adding it
		  *  - Combinations of adding and removing a label, ultimately removing it
		  *  - Adding a property
		  *  - Removing a property
		  *  - Changing an existing property
		  *  - Combinations of adding and removing a property, ultimately adding it
		  *  - Combinations of adding and removing a property, ultimately removing it
		  *  - Likewise combinations of adding, removing and changing a property
		  *
		  * To make matters worse, we index a number of different types, some of which may or
		  * may not collide in the index because of coercion. We need to make sure that the
		  * indexes deal with these values correctly. And we also have the ways in which these
		  * operations can be performed in any number of transactions, for instance, if all
		  * the conflicting nodes were added in the same transaction or not.
		  *
		  * All in all, we have many cases to test for!
		  *
		  * Still, it is possible to boil things down a little bit, because there are fewer
		  * outcomes than there are scenarios that lead to those outcomes. With a bit of
		  * luck, we can abstract over the scenarios that lead to those outcomes, and then
		  * only write a test per outcome. These are the outcomes I see:
		  *  - Populating an index succeeds
		  *  - Populating an index fails because of the existing data
		  *  - Populating an index fails because of updates to data
		  *  - Adding to an online index succeeds
		  *  - Adding to an online index fails because of existing data
		  *  - Adding to an online index fails because of data in the same transaction
		  *
		  * There's a lot of work to be done here.
		  */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  TestGraphDatabaseFactory dbFactory = new TestGraphDatabaseFactory();
			  dbFactory.KernelExtensions = Collections.singletonList( new PredefinedIndexProviderFactory( IndexProvider ) );
			  _db = dbFactory.NewImpermanentDatabaseBuilder( GraphDbDir ).setConfig( default_schema_provider, IndexProvider.ProviderDescriptor.name() ).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _db.shutdown();
		 }

		 // -- Tests:

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldAcceptDistinctValuesInDifferentTransactions()
		 public virtual void OnlineConstraintShouldAcceptDistinctValuesInDifferentTransactions()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Node n;
			  using ( Transaction tx = _db.beginTx() )
			  {
					n = _db.createNode( _label );
					n.SetProperty( _property, "n" );
					tx.Success();
			  }

			  // Then
			  Transaction( AssertLookupNode( "a", @is( _a ) ), AssertLookupNode( "n", @is( n ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldAcceptDistinctValuesInSameTransaction()
		 public virtual void OnlineConstraintShouldAcceptDistinctValuesInSameTransaction()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Node n;
			  Node m;
			  using ( Transaction tx = _db.beginTx() )
			  {
					n = _db.createNode( _label );
					n.SetProperty( _property, "n" );

					m = _db.createNode( _label );
					m.SetProperty( _property, "m" );
					tx.Success();
			  }

			  // Then
			  Transaction( AssertLookupNode( "n", @is( n ) ), AssertLookupNode( "m", @is( m ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldNotFalselyCollideOnFindNodesByLabelAndProperty()
		 public virtual void OnlineConstraintShouldNotFalselyCollideOnFindNodesByLabelAndProperty()
		 {
			  // Given
			  GivenOnlineConstraint();
			  Node n;
			  Node m;
			  using ( Transaction tx = _db.beginTx() )
			  {
					n = _db.createNode( _label );
					n.SetProperty( _property, COLLISION_X );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					m = _db.createNode( _label );
					m.SetProperty( _property, COLLISION_Y );
					tx.Success();
			  }

			  // Then
			  Transaction( AssertLookupNode( COLLISION_X, @is( n ) ), AssertLookupNode( COLLISION_Y, @is( m ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldNotConflictOnIntermediateStatesInSameTransaction()
		 public virtual void OnlineConstraintShouldNotConflictOnIntermediateStatesInSameTransaction()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Transaction( SetProperty( _a, "b" ), SetProperty( _b, "a" ), success );

			  // Then
			  Transaction( AssertLookupNode( "a", @is( _b ) ), AssertLookupNode( "b", @is( _a ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectChangingEntryToAlreadyIndexedValue()
		 public virtual void OnlineConstraintShouldRejectChangingEntryToAlreadyIndexedValue()
		 {
			  // Given
			  GivenOnlineConstraint();
			  Transaction( SetProperty( _b, "b" ), success );

			  // When
			  Transaction( SetProperty( _b, "a" ), success, Fail( "Changing a property to an already indexed value should have thrown" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectConflictsInTheSameTransaction()
		 public virtual void OnlineConstraintShouldRejectConflictsInTheSameTransaction()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // Then
			  Transaction( SetProperty( _a, "x" ), SetProperty( _b, "x" ), success, Fail( "Should have rejected changes of two node/properties to the same index value" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldRejectChangingEntryToAlreadyIndexedValueThatOtherTransactionsAreRemoving()
		 public virtual void OnlineConstraintShouldRejectChangingEntryToAlreadyIndexedValueThatOtherTransactionsAreRemoving()
		 {
			  // Given
			  GivenOnlineConstraint();
			  Transaction( SetProperty( _b, "b" ), success );

			  Transaction otherTx = _db.beginTx();
			  _a.removeLabel( _label );
			  Suspend( otherTx );

			  // When
			  try
			  {
					Transaction( SetProperty( _b, "a" ), success, Fail( "Changing a property to an already indexed value should have thrown" ) );
			  }
			  catch ( ConstraintViolationException )
			  {
					// we're happy
			  }
			  finally
			  {
					Resume( otherTx );
					otherTx.Failure();
					otherTx.Close();
			  }
		 }

		 // Replaces UniqueIAC: shouldRemoveAndAddEntries
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldAddAndRemoveFromIndexAsPropertiesAndLabelsChange()
		 public virtual void OnlineConstraintShouldAddAndRemoveFromIndexAsPropertiesAndLabelsChange()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Transaction( SetProperty( _b, "b" ), success );
			  Transaction( SetProperty( _c, "c" ), AddLabel( _c, _label ), success );
			  Transaction( SetProperty( _d, "d" ), AddLabel( _d, _label ), success );
			  Transaction( RemoveProperty( _a ), success );
			  Transaction( RemoveProperty( _b ), success );
			  Transaction( RemoveProperty( _c ), success );
			  Transaction( SetProperty( _a, "a" ), success );
			  Transaction( SetProperty( _c, "c2" ), success );

			  // Then
			  Transaction( AssertLookupNode( "a", @is( _a ) ), AssertLookupNode( "b", @is( nullValue( typeof( Node ) ) ) ), AssertLookupNode( "c", @is( nullValue( typeof( Node ) ) ) ), AssertLookupNode( "d", @is( _d ) ), AssertLookupNode( "c2", @is( _c ) ) );
		 }

		 // Replaces UniqueIAC: shouldRejectEntryWithAlreadyIndexedValue
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectConflictingPropertyChange()
		 public virtual void OnlineConstraintShouldRejectConflictingPropertyChange()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // Then
			  Transaction( SetProperty( _b, "a" ), success, Fail( "Setting b.name = \"a\" should have caused a conflict" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectConflictingLabelChange()
		 public virtual void OnlineConstraintShouldRejectConflictingLabelChange()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // Then
			  Transaction( AddLabel( _c, _label ), success, Fail( "Setting c:Cybermen should have caused a conflict" ) );
		 }

		 // Replaces UniqueIAC: shouldRejectAddingEntryToValueAlreadyIndexedByPriorChange
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectAddingEntryForValueAlreadyIndexedByPriorChange()
		 public virtual void OnlineConstraintShouldRejectAddingEntryForValueAlreadyIndexedByPriorChange()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Transaction( SetProperty( _a, "a1" ), success ); // This is a CHANGE update

			  // Then
			  Transaction( SetProperty( _b, "a1" ), success, Fail( "Setting b.name = \"a1\" should have caused a conflict" ) );
		 }

		 // Replaces UniqueIAC: shouldAddUniqueEntries
		 // Replaces UniqueIPC: should*EnforceUniqueConstraintsAgainstDataAddedOnline
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldAcceptUniqueEntries()
		 public virtual void OnlineConstraintShouldAcceptUniqueEntries()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Transaction( SetProperty( _b, "b" ), AddLabel( _d, _label ), success );
			  Transaction( SetProperty( _c, "c" ), AddLabel( _c, _label ), success );

			  // Then
			  Transaction( AssertLookupNode( "a", @is( _a ) ), AssertLookupNode( "b", @is( _b ) ), AssertLookupNode( "c", @is( _c ) ), AssertLookupNode( "d", @is( _d ) ) );
		 }

		 // Replaces UniqueIAC: shouldUpdateUniqueEntries
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlineConstraintShouldAcceptUniqueEntryChanges()
		 public virtual void OnlineConstraintShouldAcceptUniqueEntryChanges()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // When
			  Transaction( SetProperty( _a, "a1" ), success ); // This is a CHANGE update

			  // Then
			  Transaction( AssertLookupNode( "a1", @is( _a ) ) );
		 }

		 // Replaces UniqueIAC: shouldRejectEntriesInSameTransactionWithDuplicateIndexedValue\
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void onlineConstraintShouldRejectDuplicateEntriesAddedInSameTransaction()
		 public virtual void OnlineConstraintShouldRejectDuplicateEntriesAddedInSameTransaction()
		 {
			  // Given
			  GivenOnlineConstraint();

			  // Then
			  Transaction( SetProperty( _b, "d" ), AddLabel( _d, _label ), success, Fail( "Setting b.name = \"d\" and d:Cybermen should have caused a conflict" ) );
		 }

		 // Replaces UniqueIPC: should*EnforceUniqueConstraints
		 // Replaces UniqueIPC: should*EnforceUniqueConstraintsAgainstDataAddedThroughPopulator
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustAcceptDatasetOfUniqueEntries()
		 public virtual void PopulatingConstraintMustAcceptDatasetOfUniqueEntries()
		 {
			  // Given
			  GivenUniqueDataset();

			  // Then this does not throw:
			  CreateUniqueConstraint();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.ConstraintViolationException.class) public void populatingConstraintMustRejectDatasetWithDuplicateEntries()
		 public virtual void PopulatingConstraintMustRejectDatasetWithDuplicateEntries()
		 {
			  // Given
			  GivenUniqueDataset();
			  Transaction( SetProperty( _c, "b" ), success );

			  // Then this must throw:
			  CreateUniqueConstraint();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustAcceptDatasetWithDalseIndexCollisions()
		 public virtual void PopulatingConstraintMustAcceptDatasetWithDalseIndexCollisions()
		 {
			  // Given
			  GivenUniqueDataset();
			  Transaction( SetProperty( _b, COLLISION_X ), SetProperty( _c, COLLISION_Y ), success );

			  // Then this does not throw:
			  CreateUniqueConstraint();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustAcceptDatasetThatGetsUpdatedWithUniqueEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustAcceptDatasetThatGetsUpdatedWithUniqueEntries()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, "d1"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, "d1" ) );

			  // Then observe that our constraint was created successfully:
			  createConstraintTransaction.get();
			  // Future.get() will throw an ExecutionException, if the Runnable threw an exception.
		 }

		 // Replaces UniqueLucIAT: shouldRejectEntryWithAlreadyIndexedValue
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicateAddition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicateAddition()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), createNode("b"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, CreateNode( "b" ) );

			  // Then observe that our constraint creation failed:
			  try
			  {
					createConstraintTransaction.get();
					Assert.fail( "expected to throw when PopulatingUpdater got duplicates" );
			  }
			  catch ( ExecutionException ee )
			  {
					Exception cause = ee.InnerException;
					assertThat( cause, instanceOf( typeof( ConstraintViolationException ) ) );
			  }
		 }

		 // Replaces UniqueLucIAT: shouldRejectChangingEntryToAlreadyIndexedValue
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicates()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, "b"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, "b" ) );

			  // Then observe that our constraint creation failed:
			  try
			  {
					createConstraintTransaction.get();
					Assert.fail( "expected to throw when PopulatingUpdater got duplicates" );
			  }
			  catch ( ExecutionException ee )
			  {
					Exception cause = ee.InnerException;
					assertThat( cause, instanceOf( typeof( ConstraintViolationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustAcceptDatasetThatGestUpdatedWithFalseIndexCollisions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustAcceptDatasetThatGestUpdatedWithFalseIndexCollisions()
		 {
			  // Given
			  GivenUniqueDataset();
			  Transaction( SetProperty( _a, COLLISION_X ), success );

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, COLLISION_Y));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, COLLISION_Y ) );

			  // Then observe that our constraint was created successfully:
			  createConstraintTransaction.get();
			  // Future.get() will throw an ExecutionException, if the Runnable threw an exception.
		 }

		 // Replaces UniqueLucIAT: shouldRejectEntriesInSameTransactionWithDuplicatedIndexedValues
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicatesInSameTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustRejectDatasetThatGetsUpdatedWithDuplicatesInSameTransaction()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, "x"), setProperty(c, "x"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, "x" ), SetProperty( _c, "x" ) );

			  // Then observe that our constraint creation failed:
			  try
			  {
					createConstraintTransaction.get();
					Assert.fail( "expected to throw when PopulatingUpdater got duplicates" );
			  }
			  catch ( ExecutionException ee )
			  {
					Exception cause = ee.InnerException;
					assertThat( cause, instanceOf( typeof( ConstraintViolationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingConstraintMustAcceptDatasetThatGetsUpdatedWithDuplicatesThatAreLaterResolved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingConstraintMustAcceptDatasetThatGetsUpdatedWithDuplicatesThatAreLaterResolved()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, "b"), setProperty(b, "c"), setProperty(c, "d"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, "b" ), SetProperty( _b, "c" ), SetProperty( _c, "d" ) );

			  // Then observe that our constraint was created successfully:
			  createConstraintTransaction.get();
			  // Future.get() will throw an ExecutionException, if the Runnable threw an exception.
		 }

		 // Replaces UniqueLucIAT: shouldRejectAddingEntryToValueAlreadyIndexedByPriorChange
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatingUpdaterMustRejectDatasetWhereAdditionsConflictsWithPriorChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatingUpdaterMustRejectDatasetWhereAdditionsConflictsWithPriorChanges()
		 {
			  // Given
			  GivenUniqueDataset();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = applyChangesToPopulatingUpdater(d.getId(), a.getId(), setProperty(d, "x"), createNode("x"));
			  Future<object> createConstraintTransaction = ApplyChangesToPopulatingUpdater( _d.Id, _a.Id, SetProperty( _d, "x" ), CreateNode( "x" ) );

			  // Then observe that our constraint creation failed:
			  try
			  {
					createConstraintTransaction.get();
					Assert.fail( "expected to throw when PopulatingUpdater got duplicates" );
			  }
			  catch ( ExecutionException ee )
			  {
					Exception cause = ee.InnerException;
					assertThat( cause, instanceOf( typeof( ConstraintViolationException ) ) );
			  }
		 }

		 /// <summary>
		 /// NOTE the tests using this will currently succeed for the wrong reasons,
		 /// because the data-changing transaction does not actually release the
		 /// schema read lock early enough for the PopulatingUpdater to come into
		 /// play.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Future<?> applyChangesToPopulatingUpdater(long blockDataChangeTransactionOnLockOnId, long blockPopulatorOnLockOnId, final Action... actions) throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Future<object> ApplyChangesToPopulatingUpdater( long blockDataChangeTransactionOnLockOnId, long blockPopulatorOnLockOnId, params Action[] actions )
		 {
			  // We want to issue an update to an index populator for a constraint.
			  // However, creating a constraint takes a schema write lock, while
			  // creating nodes and setting their properties takes a schema read
			  // lock. We need to sneak past these locks.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch createNodeReadyLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent createNodeReadyLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch createNodeCommitLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent createNodeCommitLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> updatingTransaction = executor.submit(() ->
			  Future<object> updatingTransaction = _executor.submit(() =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 foreach ( Action action in actions )
					 {
						  action.accept( tx );
					 }
					 tx.success();
					 createNodeReadyLatch.Signal();
					 AwaitUninterruptibly( createNodeCommitLatch );
				}
			  });
			  createNodeReadyLatch.await();

			  // The above transaction now contain the changes we want to expose to
			  // the IndexUpdater as updates. This will happen when we commit the
			  // transaction. The transaction now also holds the schema read lock,
			  // so we can't begin creating our constraint just yet.
			  // We first have to unlock the schema, and then block just before we
			  // send off our updates. We can do that by making another thread take a
			  // read lock on the node we just created, and then initiate our commit.
			  Lock lockBlockingDataChangeTransaction = LockService.acquireNodeLock( blockDataChangeTransactionOnLockOnId, LockType.WRITE_LOCK );

			  // Before we begin creating the constraint, we take a write lock on an
			  // "earlier" node, to hold up the populator for the constraint index.
			  Lock lockBlockingIndexPopulator = LockService.acquireNodeLock( blockPopulatorOnLockOnId, LockType.WRITE_LOCK );

			  // This thread tries to create a constraint. It should block, waiting for it's
			  // population job to finish, and it's population job should in turn be blocked
			  // on the lockBlockingIndexPopulator above:
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch createConstraintTransactionStarted = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent createConstraintTransactionStarted = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> createConstraintTransaction = executor.submit(() -> createUniqueConstraint(createConstraintTransactionStarted));
			  Future<object> createConstraintTransaction = _executor.submit( () => createUniqueConstraint(createConstraintTransactionStarted) );
			  createConstraintTransactionStarted.await();

			  // Now we can initiate the data-changing commit. It should then
			  // release the schema read lock, and block on the
			  // lockBlockingDataChangeTransaction.
			  createNodeCommitLatch.Signal();

			  // Now we can issue updates to the populator in the still ongoing population job.
			  // We do that by releasing the lock that is currently preventing our
			  // data-changing transaction from committing.
			  lockBlockingDataChangeTransaction.Release();

			  // And we observe that our updating transaction has completed as well:
			  updatingTransaction.get();

			  // Now we can release the lock blocking the populator, allowing it to finish:
			  lockBlockingIndexPopulator.Release();

			  // And return the future for examination:
			  return createConstraintTransaction;
		 }

		 // -- Set Up: Data parts

		 // These two values coalesce to the same double value, and therefor collides in our current index implementation:
		 private const long COLLISION_X = 4611686018427387905L;
		 private const long COLLISION_Y = 4611686018427387907L;
		 private static readonly ExecutorService _executor = Executors.newCachedThreadPool();

		 private readonly Label _label = Label.label( "Cybermen" );
		 private readonly string _property = "name";
		 private Node _a;
		 private Node _b;
		 private Node _c;
		 private Node _d;

		 private IGraphDatabaseService _db;

		 /// <summary>
		 /// Effectively:
		 /// 
		 /// <pre><code>
		 ///     CREATE CONSTRAINT ON (n:Cybermen) assert n.name is unique
		 ///     ;
		 /// 
		 ///     CREATE (a:Cybermen {name: "a"}),
		 ///            (b:Cybermen),
		 ///            (c: {name: "a"}),
		 ///            (d: {name: "d"})
		 ///     ;
		 /// </code></pre>
		 /// </summary>
		 private void GivenOnlineConstraint()
		 {
			  CreateUniqueConstraint();
			  using ( Transaction tx = _db.beginTx() )
			  {
					_a = _db.createNode( _label );
					_a.setProperty( _property, "a" );
					_b = _db.createNode( _label );
					_c = _db.createNode();
					_c.setProperty( _property, "a" );
					_d = _db.createNode();
					_d.setProperty( _property, "d" );
					tx.Success();
			  }
		 }

		 /// <summary>
		 /// Effectively:
		 /// 
		 /// <pre><code>
		 ///     CREATE (a:Cybermen {name: "a"}),
		 ///            (b:Cybermen {name: "b"}),
		 ///            (c:Cybermen {name: "c"}),
		 ///            (d:Cybermen {name: "d"})
		 ///     ;
		 /// </code></pre>
		 /// </summary>
		 private void GivenUniqueDataset()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_a = _db.createNode( _label );
					_a.setProperty( _property, "a" );
					_b = _db.createNode( _label );
					_b.setProperty( _property, "b" );
					_c = _db.createNode( _label );
					_c.setProperty( _property, "c" );
					_d = _db.createNode( _label );
					_d.setProperty( _property, "d" );
					tx.Success();
			  }
		 }

		 /// <summary>
		 /// Effectively:
		 /// 
		 /// <pre><code>
		 ///     CREATE CONSTRAINT ON (n:Cybermen) assert n.name is unique
		 ///     ;
		 /// </code></pre>
		 /// </summary>
		 private void CreateUniqueConstraint()
		 {
			  CreateUniqueConstraint( null );
		 }

		 /// <summary>
		 /// Effectively:
		 /// 
		 /// <pre><code>
		 ///     CREATE CONSTRAINT ON (n:Cybermen) assert n.name is unique
		 ///     ;
		 /// </code></pre>
		 /// 
		 /// Also counts down the given latch prior to creating the constraint.
		 /// </summary>
		 private void CreateUniqueConstraint( System.Threading.CountdownEvent preCreateLatch )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					if ( preCreateLatch != null )
					{
						 preCreateLatch.Signal();
					}
					_db.schema().constraintFor(_label).assertPropertyIsUnique(_property).create();
					tx.Success();
			  }
		 }

		 /// <summary>
		 /// Effectively:
		 /// 
		 /// <pre><code>
		 ///     return single( db.findNodesByLabelAndProperty( label, property, value ), null );
		 /// </code></pre>
		 /// </summary>
		 private Node LookUpNode( object value )
		 {
			  return _db.findNode( _label, _property, value );
		 }

		 // -- Set Up: Transaction handling

		 public virtual void Transaction( params Action[] actions )
		 {
			  int progress = 0;
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						foreach ( Action action in actions )
						{
							 action.accept( tx );
							 progress++;
						}
					  }
			  }
			  catch ( Exception ex )
			  {
					StringBuilder sb = new StringBuilder( "Transaction failed:\n\n" );
					for ( int i = 0; i < actions.Length; i++ )
					{
						 string mark = progress == i ? " failed --> " : "            ";
						 sb.Append( mark ).Append( actions[i] ).Append( '\n' );
					}
					ex.addSuppressed( new AssertionError( sb.ToString() ) );
					throw ex;
			  }
		 }

		 private abstract class Action : System.Action<Transaction>
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			  internal readonly string Name;

			  protected internal Action( UniqueConstraintCompatibility outerInstance, string name )
			  {
				  this._outerInstance = outerInstance;
					this.Name = name;
			  }

			  public override string ToString()
			  {
					return Name;
			  }
		 }

		 private readonly Action success = new ActionAnonymousInnerClass();

		 private class ActionAnonymousInnerClass : Action
		 {
			 public ActionAnonymousInnerClass() : base("tx.success();")
			 {
			 }

			 public override void accept( Transaction transaction )
			 {
				  transaction.Success();
				  // We also call close() here, because some validations and checks don't run until commit
				  transaction.Close();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action createNode(final Object propertyValue)
		 private Action CreateNode( object propertyValue )
		 {
			  return new ActionAnonymousInnerClass2( this, propertyValue );
		 }

		 private class ActionAnonymousInnerClass2 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private object _propertyValue;

			 public ActionAnonymousInnerClass2( UniqueConstraintCompatibility outerInstance, object propertyValue ) : base( outerInstance, "Node node = db.createNode( label ); " + "node.setProperty( property, " + outerInstance.reprValue( propertyValue ) + " );" )
			 {
				 this.outerInstance = outerInstance;
				 this._propertyValue = propertyValue;
			 }

			 public override void accept( Transaction transaction )
			 {
				  Node node = _outerInstance.db.createNode( _outerInstance.label );
				  node.SetProperty( _outerInstance.property, _propertyValue );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action setProperty(final org.Neo4Net.graphdb.Node node, final Object value)
		 private Action SetProperty( Node node, object value )
		 {
			  return new ActionAnonymousInnerClass3( this, node, value );
		 }

		 private class ActionAnonymousInnerClass3 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private Node _node;
			 private object _value;

			 public ActionAnonymousInnerClass3( UniqueConstraintCompatibility outerInstance, Node node, object value ) : base( outerInstance, outerInstance.reprNode( node ) + ".setProperty( property, " + outerInstance.reprValue( value ) + " );" )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
				 this._value = value;
			 }

			 public override void accept( Transaction transaction )
			 {
				  _node.setProperty( _outerInstance.property, _value );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action removeProperty(final org.Neo4Net.graphdb.Node node)
		 private Action RemoveProperty( Node node )
		 {
			  return new ActionAnonymousInnerClass4( this, node );
		 }

		 private class ActionAnonymousInnerClass4 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private Node _node;

			 public ActionAnonymousInnerClass4( UniqueConstraintCompatibility outerInstance, Node node ) : base( outerInstance, outerInstance.reprNode( node ) + ".removeProperty( property );" )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
			 }

			 public override void accept( Transaction transaction )
			 {
				  _node.removeProperty( _outerInstance.property );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action addLabel(final org.Neo4Net.graphdb.Node node, final org.Neo4Net.graphdb.Label label)
		 private Action AddLabel( Node node, Label label )
		 {
			  return new ActionAnonymousInnerClass5( this, node, label );
		 }

		 private class ActionAnonymousInnerClass5 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private Node _node;
			 private Label _label;

			 public ActionAnonymousInnerClass5( UniqueConstraintCompatibility outerInstance, Node node, Label label ) : base( outerInstance, outerInstance.reprNode( node ) + ".addLabel( " + label + " );" )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
				 this._label = label;
			 }

			 public override void accept( Transaction transaction )
			 {
				  _node.addLabel( _label );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action fail(final String message)
		 private Action Fail( string message )
		 {
			  return new ActionAnonymousInnerClass6( this, message );
		 }

		 private class ActionAnonymousInnerClass6 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private string _message;

			 public ActionAnonymousInnerClass6( UniqueConstraintCompatibility outerInstance, string message ) : base( outerInstance, "fail( \"" + message + "\" );" )
			 {
				 this.outerInstance = outerInstance;
				 this._message = message;
			 }

			 public override void accept( Transaction transaction )
			 {
				  Assert.fail( _message );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Action assertLookupNode(final Object propertyValue, final org.hamcrest.Matcher<org.Neo4Net.graphdb.Node> matcher)
		 private Action AssertLookupNode( object propertyValue, Matcher<Node> matcher )
		 {
			  return new ActionAnonymousInnerClass7( this, propertyValue, matcher );
		 }

		 private class ActionAnonymousInnerClass7 : Action
		 {
			 private readonly UniqueConstraintCompatibility _outerInstance;

			 private object _propertyValue;
			 private Matcher<Node> _matcher;

			 public ActionAnonymousInnerClass7( UniqueConstraintCompatibility outerInstance, object propertyValue, Matcher<Node> matcher ) : base( outerInstance, "assertThat( lookUpNode( " + outerInstance.reprValue( propertyValue ) + " ), " + matcher + " );" )
			 {
				 this.outerInstance = outerInstance;
				 this._propertyValue = propertyValue;
				 this._matcher = matcher;
			 }

			 public override void accept( Transaction transaction )
			 {
				  assertThat( outerInstance.lookUpNode( _propertyValue ), _matcher );
			 }
		 }

		 private string ReprValue( object value )
		 {
			  return value is string ? "\"" + value + "\"" : value.ToString();
		 }

		 private string ReprNode( Node node )
		 {
			  return node == _a ? "a" : node == _b ? "b" : node == _c ? "c" : node == _d ? "d" : "n";
		 }

		 // -- Set Up: Advanced transaction handling

		 private readonly IDictionary<Transaction, KernelTransaction> _txMap = new IdentityHashMap<Transaction, KernelTransaction>();

		 private void Suspend( Transaction tx )
		 {
			  ThreadToStatementContextBridge txManager = TransactionManager;
			  _txMap[tx] = txManager.GetKernelTransactionBoundToThisThread( true );
			  txManager.UnbindTransactionFromCurrentThread();
		 }

		 private void Resume( Transaction tx )
		 {
			  ThreadToStatementContextBridge txManager = TransactionManager;
			  txManager.BindTransactionToCurrentThread( _txMap.Remove( tx ) );
		 }

		 private ThreadToStatementContextBridge TransactionManager
		 {
			 get
			 {
				  return ResolveInternalDependency( typeof( ThreadToStatementContextBridge ) );
			 }
		 }

		 // -- Set Up: Misc. sharp tools

		 /// <summary>
		 /// Locks controlling concurrent access to the store files.
		 /// </summary>
		 private LockService LockService
		 {
			 get
			 {
				  return ResolveInternalDependency( typeof( LockService ) );
			 }
		 }

		 private T ResolveInternalDependency<T>( Type type )
		 {
				 type = typeof( T );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") org.Neo4Net.kernel.internal.GraphDatabaseAPI api = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) db;
			  GraphDatabaseAPI api = ( GraphDatabaseAPI ) _db;
			  DependencyResolver resolver = api.DependencyResolver;
			  return resolver.ResolveDependency( type );
		 }

		 private static void AwaitUninterruptibly( System.Threading.CountdownEvent latch )
		 {
			  try
			  {
					latch.await();
			  }
			  catch ( InterruptedException e )
			  {
					throw new AssertionError( "Interrupted", e );
			  }
		 }

		 private class PredefinedIndexProviderFactory : KernelExtensionFactory<PredefinedIndexProviderFactory.NoDeps>
		 {
			  internal readonly IndexProvider IndexProvider;

			  public override Lifecycle NewInstance( KernelContext context, NoDeps noDeps )
			  {
					return IndexProvider;
			  }

			  internal interface NoDeps
			  {
			  }

			  internal PredefinedIndexProviderFactory( IndexProvider indexProvider ) : base( ExtensionType.DATABASE, indexProvider.GetType().Name )
			  {
					this.IndexProvider = indexProvider;
			  }
		 }
	}

}