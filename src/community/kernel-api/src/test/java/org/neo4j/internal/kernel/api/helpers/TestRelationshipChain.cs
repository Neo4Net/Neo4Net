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

	public class TestRelationshipChain
	{
		 private IList<Data> _data;
		 private long _originNodeId;

		 public TestRelationshipChain( long originNodeId ) : this( originNodeId, new List<Data>() )
		 {
		 }

		 private TestRelationshipChain( long originNodeId, IList<Data> data )
		 {
			  this._originNodeId = originNodeId;
			  this._data = data;
		 }

		 public virtual TestRelationshipChain Outgoing( long id, long targetNode, int type )
		 {
			  _data.Add( new Data( id, _originNodeId, targetNode, type ) );
			  return this;
		 }

		 public virtual TestRelationshipChain Incoming( long id, long sourceNode, int type )
		 {
			  _data.Add( new Data( id, sourceNode, _originNodeId, type ) );
			  return this;
		 }

		 public virtual TestRelationshipChain Loop( long id, int type )
		 {
			  _data.Add( new Data( id, _originNodeId, _originNodeId, type ) );
			  return this;
		 }

		 public virtual Data Get( int offset )
		 {
			  return _data[offset];
		 }

		 internal virtual bool IsValidOffset( int offset )
		 {
			  return offset >= 0 && offset < _data.Count;
		 }

		 internal virtual long OriginNodeId()
		 {
			  return _originNodeId;
		 }

		 public virtual TestRelationshipChain Tail()
		 {
			  return new TestRelationshipChain( _originNodeId, _data.subList( 1, _data.Count ) );
		 }

		 internal class Data
		 {
			  internal readonly long Id;
			  internal readonly long Source;
			  internal readonly long Target;
			  internal readonly int Type;

			  internal Data( long id, long source, long target, int type )
			  {
					this.Id = id;
					this.Source = source;
					this.Target = target;
					this.Type = type;
			  }
		 }
	}

}