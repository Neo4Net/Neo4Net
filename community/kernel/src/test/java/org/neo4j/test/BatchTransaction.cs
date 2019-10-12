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
namespace Org.Neo4j.Test
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;

	public class BatchTransaction : AutoCloseable
	{
		 private const int DEFAULT_INTERMEDIARY_SIZE = 10000;

		 public static BatchTransaction BeginBatchTx( GraphDatabaseService db )
		 {
			  return new BatchTransaction( db );
		 }

		 private readonly GraphDatabaseService _db;
		 private Transaction _tx;
		 private int _txSize;
		 private int _total;
		 private int _intermediarySize = DEFAULT_INTERMEDIARY_SIZE;
		 private ProgressListener _progressListener = Org.Neo4j.Helpers.progress.ProgressListener_Fields.None;

		 private BatchTransaction( GraphDatabaseService db )
		 {
			  this._db = db;
			  BeginTx();
		 }

		 private void BeginTx()
		 {
			  this._tx = _db.beginTx();
		 }

		 public virtual GraphDatabaseService Db
		 {
			 get
			 {
				  return _db;
			 }
		 }

		 public virtual bool Increment()
		 {
			  return Increment( 1 );
		 }

		 public virtual bool Increment( int count )
		 {
			  _txSize += count;
			  _total += count;
			  _progressListener.add( count );
			  if ( _txSize >= _intermediarySize )
			  {
					_txSize = 0;
					IntermediaryCommit();
					return true;
			  }
			  return false;
		 }

		 public virtual void IntermediaryCommit()
		 {
			  CloseTx();
			  BeginTx();
		 }

		 private void CloseTx()
		 {
			  _tx.success();
			  _tx.close();
		 }

		 public override void Close()
		 {
			  CloseTx();
			  _progressListener.done();
		 }

		 public virtual int Total()
		 {
			  return _total;
		 }

		 public virtual BatchTransaction WithIntermediarySize( int intermediarySize )
		 {
			  this._intermediarySize = intermediarySize;
			  return this;
		 }

		 public virtual BatchTransaction WithProgress( ProgressListener progressListener )
		 {
			  this._progressListener = progressListener;
			  this._progressListener.started();
			  return this;
		 }
	}

}