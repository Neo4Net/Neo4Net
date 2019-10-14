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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;
	using Neo4Net.Kernel.impl.transaction.command.Command;
	using LabelTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using PropertyKeyTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using RelationshipTypeTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;
	using SchemaRuleCommand = Neo4Net.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using Neo4Net.Kernel.impl.transaction.command.Command;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	public class HighIdTransactionApplier : Neo4Net.Kernel.Impl.Api.TransactionApplier_Adapter
	{
		 private readonly NeoStores _neoStores;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.neo4j.kernel.impl.store.RecordStore<?>,HighId> highIds = new java.util.HashMap<>();
		 private readonly IDictionary<RecordStore<object>, HighId> _highIds = new Dictionary<RecordStore<object>, HighId>();

		 public HighIdTransactionApplier( NeoStores neoStores )
		 {
			  this._neoStores = neoStores;
		 }

		 public override bool VisitNodeCommand( NodeCommand command )
		 {
			  NodeStore nodeStore = _neoStores.NodeStore;
			  Track( nodeStore, command );
			  Track( nodeStore.DynamicLabelStore, command.After.DynamicLabelRecords );
			  return false;
		 }

		 public override bool VisitRelationshipCommand( RelationshipCommand command )
		 {
			  Track( _neoStores.RelationshipStore, command );
			  return false;
		 }

		 public override bool VisitPropertyCommand( PropertyCommand command )
		 {
			  PropertyStore propertyStore = _neoStores.PropertyStore;
			  Track( propertyStore, command );
			  foreach ( PropertyBlock block in command.After )
			  {
					switch ( block.Type )
					{
					case STRING:
						 Track( propertyStore.StringStore, block.ValueRecords );
						 break;
					case ARRAY:
						 Track( propertyStore.ArrayStore, block.ValueRecords );
						 break;
					default:
						 // Not needed, no dynamic records then
						 break;
					}
			  }
			  return false;
		 }

		 public override bool VisitRelationshipGroupCommand( RelationshipGroupCommand command )
		 {
			  Track( _neoStores.RelationshipGroupStore, command );
			  return false;
		 }

		 public override bool VisitRelationshipTypeTokenCommand( RelationshipTypeTokenCommand command )
		 {
			  TrackToken( _neoStores.RelationshipTypeTokenStore, command );
			  return false;
		 }

		 public override bool VisitLabelTokenCommand( LabelTokenCommand command )
		 {
			  TrackToken( _neoStores.LabelTokenStore, command );
			  return false;
		 }

		 public override bool VisitPropertyKeyTokenCommand( PropertyKeyTokenCommand command )
		 {
			  TrackToken( _neoStores.PropertyKeyTokenStore, command );
			  return false;
		 }

		 public override bool VisitSchemaRuleCommand( SchemaRuleCommand command )
		 {
			  SchemaStore schemaStore = _neoStores.SchemaStore;
			  foreach ( DynamicRecord record in command.RecordsAfter )
			  {
					Track( schemaStore, record );
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  // Notifies the stores about the recovered ids and will bump those high ids atomically if
			  // they surpass the current high ids
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<org.neo4j.kernel.impl.store.RecordStore<?>,HighId> highId : highIds.entrySet())
			  foreach ( KeyValuePair<RecordStore<object>, HighId> highId in _highIds.SetOfKeyValuePairs() )
			  {
					highId.Key.HighestPossibleIdInUse = highId.Value.id;
			  }
		 }

		 private void Track<T1>( RecordStore<T1> store, AbstractBaseRecord record )
		 {
			  long id = max( record.Id, record.RequiresSecondaryUnit() ? record.SecondaryUnitId : -1 );
			  HighId highId = _highIds[store];
			  if ( highId == null )
			  {
					_highIds[store] = new HighId( id );
			  }
			  else
			  {
					highId.Track( id );
			  }
		 }

		 private void Track<RECORD>( RecordStore<RECORD> store, BaseCommand<RECORD> command ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  Track( store, command.After );
		 }

		 private void Track<T1, T2>( RecordStore<T1> store, ICollection<T2> records ) where T2 : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  foreach ( AbstractBaseRecord record in records )
			  {
					Track( store, record );
			  }
		 }

		 private void TrackToken<RECORD>( TokenStore<RECORD> tokenStore, TokenCommand<RECORD> tokenCommand ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.TokenRecord
		 {
			  Track( tokenStore, tokenCommand.After );
			  Track( tokenStore.NameStore, tokenCommand.After.NameRecords );
		 }

		 private class HighId
		 {
			  internal long Id;

			  internal HighId( long id )
			  {
					this.Id = id;
			  }

			  internal virtual void Track( long id )
			  {
					if ( id > this.Id )
					{
						 this.Id = id;
					}
			  }
		 }
	}

}