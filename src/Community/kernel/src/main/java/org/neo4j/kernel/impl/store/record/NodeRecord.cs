using System;
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
namespace Neo4Net.Kernel.impl.store.record
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;

	public class NodeRecord : PrimitiveRecord
	{
		 private long _nextRel;
		 private long _labels;
		 private ICollection<DynamicRecord> _dynamicLabelRecords;
		 private bool _isLight;
		 private bool _dense;

		 public NodeRecord( long id ) : base( id )
		 {
		 }

		 public virtual NodeRecord Initialize( bool inUse, long nextProp, bool dense, long nextRel, long labels )
		 {
			  base.Initialize( inUse, nextProp );
			  this._nextRel = nextRel;
			  this._dense = dense;
			  this._labels = labels;
			  this._dynamicLabelRecords = emptyList();
			  this._isLight = true;
			  return this;
		 }

		 [Obsolete]
		 public NodeRecord( long id, bool dense, long nextRel, long nextProp ) : this( id, false, dense, nextRel, nextProp, 0 )
		 {
		 }

		 [Obsolete]
		 public NodeRecord( long id, bool inUse, bool dense, long nextRel, long nextProp, long labels ) : base( id, nextProp )
		 {
			  this._nextRel = nextRel;
			  this._dense = dense;
			  this._labels = labels;
			  InUse = inUse;
		 }

		 [Obsolete]
		 public NodeRecord( long id, bool dense, long nextRel, long nextProp, bool inUse ) : this( id, dense, nextRel, nextProp )
		 {
			  InUse = inUse;
		 }

		 public override void Clear()
		 {
			  Initialize( false, Record.NoNextProperty.intValue(), false, Record.NoNextRelationship.intValue(), Record.NoLabelsField.intValue() );
		 }

		 public virtual long NextRel
		 {
			 get
			 {
				  return _nextRel;
			 }
			 set
			 {
				  this._nextRel = value;
			 }
		 }


		 /// <summary>
		 /// Sets the label field to a pointer to the first changed dynamic record. All changed
		 /// dynamic records by doing this are supplied here.
		 /// </summary>
		 /// <param name="labels"> this will be either in-lined labels, or an id where to get the labels </param>
		 /// <param name="dynamicRecords"> all changed dynamic records by doing this. </param>
		 public virtual void SetLabelField( long labels, ICollection<DynamicRecord> dynamicRecords )
		 {
			  this._labels = labels;
			  this._dynamicLabelRecords = dynamicRecords;

			  // Only mark it as heavy if there are dynamic records, since there's a possibility that we just
			  // loaded a light version of the node record where this method was called for setting the label field.
			  // Keeping it as light in this case would make it possible to load it fully later on.
			  this._isLight = dynamicRecords.Count == 0;
		 }

		 public virtual long LabelField
		 {
			 get
			 {
				  return this._labels;
			 }
		 }

		 public virtual bool Light
		 {
			 get
			 {
				  return _isLight;
			 }
		 }

		 public virtual ICollection<DynamicRecord> DynamicLabelRecords
		 {
			 get
			 {
				  return this._dynamicLabelRecords;
			 }
		 }

		 public virtual IEnumerable<DynamicRecord> UsedDynamicLabelRecords
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return filter( AbstractBaseRecord::inUse, _dynamicLabelRecords );
			 }
		 }

		 public virtual bool Dense
		 {
			 get
			 {
				  return _dense;
			 }
			 set
			 {
				  this._dense = value;
			 }
		 }


		 public override string ToString()
		 {
			  string denseInfo = ( _dense ? "group" : "rel" ) + "=" + _nextRel;
			  string lightHeavyInfo = _isLight ? "light" : _dynamicLabelRecords.Count == 0 ? "heavy" : "heavy,dynlabels=" + _dynamicLabelRecords;

			  return "Node[" + Id +
						",used=" + InUse() +
						"," + denseInfo +
						",prop=" + NextProp +
						",labels=" + parseLabelsField( this ) +
						"," + lightHeavyInfo +
						",secondaryUnitId=" + SecondaryUnitId + "]";
		 }

		 public override PropertyRecord IdTo
		 {
			 set
			 {
				  value.NodeId = Id;
			 }
		 }

		 public override NodeRecord Clone()
		 {
			  NodeRecord clone = ( new NodeRecord( Id ) ).initialize( InUse(), NextPropConflict, _dense, _nextRel, _labels );
			  clone._isLight = _isLight;

			  if ( _dynamicLabelRecords.Count > 0 )
			  {
					IList<DynamicRecord> clonedLabelRecords = new List<DynamicRecord>( _dynamicLabelRecords.Count );
					foreach ( DynamicRecord labelRecord in _dynamicLabelRecords )
					{
						 clonedLabelRecords.Add( labelRecord.Clone() );
					}
					clone._dynamicLabelRecords = clonedLabelRecords;
			  }
			  clone.SecondaryUnitId = SecondaryUnitId;
			  return clone;
		 }
	}

}