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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

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
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
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
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( EntityUpdates.ForEntity( 0, false ).added( key, value ).build() ) );
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
			  EntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  EntityUpdates expected = EntityUpdates.ForEntity( 0, false ).changed( key, valueBefore, valueAfter ).build();
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
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( EntityUpdates.ForEntity( 0, false ).build() ) );
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
			  EntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  EntityUpdates expected = EntityUpdates.ForEntity( 0, false ).removed( key, value ).build();
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
			  assertThat( Convert( _none, _none, Change( before, after ) ), equalTo( EntityUpdates.ForEntity( 0, false ).added( key, _longString ).build() ) );
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
			  EntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  EntityUpdates expected = EntityUpdates.ForEntity( 0, false ).changed( key, _longString, _longerString ).build();
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
			  EntityUpdates update = Convert( _none, _none, Change( before, after ) );

			  // THEN
			  EntityUpdates expected = EntityUpdates.ForEntity( 0, false ).removed( key, _longString ).build();
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
			  EntityUpdates update = Convert( _none, _none, movedFrom, movedTo );

			  // THEN
			  EntityUpdates expected = EntityUpdates.ForEntity( 0, false ).changed( key, oldValue, newValue ).build();
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

		 private EntityUpdates Convert( long[] labelsBefore, long[] labelsAfter, params Command.PropertyCommand[] changes )
		 {
			  long nodeId = 0;
			  EntityUpdates.Builder updates = EntityUpdates.ForEntity( ( long ) 0, false ).withTokens( labelsBefore ).withTokensAfter( labelsAfter );
			  EntityCommandGrouper grouper = new EntityCommandGrouper<>( typeof( Command.NodeCommand ), 8 );
			  grouper.add( new Command.NodeCommand( new NodeRecord( nodeId ), new NodeRecord( nodeId ) ) );
			  foreach ( Command.PropertyCommand change in changes )
			  {
					grouper.add( change );
			  }
			  EntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();
			  assertTrue( cursor.NextEntity() );
			  _converter.convertPropertyRecord( cursor, updates );
			  return updates.Build();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.command.Command.PropertyCommand change(final org.neo4j.kernel.impl.store.record.PropertyRecord before, final org.neo4j.kernel.impl.store.record.PropertyRecord after)
		 private Command.PropertyCommand Change( PropertyRecord before, PropertyRecord after )
		 {
			  return new Command.PropertyCommand( before, after );
		 }
	}

}