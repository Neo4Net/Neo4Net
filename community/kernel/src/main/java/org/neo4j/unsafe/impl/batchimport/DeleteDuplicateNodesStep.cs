﻿using System.Diagnostics;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using LonelyProcessingStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.LonelyProcessingStep;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.PropertyDeleter.deletePropertyRecordIncludingValueRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class DeleteDuplicateNodesStep : LonelyProcessingStep
	{
		 private readonly NodeStore _nodeStore;
		 private readonly PropertyStore _propertyStore;
		 private readonly LongIterator _nodeIds;
		 private readonly DataImporter.Monitor _storeMonitor;

		 private long _nodesRemoved;
		 private long _propertiesRemoved;

		 public DeleteDuplicateNodesStep( StageControl control, Configuration config, LongIterator nodeIds, NodeStore nodeStore, PropertyStore propertyStore, DataImporter.Monitor storeMonitor ) : base( control, "DEDUP", config )
		 {
			  this._nodeStore = nodeStore;
			  this._propertyStore = propertyStore;
			  this._nodeIds = nodeIds;
			  this._storeMonitor = storeMonitor;
		 }

		 protected internal override void Process()
		 {
			  NodeRecord nodeRecord = _nodeStore.newRecord();
			  PropertyRecord propertyRecord = _propertyStore.newRecord();
			  using ( PageCursor cursor = _nodeStore.openPageCursorForReading( 0 ), PageCursor propertyCursor = _propertyStore.openPageCursorForReading( 0 ) )
			  {
					while ( _nodeIds.hasNext() )
					{
						 long duplicateNodeId = _nodeIds.next();
						 _nodeStore.getRecordByCursor( duplicateNodeId, nodeRecord, NORMAL, cursor );
						 Debug.Assert( nodeRecord.InUse(), nodeRecord );
						 // Ensure heavy so that the dynamic label records gets loaded (and then deleted) too
						 _nodeStore.ensureHeavy( nodeRecord );

						 // Delete property records
						 long nextProp = nodeRecord.NextProp;
						 while ( !Record.NULL_REFERENCE.@is( nextProp ) )
						 {
							  _propertyStore.getRecordByCursor( nextProp, propertyRecord, NORMAL, propertyCursor );
							  Debug.Assert( propertyRecord.InUse(), propertyRecord + " for " + nodeRecord );
							  _propertyStore.ensureHeavy( propertyRecord );
							  _propertiesRemoved += propertyRecord.NumberOfProperties();
							  nextProp = propertyRecord.NextProp;
							  deletePropertyRecordIncludingValueRecords( propertyRecord );
							  _propertyStore.updateRecord( propertyRecord );
						 }

						 // Delete node (and dynamic label records, if any)
						 nodeRecord.InUse = false;
						 foreach ( DynamicRecord labelRecord in nodeRecord.DynamicLabelRecords )
						 {
							  labelRecord.InUse = false;
						 }
						 _nodeStore.updateRecord( nodeRecord );
						 _nodesRemoved++;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  base.Close();
			  _storeMonitor.nodesRemoved( _nodesRemoved );
			  _storeMonitor.propertiesRemoved( _propertiesRemoved );
		 }
	}

}