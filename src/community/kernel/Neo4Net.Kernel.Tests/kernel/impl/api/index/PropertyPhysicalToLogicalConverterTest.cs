﻿/*
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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PropertyPhysicalToLogicalConverterTest
	{
		private bool InstanceFieldsInitialized = false;

		public PropertyPhysicalToLogicalConverterTest()
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
//ORIGINAL LINE: @ClassRule public static org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public static PageCacheRule PageCacheRule = new PageCacheRule();
		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory);
		 public RuleChain RuleChain;

		 private NeoStores _neoStores;
		 private PropertyStore _store;
		 private readonly Value _longString = Values.of( "my super looooooooooooooooooooooooooooooooooooooong striiiiiiiiiiiiiiiiiiiiiiing" );
		 private readonly Value _longerString = Values.of( "my super looooooooooooooooooooooooooooooooooooooong striiiiiiiiiiiiiiiiiiiiiiingdd" );
		 private PropertyPhysicalToLogicalConverter _converter;
		 private readonly long[] _none = new long[0];

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fs.get()), PageCacheRule.getPageCache(_fs.get()), _fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = storeFactory.OpenAllNeoStores( true );
			  _store = _neoStores.PropertyStore;
			  _converter = new PropertyPhysicalToLogicalConverter( _store );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertInlinedAddedProperty()
		 public virtual void ShouldConvertInlinedAddedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  Value value = Values.of( 12345 );
			  PropertyRecord before = PropertyRecord();
			  PropertyRecord after = PropertyRecord( Property( key, value ) );

			  // WHEN
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( IEntityUpdates.ForEntity( 0, false ).added( key, value ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertInlinedChangedProperty()
		 public virtual void ShouldConvertInlinedChangedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  Value valueBefore = Values.of( 12341 );
			  Value valueAfter = Values.of( 738 );
			  PropertyRecord before = PropertyRecord( Property( key, valueBefore ) );
			  PropertyRecord after = PropertyRecord( Property( key, valueAfter ) );

			  // WHEN
			  IEntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  IEntityUpdates expected = IEntityUpdates.ForEntity( 0, false ).changed( key, valueBefore, valueAfter ).build();
			  assertEquals( expected, update );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreInlinedUnchangedProperty()
		 public virtual void ShouldIgnoreInlinedUnchangedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  Value value = Values.of( 12341 );
			  PropertyRecord before = PropertyRecord( Property( key, value ) );
			  PropertyRecord after = PropertyRecord( Property( key, value ) );

			  // WHEN
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( IEntityUpdates.ForEntity( 0, false ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertInlinedRemovedProperty()
		 public virtual void ShouldConvertInlinedRemovedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  Value value = Values.of( 12341 );
			  PropertyRecord before = PropertyRecord( Property( key, value ) );
			  PropertyRecord after = PropertyRecord();

			  // WHEN
			  IEntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  IEntityUpdates expected = IEntityUpdates.ForEntity( 0, false ).removed( key, value ).build();
			  assertEquals( expected, update );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertDynamicAddedProperty()
		 public virtual void ShouldConvertDynamicAddedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  PropertyRecord before = PropertyRecord();
			  PropertyRecord after = PropertyRecord( Property( key, _longString ) );

			  // THEN
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( IEntityUpdates.ForEntity( 0, false ).added( key, _longString ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertDynamicChangedProperty()
		 public virtual void ShouldConvertDynamicChangedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  PropertyRecord before = PropertyRecord( Property( key, _longString ) );
			  PropertyRecord after = PropertyRecord( Property( key, _longerString ) );

			  // WHEN
			  IEntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  IEntityUpdates expected = IEntityUpdates.ForEntity( 0, false ).changed( key, _longString, _longerString ).build();
			  assertEquals( expected, update );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertDynamicInlinedRemovedProperty()
		 public virtual void ShouldConvertDynamicInlinedRemovedProperty()
		 {
			  // GIVEN
			  int key = 10;
			  PropertyRecord before = PropertyRecord( Property( key, _longString ) );
			  PropertyRecord after = PropertyRecord();

			  // WHEN
			  IEntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  IEntityUpdates expected = IEntityUpdates.ForEntity( 0, false ).removed( key, _longString ).build();
			  assertEquals( expected, update );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatPropertyThatMovedToAnotherRecordAsChange()
		 public virtual void ShouldTreatPropertyThatMovedToAnotherRecordAsChange()
		 {
			  // GIVEN
			  int key = 12;
			  Value oldValue = Values.of( "value1" );
			  Value newValue = Values.of( "value two" );
			  Command.PropertyCommand movedFrom = Change( PropertyRecord( Property( key, oldValue ) ), PropertyRecord() );
			  Command.PropertyCommand movedTo = Change( PropertyRecord(), PropertyRecord(Property(key, newValue)) );

			  // WHEN
			  IEntityUpdates update = Convert( _none, _none, movedFrom, movedTo );

			  // THEN
			  IEntityUpdates expected = IEntityUpdates.ForEntity( 0, false ).changed( key, oldValue, newValue ).build();
			  assertEquals( expected, update );
		 }

		 private static PropertyRecord PropertyRecord( params PropertyBlock[] propertyBlocks )
		 {
			  PropertyRecord record = new PropertyRecord( 0 );
			  if ( propertyBlocks != null )
			  {
					record.InUse = true;
					foreach ( PropertyBlock propertyBlock in propertyBlocks )
					{
						 record.AddPropertyBlock( propertyBlock );
					}
			  }
			  record.NodeId = 0;
			  return record;
		 }

		 private PropertyBlock Property( long key, Value value )
		 {
			  PropertyBlock block = new PropertyBlock();
			  _store.encodeValue( block, ( int ) key, value );
			  return block;
		 }

		 private IEntityUpdates Convert( long[] labelsBefore, long[] labelsAfter, params Command.PropertyCommand[] changes )
		 {
			  long nodeId = 0;
			  IEntityUpdates.Builder updates = IEntityUpdates.ForEntity( ( long ) 0, false ).withTokens( labelsBefore ).withTokensAfter( labelsAfter );
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( typeof( Command.NodeCommand ), 8 );
			  grouper.add( new Command.NodeCommand( new NodeRecord( nodeId ), new NodeRecord( nodeId ) ) );
			  foreach ( Command.PropertyCommand change in changes )
			  {
					grouper.add( change );
			  }
			  IEntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();
			  assertTrue( cursor.NextEntity() );
			  _converter.convertPropertyRecord( cursor, updates );
			  return updates.Build();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.kernel.impl.transaction.command.Command.PropertyCommand change(final org.Neo4Net.kernel.impl.store.record.PropertyRecord before, final org.Neo4Net.kernel.impl.store.record.PropertyRecord after)
		 private Command.PropertyCommand Change( PropertyRecord before, PropertyRecord after )
		 {
			  return new Command.PropertyCommand( before, after );
		 }
	}

}