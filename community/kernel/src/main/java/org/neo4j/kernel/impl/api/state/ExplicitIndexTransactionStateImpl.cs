﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.state
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using ExplicitIndexTransactionState = Org.Neo4j.Kernel.api.txstate.ExplicitIndexTransactionState;
	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using AddNodeCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using CreateCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.CreateCommand;
	using DeleteCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.DeleteCommand;
	using RemoveCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using TransactionRecordState = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState;
	using ExplicitIndexProviderTransaction = Org.Neo4j.Kernel.spi.explicitindex.ExplicitIndexProviderTransaction;
	using IndexCommandFactory = Org.Neo4j.Kernel.spi.explicitindex.IndexCommandFactory;
	using IndexImplementation = Org.Neo4j.Kernel.spi.explicitindex.IndexImplementation;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.ExplicitIndexStore.assertConfigMatches;

	/// <summary>
	/// Provides access to <seealso cref="ExplicitIndex indexes"/>. Holds transaction state for all providers in a transaction.
	/// A equivalent to TransactionRecordState, but for explicit indexes.
	/// </summary>
	/// <seealso cref= TransactionRecordState </seealso>
	public class ExplicitIndexTransactionStateImpl : ExplicitIndexTransactionState, IndexCommandFactory
	{
		 private readonly IDictionary<string, ExplicitIndexProviderTransaction> _transactions = new Dictionary<string, ExplicitIndexProviderTransaction>();
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly ExplicitIndexProvider _providerLookup;

		 // Commands
		 private IndexDefineCommand _defineCommand;
		 private readonly IDictionary<string, IList<IndexCommand>> _nodeCommands = new Dictionary<string, IList<IndexCommand>>();
		 private readonly IDictionary<string, IList<IndexCommand>> _relationshipCommands = new Dictionary<string, IList<IndexCommand>>();

		 public ExplicitIndexTransactionStateImpl( IndexConfigStore indexConfigStore, ExplicitIndexProvider explicitIndexProvider )
		 {
			  this._indexConfigStore = indexConfigStore;
			  this._providerLookup = explicitIndexProvider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndex nodeChanges(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndex NodeChanges( string indexName )
		 {
			  IDictionary<string, string> configuration = _indexConfigStore.get( typeof( Node ), indexName );
			  if ( configuration == null )
			  {
					throw new ExplicitIndexNotFoundKernelException( "Node index '" + indexName + " not found" );
			  }
			  string providerName = configuration[Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER];
			  IndexImplementation provider = _providerLookup.getProviderByName( providerName );
			  ExplicitIndexProviderTransaction transaction = _transactions.computeIfAbsent( providerName, k => provider.NewTransaction( this ) );
			  return transaction.NodeIndex( indexName, configuration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndex relationshipChanges(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndex RelationshipChanges( string indexName )
		 {
			  IDictionary<string, string> configuration = _indexConfigStore.get( typeof( Relationship ), indexName );
			  if ( configuration == null )
			  {
					throw new ExplicitIndexNotFoundKernelException( "Relationship index '" + indexName + " not found" );
			  }
			  string providerName = configuration[Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER];
			  IndexImplementation provider = _providerLookup.getProviderByName( providerName );
			  ExplicitIndexProviderTransaction transaction = _transactions[providerName];
			  if ( transaction == null )
			  {
					_transactions[providerName] = transaction = provider.NewTransaction( this );
			  }
			  return transaction.RelationshipIndex( indexName, configuration );
		 }

		 public override void ExtractCommands( ICollection<StorageCommand> target )
		 {
			  if ( _defineCommand != null )
			  {
					target.Add( _defineCommand );
					ExtractCommands( target, _nodeCommands );
					ExtractCommands( target, _relationshipCommands );
			  }

			  foreach ( ExplicitIndexProviderTransaction providerTransaction in _transactions.Values )
			  {
					providerTransaction.Dispose();
			  }
		 }

		 private void ExtractCommands( ICollection<StorageCommand> target, IDictionary<string, IList<IndexCommand>> commandMap )
		 {
			  if ( commandMap != null )
			  {
					foreach ( IList<IndexCommand> commands in commandMap.Values )
					{
						 target.addAll( commands );
					}
			  }
		 }

		 // Methods for adding commands
		 private IndexDefineCommand Definitions()
		 {
			  if ( _defineCommand == null )
			  {
					_defineCommand = new IndexDefineCommand();
			  }
			  return _defineCommand;
		 }

		 private void AddCommand( string indexName, IndexCommand command )
		 {
			  AddCommand( indexName, command, false );
		 }

		 private void AddCommand( string indexName, IndexCommand command, bool clearFirst )
		 {
			  IList<IndexCommand> commands;
			  if ( command.EntityType == IndexEntityType.Node.id() )
			  {
					commands = _nodeCommands.computeIfAbsent( indexName, k => new List<>() );
			  }
			  else if ( command.EntityType == IndexEntityType.Relationship.id() )
			  {
					commands = _relationshipCommands.computeIfAbsent( indexName, k => new List<>() );
			  }
			  else
			  {
					throw new System.ArgumentException( "" + command.EntityType );
			  }

			  if ( clearFirst )
			  {
					commands.Clear();
			  }

			  commands.Add( command );
		 }

		 public override void AddNode( string indexName, long id, string key, object value )
		 {
			  IndexCommand.AddNodeCommand command = new IndexCommand.AddNodeCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), id, Definitions().getOrAssignKeyId(key), value );
			  AddCommand( indexName, command );
		 }

		 public override void AddRelationship( string indexName, long id, string key, object value, long startNode, long endNode )
		 {
			  IndexCommand.AddRelationshipCommand command = new IndexCommand.AddRelationshipCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), id, Definitions().getOrAssignKeyId(key), value, startNode, endNode );
			  AddCommand( indexName, command );
		 }

		 public override void RemoveNode( string indexName, long id, string keyOrNull, object valueOrNull )
		 {
			  IndexCommand.RemoveCommand command = new IndexCommand.RemoveCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), IndexEntityType.Node.id(), id, Definitions().getOrAssignKeyId(keyOrNull), valueOrNull );
			  AddCommand( indexName, command );
		 }

		 public override void RemoveRelationship( string indexName, long id, string keyOrNull, object valueOrNull )
		 {
			  IndexCommand.RemoveCommand command = new IndexCommand.RemoveCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), IndexEntityType.Relationship.id(), id, Definitions().getOrAssignKeyId(keyOrNull), valueOrNull );
			  AddCommand( indexName, command );
		 }

		 public override void DeleteIndex( IndexEntityType entityType, string indexName )
		 {
			  IndexCommand.DeleteCommand command = new IndexCommand.DeleteCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), entityType.id() );
			  AddCommand( indexName, command, true );
		 }

		 public override void CreateIndex( IndexEntityType entityType, string indexName, IDictionary<string, string> config )
		 {
			  IndexCommand.CreateCommand command = new IndexCommand.CreateCommand();
			  command.Init( Definitions().getOrAssignIndexNameId(indexName), entityType.id(), config );
			  AddCommand( indexName, command );
		 }

		 public override bool HasChanges()
		 {
			  return _defineCommand != null;
		 }

		 public override bool CheckIndexExistence( IndexEntityType entityType, string indexName, IDictionary<string, string> config )
		 {
			  IDictionary<string, string> configuration = _indexConfigStore.get( entityType.entityClass(), indexName );
			  if ( configuration == null )
			  {
					return false;
			  }

			  string providerName = configuration[Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER];
			  IndexImplementation provider = _providerLookup.getProviderByName( providerName );
			  assertConfigMatches( provider, indexName, configuration, config );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  // We have nothing to close.
		 }
	}

}