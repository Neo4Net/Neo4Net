using System;
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
namespace Neo4Net.Consistency.checking
{

	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.TokenStore.NAME_STORE_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NO_PREV_RELATIONSHIP;

	public class TransactionWriter
	{
		 private readonly NeoStores _neoStores;

		 private readonly IList<Command.NodeCommand> _nodeCommands = new List<Command.NodeCommand>();
		 private readonly IList<Command.RelationshipCommand> _relationshipCommands = new List<Command.RelationshipCommand>();
		 private readonly IList<Command.RelationshipGroupCommand> _relationshipGroupCommands = new List<Command.RelationshipGroupCommand>();
		 private readonly IList<Command> _otherCommands = new List<Command>();

		 public TransactionWriter( NeoStores neoStores )
		 {
			  this._neoStores = neoStores;
		 }

		 public virtual TransactionRepresentation Representation( sbyte[] additionalHeader, int masterId, int authorId, long startTime, long lastCommittedTx, long committedTime )
		 {
			  PrepareForCommit();
			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( AllCommands() );
			  representation.SetHeader( additionalHeader, masterId, authorId, startTime, lastCommittedTx, committedTime, -1 );
			  return representation;
		 }

		 public virtual void PropertyKey( int id, string key, params int[] dynamicIds )
		 {
			  PropertyKeyTokenRecord before = new PropertyKeyTokenRecord( id );
			  PropertyKeyTokenRecord after = WithName( new PropertyKeyTokenRecord( id ), dynamicIds, key );
			  _otherCommands.Add( new Command.PropertyKeyTokenCommand( before, after ) );
		 }

		 public virtual void Label( int id, string name, params int[] dynamicIds )
		 {
			  LabelTokenRecord before = new LabelTokenRecord( id );
			  LabelTokenRecord after = WithName( new LabelTokenRecord( id ), dynamicIds, name );
			  _otherCommands.Add( new Command.LabelTokenCommand( before, after ) );
		 }

		 public virtual void RelationshipType( int id, string label, params int[] dynamicIds )
		 {
			  RelationshipTypeTokenRecord before = new RelationshipTypeTokenRecord( id );
			  RelationshipTypeTokenRecord after = WithName( new RelationshipTypeTokenRecord( id ), dynamicIds, label );
			  _otherCommands.Add( new Command.RelationshipTypeTokenCommand( before, after ) );
		 }

		 public virtual void Update( NeoStoreRecord before, NeoStoreRecord after )
		 {
			  _otherCommands.Add( new Command.NeoStoreCommand( before, after ) );
		 }

		 public virtual void Update( LabelTokenRecord before, LabelTokenRecord after )
		 {
			  _otherCommands.Add( new Command.LabelTokenCommand( before, after ) );
		 }

		 public virtual void Create( NodeRecord node )
		 {
			  node.SetCreated();
			  Update( new NodeRecord( node.Id, false, NO_PREV_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() ), node );
		 }

		 public virtual void Create( LabelTokenRecord labelToken )
		 {
			  labelToken.SetCreated();
			  Update( new LabelTokenRecord( labelToken.IntId ), labelToken );
		 }

		 public virtual void Create( PropertyKeyTokenRecord token )
		 {
			  token.SetCreated();
			  Update( new PropertyKeyTokenRecord( token.IntId ), token );
		 }

		 public virtual void Create( RelationshipGroupRecord group )
		 {
			  group.SetCreated();
			  Update( new RelationshipGroupRecord( group.Id, group.Type ), group );
		 }

		 public virtual void Update( NodeRecord before, NodeRecord node )
		 {
			  node.InUse = true;
			  Add( before, node );
		 }

		 public virtual void Update( PropertyKeyTokenRecord before, PropertyKeyTokenRecord after )
		 {
			  after.InUse = true;
			  Add( before, after );
		 }

		 public virtual void Delete( NodeRecord node )
		 {
			  node.InUse = false;
			  Add( node, new NodeRecord( node.Id, false, NO_PREV_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() ) );
		 }

		 public virtual void Create( RelationshipRecord record )
		 {
			  record.SetCreated();
			  Update( new RelationshipRecord( record.Id ), record );
		 }

		 public virtual void Delete( RelationshipGroupRecord group )
		 {
			  group.InUse = false;
			  Add( group, new RelationshipGroupRecord( group.Id, group.Type ) );
		 }

		 public virtual void CreateSchema( ICollection<DynamicRecord> beforeRecord, ICollection<DynamicRecord> afterRecord, SchemaRule rule )
		 {
			  foreach ( DynamicRecord record in afterRecord )
			  {
					record.SetCreated();
			  }
			  UpdateSchema( beforeRecord, afterRecord, rule );
		 }

		 public virtual void UpdateSchema( ICollection<DynamicRecord> beforeRecords, ICollection<DynamicRecord> afterRecords, SchemaRule rule )
		 {
			  foreach ( DynamicRecord record in afterRecords )
			  {
					record.InUse = true;
			  }
			  AddSchema( beforeRecords, afterRecords, rule );
		 }

		 public virtual void Update( RelationshipRecord before, RelationshipRecord after )
		 {
			  after.InUse = true;
			  Add( before, after );
		 }

		 public virtual void Update( RelationshipGroupRecord before, RelationshipGroupRecord after )
		 {
			  after.InUse = true;
			  Add( before, after );
		 }

		 public virtual void Delete( RelationshipRecord record )
		 {
			  record.InUse = false;
			  Add( record, new RelationshipRecord( record.Id ) );
		 }

		 public virtual void Create( PropertyRecord property )
		 {
			  property.SetCreated();
			  PropertyRecord before = new PropertyRecord( property.Id );
			  if ( property.NodeSet )
			  {
					before.NodeId = property.NodeId;
			  }
			  if ( property.RelSet )
			  {
					before.RelId = property.RelId;
			  }
			  Update( before, property );
		 }

		 public virtual void Update( PropertyRecord before, PropertyRecord after )
		 {
			  after.InUse = true;
			  Add( before, after );
		 }

		 public virtual void Delete( PropertyRecord before, PropertyRecord after )
		 {
			  after.InUse = false;
			  Add( before, after );
		 }

		 // Internals

		 private void AddSchema( ICollection<DynamicRecord> beforeRecords, ICollection<DynamicRecord> afterRecords, SchemaRule rule )
		 {
			  _otherCommands.Add( new Command.SchemaRuleCommand( beforeRecords, afterRecords, rule ) );
		 }

		 public virtual void Add( NodeRecord before, NodeRecord after )
		 {
			  _nodeCommands.Add( new Command.NodeCommand( before, after ) );
		 }

		 public virtual void Add( RelationshipRecord before, RelationshipRecord after )
		 {
			  _relationshipCommands.Add( new Command.RelationshipCommand( before, after ) );
		 }

		 public virtual void Add( RelationshipGroupRecord before, RelationshipGroupRecord after )
		 {
			  _relationshipGroupCommands.Add( new Command.RelationshipGroupCommand( before, after ) );
		 }

		 public virtual void Add( PropertyRecord before, PropertyRecord property )
		 {
			  _otherCommands.Add( new Command.PropertyCommand( before, property ) );
		 }

		 public virtual void Add( RelationshipTypeTokenRecord before, RelationshipTypeTokenRecord after )
		 {
			  _otherCommands.Add( new Command.RelationshipTypeTokenCommand( before, after ) );
		 }

		 public virtual void Add( PropertyKeyTokenRecord before, PropertyKeyTokenRecord after )
		 {
			  _otherCommands.Add( new Command.PropertyKeyTokenCommand( before, after ) );
		 }

		 public virtual void Add( NeoStoreRecord before, NeoStoreRecord after )
		 {
			  _otherCommands.Add( new Command.NeoStoreCommand( before, after ) );
		 }

		 public virtual void IncrementNodeCount( int labelId, long delta )
		 {
			  _otherCommands.Add( new Command.NodeCountsCommand( labelId, delta ) );
		 }

		 public virtual void IncrementRelationshipCount( int startLabelId, int typeId, int endLabelId, long delta )
		 {
			  _otherCommands.Add( new Command.RelationshipCountsCommand( startLabelId, typeId, endLabelId, delta ) );
		 }

		 private static T WithName<T>( T record, int[] dynamicIds, string name ) where T : Neo4Net.Kernel.Impl.Store.Records.TokenRecord
		 {
			  if ( dynamicIds == null || dynamicIds.Length == 0 )
			  {
					throw new System.ArgumentException( "No dynamic records for storing the name." );
			  }
			  record.InUse = true;
			  sbyte[] data = PropertyStore.encodeString( name );
			  if ( data.Length > dynamicIds.Length * NAME_STORE_BLOCK_SIZE )
			  {
					throw new System.ArgumentException( string.Format( "[{0}] is too long to fit in {1:D} blocks", name, dynamicIds.Length ) );
			  }
			  else if ( data.Length <= ( dynamicIds.Length - 1 ) * NAME_STORE_BLOCK_SIZE )
			  {
					throw new System.ArgumentException( string.Format( "[{0}] is to short to fill {1:D} blocks", name, dynamicIds.Length ) );
			  }

			  for ( int i = 0; i < dynamicIds.Length; i++ )
			  {
					sbyte[] part = new sbyte[Math.Min( NAME_STORE_BLOCK_SIZE, data.Length - i * NAME_STORE_BLOCK_SIZE )];
					Array.Copy( data, i * NAME_STORE_BLOCK_SIZE, part, 0, part.Length );

					DynamicRecord dynamicRecord = new DynamicRecord( dynamicIds[i] );
					dynamicRecord.InUse = true;
					dynamicRecord.Data = part;
					dynamicRecord.SetCreated();
					record.addNameRecord( dynamicRecord );
			  }
			  record.NameId = dynamicIds[0];
			  return record;
		 }

		 private void PrepareForCommit()
		 {
			  foreach ( Command.NodeCommand command in _nodeCommands )
			  {
					_neoStores.NodeStore.prepareForCommit( command.After );
			  }
			  foreach ( Command.RelationshipCommand command in _relationshipCommands )
			  {
					_neoStores.RelationshipStore.prepareForCommit( command.After );
			  }
			  foreach ( Command.RelationshipGroupCommand command in _relationshipGroupCommands )
			  {
					_neoStores.RelationshipGroupStore.prepareForCommit( command.After );
			  }
		 }

		 private IList<StorageCommand> AllCommands()
		 {
			  IList<StorageCommand> allCommands = new List<StorageCommand>();
			  ( ( IList<StorageCommand> )allCommands ).AddRange( _nodeCommands );
			  ( ( IList<StorageCommand> )allCommands ).AddRange( _relationshipCommands );
			  ( ( IList<StorageCommand> )allCommands ).AddRange( _relationshipGroupCommands );
			  ( ( IList<StorageCommand> )allCommands ).AddRange( _otherCommands );
			  return allCommands;
		 }
	}

}