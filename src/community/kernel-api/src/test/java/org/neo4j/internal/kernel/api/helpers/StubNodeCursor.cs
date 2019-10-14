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
namespace Neo4Net.Internal.Kernel.Api.helpers
{

	using Value = Neo4Net.Values.Storable.Value;

	public class StubNodeCursor : NodeCursor
	{
		 private int _offset = -1;
		 private bool _dense;
		 private IList<NodeData> _nodes = new List<NodeData>();

		 public StubNodeCursor() : this(true)
		 {
		 }

		 public StubNodeCursor( bool dense )
		 {
			  this._dense = dense;
		 }

		 internal virtual void Single( long reference )
		 {
			  _offset = int.MaxValue;
			  for ( int i = 0; i < _nodes.Count; i++ )
			  {
					if ( reference == _nodes[i].id )
					{
						 _offset = i - 1;
					}
			  }
		 }

		 internal virtual void Scan()
		 {
			  _offset = -1;
		 }

		 public virtual StubNodeCursor WithNode( long id )
		 {
			  _nodes.Add( new NodeData( id, new long[]{}, Collections.emptyMap() ) );
			  return this;
		 }

		 public virtual StubNodeCursor WithNode( long id, params long[] labels )
		 {
			  _nodes.Add( new NodeData( id, labels, Collections.emptyMap() ) );
			  return this;
		 }

		 public virtual StubNodeCursor WithNode( long id, long[] labels, IDictionary<int, Value> properties )
		 {
			  _nodes.Add( new NodeData( id, labels, properties ) );
			  return this;
		 }

		 public override long NodeReference()
		 {
			  return _offset >= 0 && _offset < _nodes.Count ? _nodes[_offset].id : -1;
		 }

		 public override LabelSet Labels()
		 {
			  return _offset >= 0 && _offset < _nodes.Count ? _nodes[_offset].labelSet() : LabelSet.NONE;
		 }

		 public override bool HasLabel( int label )
		 {
			  return Labels().contains(label);
		 }

		 public override void Relationships( RelationshipGroupCursor cursor )
		 {
			  ( ( StubGroupCursor ) cursor ).Rewind();
		 }

		 public override void AllRelationships( RelationshipTraversalCursor relationships )
		 {
			  ( ( StubRelationshipCursor ) relationships ).Rewind();
		 }

		 public override void Properties( PropertyCursor cursor )
		 {
			  ( ( StubPropertyCursor ) cursor ).Init( _nodes[_offset].properties );
		 }

		 public override long RelationshipGroupReference()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long AllRelationshipsReference()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long PropertiesReference()
		 {
			  if ( _offset >= 0 && _offset < _nodes.Count )
			  {
					NodeData node = _nodes[_offset];
					if ( node.Properties.Count > 0 )
					{
						 return node.Id;
					}
			  }
			  return -1;
		 }

		 public virtual bool Dense
		 {
			 get
			 {
				  return _dense;
			 }
		 }

		 public override bool Next()
		 {
			  if ( _offset == int.MaxValue )
			  {
					return false;
			  }
			  return ++_offset < _nodes.Count;
		 }

		 public override void Close()
		 {

		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return false;
			 }
		 }

	}

}