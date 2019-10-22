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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexProgressor_ExplicitClient = Neo4Net.Storageengine.Api.schema.IndexProgressor_ExplicitClient;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultNodeExplicitIndexCursor : IndexCursor<IndexProgressor>, Neo4Net.Internal.Kernel.Api.NodeExplicitIndexCursor, IndexProgressor_ExplicitClient
	{
		 private Read _read;
		 private int _expectedSize;
		 private long _node;
		 private float _score;

		 private readonly DefaultCursors _pool;

		 internal DefaultNodeExplicitIndexCursor( DefaultCursors pool )
		 {
			  this._pool = pool;
			  _node = NO_ID;
		 }

		 public override void Initialize( IndexProgressor progressor, int expectedSize )
		 {
			  base.Initialize( progressor );
			  this._expectedSize = expectedSize;
		 }

		 public override bool AcceptEntity( long reference, float score )
		 {
			  this._node = reference;
			  this._score = score;
			  return true;
		 }

		 public override bool Next()
		 {
			  return InnerNext();
		 }

		 public virtual Read Read
		 {
			 set
			 {
				  this._read = value;
			 }
		 }

		 public override int ExpectedTotalNumberOfResults()
		 {
			  return _expectedSize;
		 }

		 public override float Score()
		 {
			  return _score;
		 }

		 public override void Node( NodeCursor cursor )
		 {
			  _read.singleNode( _node, cursor );
		 }

		 public override long NodeReference()
		 {
			  return _node;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					base.Close();
					_node = NO_ID;
					_score = 0;
					_expectedSize = 0;
					_read = null;

					_pool.accept( this );
			  }
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return base.Closed;
			 }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "NodeExplicitIndexCursor[closed state]";
			  }
			  else
			  {
					return "NodeExplicitIndexCursor[node=" + _node + ", expectedSize=" + _expectedSize + ", score=" + _score +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 public virtual void Release()
		 {
			  // nothing to do
		 }
	}

}