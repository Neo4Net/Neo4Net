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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using PhysicalLogCommandReaderV3_0 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV3_0;
	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CommandReader = Neo4Net.Storageengine.Api.CommandReader;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.dynamicPointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.ShortArray.LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.DynamicRecord.dynamicRecord;

	public class NodeCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public NodeCommandTest()
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
		 private NodeStore _nodeStore;
		 private readonly InMemoryClosableChannel _channel = new InMemoryClosableChannel();
		 private readonly CommandReader _commandReader = new PhysicalLogCommandReaderV3_0();
		 private NeoStores _neoStores;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fs.get()), PageCacheRule.getPageCache(_fs.get()), _fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = storeFactory.OpenAllNeoStores( true );
			  _nodeStore = _neoStores.NodeStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeUnusedRecords() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeUnusedRecords()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeCreatedRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeCreatedRecord()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  after.SetCreated();
			  after.InUse = true;
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeDenseRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeDenseRecord()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  before.InUse = true;
			  NodeRecord after = new NodeRecord( 12, true, 2, 1 );
			  after.InUse = true;
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeUpdatedRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeUpdatedRecord()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  before.InUse = true;
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  after.InUse = true;
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeInlineLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeInlineLabels()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  before.InUse = true;
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  after.InUse = true;
			  NodeLabels nodeLabels = parseLabelsField( after );
			  nodeLabels.Add( 1337, _nodeStore, _nodeStore.DynamicLabelStore );
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeSecondaryUnitUsage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeSecondaryUnitUsage()
		 {
			  // Given
			  // a record that is changed to include a secondary unit
			  NodeRecord before = new NodeRecord( 13, false, 1, 2 );
			  before.InUse = true;
			  before.RequiresSecondaryUnit = false;
			  before.SecondaryUnitId = NO_ID; // this and the previous line set the defaults, they are here for clarity
			  NodeRecord after = new NodeRecord( 13, false, 1, 2 );
			  after.InUse = true;
			  after.RequiresSecondaryUnit = true;
			  after.SecondaryUnitId = 14L;

			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  // Then
			  AssertSerializationWorksFor( command );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeDynamicRecordLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeDynamicRecordLabels()
		 {
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  before.InUse = true;
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  after.InUse = true;
			  NodeLabels nodeLabels = parseLabelsField( after );
			  for ( int i = 10; i < 100; i++ )
			  {
					nodeLabels.Add( i, _nodeStore, _nodeStore.DynamicLabelStore );
			  }
			  // When
			  AssertSerializationWorksFor( new Command.NodeCommand( before, after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeDynamicRecordsRemoved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeDynamicRecordsRemoved()
		 {
			  _channel.reset();
			  // Given
			  NodeRecord before = new NodeRecord( 12, false, 1, 2 );
			  before.InUse = true;
			  IList<DynamicRecord> beforeDyn = singletonList( dynamicRecord( 0, true, true, -1L, LONG.intValue(), new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8 } ) );
			  before.SetLabelField( dynamicPointer( beforeDyn ), beforeDyn );
			  NodeRecord after = new NodeRecord( 12, false, 2, 1 );
			  after.InUse = true;
			  IList<DynamicRecord> dynamicRecords = singletonList( dynamicRecord( 0, false, true, -1L, LONG.intValue(), new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8 } ) );
			  after.SetLabelField( dynamicPointer( dynamicRecords ), dynamicRecords );
			  // When
			  Command.NodeCommand cmd = new Command.NodeCommand( before, after );
			  cmd.Serialize( _channel );
			  Command.NodeCommand result = ( Command.NodeCommand ) _commandReader.read( _channel );
			  // Then
			  assertThat( result, equalTo( cmd ) );
			  assertThat( result.Mode, equalTo( cmd.Mode ) );
			  assertThat( result.Before, equalTo( cmd.Before ) );
			  assertThat( result.After, equalTo( cmd.After ) );
			  // And dynamic records should be the same
			  assertThat( result.Before.DynamicLabelRecords, equalTo( cmd.Before.DynamicLabelRecords ) );
			  assertThat( result.After.DynamicLabelRecords, equalTo( cmd.After.DynamicLabelRecords ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializationWorksFor(org.neo4j.kernel.impl.transaction.command.Command.NodeCommand cmd) throws java.io.IOException
		 private void AssertSerializationWorksFor( Command.NodeCommand cmd )
		 {
			  _channel.reset();
			  cmd.Serialize( _channel );
			  Command.NodeCommand result = ( Command.NodeCommand ) _commandReader.read( _channel );
			  // Then
			  assertThat( result, equalTo( cmd ) );
			  assertThat( result.Mode, equalTo( cmd.Mode ) );
			  assertThat( result.Before, equalTo( cmd.Before ) );
			  assertThat( result.After, equalTo( cmd.After ) );
			  // And created and dense flags should be the same
			  assertThat( result.Before.Created, equalTo( cmd.Before.Created ) );
			  assertThat( result.After.Created, equalTo( cmd.After.Created ) );
			  assertThat( result.Before.Dense, equalTo( cmd.Before.Dense ) );
			  assertThat( result.After.Dense, equalTo( cmd.After.Dense ) );
			  // And labels should be the same
			  assertThat( Labels( result.Before ), equalTo( Labels( cmd.Before ) ) );
			  assertThat( Labels( result.After ), equalTo( Labels( cmd.After ) ) );
			  // And dynamic records should be the same
			  assertThat( result.Before.DynamicLabelRecords, equalTo( cmd.Before.DynamicLabelRecords ) );
			  assertThat( result.After.DynamicLabelRecords, equalTo( cmd.After.DynamicLabelRecords ) );
			  // And the secondary unit information should be the same
			  // Before
			  assertThat( result.Before.requiresSecondaryUnit(), equalTo(cmd.Before.requiresSecondaryUnit()) );
			  assertThat( result.Before.hasSecondaryUnitId(), equalTo(cmd.Before.hasSecondaryUnitId()) );
			  assertThat( result.Before.SecondaryUnitId, equalTo( cmd.Before.SecondaryUnitId ) );
			  // and after
			  assertThat( result.After.requiresSecondaryUnit(), equalTo(cmd.After.requiresSecondaryUnit()) );
			  assertThat( result.After.hasSecondaryUnitId(), equalTo(cmd.After.hasSecondaryUnitId()) );
			  assertThat( result.After.SecondaryUnitId, equalTo( cmd.After.SecondaryUnitId ) );
		 }

		 private ISet<int> Labels( NodeRecord record )
		 {
			  long[] rawLabels = parseLabelsField( record ).get( _nodeStore );
			  ISet<int> labels = new HashSet<int>( rawLabels.Length );
			  foreach ( long label in rawLabels )
			  {
					labels.Add( safeCastLongToInt( label ) );
			  }
			  return labels;
		 }
	}

}