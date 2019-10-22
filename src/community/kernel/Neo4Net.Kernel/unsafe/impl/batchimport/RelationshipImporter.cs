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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using Monitor = Neo4Net.@unsafe.Impl.Batchimport.DataImporter.Monitor;
	using Client = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.Client;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using MissingRelationshipDataException = Neo4Net.@unsafe.Impl.Batchimport.input.MissingRelationshipDataException;
	using Type = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Type;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;
	using BatchingRelationshipTypeTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingRelationshipTypeTokenRepository;
	using PrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.PrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.idmapping.IdMapper_Fields.ID_NOT_FOUND;

	/// <summary>
	/// Imports relationships using data from <seealso cref="InputChunk"/>.
	/// </summary>
	public class RelationshipImporter : IEntityImporter
	{
		 private readonly BatchingRelationshipTypeTokenRepository _relationshipTypeTokenRepository;
		 private readonly IdMapper _idMapper;
		 private readonly RelationshipStore _relationshipStore;
		 private readonly RelationshipRecord _relationshipRecord;
		 private readonly BatchingIdGetter _relationshipIds;
		 private readonly Client _typeCounts;
		 private readonly Collector _badCollector;
		 private readonly bool _validateRelationshipData;
		 private readonly bool _doubleRecordUnits;
		 private readonly System.Func<long, IdSequence> _prepareIdSequence;

		 private long _relationshipCount;

		 // State to keep in the event of bad relationships that need to be handed to the Collector
		 private object _startId;
		 private Group _startIdGroup;
		 private object _endId;
		 private Group _endIdGroup;
		 private string _type;

		 protected internal RelationshipImporter( BatchingNeoStores stores, IdMapper idMapper, DataStatistics typeDistribution, Monitor monitor, Collector badCollector, bool validateRelationshipData, bool doubleRecordUnits ) : base( stores, monitor )
		 {
			  this._doubleRecordUnits = doubleRecordUnits;
			  this._relationshipTypeTokenRepository = stores.RelationshipTypeRepository;
			  this._idMapper = idMapper;
			  this._badCollector = badCollector;
			  this._validateRelationshipData = validateRelationshipData;
			  this._relationshipStore = stores.RelationshipStore;
			  this._relationshipRecord = _relationshipStore.newRecord();
			  this._relationshipIds = new BatchingIdGetter( _relationshipStore );
			  this._typeCounts = typeDistribution.NewClient();
			  this._prepareIdSequence = PrepareIdSequence.of( doubleRecordUnits ).apply( stores.RelationshipStore );
			  _relationshipRecord.InUse = true;
		 }

		 protected internal override PrimitiveRecord PrimitiveRecord()
		 {
			  return _relationshipRecord;
		 }

		 public override bool StartId( long id )
		 {
			  _relationshipRecord.FirstNode = id;
			  return true;
		 }

		 public override bool StartId( object id, Group group )
		 {
			  this._startId = id;
			  this._startIdGroup = group;

			  long nodeId = nodeId( id, group );
			  _relationshipRecord.FirstNode = nodeId;
			  return true;
		 }

		 public override bool EndId( long id )
		 {
			  _relationshipRecord.SecondNode = id;
			  return true;
		 }

		 public override bool EndId( object id, Group group )
		 {
			  this._endId = id;
			  this._endIdGroup = group;

			  long nodeId = nodeId( id, group );
			  _relationshipRecord.SecondNode = nodeId;
			  return true;
		 }

		 private long NodeId( object id, Group group )
		 {
			  long nodeId = _idMapper.get( id, group );
			  if ( nodeId == ID_NOT_FOUND )
			  {
					_relationshipRecord.InUse = false;
					return ID_NOT_FOUND;
			  }

			  return nodeId;
		 }

		 public override bool Type( int typeId )
		 {
			  _relationshipRecord.Type = typeId;
			  return true;
		 }

		 public override bool Type( string type )
		 {
			  this._type = type;
			  int typeId = _relationshipTypeTokenRepository.getOrCreateId( type );
			  return type( typeId );
		 }

		 public override void EndOfEntity()
		 {
			  if ( _relationshipRecord.inUse() && _relationshipRecord.FirstNode != ID_NOT_FOUND && _relationshipRecord.SecondNode != ID_NOT_FOUND && _relationshipRecord.Type != -1 )
			  {
					_relationshipRecord.Id = _relationshipIds.next();
					if ( _doubleRecordUnits )
					{
						 // simply reserve one id for this relationship to grow during linking stage
						 _relationshipIds.next();
					}
					_relationshipRecord.NextProp = CreateAndWritePropertyChain();
					_relationshipRecord.FirstInFirstChain = false;
					_relationshipRecord.FirstInSecondChain = false;
					_relationshipRecord.FirstPrevRel = Record.NO_NEXT_RELATIONSHIP.intValue();
					_relationshipRecord.SecondPrevRel = Record.NO_NEXT_RELATIONSHIP.intValue();
					_relationshipStore.prepareForCommit( _relationshipRecord, _prepareIdSequence.apply( _relationshipRecord.Id ) );
					_relationshipStore.updateRecord( _relationshipRecord );
					_relationshipCount++;
					_typeCounts.increment( _relationshipRecord.Type );
			  }
			  else
			  {
					if ( _validateRelationshipData )
					{
						 ValidateNode( _startId, Type.START_ID );
						 ValidateNode( _endId, Type.END_ID );
						 if ( _relationshipRecord.Type == -1 )
						 {
							  throw new MissingRelationshipDataException( Type.TYPE, RelationshipDataString() + " is missing " + Type.TYPE + " field" );
						 }
					}
					_badCollector.collectBadRelationship( _startId, Group( _startIdGroup ).name(), _type, _endId, Group(_endIdGroup).name(), _relationshipRecord.FirstNode == ID_NOT_FOUND ? _startId : _endId );
					EntityPropertyCount = 0;
			  }

			  _relationshipRecord.clear();
			  _relationshipRecord.InUse = true;
			  _startId = null;
			  _startIdGroup = null;
			  _endId = null;
			  _endIdGroup = null;
			  _type = null;
			  base.EndOfEntity();
		 }

		 private Group Group( Group group )
		 {
			  return group != null ? group : Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global;
		 }

		 private void ValidateNode( object id, Type fieldType )
		 {
			  if ( id == null )
			  {
					throw new MissingRelationshipDataException( fieldType, RelationshipDataString() + " is missing " + fieldType + " field" );
			  }
		 }

		 private string RelationshipDataString()
		 {
			  return format( "start:%s (%s) type:%s end:%s (%s)", _startId, Group( _startIdGroup ).name(), _type, _endId, Group(_endIdGroup).name() );
		 }

		 public override void Close()
		 {
			  base.Dispose();
			  _typeCounts.close();
			  Monitor.relationshipsImported( _relationshipCount );
		 }
	}

}