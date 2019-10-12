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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using RelationshipExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipExplicitIndexCursor;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexProgressor_ExplicitClient = Org.Neo4j.Storageengine.Api.schema.IndexProgressor_ExplicitClient;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultRelationshipExplicitIndexCursor : IndexCursor<IndexProgressor>, RelationshipExplicitIndexCursor, IndexProgressor_ExplicitClient
	{
		 private Read _read;
		 private int _expectedSize;
		 private long _relationship;
		 private float _score;
		 private readonly DefaultRelationshipScanCursor _scanCursor;

		 private readonly DefaultCursors _pool;

		 internal DefaultRelationshipExplicitIndexCursor( DefaultRelationshipScanCursor scanCursor, DefaultCursors pool )
		 {
			  this._scanCursor = scanCursor;
			  this._pool = pool;
		 }

		 public override void Initialize( IndexProgressor progressor, int expectedSize )
		 {
			  base.Initialize( progressor );
			  this._expectedSize = expectedSize;
		 }

		 public override bool AcceptEntity( long reference, float score )
		 {
			  this._relationship = reference;
			  _read.singleRelationship( reference, _scanCursor );
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

		 public override void Relationship( RelationshipScanCursor cursor )
		 {
			  _read.singleRelationship( _relationship, cursor );
		 }

		 public override void SourceNode( NodeCursor cursor )
		 {
			  _read.singleNode( SourceNodeReference(), cursor );
		 }

		 public override void TargetNode( NodeCursor cursor )
		 {
			  _read.singleNode( TargetNodeReference(), cursor );
		 }

		 public override int Type()
		 {
			  return _scanCursor.type();
		 }

		 public override long SourceNodeReference()
		 {
			  return _scanCursor.sourceNodeReference();
		 }

		 public override long TargetNodeReference()
		 {
			  return _scanCursor.targetNodeReference();
		 }

		 public override long RelationshipReference()
		 {
			  return _relationship;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					base.Close();
					_relationship = NO_ID;
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
					return "RelationshipExplicitIndexCursor[closed state]";
			  }
			  else
			  {
					return "RelationshipExplicitIndexCursor[relationship=" + _relationship + ", expectedSize=" + _expectedSize + ", score=" + _score +
							  " ,underlying record=" + base.ToString() + "]";
			  }
		 }

		 public virtual void Release()
		 {
			  _scanCursor.release();
		 }
	}

}