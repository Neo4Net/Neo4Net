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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using SchemaRuleSerialization = Org.Neo4j.Kernel.impl.store.record.SchemaRuleSerialization;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static ByteBuffer.wrap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class SchemaStoreTest
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaStoreTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			RuleChain = RuleChain.outerRule( _fs ).around( _testDirectory );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();
		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory);
		 public RuleChain RuleChain;

		 private Config _config;
		 private SchemaStore _store;
		 private NeoStores _neoStores;
		 private StoreFactory _storeFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _config = Config.defaults();
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( _fs.get() );
			  _storeFactory = new StoreFactory( _testDirectory.databaseLayout(), _config, idGeneratorFactory, PageCacheRule.getPageCache(_fs.get()), _fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = _storeFactory.openAllNeoStores( true );
			  _store = _neoStores.SchemaStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoadSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadSchemaRule()
		 {
			  // GIVEN
			  StoreIndexDescriptor indexRule = forSchema( forLabel( 1, 4 ), PROVIDER_DESCRIPTOR ).withId( _store.nextId() );

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoadCompositeSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadCompositeSchemaRule()
		 {
			  // GIVEN
			  int[] propertyIds = new int[] { 4, 5, 6, 7 };
			  StoreIndexDescriptor indexRule = forSchema( forLabel( 2, propertyIds ), PROVIDER_DESCRIPTOR ).withId( _store.nextId() );

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoadMultiTokenSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadMultiTokenSchemaRule()
		 {
			  // GIVEN
			  int[] propertyIds = new int[] { 4, 5, 6, 7 };
			  int[] entityTokens = new int[] { 2, 3, 4 };
			  StoreIndexDescriptor indexRule = forSchema( multiToken( entityTokens, EntityType.RELATIONSHIP, propertyIds ), PROVIDER_DESCRIPTOR ).withId( _store.nextId() );

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoadAnyTokenMultiTokenSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadAnyTokenMultiTokenSchemaRule()
		 {
			  // GIVEN
			  int[] propertyIds = new int[] { 4, 5, 6, 7 };
			  int[] entityTokens = new int[] {};
			  StoreIndexDescriptor indexRule = forSchema( multiToken( entityTokens, EntityType.NODE, propertyIds ), PROVIDER_DESCRIPTOR ).withId( _store.nextId() );

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoad_Big_CompositeSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadBigCompositeSchemaRule()
		 {
			  // GIVEN
			  StoreIndexDescriptor indexRule = forSchema( forLabel( 2, IntStream.range( 1, 200 ).toArray() ), PROVIDER_DESCRIPTOR ).withId(_store.nextId());

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoad_Big_CompositeMultiTokenSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreAndLoadBigCompositeMultiTokenSchemaRule()
		 {
			  // GIVEN
			  StoreIndexDescriptor indexRule = forSchema( multiToken( IntStream.range( 1, 200 ).toArray(), EntityType.RELATIONSHIP, IntStream.range(1, 200).toArray() ), PROVIDER_DESCRIPTOR ).withId(_store.nextId());

			  // WHEN
			  StoreIndexDescriptor readIndexRule = ( StoreIndexDescriptor ) SchemaRuleSerialization.deserialize( indexRule.Id, wrap( SchemaRuleSerialization.serialize( indexRule ) ) );

			  // THEN
			  assertEquals( indexRule.Id, readIndexRule.Id );
			  assertEquals( indexRule.Schema(), readIndexRule.Schema() );
			  assertEquals( indexRule, readIndexRule );
			  assertEquals( indexRule.ProviderDescriptor(), readIndexRule.ProviderDescriptor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeAndLoadAllRules()
		 public virtual void StoreAndLoadAllRules()
		 {
			  // GIVEN
			  long indexId = _store.nextId();
			  long constraintId = _store.nextId();
			  ICollection<SchemaRule> rules = Arrays.asList( UniqueIndexRule( indexId, constraintId, PROVIDER_DESCRIPTOR, 2, 5, 3 ), ConstraintUniqueRule( constraintId, indexId, 2, 5, 3 ), IndexRule( _store.nextId(), PROVIDER_DESCRIPTOR, 0, 5 ), IndexRule(_store.nextId(), PROVIDER_DESCRIPTOR, 1, 6, 10, 99), ConstraintExistsRule(_store.nextId(), 5, 1) );

			  foreach ( SchemaRule rule in rules )
			  {
					StoreRule( rule );
			  }

			  // WHEN
			  ICollection<SchemaRule> readRules = asCollection( _store.loadAllSchemaRules() );

			  // THEN
			  assertEquals( rules, readRules );
		 }

		 private long StoreRule( SchemaRule rule )
		 {
			  ICollection<DynamicRecord> records = _store.allocateFrom( rule );
			  foreach ( DynamicRecord record in records )
			  {
					_store.updateRecord( record );
			  }
			  return Iterables.first( records ).Id;
		 }

		 private StoreIndexDescriptor IndexRule( long ruleId, IndexProviderDescriptor descriptor, int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( labelId, propertyIds ), descriptor ).withId( ruleId );
		 }

		 private StoreIndexDescriptor UniqueIndexRule( long ruleId, long owningConstraint, IndexProviderDescriptor descriptor, int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( forLabel( labelId, propertyIds ), descriptor ).withIds( ruleId, owningConstraint );
		 }

		 private ConstraintRule ConstraintUniqueRule( long ruleId, long ownedIndexId, int labelId, params int[] propertyIds )
		 {
			  return ConstraintRule.constraintRule( ruleId, ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyIds ), ownedIndexId );
		 }

		 private ConstraintRule ConstraintExistsRule( long ruleId, int labelId, params int[] propertyIds )
		 {
			  return ConstraintRule.constraintRule( ruleId, ConstraintDescriptorFactory.existsForLabel( labelId, propertyIds ) );
		 }
	}

}