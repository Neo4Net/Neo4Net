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

	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Neo4Net.Collections.Helpers;
	using ReusableRecordsCompositeAllocator = Neo4Net.Kernel.Impl.Store.Allocators.ReusableRecordsCompositeAllocator;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.AbstractDynamicStore.readFullByteArrayFromHeavyRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.DynamicArrayStore.getRightArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.LabelIdArray.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.LabelIdArray.stripNodeId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.NodeLabelsField.fieldPointsToDynamicRecordOfLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.NodeLabelsField.firstDynamicLabelRecordId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.NodeLabelsField.parseLabelsBody;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.PropertyType.ARRAY;

	public class DynamicNodeLabels : NodeLabels
	{
		 private readonly NodeRecord _node;

		 public DynamicNodeLabels( NodeRecord node )
		 {
			  this._node = node;
		 }

		 public override long[] Get( NodeStore nodeStore )
		 {
			  return Get( _node, nodeStore );
		 }

		 public static long[] Get( NodeRecord node, NodeStore nodeStore )
		 {
			  if ( node.Light )
			  {
					nodeStore.EnsureHeavy( node, firstDynamicLabelRecordId( node.LabelField ) );
			  }
			  return GetDynamicLabelsArray( node.UsedDynamicLabelRecords, nodeStore.DynamicLabelStore );
		 }

		 public virtual long[] IfLoaded
		 {
			 get
			 {
				  if ( _node.Light )
				  {
						return null;
				  }
				  return stripNodeId( ( long[] ) getRightArray( readFullByteArrayFromHeavyRecords( _node.UsedDynamicLabelRecords, ARRAY ) ).asObject() );
			 }
		 }

		 public override ICollection<DynamicRecord> Put( long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  Arrays.sort( labelIds );
			  return PutSorted( _node, labelIds, nodeStore, allocator );
		 }

		 internal static ICollection<DynamicRecord> PutSorted( NodeRecord node, long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  long existingLabelsField = node.LabelField;
			  long existingLabelsBits = parseLabelsBody( existingLabelsField );

			  ICollection<DynamicRecord> changedDynamicRecords = node.DynamicLabelRecords;

			  long labelField = node.LabelField;
			  if ( fieldPointsToDynamicRecordOfLabels( labelField ) )
			  {
					// There are existing dynamic label records, get them
					nodeStore.EnsureHeavy( node, existingLabelsBits );
					changedDynamicRecords = node.DynamicLabelRecords;
					NotInUse = changedDynamicRecords;
			  }

			  if ( !InlineNodeLabels.TryInlineInNodeRecord( node, labelIds, changedDynamicRecords ) )
			  {
					IEnumerator<DynamicRecord> recycledRecords = changedDynamicRecords.GetEnumerator();
					ICollection<DynamicRecord> allocatedRecords = AllocateRecordsForDynamicLabels( node.Id, labelIds, new ReusableRecordsCompositeAllocator( recycledRecords, allocator ) );
					// Set the rest of the previously set dynamic records as !inUse
					while ( recycledRecords.MoveNext() )
					{
						 DynamicRecord removedRecord = recycledRecords.Current;
						 removedRecord.InUse = false;
						 allocatedRecords.Add( removedRecord );
					}
					node.SetLabelField( DynamicPointer( allocatedRecords ), allocatedRecords );
					changedDynamicRecords = allocatedRecords;
			  }

			  return changedDynamicRecords;
		 }

		 public override ICollection<DynamicRecord> Add( long labelId, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  nodeStore.EnsureHeavy( _node, firstDynamicLabelRecordId( _node.LabelField ) );
			  long[] existingLabelIds = GetDynamicLabelsArray( _node.UsedDynamicLabelRecords, nodeStore.DynamicLabelStore );
			  long[] newLabelIds = LabelIdArray.ConcatAndSort( existingLabelIds, labelId );
			  ICollection<DynamicRecord> existingRecords = _node.DynamicLabelRecords;
			  ICollection<DynamicRecord> changedDynamicRecords = AllocateRecordsForDynamicLabels( _node.Id, newLabelIds, new ReusableRecordsCompositeAllocator( existingRecords, allocator ) );
			  _node.setLabelField( DynamicPointer( changedDynamicRecords ), changedDynamicRecords );
			  return changedDynamicRecords;
		 }

		 public override ICollection<DynamicRecord> Remove( long labelId, NodeStore nodeStore )
		 {
			  nodeStore.EnsureHeavy( _node, firstDynamicLabelRecordId( _node.LabelField ) );
			  long[] existingLabelIds = GetDynamicLabelsArray( _node.UsedDynamicLabelRecords, nodeStore.DynamicLabelStore );
			  long[] newLabelIds = filter( existingLabelIds, labelId );
			  ICollection<DynamicRecord> existingRecords = _node.DynamicLabelRecords;
			  if ( InlineNodeLabels.TryInlineInNodeRecord( _node, newLabelIds, existingRecords ) )
			  {
					NotInUse = existingRecords;
			  }
			  else
			  {
					ICollection<DynamicRecord> newRecords = AllocateRecordsForDynamicLabels( _node.Id, newLabelIds, new ReusableRecordsCompositeAllocator( existingRecords, nodeStore.DynamicLabelStore ) );
					_node.setLabelField( DynamicPointer( newRecords ), existingRecords );
					if ( !newRecords.Equals( existingRecords ) )
					{ // One less dynamic record, mark that one as not in use
						 foreach ( DynamicRecord record in existingRecords )
						 {
							  if ( !newRecords.Contains( record ) )
							  {
									record.InUse = false;
							  }
						 }
					}
			  }
			  return existingRecords;
		 }

		 public virtual long FirstDynamicRecordId
		 {
			 get
			 {
				  return firstDynamicLabelRecordId( _node.LabelField );
			 }
		 }

		 public static long DynamicPointer( ICollection<DynamicRecord> newRecords )
		 {
			  return 0x8000000000L | Iterables.first( newRecords ).Id;
		 }

		 private static ICollection<DynamicRecord> NotInUse
		 {
			 set
			 {
				  foreach ( DynamicRecord record in value )
				  {
						record.InUse = false;
				  }
			 }
		 }

		 public virtual bool Inlined
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override string ToString()
		 {
			  if ( _node.Light )
			  {
					return format( "Dynamic(id:%d)", firstDynamicLabelRecordId( _node.LabelField ) );
			  }
			  return format( "Dynamic(id:%d,[%s])", firstDynamicLabelRecordId( _node.LabelField ), Arrays.ToString( GetDynamicLabelsArrayFromHeavyRecords( _node.UsedDynamicLabelRecords ) ) );
		 }

		 public static ICollection<DynamicRecord> AllocateRecordsForDynamicLabels( long nodeId, long[] labels, AbstractDynamicStore dynamicLabelStore )
		 {
			  return AllocateRecordsForDynamicLabels( nodeId, labels, ( DynamicRecordAllocator )dynamicLabelStore );
		 }

		 public static ICollection<DynamicRecord> AllocateRecordsForDynamicLabels( long nodeId, long[] labels, DynamicRecordAllocator allocator )
		 {
			  long[] storedLongs = LabelIdArray.PrependNodeId( nodeId, labels );
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  // since we can't store points in long array we passing false as possibility to store points
			  DynamicArrayStore.AllocateRecords( records, storedLongs, allocator, false );
			  return records;
		 }

		 public static long[] GetDynamicLabelsArray( IEnumerable<DynamicRecord> records, AbstractDynamicStore dynamicLabelStore )
		 {
			  long[] storedLongs = ( long[] ) DynamicArrayStore.GetRightArray( dynamicLabelStore.ReadFullByteArray( records, PropertyType.Array ) ).asObject();
			  return LabelIdArray.StripNodeId( storedLongs );
		 }

		 public static long[] GetDynamicLabelsArrayFromHeavyRecords( IEnumerable<DynamicRecord> records )
		 {
			  long[] storedLongs = ( long[] ) DynamicArrayStore.GetRightArray( readFullByteArrayFromHeavyRecords( records, PropertyType.Array ) ).asObject();
			  return LabelIdArray.StripNodeId( storedLongs );
		 }

		 public static Pair<long, long[]> GetDynamicLabelsArrayAndOwner( IEnumerable<DynamicRecord> records, AbstractDynamicStore dynamicLabelStore )
		 {
			  long[] storedLongs = ( long[] ) DynamicArrayStore.GetRightArray( dynamicLabelStore.ReadFullByteArray( records, PropertyType.Array ) ).asObject();
			  return Pair.of( storedLongs[0], LabelIdArray.StripNodeId( storedLongs ) );
		 }
	}

}