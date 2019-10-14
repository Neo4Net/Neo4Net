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


	public class StubRelationshipCursor : RelationshipTraversalCursor
	{
		 private readonly IList<TestRelationshipChain> _store;

		 private int _offset;
		 private int _chainId;
		 private bool _isClosed;

		 public StubRelationshipCursor( TestRelationshipChain chain ) : this( Collections.singletonList( chain ) )
		 {
		 }

		 internal StubRelationshipCursor( IList<TestRelationshipChain> store )
		 {
			  this._store = store;
			  this._chainId = 0;
			  this._offset = -1;
			  this._isClosed = true;
		 }

		 internal virtual void Rewind()
		 {
			  this._offset = -1;
			  this._isClosed = true;
		 }

		 internal virtual void Read( int chainId )
		 {
			  this._chainId = chainId;
			  Rewind();
		 }

		 public override long RelationshipReference()
		 {
			  return _store[_chainId].get( _offset ).id;
		 }

		 public override int Type()
		 {
			  return _store[_chainId].get( _offset ).type;
		 }

		 public override void Source( NodeCursor cursor )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Target( NodeCursor cursor )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Properties( PropertyCursor cursor )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long SourceNodeReference()
		 {
			  return _store[_chainId].get( _offset ).source;
		 }

		 public override long TargetNodeReference()
		 {
			  return _store[_chainId].get( _offset ).target;
		 }

		 public override long PropertiesReference()
		 {
			  return -1;
		 }

		 public override Neo4Net.Internal.Kernel.Api.RelationshipTraversalCursor_Position Suspend()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Resume( Neo4Net.Internal.Kernel.Api.RelationshipTraversalCursor_Position position )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Neighbour( NodeCursor cursor )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long NeighbourNodeReference()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long OriginNodeReference()
		 {
			  return _store[_chainId].originNodeId();
		 }

		 public override bool Next()
		 {
			  _offset++;
			  return _store[_chainId].isValidOffset( _offset );
		 }

		 public override void Close()
		 {
			  _isClosed = true;
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return _isClosed;
			 }
		 }
	}

}