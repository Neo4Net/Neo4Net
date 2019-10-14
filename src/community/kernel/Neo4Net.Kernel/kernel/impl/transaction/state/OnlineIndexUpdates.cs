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
namespace Neo4Net.Kernel.impl.transaction.state
{

	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using Neo4Net.Kernel.Api.Index;
	using Neo4Net.Kernel.Impl.Api.index;
	using EntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using PropertyPhysicalToLogicalConverter = Neo4Net.Kernel.Impl.Api.index.PropertyPhysicalToLogicalConverter;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Command.Mode.CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Command.Mode.DELETE;

	/// <summary>
	/// Derives logical index updates from physical records, provided by <seealso cref="NodeCommand node commands"/>,
	/// <seealso cref="RelationshipCommand relationship commands"/> and <seealso cref="PropertyCommand property commands"/>. For some
	/// types of updates state from store is also needed, for example if adding a label to a node which already has
	/// properties matching existing and online indexes; in that case the properties for that node needs to be read
	/// from store since the commands in that transaction cannot itself provide enough information.
	/// 
	/// One instance can be <seealso cref="IndexUpdates.feed(EntityCommandGrouper.Cursor,EntityCommandGrouper.Cursor) fed"/> data about
	/// multiple transactions, to be <seealso cref="iterator() accessed"/> later.
	/// </summary>
	public class OnlineIndexUpdates : IndexUpdates
	{
		 private readonly NodeStore _nodeStore;
		 private readonly RelationshipStore _relationshipStore;
		 private readonly IndexingUpdateService _updateService;
		 private readonly PropertyPhysicalToLogicalConverter _converter;
		 private readonly ICollection<IndexEntryUpdate<SchemaDescriptor>> _updates = new List<IndexEntryUpdate<SchemaDescriptor>>();
		 private NodeRecord _nodeRecord;
		 private RelationshipRecord _relationshipRecord;

		 public OnlineIndexUpdates( NodeStore nodeStore, RelationshipStore relationshipStore, IndexingUpdateService updateService, PropertyPhysicalToLogicalConverter converter )
		 {
			  this._nodeStore = nodeStore;
			  this._relationshipStore = relationshipStore;
			  this._updateService = updateService;
			  this._converter = converter;
		 }

		 public override IEnumerator<IndexEntryUpdate<SchemaDescriptor>> Iterator()
		 {
			  return _updates.GetEnumerator();
		 }

		 public override void Feed( EntityCommandGrouper<Command.NodeCommand>.Cursor nodeCommands, EntityCommandGrouper<Command.RelationshipCommand>.Cursor relationshipCommands )
		 {
			  while ( nodeCommands.nextEntity() )
			  {
					GatherUpdatesFor( nodeCommands.currentEntityId(), nodeCommands.currentEntityCommand(), nodeCommands );
			  }
			  while ( relationshipCommands.nextEntity() )
			  {
					GatherUpdatesFor( relationshipCommands.currentEntityId(), relationshipCommands.currentEntityCommand(), relationshipCommands );
			  }
		 }

		 public override bool HasUpdates()
		 {
			  return _updates.Count > 0;
		 }

		 private void GatherUpdatesFor( long nodeId, Command.NodeCommand nodeCommand, EntityCommandGrouper<Command.NodeCommand>.Cursor propertyCommands )
		 {
			  EntityUpdates.Builder nodePropertyUpdate = GatherUpdatesFromCommandsForNode( nodeId, nodeCommand, propertyCommands );

			  EntityUpdates entityUpdates = nodePropertyUpdate.Build();
			  // we need to materialize the IndexEntryUpdates here, because when we
			  // consume (later in separate thread) the store might have changed.
			  foreach ( IndexEntryUpdate<SchemaDescriptor> update in _updateService.convertToIndexUpdates( entityUpdates, EntityType.NODE ) )
			  {
					_updates.Add( update );
			  }
		 }

		 private void GatherUpdatesFor( long relationshipId, Command.RelationshipCommand relationshipCommand, EntityCommandGrouper<Command.RelationshipCommand>.Cursor propertyCommands )
		 {
			  EntityUpdates.Builder relationshipPropertyUpdate = GatherUpdatesFromCommandsForRelationship( relationshipId, relationshipCommand, propertyCommands );

			  EntityUpdates entityUpdates = relationshipPropertyUpdate.Build();
			  // we need to materialize the IndexEntryUpdates here, because when we
			  // consume (later in separate thread) the store might have changed.
			  foreach ( IndexEntryUpdate<SchemaDescriptor> update in _updateService.convertToIndexUpdates( entityUpdates, EntityType.RELATIONSHIP ) )
			  {
					_updates.Add( update );
			  }
		 }

		 private EntityUpdates.Builder GatherUpdatesFromCommandsForNode( long nodeId, Command.NodeCommand nodeChanges, EntityCommandGrouper<Command.NodeCommand>.Cursor propertyCommandsForNode )
		 {
			  long[] nodeLabelsBefore;
			  long[] nodeLabelsAfter;
			  if ( nodeChanges != null )
			  {
					nodeLabelsBefore = parseLabelsField( nodeChanges.Before ).get( _nodeStore );
					nodeLabelsAfter = parseLabelsField( nodeChanges.After ).get( _nodeStore );
			  }
			  else
			  {
					/* If the node doesn't exist here then we've most likely encountered this scenario:
					 * - TX1: Node N exists and has property record P
					 * - rotate log
					 * - TX2: P gets changed
					 * - TX3: N gets deleted (also P, but that's irrelevant for this scenario)
					 * - N is persisted to disk for some reason
					 * - crash
					 * - recover
					 * - TX2: P has changed and updates to indexes are gathered. As part of that it tries to read
					 *        the labels of N (which does not exist a.t.m.).
					 *
					 * We can actually (if we disregard any potential inconsistencies) just assume that
					 * if this happens and we're in recovery mode that the node in question will be deleted
					 * in an upcoming transaction, so just skip this update.
					 */
					NodeRecord nodeRecord = LoadNode( nodeId );
					nodeLabelsBefore = nodeLabelsAfter = parseLabelsField( nodeRecord ).get( _nodeStore );
			  }

			  // First get possible Label changes
			  bool complete = ProvidesCompleteListOfProperties( nodeChanges );
			  EntityUpdates.Builder nodePropertyUpdates = EntityUpdates.forEntity( nodeId, complete ).withTokens( nodeLabelsBefore ).withTokensAfter( nodeLabelsAfter );

			  // Then look for property changes
			  _converter.convertPropertyRecord( propertyCommandsForNode, nodePropertyUpdates );
			  return nodePropertyUpdates;
		 }

		 private static bool ProvidesCompleteListOfProperties( Command entityCommand )
		 {
			  return entityCommand != null && ( entityCommand.GetMode() == CREATE || entityCommand.GetMode() == DELETE );
		 }

		 private EntityUpdates.Builder GatherUpdatesFromCommandsForRelationship( long relationshipId, Command.RelationshipCommand relationshipCommand, EntityCommandGrouper<Command.RelationshipCommand>.Cursor propertyCommands )
		 {
			  long reltypeBefore;
			  long reltypeAfter;
			  if ( relationshipCommand != null )
			  {
					reltypeBefore = relationshipCommand.Before.Type;
					reltypeAfter = relationshipCommand.After.Type;
			  }
			  else
			  {
					RelationshipRecord relationshipRecord = LoadRelationship( relationshipId );
					reltypeBefore = reltypeAfter = relationshipRecord.Type;
			  }
			  bool complete = ProvidesCompleteListOfProperties( relationshipCommand );
			  EntityUpdates.Builder relationshipPropertyUpdates = EntityUpdates.forEntity( relationshipId, complete ).withTokens( reltypeBefore ).withTokensAfter( reltypeAfter );
			  _converter.convertPropertyRecord( propertyCommands, relationshipPropertyUpdates );
			  return relationshipPropertyUpdates;
		 }

		 private NodeRecord LoadNode( long nodeId )
		 {
			  if ( _nodeRecord == null )
			  {
					_nodeRecord = _nodeStore.newRecord();
			  }
			  _nodeStore.getRecord( nodeId, _nodeRecord, RecordLoad.NORMAL );
			  return _nodeRecord;
		 }

		 private RelationshipRecord LoadRelationship( long relationshipId )
		 {
			  if ( _relationshipRecord == null )
			  {
					_relationshipRecord = _relationshipStore.newRecord();
			  }
			  _relationshipStore.getRecord( relationshipId, _relationshipRecord, RecordLoad.NORMAL );
			  return _relationshipRecord;
		 }
	}

}