using System.Collections.Generic;
using System.Diagnostics;

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

	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Bits = Org.Neo4j.Kernel.impl.util.Bits;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.highestOneBit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.LabelIdArray.concatAndSort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.LabelIdArray.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsBody;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bitsFromLongs;

	public class InlineNodeLabels : NodeLabels
	{
		 private const int LABEL_BITS = 36;
		 private readonly NodeRecord _node;

		 public InlineNodeLabels( NodeRecord node )
		 {
			  this._node = node;
		 }

		 public override long[] Get( NodeStore nodeStore )
		 {
			  return Get( _node );
		 }

		 public static long[] Get( NodeRecord node )
		 {
			  return ParseInlined( node.LabelField );
		 }

		 public virtual long[] IfLoaded
		 {
			 get
			 {
				  return ParseInlined( _node.LabelField );
			 }
		 }

		 public override ICollection<DynamicRecord> Put( long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  Arrays.sort( labelIds );
			  return PutSorted( _node, labelIds, nodeStore, allocator );
		 }

		 public static ICollection<DynamicRecord> PutSorted( NodeRecord node, long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  if ( TryInlineInNodeRecord( node, labelIds, node.DynamicLabelRecords ) )
			  {
					return Collections.emptyList();
			  }

			  return DynamicNodeLabels.PutSorted( node, labelIds, nodeStore, allocator );
		 }

		 public override ICollection<DynamicRecord> Add( long labelId, NodeStore nodeStore, DynamicRecordAllocator allocator )
		 {
			  long[] augmentedLabelIds = LabelCount( _node.LabelField ) == 0 ? new long[]{ labelId } : concatAndSort( ParseInlined( _node.LabelField ), labelId );

			  return PutSorted( _node, augmentedLabelIds, nodeStore, allocator );
		 }

		 public override ICollection<DynamicRecord> Remove( long labelId, NodeStore nodeStore )
		 {
			  long[] newLabelIds = filter( ParseInlined( _node.LabelField ), labelId );
			  bool inlined = TryInlineInNodeRecord( _node, newLabelIds, _node.DynamicLabelRecords );
			  Debug.Assert( inlined );
			  return Collections.emptyList();
		 }

		 internal static bool TryInlineInNodeRecord( NodeRecord node, long[] ids, ICollection<DynamicRecord> changedDynamicRecords )
		 {
			  // We reserve the high header bit for future extensions of the format of the in-lined label bits
			  // i.e. the 0-valued high header bit can allow for 0-7 in-lined labels in the bit-packed format.
			  if ( ids.Length > 7 )
			  {
					return false;
			  }

			  sbyte bitsPerLabel = ( sbyte )( ids.Length > 0 ? ( LABEL_BITS / ids.Length ) : LABEL_BITS );
			  Bits bits = bits( 5 );
			  if ( !InlineValues( ids, bitsPerLabel, bits ) )
			  {
					return false;
			  }
			  node.SetLabelField( CombineLabelCountAndLabelStorage( ( sbyte ) ids.Length, bits.Longs[0] ), changedDynamicRecords );
			  return true;
		 }

		 private static bool InlineValues( long[] values, int maxBitsPerLabel, Bits target )
		 {
			  long limit = 1L << maxBitsPerLabel;
			  foreach ( long value in values )
			  {
					if ( highestOneBit( value ) < limit )
					{
						 target.Put( value, maxBitsPerLabel );
					}
					else
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static long[] ParseInlined( long labelField )
		 {
			  sbyte numberOfLabels = LabelCount( labelField );
			  if ( numberOfLabels == 0 )
			  {
					return EMPTY_LONG_ARRAY;
			  }

			  long existingLabelsField = parseLabelsBody( labelField );
			  sbyte bitsPerLabel = ( sbyte )( LABEL_BITS / numberOfLabels );
			  Bits bits = bitsFromLongs( new long[]{ existingLabelsField } );
			  long[] result = new long[numberOfLabels];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = bits.GetLong( bitsPerLabel );
			  }
			  return result;
		 }

		 private static long CombineLabelCountAndLabelStorage( sbyte labelCount, long labelBits )
		 {
			  return ( ( long )labelCount << 36 ) | labelBits;
		 }

		 private static sbyte LabelCount( long labelField )
		 {
			  return ( sbyte )( ( long )( ( ulong )( labelField & 0xF000000000L ) >> 36 ) );
		 }

		 public virtual bool Inlined
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override string ToString()
		 {
			  return format( "Inline(0x%x:%s)", _node.LabelField, Arrays.ToString( IfLoaded ) );
		 }
	}

}