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
namespace Neo4Net.Kernel.api.labelscan
{

	public class NodeLabelUpdate
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static final java.util.Comparator<? super NodeLabelUpdate> SORT_BY_NODE_ID = java.util.Comparator.comparingLong(NodeLabelUpdate::getNodeId);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 public static readonly IComparer<object> SortByNodeId = System.Collections.IComparer.comparingLong( NodeLabelUpdate::getNodeId );

		 private readonly long _nodeId;
		 private readonly long[] _labelsBefore;
		 private readonly long[] _labelsAfter;
		 private readonly long _txId;

		 private NodeLabelUpdate( long nodeId, long[] labelsBefore, long[] labelsAfter, long txId )
		 {
			  this._nodeId = nodeId;
			  this._labelsBefore = labelsBefore;
			  this._labelsAfter = labelsAfter;
			  this._txId = txId;
		 }

		 public virtual long NodeId
		 {
			 get
			 {
				  return _nodeId;
			 }
		 }

		 public virtual long[] LabelsBefore
		 {
			 get
			 {
				  return _labelsBefore;
			 }
		 }

		 public virtual long[] LabelsAfter
		 {
			 get
			 {
				  return _labelsAfter;
			 }
		 }

		 public virtual long TxId
		 {
			 get
			 {
				  return _txId;
			 }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[node:" + _nodeId + ", labelsBefore:" + Arrays.ToString(_labelsBefore) +
						 ", labelsAfter:" + Arrays.ToString( _labelsAfter ) + "]";
		 }

		 public static NodeLabelUpdate LabelChanges( long nodeId, long[] labelsBeforeChange, long[] labelsAfterChange )
		 {
			  return LabelChanges( nodeId, labelsBeforeChange, labelsAfterChange, -1 );
		 }

		 public static NodeLabelUpdate LabelChanges( long nodeId, long[] labelsBeforeChange, long[] labelsAfterChange, long txId )
		 {
			  return new NodeLabelUpdate( nodeId, labelsBeforeChange, labelsAfterChange, txId );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  NodeLabelUpdate that = ( NodeLabelUpdate ) o;

			  if ( _nodeId != that._nodeId )
			  {
					return false;
			  }
			  if ( !Arrays.Equals( _labelsAfter, that._labelsAfter ) )
			  {
					return false;
			  }
			  return Arrays.Equals( _labelsBefore, that._labelsBefore );
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _nodeId ^ ( ( long )( ( ulong )_nodeId >> 32 ) ) );
			  result = 31 * result + ( _labelsBefore != null ? Arrays.GetHashCode( _labelsBefore ) : 0 );
			  result = 31 * result + ( _labelsAfter != null ? Arrays.GetHashCode( _labelsAfter ) : 0 );
			  return result;
		 }
	}

}