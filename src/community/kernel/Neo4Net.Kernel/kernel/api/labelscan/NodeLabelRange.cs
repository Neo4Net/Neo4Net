using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Kernel.Api.LabelScan
{
	using LongList = org.eclipse.collections.api.list.primitive.LongList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongArrayList = org.eclipse.collections.impl.list.mutable.primitive.LongArrayList;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	/// <summary>
	/// Represents a range of nodes and label ids attached to those nodes. All nodes in the range are present in
	/// <seealso cref="nodes() nodes array"/>, but not all node ids will have corresponding <seealso cref="labels(long) labels"/>,
	/// where an empty long[] will be returned instead.
	/// </summary>
	public class NodeLabelRange
	{
		 private readonly long _idRange;
		 private readonly long[] _nodes;
		 private readonly long[][] _labels;

		 /// <param name="idRange"> node id range, e.g. in which id span the nodes are. </param>
		 /// <param name="labels"> long[][] where first dimension is relative node id in this range, i.e. 0-rangeSize
		 /// and second the label ids for that node, potentially empty if there are none for that node.
		 /// The first dimension must be the size of the range. </param>
		 public NodeLabelRange( long idRange, long[][] labels )
		 {
			  this._idRange = idRange;
			  this._labels = labels;
			  int rangeSize = labels.Length;
			  long baseNodeId = idRange * rangeSize;

			  this._nodes = new long[rangeSize];
			  for ( int i = 0; i < rangeSize; i++ )
			  {
					_nodes[i] = baseNodeId + i;
			  }
		 }

		 /// <returns> the range id of this range. This is the base node id divided by range size.
		 /// Example: A store with nodes 1,3,20,22 and a range size of 16 would return ranges:
		 /// - rangeId=0, nodes=1,3
		 /// - rangeId=1, nodes=20,22 </returns>
		 public virtual long Id()
		 {
			  return _idRange;
		 }

		 /// <returns> node ids in this range, the nodes in this array may or may not have <seealso cref="labels(long) labels"/>
		 /// attached to it. </returns>
		 public virtual long[] Nodes()
		 {
			  return _nodes;
		 }

		 /// <summary>
		 /// Returns the label ids (as longs) for the given node id. The {@code nodeId} must be one of the ids
		 /// from <seealso cref="nodes()"/>.
		 /// </summary>
		 /// <param name="nodeId"> the node id to return labels for. </param>
		 /// <returns> label ids for the given {@code nodeId}. </returns>
		 public virtual long[] Labels( long nodeId )
		 {
			  long firstNodeId = _idRange * _labels.Length;
			  int index = toIntExact( nodeId - firstNodeId );
			  Debug.Assert( index >= 0 && index < _labels.Length, "nodeId:" + nodeId + ", idRange:" + _idRange );
			  return _labels[index] != null ? _labels[index] : EMPTY_LONG_ARRAY;
		 }

		 private static string ToString( string prefix, long[] nodes, long[][] labels )
		 {
			  StringBuilder result = new StringBuilder( prefix );
			  result.Append( "; {" );
			  for ( int i = 0; i < nodes.Length; i++ )
			  {
					if ( i != 0 )
					{
						 result.Append( ", " );
					}
					result.Append( "Node[" ).Append( nodes[i] ).Append( "]: Labels[" );
					string sep = "";
					if ( labels[i] != null )
					{
						 foreach ( long labelId in labels[i] )
						 {
							  result.Append( sep ).Append( labelId );
							  sep = ", ";
						 }
					}
					else
					{
						 result.Append( "null" );
					}
					result.Append( ']' );
			  }
			  return result.Append( "}]" ).ToString();
		 }

		 public override string ToString()
		 {
			  string rangeString = _idRange * _labels.Length + "-" + ( _idRange + 1 ) * _labels.Length;
			  string prefix = "NodeLabelRange[idRange=" + rangeString;
			  return ToString( prefix, _nodes, _labels );
		 }

		 public static void ReadBitmap( long bitmap, long labelId, MutableLongList[] labelsPerNode )
		 {
			  while ( bitmap != 0 )
			  {
					int relativeNodeId = Long.numberOfTrailingZeros( bitmap );
					if ( labelsPerNode[relativeNodeId] == null )
					{
						 labelsPerNode[relativeNodeId] = new LongArrayList();
					}
					labelsPerNode[relativeNodeId].add( labelId );
					bitmap &= bitmap - 1;
			  }
		 }

		 public static long[][] ConvertState( LongList[] state )
		 {
			  long[][] labelIdsByNodeIndex = new long[state.Length][];
			  for ( int i = 0; i < state.Length; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.list.primitive.LongList labelIdList = state[i];
					LongList labelIdList = state[i];
					if ( labelIdList != null )
					{
						 labelIdsByNodeIndex[i] = labelIdList.toArray();
					}
			  }
			  return labelIdsByNodeIndex;
		 }
	}

}