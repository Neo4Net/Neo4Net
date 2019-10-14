using System.Diagnostics;

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
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;

	/// <summary>
	/// Logic for parsing and constructing <seealso cref="NodeRecord.getLabelField()"/> and dynamic label
	/// records in <seealso cref="NodeRecord.getDynamicLabelRecords()"/> from label ids.
	/// <para>
	/// Each node has a label field of 5 bytes, where labels will be stored, if sufficient space
	/// (max bits required for storing each label id is considered). If not then the field will
	/// point to a dynamic record where the labels will be stored in the format of an array property.
	/// </para>
	/// <para>
	/// [hhhh,bbbb][bbbb,bbbb][bbbb,bbbb][bbbb,bbbb][bbbb,bbbb]
	/// h: header
	/// - 0x0<=h<=0x7 (leaving high bit reserved): number of in-lined labels in the body
	/// - 0x8: body will be a pointer to first dynamic record in node-labels dynamic store
	/// b: body
	/// - 0x0<=h<=0x7 (leaving high bit reserved): bits of this many in-lined label ids
	/// - 0x8: pointer to node-labels store
	/// </para>
	/// </summary>
	public class NodeLabelsField
	{
		 private NodeLabelsField()
		 {
		 }

		 public static NodeLabels ParseLabelsField( NodeRecord node )
		 {
			  long labelField = node.LabelField;
			  return FieldPointsToDynamicRecordOfLabels( labelField ) ? new DynamicNodeLabels( node ) : new InlineNodeLabels( node );
		 }

		 public static long[] Get( NodeRecord node, NodeStore nodeStore )
		 {
			  return FieldPointsToDynamicRecordOfLabels( node.LabelField ) ? DynamicNodeLabels.Get( node, nodeStore ) : InlineNodeLabels.Get( node );
		 }

		 public static bool FieldPointsToDynamicRecordOfLabels( long labelField )
		 {
			  return ( labelField & 0x8000000000L ) != 0;
		 }

		 public static long ParseLabelsBody( long labelField )
		 {
			  return labelField & 0xFFFFFFFFFL;
		 }

		 /// <seealso cref= NodeRecord
		 /// </seealso>
		 /// <param name="labelField"> label field value from a node record </param>
		 /// <returns> the id of the dynamic record this label field points to or null if it is an inline label field </returns>
		 public static long FirstDynamicLabelRecordId( long labelField )
		 {
			  Debug.Assert( FieldPointsToDynamicRecordOfLabels( labelField ) );
			  return ParseLabelsBody( labelField );
		 }

		 /// <summary>
		 /// Checks so that a label id array is sane, i.e. that it's sorted and contains no duplicates.
		 /// </summary>
		 public static bool IsSane( long[] labelIds )
		 {
			  long prev = -1;
			  foreach ( long labelId in labelIds )
			  {
					if ( labelId <= prev )
					{
						 return false;
					}
					prev = labelId;
			  }
			  return true;
		 }
	}

}