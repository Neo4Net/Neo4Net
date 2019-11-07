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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;


	using IDatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using CommandVisitor = Neo4Net.Kernel.Impl.Api.CommandVisitor;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state.RecordChanges;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class WriteTransactionCommandOrderingTest
	{
		 private static NodeRecord MissingNode()
		 {
			  return new NodeRecord( -1, false, -1, -1 );
		 }

		 private static NodeRecord CreatedNode()
		 {
			  NodeRecord record = new NodeRecord( 2, false, -1, -1 );
			  record.InUse = true;
			  record.SetCreated();
			  return record;
		 }

		 private static NodeRecord InUseNode()
		 {
			  NodeRecord record = new NodeRecord( 1, false, -1, -1 );
			  record.InUse = true;
			  return record;
		 }

		 private static string CommandActionToken( AbstractBaseRecord record )
		 {
			  if ( !record.InUse() )
			  {
					return "deleted";
			  }
			  if ( record.Created )
			  {
					return "created";
			  }
			  return "updated";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteCommandsInTheSameOrderRegardlessOfItBeingRecoveredOrNot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExecuteCommandsInTheSameOrderRegardlessOfItBeingRecoveredOrNot()
		 {
			  // Given
			  TransactionRecordState tx = InjectAllPossibleCommands();

			  // When
			  PhysicalTransactionRepresentation commands = TransactionRepresentationOf( tx );

			  // Then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OrderVerifyingCommandHandler orderVerifyingCommandHandler = new OrderVerifyingCommandHandler();
			  OrderVerifyingCommandHandler orderVerifyingCommandHandler = new OrderVerifyingCommandHandler();
			  commands.Accept( element => ( ( Command )element ).handle( orderVerifyingCommandHandler ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.kernel.impl.transaction.log.PhysicalTransactionRepresentation transactionRepresentationOf(TransactionRecordState tx) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private PhysicalTransactionRepresentation TransactionRepresentationOf( TransactionRecordState tx )
		 {
			  IList<StorageCommand> commands = new List<StorageCommand>();
			  tx.ExtractCommands( commands );
			  return new PhysicalTransactionRepresentation( commands );
		 }

		 private TransactionRecordState InjectAllPossibleCommands()
		 {
			  RecordChangeSet recordChangeSet = mock( typeof( RecordChangeSet ) );

			  RecordChanges<LabelTokenRecord, Void> labelTokenChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<RelationshipTypeTokenRecord, Void> relationshipTypeTokenChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<PropertyKeyTokenRecord, Void> propertyKeyTokenChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<NodeRecord, Void> nodeRecordChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<RelationshipRecord, Void> relationshipRecordChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<PropertyRecord, PrimitiveRecord> propertyRecordChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<RelationshipGroupRecord, int> relationshipGroupChanges = mock( typeof( RecordChanges ) );
			  RecordChanges<SchemaRecord, SchemaRule> schemaRuleChanges = mock( typeof( RecordChanges ) );

			  when( recordChangeSet.LabelTokenChanges ).thenReturn( labelTokenChanges );
			  when( recordChangeSet.RelationshipTypeTokenChanges ).thenReturn( relationshipTypeTokenChanges );
			  when( recordChangeSet.PropertyKeyTokenChanges ).thenReturn( propertyKeyTokenChanges );
			  when( recordChangeSet.NodeRecords ).thenReturn( nodeRecordChanges );
			  when( recordChangeSet.RelRecords ).thenReturn( relationshipRecordChanges );
			  when( recordChangeSet.PropertyRecords ).thenReturn( propertyRecordChanges );
			  when( recordChangeSet.RelGroupRecords ).thenReturn( relationshipGroupChanges );
			  when( recordChangeSet.SchemaRuleChanges ).thenReturn( schemaRuleChanges );

			  IList<RecordAccess_RecordProxy<NodeRecord, Void>> nodeChanges = new LinkedList<RecordAccess_RecordProxy<NodeRecord, Void>>();

			  RecordChanges.RecordChange<NodeRecord, Void> deletedNode = mock( typeof( RecordChanges.RecordChange ) );
			  when( deletedNode.Before ).thenReturn( InUseNode() );
			  when( deletedNode.ForReadingLinkage() ).thenReturn(MissingNode());
			  nodeChanges.Add( deletedNode );

			  RecordChanges.RecordChange<NodeRecord, Void> createdNode = mock( typeof( RecordChanges.RecordChange ) );
			  when( createdNode.Before ).thenReturn( MissingNode() );
			  when( createdNode.ForReadingLinkage() ).thenReturn(createdNode());
			  nodeChanges.Add( createdNode );

			  RecordChanges.RecordChange<NodeRecord, Void> updatedNode = mock( typeof( RecordChanges.RecordChange ) );
			  when( updatedNode.Before ).thenReturn( InUseNode() );
			  when( updatedNode.ForReadingLinkage() ).thenReturn(InUseNode());
			  nodeChanges.Add( updatedNode );

			  when( nodeRecordChanges.Changes() ).thenReturn(nodeChanges);
			  when( nodeRecordChanges.ChangeSize() ).thenReturn(3);
			  when( recordChangeSet.ChangeSize() ).thenReturn(3);

			  when( labelTokenChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( relationshipTypeTokenChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( propertyKeyTokenChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( relationshipRecordChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( propertyRecordChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( relationshipGroupChanges.Changes() ).thenReturn(Collections.emptyList());
			  when( schemaRuleChanges.Changes() ).thenReturn(Collections.emptyList());

			  NeoStores neoStores = mock( typeof( NeoStores ) );
			  NodeStore store = mock( typeof( NodeStore ) );
			  when( neoStores.NodeStore ).thenReturn( store );
			  RelationshipGroupStore relationshipGroupStore = mock( typeof( RelationshipGroupStore ) );
			  when( neoStores.RelationshipGroupStore ).thenReturn( relationshipGroupStore );
			  RelationshipStore relationshipStore = mock( typeof( RelationshipStore ) );
			  when( neoStores.RelationshipStore ).thenReturn( relationshipStore );

			  return new TransactionRecordState( neoStores, mock( typeof( IntegrityValidator ) ), recordChangeSet, 0, null, null, null, null, null );
		 }

		 private class OrderVerifyingCommandHandler : Neo4Net.Kernel.Impl.Api.CommandVisitor_Adapter
		 {
			  internal bool NodeVisited;

			  // Commands should appear in this order
			  internal bool Updated;
			  internal bool Deleted;

			  public override bool VisitNodeCommand( Command.NodeCommand command )
			  {
					if ( !NodeVisited )
					{
						 Updated = false;
						 Deleted = false;
					}
					NodeVisited = true;

					switch ( command.Mode )
					{
					case CREATE:
						 assertFalse( Updated );
						 assertFalse( Deleted );
						 break;
					case UPDATE:
						 Updated = true;
						 assertFalse( Deleted );
						 break;
					case DELETE:
						 Deleted = true;
						 break;
					default:
						 throw new System.InvalidOperationException( "Unknown command mode: " + command.Mode );
					}
					return false;
			  }
		 }
	}

}