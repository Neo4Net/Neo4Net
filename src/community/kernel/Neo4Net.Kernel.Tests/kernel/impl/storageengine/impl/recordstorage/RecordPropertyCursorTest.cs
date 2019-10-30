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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Collections.Helpers;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DirectRecordAccessSet = Neo4Net.@unsafe.Batchinsert.Internal.DirectRecordAccessSet;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;

	public class RecordPropertyCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new org.Neo4Net.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule().withConfiguration(new org.Neo4Net.values.storable.RandomValues.Default()
		 public readonly RandomRule random = new RandomRule().withConfiguration(new DefaultAnonymousInnerClass());

		 private class DefaultAnonymousInnerClass : RandomValues.Default
		 {
			 public override int stringMaxLength()
			 {
				  return 10_000;
			 }
		 }
		 private NeoStores _neoStores;
		 private PropertyCreator _creator;
		 private NodeRecord _owner;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _neoStores = ( new StoreFactory( Storage.directory().databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(Storage.fileSystem()), Storage.pageCache(), Storage.fileSystem(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openAllNeoStores(true);
			  _creator = new PropertyCreator( _neoStores.PropertyStore, new PropertyTraverser() );
			  _owner = _neoStores.NodeStore.newRecord();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeStore()
		 public virtual void CloseStore()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadPropertyChain()
		 public virtual void ShouldReadPropertyChain()
		 {
			  // given
			  Value[] values = CreateValues();
			  long firstPropertyId = StoreValuesAsPropertyChain( _creator, _owner, values );

			  // when
			  AssertPropertyChain( values, firstPropertyId, CreateCursor() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseCursor()
		 public virtual void ShouldReuseCursor()
		 {
			  // given
			  Value[] valuesA = CreateValues();
			  long firstPropertyIdA = StoreValuesAsPropertyChain( _creator, _owner, valuesA );
			  Value[] valuesB = CreateValues();
			  long firstPropertyIdB = StoreValuesAsPropertyChain( _creator, _owner, valuesB );

			  // then
			  RecordPropertyCursor cursor = CreateCursor();
			  AssertPropertyChain( valuesA, firstPropertyIdA, cursor );
			  AssertPropertyChain( valuesB, firstPropertyIdB, cursor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeShouldBeIdempotent()
		 public virtual void CloseShouldBeIdempotent()
		 {
			  // given
			  RecordPropertyCursor cursor = CreateCursor();

			  // when
			  cursor.Close();

			  // then
			  cursor.Close();
		 }

		 private RecordPropertyCursor CreateCursor()
		 {
			  return new RecordPropertyCursor( _neoStores.PropertyStore );
		 }

		 private void AssertPropertyChain( Value[] values, long firstPropertyId, RecordPropertyCursor cursor )
		 {
			  IDictionary<int, Value> expectedValues = AsMap( values );
			  cursor.Init( firstPropertyId );
			  while ( cursor.Next() )
			  {
					// then
					assertEquals( expectedValues.Remove( cursor.PropertyKey() ), cursor.PropertyValue() );
			  }
			  assertTrue( expectedValues.Count == 0 );
		 }

		 private Value[] CreateValues()
		 {
			  int numberOfProperties = random.Next( 1, 20 );
			  Value[] values = new Value[numberOfProperties];
			  for ( int key = 0; key < numberOfProperties; key++ )
			  {
					values[key] = random.nextValue();
			  }
			  return values;
		 }

		 private long StoreValuesAsPropertyChain( PropertyCreator creator, NodeRecord owner, Value[] values )
		 {
			  DirectRecordAccessSet access = new DirectRecordAccessSet( _neoStores );
			  long firstPropertyId = creator.CreatePropertyChain( owner, BlocksOf( creator, values ), access.PropertyRecords );
			  access.Close();
			  return firstPropertyId;
		 }

		 private IDictionary<int, Value> AsMap( Value[] values )
		 {
			  IDictionary<int, Value> map = new Dictionary<int, Value>();
			  for ( int key = 0; key < values.Length; key++ )
			  {
					map[key] = values[key];
			  }
			  return map;
		 }

		 private IEnumerator<PropertyBlock> BlocksOf( PropertyCreator creator, Value[] values )
		 {
			  return new IteratorWrapperAnonymousInnerClass( this, iterator( values ), creator );
		 }

		 private class IteratorWrapperAnonymousInnerClass : IteratorWrapper<PropertyBlock, Value>
		 {
			 private readonly RecordPropertyCursorTest _outerInstance;

			 private Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyCreator _creator;

			 public IteratorWrapperAnonymousInnerClass( RecordPropertyCursorTest outerInstance, UnknownType iterator, Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyCreator creator ) : base( iterator )
			 {
				 this.outerInstance = outerInstance;
				 this._creator = creator;
			 }

			 internal int key;

			 protected internal override PropertyBlock underlyingObjectToObject( Value value )
			 {
				  return _creator.encodePropertyValue( key++, value );
			 }
		 }
	}

}