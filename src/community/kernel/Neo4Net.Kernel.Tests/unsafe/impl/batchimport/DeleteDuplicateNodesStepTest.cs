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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Helpers.Collections;
	using Loaders = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using PropertyCreator = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyCreator;
	using PropertyTraverser = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyTraverser;
	using AbstractDynamicStore = Neo4Net.Kernel.impl.store.AbstractDynamicStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using Neo4Net.Kernel.impl.transaction.state;
	using NeoStoresRule = Neo4Net.Test.rule.NeoStoresRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using Repeat = Neo4Net.Test.rule.RepeatRule.Repeat;
	using Neo4Net.@unsafe.Batchinsert.Internal;
	using SimpleStageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.SimpleStageControl;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DeleteDuplicateNodesStepTest
	{
		private bool InstanceFieldsInitialized = false;

		public DeleteDuplicateNodesStepTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _repeater ).around( random ).around( _neoStoresRule );
		}

		 private readonly RandomRule random = new RandomRule().withConfiguration(new DefaultAnonymousInnerClass());

		 private class DefaultAnonymousInnerClass : RandomValues.Default
		 {
			 public override int stringMaxLength()
			 {
				  return 200;
			 }

			 public override int arrayMaxLength()
			 {
				  return 200;
			 }
		 }
		 private readonly NeoStoresRule _neoStoresRule = new NeoStoresRule( this.GetType() );
		 private readonly RepeatRule _repeater = new RepeatRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(repeater).around(random).around(neoStoresRule);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Repeat(times = 10) @Test public void shouldDeleteEverythingAboutTheDuplicatedNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Repeat(times : 10)]
		 public virtual void ShouldDeleteEverythingAboutTheDuplicatedNodes()
		 {
			  // given
			  NeoStores neoStores = _neoStoresRule.builder().build();
			  Ids[] ids = new Ids[9];
			  DataImporter.Monitor monitor = new DataImporter.Monitor();
			  ids[0] = CreateNode( monitor, neoStores, 10, 10 ); // node with many properties and many labels
			  ids[1] = CreateNode( monitor, neoStores, 10, 1 ); // node with many properties and few labels
			  ids[2] = CreateNode( monitor, neoStores, 10, 0 ); // node with many properties and no labels
			  ids[3] = CreateNode( monitor, neoStores, 1, 10 ); // node with few properties and many labels
			  ids[4] = CreateNode( monitor, neoStores, 1, 1 ); // node with few properties and few labels
			  ids[5] = CreateNode( monitor, neoStores, 1, 0 ); // node with few properties and no labels
			  ids[6] = CreateNode( monitor, neoStores, 0, 10 ); // node with no properties and many labels
			  ids[7] = CreateNode( monitor, neoStores, 0, 1 ); // node with no properties and few labels
			  ids[8] = CreateNode( monitor, neoStores, 0, 0 ); // node with no properties and no labels

			  // when
			  long[] duplicateNodeIds = RandomNodes( ids );
			  SimpleStageControl control = new SimpleStageControl();
			  using ( DeleteDuplicateNodesStep step = new DeleteDuplicateNodesStep( control, Configuration.DEFAULT, PrimitiveLongCollections.iterator( duplicateNodeIds ), neoStores.NodeStore, neoStores.PropertyStore, monitor ) )
			  {
					control.Steps( step );
					StartAndAwaitCompletionOf( step );
			  }
			  control.AssertHealthy();

			  // then
			  int expectedNodes = 0;
			  int expectedProperties = 0;
			  foreach ( Ids entity in ids )
			  {
					bool expectedToBeInUse = !ArrayUtils.contains( duplicateNodeIds, entity.Node.Id );
					int stride = expectedToBeInUse ? 1 : 0;
					expectedNodes += stride;

					// Verify node record
					assertEquals( expectedToBeInUse, neoStores.NodeStore.isInUse( entity.Node.Id ) );

					// Verify label records
					foreach ( DynamicRecord labelRecord in entity.Node.DynamicLabelRecords )
					{
						 assertEquals( expectedToBeInUse, neoStores.NodeStore.DynamicLabelStore.isInUse( labelRecord.Id ) );
					}

					// Verify property records
					foreach ( PropertyRecord propertyRecord in entity.Properties )
					{
						 assertEquals( expectedToBeInUse, neoStores.PropertyStore.isInUse( propertyRecord.Id ) );
						 foreach ( PropertyBlock property in propertyRecord )
						 {
							  // Verify property dynamic value records
							  foreach ( DynamicRecord valueRecord in property.ValueRecords )
							  {
									AbstractDynamicStore valueStore;
									switch ( property.Type )
									{
									case STRING:
										 valueStore = neoStores.PropertyStore.StringStore;
										 break;
									case ARRAY:
										 valueStore = neoStores.PropertyStore.ArrayStore;
										 break;
									default:
										throw new System.ArgumentException( propertyRecord + " " + property );
									}
									assertEquals( expectedToBeInUse, valueStore.IsInUse( valueRecord.Id ) );
							  }
							  expectedProperties += stride;
						 }
					}
			  }

			  assertEquals( expectedNodes, monitor.NodesImported() );
			  assertEquals( expectedProperties, monitor.PropertiesImported() );
		 }

		 private long[] RandomNodes( Ids[] ids )
		 {
			  long[] nodeIds = new long[ids.Length];
			  int cursor = 0;
			  foreach ( Ids id in ids )
			  {
					if ( random.nextBoolean() )
					{
						 nodeIds[cursor++] = id.Node.Id;
					}
			  }

			  // If none was selected, then pick just one
			  if ( cursor == 0 )
			  {
					nodeIds[cursor++] = random.among( ids ).node.Id;
			  }
			  return Arrays.copyOf( nodeIds, cursor );
		 }

		 private class Ids
		 {
			  internal readonly NodeRecord Node;
			  internal readonly PropertyRecord[] Properties;

			  internal Ids( NodeRecord node, PropertyRecord[] properties )
			  {
					this.Node = node;
					this.Properties = properties;
			  }
		 }

		 private Ids CreateNode( DataImporter.Monitor monitor, NeoStores neoStores, int propertyCount, int labelCount )
		 {
			  PropertyStore propertyStore = neoStores.PropertyStore;
			  RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecordAccess = new DirectRecordAccess<PropertyRecord, PrimitiveRecord>( propertyStore, ( new Loaders( neoStores ) ).propertyLoader() );
			  NodeStore nodeStore = neoStores.NodeStore;
			  NodeRecord nodeRecord = nodeStore.NewRecord();
			  nodeRecord.Id = nodeStore.NextId();
			  nodeRecord.InUse = true;
			  NodeLabelsField.parseLabelsField( nodeRecord ).put( LabelIds( labelCount ), nodeStore, nodeStore.DynamicLabelStore );
			  long nextProp = ( new PropertyCreator( propertyStore, new PropertyTraverser() ) ).createPropertyChain(nodeRecord, Properties(propertyStore, propertyCount), propertyRecordAccess);
			  nodeRecord.NextProp = nextProp;
			  nodeStore.UpdateRecord( nodeRecord );
			  PropertyRecord[] propertyRecords = ExtractPropertyRecords( propertyRecordAccess, nextProp );
			  propertyRecordAccess.Close();
			  monitor.NodesImported( 1 );
			  monitor.PropertiesImported( propertyCount );
			  return new Ids( nodeRecord, propertyRecords );
		 }

		 private static PropertyRecord[] ExtractPropertyRecords( RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecordAccess, long nextProp )
		 {
			  IList<PropertyRecord> result = new List<PropertyRecord>();
			  while ( !Record.NULL_REFERENCE.@is( nextProp ) )
			  {
					PropertyRecord record = propertyRecordAccess.GetIfLoaded( nextProp ).forReadingLinkage();
					result.Add( record );
					nextProp = record.NextProp;
			  }
			  return result.ToArray();
		 }

		 private IEnumerator<PropertyBlock> Properties( PropertyStore propertyStore, int propertyCount )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this, propertyStore, propertyCount );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<PropertyBlock>
		 {
			 private readonly DeleteDuplicateNodesStepTest _outerInstance;

			 private PropertyStore _propertyStore;
			 private int _propertyCount;

			 public PrefetchingIteratorAnonymousInnerClass( DeleteDuplicateNodesStepTest outerInstance, PropertyStore propertyStore, int propertyCount )
			 {
				 this.outerInstance = outerInstance;
				 this._propertyStore = propertyStore;
				 this._propertyCount = propertyCount;
			 }

			 private int i;

			 protected internal override PropertyBlock fetchNextOrNull()
			 {
				  if ( i >= _propertyCount )
				  {
						return null;
				  }
				  PropertyBlock block = new PropertyBlock();
				  _propertyStore.encodeValue( block, i, random.nextValue() );
				  i++;
				  return block;
			 }
		 }

		 private static long[] LabelIds( int labelCount )
		 {
			  long[] result = new long[labelCount];
			  for ( int i = 0; i < labelCount; i++ )
			  {
					result[i] = i;
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void startAndAwaitCompletionOf(DeleteDuplicateNodesStep step) throws InterruptedException
		 private static void StartAndAwaitCompletionOf( DeleteDuplicateNodesStep step )
		 {
			  step.start( 0 );
			  step.Receive( 0, null );
			  step.awaitCompleted();
		 }
	}

}