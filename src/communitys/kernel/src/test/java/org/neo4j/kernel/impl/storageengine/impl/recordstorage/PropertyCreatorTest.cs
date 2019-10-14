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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using Neo4Net.Kernel.impl.transaction.state;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using Neo4Net.@unsafe.Batchinsert.@internal;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PropertyCreatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

		 private readonly MyPrimitiveProxy _primitive = new MyPrimitiveProxy();
		 private NeoStores _neoStores;
		 private PropertyStore _propertyStore;
		 private PropertyCreator _creator;
		 private DirectRecordAccess<PropertyRecord, PrimitiveRecord> _records;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startStore()
		 public virtual void StartStore()
		 {
			  _neoStores = ( new StoreFactory( Storage.directory().databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(Storage.fileSystem()), Storage.pageCache(), Storage.fileSystem(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openNeoStores(true, StoreType.PROPERTY, StoreType.PROPERTY_STRING, StoreType.PROPERTY_ARRAY);
			  _propertyStore = _neoStores.PropertyStore;
			  _records = new DirectRecordAccess<PropertyRecord, PrimitiveRecord>( _propertyStore, Loaders.PropertyLoader( _propertyStore ) );
			  _creator = new PropertyCreator( _propertyStore, new PropertyTraverser() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeStore()
		 public virtual void CloseStore()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToEmptyChain()
		 public virtual void ShouldAddPropertyToEmptyChain()
		 {
			  // GIVEN
			  ExistingChain();

			  // WHEN
			  SetProperty( 1, "value" );

			  // THEN
			  AssertChain( Record( Property( 1, "value" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToChainContainingOtherFullRecords()
		 public virtual void ShouldAddPropertyToChainContainingOtherFullRecords()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ), Property( 7, 7 ) ) );

			  // WHEN
			  SetProperty( 10, 10 );

			  // THEN
			  AssertChain( Record( Property( 10, 10 ) ), Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ), Property( 7, 7 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToChainContainingOtherNonFullRecords()
		 public virtual void ShouldAddPropertyToChainContainingOtherNonFullRecords()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ) ) );

			  // WHEN
			  SetProperty( 10, 10 );

			  // THEN
			  AssertChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ), Property( 10, 10 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToChainContainingOtherNonFullRecordsInMiddle()
		 public virtual void ShouldAddPropertyToChainContainingOtherNonFullRecordsInMiddle()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ) ), Record( Property( 3, 3 ), Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ) ) );

			  // WHEN
			  SetProperty( 10, 10 );

			  // THEN
			  AssertChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 10, 10 ) ), Record( Property( 3, 3 ), Property( 4, 4 ), Property( 5, 5 ), Property( 6, 6 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeOnlyProperty()
		 public virtual void ShouldChangeOnlyProperty()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, "one" ) ) );

			  // WHEN
			  SetProperty( 0, "two" );

			  // THEN
			  AssertChain( Record( Property( 0, "two" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertyInChainWithOthersBeforeIt()
		 public virtual void ShouldChangePropertyInChainWithOthersBeforeIt()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, "one" ), Property( 1, 1 ) ), Record( Property( 2, "two" ), Property( 3, 3 ) ) );

			  // WHEN
			  SetProperty( 2, "two*" );

			  // THEN
			  AssertChain( Record( Property( 0, "one" ), Property( 1, 1 ) ), Record( Property( 2, "two*" ), Property( 3, 3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertyInChainWithOthersAfterIt()
		 public virtual void ShouldChangePropertyInChainWithOthersAfterIt()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, "one" ), Property( 1, 1 ) ), Record( Property( 2, "two" ), Property( 3, 3 ) ) );

			  // WHEN
			  SetProperty( 0, "one*" );

			  // THEN
			  AssertChain( Record( Property( 0, "one*" ), Property( 1, 1 ) ), Record( Property( 2, "two" ), Property( 3, 3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertyToBiggerInFullChain()
		 public virtual void ShouldChangePropertyToBiggerInFullChain()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ) );

			  // WHEN
			  SetProperty( 1, long.MaxValue );

			  // THEN
			  AssertChain( Record( Property( 1, long.MaxValue ) ), Record( Property( 0, 0 ), Property( 2, 2 ), Property( 3, 3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertyToBiggerInChainWithHoleAfter()
		 public virtual void ShouldChangePropertyToBiggerInChainWithHoleAfter()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ) ) );

			  // WHEN
			  SetProperty( 1, long.MaxValue );

			  // THEN
			  AssertChain( Record( Property( 0, 0 ), Property( 2, 2 ), Property( 3, 3 ) ), Record( Property( 4, 4 ), Property( 5, 5 ), Property( 1, long.MaxValue ) ) );
		 }

		 // change property so that it gets bigger and fits in a record earlier in the chain
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertyToBiggerInChainWithHoleBefore()
		 public virtual void ShouldChangePropertyToBiggerInChainWithHoleBefore()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, 0 ), Property( 1, 1 ) ), Record( Property( 2, 2 ), Property( 3, 3 ), Property( 4, 4 ), Property( 5, 5 ) ) );

			  // WHEN
			  SetProperty( 2, long.MaxValue );

			  // THEN
			  AssertChain( Record( Property( 0, 0 ), Property( 1, 1 ), Property( 2, long.MaxValue ) ), Record( Property( 3, 3 ), Property( 4, 4 ), Property( 5, 5 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddMultipleShortStringsToTheSameNode()
		 public virtual void CanAddMultipleShortStringsToTheSameNode()
		 {
			  // GIVEN
			  ExistingChain();

			  // WHEN
			  SetProperty( 0, "value" );
			  SetProperty( 1, "esrever" );

			  // THEN
			  AssertChain( Record( Property( 0, "value", false ), Property( 1, "esrever", false ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canUpdateShortStringInplace()
		 public virtual void CanUpdateShortStringInplace()
		 {
			  // GIVEN
			  ExistingChain( Record( Property( 0, "value" ) ) );

			  // WHEN
			  long before = PropertyRecordsInUse();
			  SetProperty( 0, "other" );
			  long after = PropertyRecordsInUse();

			  // THEN
			  AssertChain( Record( Property( 0, "other" ) ) );
			  assertEquals( before, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReplaceLongStringWithShortString()
		 public virtual void CanReplaceLongStringWithShortString()
		 {
			  // GIVEN
			  long recordCount = DynamicStringRecordsInUse();
			  long propCount = PropertyRecordsInUse();
			  ExistingChain( Record( Property( 0, "this is a really long string, believe me!" ) ) );
			  assertEquals( recordCount + 1, DynamicStringRecordsInUse() );
			  assertEquals( propCount + 1, PropertyRecordsInUse() );

			  // WHEN
			  SetProperty( 0, "value" );

			  // THEN
			  AssertChain( Record( Property( 0, "value", false ) ) );
			  assertEquals( recordCount + 1, DynamicStringRecordsInUse() );
			  assertEquals( propCount + 1, PropertyRecordsInUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReplaceShortStringWithLongString()
		 public virtual void CanReplaceShortStringWithLongString()
		 {
			  // GIVEN
			  long recordCount = DynamicStringRecordsInUse();
			  long propCount = PropertyRecordsInUse();
			  ExistingChain( Record( Property( 0, "value" ) ) );
			  assertEquals( recordCount, DynamicStringRecordsInUse() );
			  assertEquals( propCount + 1, PropertyRecordsInUse() );

			  // WHEN
			  string longString = "this is a really long string, believe me!";
			  SetProperty( 0, longString );

			  // THEN
			  AssertChain( Record( Property( 0, longString, true ) ) );
			  assertEquals( recordCount + 1, DynamicStringRecordsInUse() );
			  assertEquals( propCount + 1, PropertyRecordsInUse() );
		 }

		 private void ExistingChain( params ExpectedRecord[] initialRecords )
		 {
			  PropertyRecord prev = null;
			  foreach ( ExpectedRecord initialRecord in initialRecords )
			  {
					PropertyRecord record = this._records.create( _propertyStore.nextId(), _primitive.record ).forChangingData();
					record.InUse = true;
					ExistingRecord( record, initialRecord );

					if ( prev == null )
					{
						 // This is the first one, update primitive to point to this
						 _primitive.record.NextProp = record.Id;
					}
					else
					{
						 // link property records together
						 record.PrevProp = prev.Id;
						 prev.NextProp = record.Id;
					}

					prev = record;
			  }
		 }

		 private void ExistingRecord( PropertyRecord record, ExpectedRecord initialRecord )
		 {
			  foreach ( ExpectedProperty initialProperty in initialRecord.Properties )
			  {
					PropertyBlock block = new PropertyBlock();
					_propertyStore.encodeValue( block, initialProperty.Key, initialProperty.Value );
					record.AddPropertyBlock( block );
			  }
			  assertTrue( record.Size() <= PropertyType.PayloadSize );
		 }

		 private void SetProperty( int key, object value )
		 {
			  _creator.primitiveSetProperty( _primitive, key, Values.of( value ), _records );
		 }

		 private void AssertChain( params ExpectedRecord[] expectedRecords )
		 {
			  long nextProp = _primitive.forReadingLinkage().NextProp;
			  int expectedRecordCursor = 0;
			  while ( !Record.NO_NEXT_PROPERTY.@is( nextProp ) )
			  {
					PropertyRecord record = _records.getIfLoaded( nextProp ).forReadingData();
					AssertRecord( record, expectedRecords[expectedRecordCursor++] );
					nextProp = record.NextProp;
			  }
		 }

		 private void AssertRecord( PropertyRecord record, ExpectedRecord expectedRecord )
		 {
			  assertEquals( expectedRecord.Properties.Length, record.NumberOfProperties() );
			  foreach ( ExpectedProperty expectedProperty in expectedRecord.Properties )
			  {
					PropertyBlock block = record.GetPropertyBlock( expectedProperty.Key );
					assertNotNull( block );
					assertEquals( expectedProperty.Value, block.Type.value( block, _propertyStore ) );
					if ( expectedProperty.AssertHasDynamicRecords != null )
					{
						 if ( expectedProperty.AssertHasDynamicRecords.Value )
						 {
							  assertThat( block.ValueRecords.Count, Matchers.greaterThan( 0 ) );
						 }
						 else
						 {
							  assertEquals( 0, block.ValueRecords.Count );
						 }
					}
			  }
		 }

		 private class ExpectedProperty
		 {
			  internal readonly int Key;
			  internal readonly Value Value;
			  internal readonly bool? AssertHasDynamicRecords;

			  internal ExpectedProperty( int key, object value ) : this( key, value, null )
			  {
			  }

			  internal ExpectedProperty( int key, object value, bool? assertHasDynamicRecords )
			  {
					this.Key = key;
					this.Value = Values.of( value );
					this.AssertHasDynamicRecords = assertHasDynamicRecords;
			  }
		 }

		 private class ExpectedRecord
		 {
			  internal readonly ExpectedProperty[] Properties;

			  internal ExpectedRecord( params ExpectedProperty[] properties )
			  {
					this.Properties = properties;
			  }
		 }

		 private static ExpectedProperty Property( int key, object value )
		 {
			  return new ExpectedProperty( key, value );
		 }

		 private static ExpectedProperty Property( int key, object value, bool hasDynamicRecords )
		 {
			  return new ExpectedProperty( key, value, hasDynamicRecords );
		 }

		 private ExpectedRecord Record( params ExpectedProperty[] properties )
		 {
			  return new ExpectedRecord( properties );
		 }

		 private class MyPrimitiveProxy : RecordAccess_RecordProxy<NodeRecord, Void>
		 {
			  internal readonly NodeRecord Record = new NodeRecord( 5 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ChangedConflict;

			  internal MyPrimitiveProxy()
			  {
					Record.InUse = true;
			  }

			  public virtual long Key
			  {
				  get
				  {
						return Record.Id;
				  }
			  }

			  public override NodeRecord ForChangingLinkage()
			  {
					ChangedConflict = true;
					return Record;
			  }

			  public override NodeRecord ForChangingData()
			  {
					ChangedConflict = true;
					return Record;
			  }

			  public override NodeRecord ForReadingLinkage()
			  {
					return Record;
			  }

			  public override NodeRecord ForReadingData()
			  {
					return Record;
			  }

			  public virtual Void AdditionalData
			  {
				  get
				  {
						return null;
				  }
			  }

			  public virtual NodeRecord Before
			  {
				  get
				  {
						return Record;
				  }
			  }

			  public virtual bool Changed
			  {
				  get
				  {
						return ChangedConflict;
				  }
			  }

			  public virtual bool Created
			  {
				  get
				  {
						return false;
				  }
			  }
		 }

		 private long PropertyRecordsInUse()
		 {
			  return _propertyStore.HighId;
		 }

		 private long DynamicStringRecordsInUse()
		 {
			  return _propertyStore.StringStore.HighId;
		 }
	}

}