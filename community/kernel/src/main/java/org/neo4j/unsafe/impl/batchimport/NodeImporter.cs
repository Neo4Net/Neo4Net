﻿using System;
using System.Diagnostics;

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

	using InlineNodeLabels = Org.Neo4j.Kernel.impl.store.InlineNodeLabels;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Monitor = Org.Neo4j.@unsafe.Impl.Batchimport.DataImporter.Monitor;
	using IdMapper = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Group = Org.Neo4j.@unsafe.Impl.Batchimport.input.Group;
	using InputChunk = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputChunk;
	using BatchingNeoStores = Org.Neo4j.@unsafe.Impl.Batchimport.store.BatchingNeoStores;
	using BatchingLabelTokenRepository = Org.Neo4j.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingLabelTokenRepository;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOf;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NULL_REFERENCE;

	/// <summary>
	/// Imports nodes using data from <seealso cref="InputChunk"/>.
	/// </summary>
	public class NodeImporter : EntityImporter
	{
		 private readonly BatchingLabelTokenRepository _labelTokenRepository;
		 private readonly NodeStore _nodeStore;
		 private readonly NodeRecord _nodeRecord;
		 private readonly IdMapper _idMapper;
		 private readonly BatchingIdGetter _nodeIds;
		 private readonly PropertyStore _idPropertyStore;
		 private readonly PropertyRecord _idPropertyRecord;
		 private readonly PropertyBlock _idPropertyBlock = new PropertyBlock();
		 private string[] _labels = new string[10];
		 private int _labelsCursor;

		 private long _nodeCount;
		 private long _highestId = -1;
		 private bool _hasLabelField;

		 public NodeImporter( BatchingNeoStores stores, IdMapper idMapper, Monitor monitor ) : base( stores, monitor )
		 {
			  this._labelTokenRepository = stores.LabelRepository;
			  this._idMapper = idMapper;
			  this._nodeStore = stores.NodeStore;
			  this._nodeRecord = _nodeStore.newRecord();
			  this._nodeIds = new BatchingIdGetter( _nodeStore );
			  this._idPropertyStore = stores.TemporaryPropertyStore;
			  this._idPropertyRecord = _idPropertyStore.newRecord();
			  _nodeRecord.InUse = true;
		 }

		 public override bool Id( long id )
		 {
			  _nodeRecord.Id = id;
			  _highestId = max( _highestId, id );
			  return true;
		 }

		 public override bool Id( object id, Group group )
		 {
			  long nodeId = _nodeIds.next();
			  _nodeRecord.Id = nodeId;
			  _idMapper.put( id, nodeId, group );

			  // also store this id as property in temp property store
			  if ( id != null )
			  {
					_idPropertyStore.encodeValue( _idPropertyBlock, 0, Values.of( id ) );
					_idPropertyRecord.addPropertyBlock( _idPropertyBlock );
					_idPropertyRecord.Id = nodeId; // yes nodeId
					_idPropertyRecord.InUse = true;
					_idPropertyStore.updateRecord( _idPropertyRecord );
					_idPropertyRecord.clear();
			  }
			  return true;
		 }

		 public override bool Labels( string[] labels )
		 {
			  Debug.Assert( !_hasLabelField );
			  if ( _labelsCursor + labels.Length > this._labels.Length )
			  {
					this._labels = copyOf( this._labels, this._labels.Length * 2 );
			  }
			  Array.Copy( labels, 0, this._labels, _labelsCursor, labels.Length );
			  _labelsCursor += labels.Length;
			  return true;
		 }

		 public override bool LabelField( long labelField )
		 {
			  _hasLabelField = true;
			  _nodeRecord.setLabelField( labelField, Collections.emptyList() );
			  return true;
		 }

		 public override void EndOfEntity()
		 {
			  // Make sure we have an ID
			  if ( _nodeRecord.Id == NULL_REFERENCE.longValue() )
			  {
					_nodeRecord.Id = _nodeIds.next();
			  }

			  // Compose the labels
			  if ( !_hasLabelField )
			  {
					long[] labelIds = _labelTokenRepository.getOrCreateIds( _labels, _labelsCursor );
					InlineNodeLabels.putSorted( _nodeRecord, labelIds, null, _nodeStore.DynamicLabelStore );
			  }
			  _labelsCursor = 0;

			  // Write data to stores
			  _nodeRecord.NextProp = CreateAndWritePropertyChain();
			  _nodeRecord.InUse = true;
			  _nodeStore.updateRecord( _nodeRecord );
			  _nodeCount++;
			  _nodeRecord.clear();
			  _nodeRecord.Id = NULL_REFERENCE.longValue();
			  _hasLabelField = false;
			  base.EndOfEntity();
		 }

		 protected internal override PrimitiveRecord PrimitiveRecord()
		 {
			  return _nodeRecord;
		 }

		 public override void Close()
		 {
			  base.Dispose();
			  Monitor.nodesImported( _nodeCount );
			  _nodeStore.HighestPossibleIdInUse = _highestId; // for the case of #id(long)
		 }
	}

}