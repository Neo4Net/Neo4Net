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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using CommandsToApply = Neo4Net.Storageengine.Api.CommandsToApply;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.fieldPointsToDynamicRecordOfLabels;

	/// <summary>
	/// Implements both BatchTransactionApplier and TransactionApplier in order to reduce garbage.
	/// Gathers node/property commands by node id, preparing for extraction of <seealso cref="EntityUpdates updates"/>.
	/// </summary>
	public class PropertyCommandsExtractor : Neo4Net.Kernel.Impl.Api.TransactionApplier_Adapter, BatchTransactionApplier
	{
		 private readonly EntityCommandGrouper<NodeCommand> _nodeCommands = new EntityCommandGrouper<NodeCommand>( typeof( NodeCommand ), 16 );
		 private readonly EntityCommandGrouper<RelationshipCommand> _relationshipCommands = new EntityCommandGrouper<RelationshipCommand>( typeof( RelationshipCommand ), 16 );
		 private bool _hasUpdates;

		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  return this;
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup )
		 {
			  return StartTx( transaction );
		 }

		 public override void Close()
		 {
			  _nodeCommands.clear();
			  _relationshipCommands.clear();
		 }

		 public override bool VisitNodeCommand( NodeCommand command )
		 {
			  _nodeCommands.add( command );
			  if ( !_hasUpdates && MayResultInIndexUpdates( command ) )
			  {
					_hasUpdates = true;
			  }
			  return false;
		 }

		 public override bool VisitRelationshipCommand( RelationshipCommand command )
		 {
			  _relationshipCommands.add( command );
			  _hasUpdates = true;
			  return false;
		 }

		 private static bool MayResultInIndexUpdates( NodeCommand command )
		 {
			  long before = command.Before.LabelField;
			  long after = command.After.LabelField;
			  return before != after || fieldPointsToDynamicRecordOfLabels( before ) || fieldPointsToDynamicRecordOfLabels( after );
		 }

		 public override bool VisitPropertyCommand( PropertyCommand command )
		 {
			  if ( command.After.NodeSet )
			  {
					_nodeCommands.add( command );
					_hasUpdates = true;
			  }
			  else if ( command.After.RelSet )
			  {
					_relationshipCommands.add( command );
					_hasUpdates = true;
			  }
			  return false;
		 }

		 public virtual bool ContainsAnyEntityOrPropertyUpdate()
		 {
			  return _hasUpdates;
		 }

		 public virtual EntityCommandGrouper<NodeCommand>.Cursor NodeCommands
		 {
			 get
			 {
				  return _nodeCommands.sortAndAccessGroups();
			 }
		 }

		 public virtual EntityCommandGrouper<RelationshipCommand>.Cursor RelationshipCommands
		 {
			 get
			 {
				  return _relationshipCommands.sortAndAccessGroups();
			 }
		 }
	}

}