﻿/*
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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;

	/// <summary>
	/// Scans <seealso cref="RelationshipGroupRecord group records"/> and discovers which should affect the owners.
	/// </summary>
	public class NodeSetFirstGroupStep : ProcessorStep<RelationshipGroupRecord[]>
	{
		 private readonly int _batchSize;
		 private readonly ByteArray _cache;
		 private readonly NodeStore _nodeStore;
		 private readonly PageCursor _nodeCursor;

		 private NodeRecord[] _current;
		 private int _cursor;

		 internal NodeSetFirstGroupStep( StageControl control, Configuration config, NodeStore nodeStore, ByteArray cache ) : base( control, "FIRST", config, 1 )
		 {
			  this._cache = cache;
			  this._batchSize = config.BatchSize();
			  this._nodeStore = nodeStore;
			  this._nodeCursor = nodeStore.OpenPageCursorForReading( 0 );
			  NewBatch();
		 }

		 public override void Start( int orderingGuarantees )
		 {
			  base.Start( orderingGuarantees );
		 }

		 private void NewBatch()
		 {
			  _current = new NodeRecord[_batchSize];
			  _cursor = 0;
		 }

		 protected internal override void Process( RelationshipGroupRecord[] batch, BatchSender sender )
		 {
			  foreach ( RelationshipGroupRecord group in batch )
			  {
					if ( !group.InUse() )
					{
						 continue;
					}

					long nodeId = group.OwningNode;
					if ( _cache.getByte( nodeId, 0 ) == 0 )
					{
						 _cache.setByte( nodeId, 0, ( sbyte ) 1 );
						 NodeRecord nodeRecord = _nodeStore.newRecord();
						 _nodeStore.getRecordByCursor( nodeId, nodeRecord, NORMAL, _nodeCursor );
						 nodeRecord.NextRel = group.Id;
						 nodeRecord.Dense = true;

						 _current[_cursor++] = nodeRecord;
						 if ( _cursor == _batchSize )
						 {
							  sender.Send( _current );
							  NewBatch();
						 }
					}
			  }
			  control.recycle( batch );
		 }

		 protected internal override void LastCallForEmittingOutstandingBatches( BatchSender sender )
		 {
			  if ( _cursor > 0 )
			  {
					sender.Send( _current );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _nodeCursor.close();
			  base.Close();
		 }
	}

}