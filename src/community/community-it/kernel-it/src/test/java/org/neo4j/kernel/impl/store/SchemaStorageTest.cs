using System;
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
namespace Neo4Net.Kernel.impl.store
{
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using Mockito = org.mockito.Mockito;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexCreator = Neo4Net.GraphDb.Schema.IndexCreator;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using SchemaDescriptorPredicates = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorPredicates;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using DuplicateSchemaRuleException = Neo4Net.Kernel.Api.Exceptions.schema.DuplicateSchemaRuleException;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using IGraphDatabaseServiceCleaner = Neo4Net.Test.GraphDatabaseServiceCleaner;
	using Neo4Net.Test.mockito.matcher;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexProvider.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.uniqueForSchema;

	public class SchemaStorageTest
	{
		 private const string LABEL1 = "Label1";
		 private const string LABEL2 = "Label2";
		 private const string TYPE1 = "Type1";
		 private const string PROP1 = "prop1";
		 private const string PROP2 = "prop2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public static readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

		 private static SchemaStorage _storage;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initStorage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void InitStorage()
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					TokenWrite tokenWrite = Transaction.tokenWrite();
					tokenWrite.PropertyKeyGetOrCreateForName( PROP1 );
					tokenWrite.PropertyKeyGetOrCreateForName( PROP2 );
					tokenWrite.LabelGetOrCreateForName( LABEL1 );
					tokenWrite.LabelGetOrCreateForName( LABEL2 );
					tokenWrite.RelationshipTypeGetOrCreateForName( TYPE1 );
					transaction.Success();
			  }
			  SchemaStore schemaStore = ResolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().SchemaStore;
			  _storage = new SchemaStorage( schemaStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void clearSchema()
		 public virtual void ClearSchema()
		 {
			  IGraphDatabaseServiceCleaner.cleanupSchema( Db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIndexRuleForLabelAndProperty()
		 public virtual void ShouldReturnIndexRuleForLabelAndProperty()
		 {
			  // Given
			  CreateSchema( Index( LABEL1, PROP1 ), Index( LABEL1, PROP2 ), Index( LABEL2, PROP1 ) );

			  // When
			  StoreIndexDescriptor rule = _storage.indexGetForSchema( IndexDescriptor( LABEL1, PROP2 ) );

			  // Then
			  assertNotNull( rule );
			  AssertRule( rule, LABEL1, PROP2, IndexDescriptor.Type.GENERAL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIndexRuleForLabelAndPropertyComposite()
		 public virtual void ShouldReturnIndexRuleForLabelAndPropertyComposite()
		 {
			  string a = "a";
			  string b = "b";
			  string c = "c";
			  string d = "d";
			  string e = "e";
			  string f = "f";
			  CreateSchema( Db => Db.schema().indexFor(Label.label(LABEL1)).on(a).on(b).on(c).on(d).on(e).on(f).create() );

			  StoreIndexDescriptor rule = _storage.indexGetForSchema( TestIndexDescriptorFactory.forLabel( LabelId( LABEL1 ), PropId( a ), PropId( b ), PropId( c ), PropId( d ), PropId( e ), PropId( f ) ) );

			  assertNotNull( rule );
			  assertTrue( SchemaDescriptorPredicates.hasLabel( rule, LabelId( LABEL1 ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( a ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( b ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( c ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( d ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( e ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( f ) ) );
			  assertEquals( IndexDescriptor.Type.GENERAL, rule.Type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIndexRuleForLabelAndVeryManyPropertiesComposite()
		 public virtual void ShouldReturnIndexRuleForLabelAndVeryManyPropertiesComposite()
		 {
			  string[] props = "abcdefghijklmnopqrstuvwxyzABCDEFGHJILKMNOPQRSTUVWXYZ".Split( "\\B", true );
			  CreateSchema(Db =>
			  {
				IndexCreator indexCreator = Db.schema().indexFor(Label.label(LABEL1));
				foreach ( string prop in props )
				{
					 indexCreator = indexCreator.on( prop );
				}
				indexCreator.create();
			  });

			  StoreIndexDescriptor rule = _storage.indexGetForSchema( TestIndexDescriptorFactory.forLabel( LabelId( LABEL1 ), java.util.props.Select( this.propId ).ToArray() ) );

			  assertNotNull( rule );
			  assertTrue( SchemaDescriptorPredicates.hasLabel( rule, LabelId( LABEL1 ) ) );
			  foreach ( string prop in props )
			  {
					assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( prop ) ) );
			  }
			  assertEquals( IndexDescriptor.Type.GENERAL, rule.Type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullIfIndexRuleForLabelAndPropertyDoesNotExist()
		 public virtual void ShouldReturnNullIfIndexRuleForLabelAndPropertyDoesNotExist()
		 {
			  // Given
			  CreateSchema( Index( LABEL1, PROP1 ) );

			  // When
			  StoreIndexDescriptor rule = _storage.indexGetForSchema( IndexDescriptor( LABEL1, PROP2 ) );

			  // Then
			  assertNull( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListIndexRulesForLabelPropertyAndKind()
		 public virtual void ShouldListIndexRulesForLabelPropertyAndKind()
		 {
			  // Given
			  CreateSchema( UniquenessConstraint( LABEL1, PROP1 ), Index( LABEL1, PROP2 ) );

			  // When
			  StoreIndexDescriptor rule = _storage.indexGetForSchema( UniqueIndexDescriptor( LABEL1, PROP1 ) );

			  // Then
			  assertNotNull( rule );
			  AssertRule( rule, LABEL1, PROP1, IndexDescriptor.Type.UNIQUE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexRules()
		 public virtual void ShouldListAllIndexRules()
		 {
			  // Given
			  CreateSchema( Index( LABEL1, PROP1 ), Index( LABEL1, PROP2 ), UniquenessConstraint( LABEL2, PROP1 ) );

			  // When
			  ISet<StoreIndexDescriptor> listedRules = asSet( _storage.indexesGetAll() );

			  // Then
			  ISet<StoreIndexDescriptor> expectedRules = new HashSet<StoreIndexDescriptor>();
			  expectedRules.Add( MakeIndexRule( 0, LABEL1, PROP1 ) );
			  expectedRules.Add( MakeIndexRule( 1, LABEL1, PROP2 ) );
			  expectedRules.Add( MakeIndexRuleForConstraint( 2, LABEL2, PROP1, 0L ) );

			  assertEquals( expectedRules, listedRules );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectUniquenessRuleForLabelAndProperty() throws org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException, org.Neo4Net.kernel.api.exceptions.schema.DuplicateSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCorrectUniquenessRuleForLabelAndProperty()
		 {
			  // Given
			  CreateSchema( UniquenessConstraint( LABEL1, PROP1 ), UniquenessConstraint( LABEL2, PROP1 ) );

			  // When
			  ConstraintRule rule = _storage.constraintsGetSingle( ConstraintDescriptorFactory.uniqueForLabel( LabelId( LABEL1 ), PropId( PROP1 ) ) );

			  // Then
			  assertNotNull( rule );
			  AssertRule( rule, LABEL1, PROP1, Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor_Type.Unique );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnNodeRuleNotFound() throws org.Neo4Net.kernel.api.exceptions.schema.DuplicateSchemaRuleException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionOnNodeRuleNotFound()
		 {
			  // GIVEN
			  TokenNameLookup tokenNameLookup = DefaultTokenNameLookup;

			  // EXPECT
			  ExpectedException.expect( typeof( SchemaRuleNotFoundException ) );
			  ExpectedException.expect( new KernelExceptionUserMessageMatcher( tokenNameLookup, "No node property existence constraint was found for :Label1(prop1)." ) );

			  // WHEN
			  _storage.constraintsGetSingle( ConstraintDescriptorFactory.existsForLabel( LabelId( LABEL1 ), PropId( PROP1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnNodeDuplicateRuleFound() throws org.Neo4Net.kernel.api.exceptions.schema.DuplicateSchemaRuleException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionOnNodeDuplicateRuleFound()
		 {
			  // GIVEN
			  TokenNameLookup tokenNameLookup = DefaultTokenNameLookup;

			  SchemaStorage schemaStorageSpy = Mockito.spy( _storage );
			  Mockito.when( schemaStorageSpy.LoadAllSchemaRules( any(), any(), anyBoolean() ) ).thenReturn(Iterators.iterator(GetUniquePropertyConstraintRule(1L, LABEL1, PROP1), GetUniquePropertyConstraintRule(2L, LABEL1, PROP1)));

			  //EXPECT
			  ExpectedException.expect( typeof( DuplicateSchemaRuleException ) );
			  ExpectedException.expect( new KernelExceptionUserMessageMatcher( tokenNameLookup, "Multiple uniqueness constraints found for :Label1(prop1)." ) );

			  // WHEN
			  schemaStorageSpy.ConstraintsGetSingle( ConstraintDescriptorFactory.uniqueForLabel( LabelId( LABEL1 ), PropId( PROP1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnRelationshipRuleNotFound() throws org.Neo4Net.kernel.api.exceptions.schema.DuplicateSchemaRuleException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionOnRelationshipRuleNotFound()
		 {
			  TokenNameLookup tokenNameLookup = DefaultTokenNameLookup;

			  // EXPECT
			  ExpectedException.expect( typeof( SchemaRuleNotFoundException ) );
			  ExpectedException.expect( new KernelExceptionUserMessageMatcher<>( tokenNameLookup, "No relationship property existence constraint was found for -[:Type1(prop1)]-." ) );

			  //WHEN
			  _storage.constraintsGetSingle( ConstraintDescriptorFactory.existsForRelType( TypeId( TYPE1 ), PropId( PROP1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnRelationshipDuplicateRuleFound() throws org.Neo4Net.kernel.api.exceptions.schema.DuplicateSchemaRuleException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionOnRelationshipDuplicateRuleFound()
		 {
			  // GIVEN
			  TokenNameLookup tokenNameLookup = DefaultTokenNameLookup;

			  SchemaStorage schemaStorageSpy = Mockito.spy( _storage );
			  Mockito.when( schemaStorageSpy.LoadAllSchemaRules( any(), any(), anyBoolean() ) ).thenReturn(Iterators.iterator(GetRelationshipPropertyExistenceConstraintRule(1L, TYPE1, PROP1), GetRelationshipPropertyExistenceConstraintRule(2L, TYPE1, PROP1)));

			  //EXPECT
			  ExpectedException.expect( typeof( DuplicateSchemaRuleException ) );
			  ExpectedException.expect( new KernelExceptionUserMessageMatcher( tokenNameLookup, "Multiple relationship property existence constraints found for -[:Type1(prop1)]-." ) );

			  // WHEN
			  schemaStorageSpy.ConstraintsGetSingle( ConstraintDescriptorFactory.existsForRelType( TypeId( TYPE1 ), PropId( PROP1 ) ) );
		 }

		 private TokenNameLookup DefaultTokenNameLookup
		 {
			 get
			 {
				  TokenNameLookup tokenNameLookup = mock( typeof( TokenNameLookup ) );
				  Mockito.when( tokenNameLookup.LabelGetName( LabelId( LABEL1 ) ) ).thenReturn( LABEL1 );
				  Mockito.when( tokenNameLookup.PropertyKeyGetName( PropId( PROP1 ) ) ).thenReturn( PROP1 );
				  Mockito.when( tokenNameLookup.RelationshipTypeGetName( TypeId( TYPE1 ) ) ).thenReturn( TYPE1 );
				  return tokenNameLookup;
			 }
		 }

		 private void AssertRule( StoreIndexDescriptor rule, string label, string propertyKey, IndexDescriptor.Type type )
		 {
			  assertTrue( SchemaDescriptorPredicates.hasLabel( rule, LabelId( label ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( propertyKey ) ) );
			  assertEquals( type, rule.Type() );
		 }

		 private void AssertRule( ConstraintRule rule, string label, string propertyKey, Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor_Type type )
		 {
			  assertTrue( SchemaDescriptorPredicates.hasLabel( rule, LabelId( label ) ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( rule, PropId( propertyKey ) ) );
			  assertEquals( type, rule.ConstraintDescriptor.type() );
		 }

		 private IndexDescriptor IndexDescriptor( string label, string property )
		 {
			  return TestIndexDescriptorFactory.forLabel( LabelId( label ), PropId( property ) );
		 }

		 private IndexDescriptor UniqueIndexDescriptor( string label, string property )
		 {
			  return TestIndexDescriptorFactory.uniqueForLabel( LabelId( label ), PropId( property ) );
		 }

		 private StoreIndexDescriptor MakeIndexRule( long ruleId, string label, string propertyKey )
		 {
			  return forSchema( forLabel( LabelId( label ), PropId( propertyKey ) ), EMPTY.ProviderDescriptor ).withId( ruleId );
		 }

		 private StoreIndexDescriptor MakeIndexRuleForConstraint( long ruleId, string label, string propertyKey, long constraintId )
		 {
			  return uniqueForSchema( forLabel( LabelId( label ), PropId( propertyKey ) ), EMPTY.ProviderDescriptor ).withIds( ruleId, constraintId );
		 }

		 private ConstraintRule GetUniquePropertyConstraintRule( long id, string label, string property )
		 {
			  return ConstraintRule.constraintRule( id, ConstraintDescriptorFactory.uniqueForLabel( LabelId( label ), PropId( property ) ), 0L );
		 }

		 private ConstraintRule GetRelationshipPropertyExistenceConstraintRule( long id, string type, string property )
		 {
			  return ConstraintRule.constraintRule( id, ConstraintDescriptorFactory.existsForRelType( TypeId( type ), PropId( property ) ) );
		 }

		 private static int LabelId( string labelName )
		 {
			  using ( Transaction ignore = Db.beginTx() )
			  {
					return Transaction.tokenRead().nodeLabel(labelName);
			  }
		 }

		 private int PropId( string propName )
		 {
			  using ( Transaction ignore = Db.beginTx() )
			  {
					return Transaction.tokenRead().propertyKey(propName);
			  }
		 }

		 private static int TypeId( string typeName )
		 {
			  using ( Transaction ignore = Db.beginTx() )
			  {
					return Transaction.tokenRead().relationshipType(typeName);
			  }
		 }

		 private static KernelTransaction Transaction
		 {
			 get
			 {
				  return ResolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
			 }
		 }

		 private static T ResolveDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return Db.GraphDatabaseAPI.DependencyResolver.resolveDependency( clazz );
		 }

		 private static System.Action<GraphDatabaseService> Index( string label, string prop )
		 {
			  return Db => Db.schema().indexFor(Label.label(label)).on(prop).create();
		 }

		 private static System.Action<GraphDatabaseService> UniquenessConstraint( string label, string prop )
		 {
			  return Db => Db.schema().constraintFor(Label.label(label)).assertPropertyIsUnique(prop).create();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static void createSchema(System.Action<org.Neo4Net.graphdb.GraphDatabaseService>... creators)
		 private static void CreateSchema( params System.Action<GraphDatabaseService>[] creators )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( System.Action<GraphDatabaseService> rule in creators )
					{
						 rule( Db );
					}
					tx.Success();
			  }
			  AwaitIndexes();
		 }

		 private static void AwaitIndexes()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }
	}

}