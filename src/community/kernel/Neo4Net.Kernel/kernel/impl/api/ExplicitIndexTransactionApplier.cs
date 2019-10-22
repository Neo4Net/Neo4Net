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
namespace Neo4Net.Kernel.Impl.Api
{

	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.index.IndexManager_Fields.PROVIDER;

	/// <summary>
	/// This class caches the appliers for different <seealso cref="IndexCommand"/>s for performance reasons. These appliers are then
	/// closed on the batch level in <seealso cref="ExplicitBatchIndexApplier.close()"/>, together with the last transaction in the
	/// batch.
	/// </summary>
	public class ExplicitIndexTransactionApplier : TransactionApplier_Adapter
	{

		 // We have these two maps here for "applier lookup" performance reasons. Every command that we apply we must
		 // redirect to the correct applier, i.e. the _single_ applier for the provider managing the specific index.
		 // Looking up provider for an index has a certain cost so those are cached in applierByIndex.
		 private IDictionary<string, TransactionApplier> _applierByNodeIndex = Collections.emptyMap();
		 private IDictionary<string, TransactionApplier> _applierByRelationshipIndex = Collections.emptyMap();
		 internal IDictionary<string, TransactionApplier> ApplierByProvider = Collections.emptyMap();

		 private readonly ExplicitIndexApplierLookup _applierLookup;
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly TransactionApplicationMode _mode;
		 private readonly IdOrderingQueue _transactionOrdering;
		 private IndexDefineCommand _defineCommand;
		 private long _transactionId = -1;

		 public ExplicitIndexTransactionApplier( ExplicitIndexApplierLookup applierLookup, IndexConfigStore indexConfigStore, TransactionApplicationMode mode, IdOrderingQueue transactionOrdering )
		 {
			  this._applierLookup = applierLookup;
			  this._indexConfigStore = indexConfigStore;
			  this._mode = mode;
			  this._transactionOrdering = transactionOrdering;
		 }

		 /// <summary>
		 /// Ability to set transaction id allows the applier instance to be cached. </summary>
		 /// <param name="txId"> the currently active TransactionId </param>
		 internal virtual long TransactionId
		 {
			 set
			 {
				  this._transactionId = value;
			 }
		 }

		 /// <summary>
		 /// Get an applier suitable for the specified IndexCommand.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private TransactionApplier applier(org.Neo4Net.kernel.impl.index.IndexCommand command) throws java.io.IOException
		 private TransactionApplier Applier( IndexCommand command )
		 {
			  // Have we got an applier for this index?
			  string indexName = _defineCommand.getIndexName( command.IndexNameId );
			  IDictionary<string, TransactionApplier> applierByIndex = ApplierByIndexMap( command );
			  TransactionApplier applier = applierByIndex[indexName];
			  if ( applier == null )
			  {
					// We don't. Have we got an applier for the provider of this index?
					IndexEntityType IEntityType = IndexEntityType.byId( command.EntityType );
					IDictionary<string, string> config = _indexConfigStore.get( IEntityType.entityClass(), indexName );
					if ( config == null )
					{
						 // This provider doesn't even exist, return an EMPTY handler, i.e. ignore these changes.
						 // Could be that the index provider is temporarily unavailable?
						 return TransactionApplier_Fields.Empty;
					}
					string providerName = config[PROVIDER];
					applier = ApplierByProvider[providerName];
					if ( applier == null )
					{
						 // We don't, so create the applier
						 applier = _applierLookup.newApplier( providerName, _mode.needsIdempotencyChecks() );
						 applier.VisitIndexDefineCommand( _defineCommand );
						 ApplierByProvider[providerName] = applier;
					}

					// Also cache this applier for this index
					applierByIndex[indexName] = applier;
			  }
			  return applier;
		 }

		 // Some lazy creation of Maps for holding appliers per provider and index
		 private IDictionary<string, TransactionApplier> ApplierByIndexMap( IndexCommand command )
		 {
			  if ( command.EntityType == IndexEntityType.Node.id() )
			  {
					if ( _applierByNodeIndex.Count == 0 )
					{
						 _applierByNodeIndex = new Dictionary<string, TransactionApplier>();
						 LazyCreateApplierByprovider();
					}
					return _applierByNodeIndex;
			  }
			  if ( command.EntityType == IndexEntityType.Relationship.id() )
			  {
					if ( _applierByRelationshipIndex.Count == 0 )
					{
						 _applierByRelationshipIndex = new Dictionary<string, TransactionApplier>();
						 LazyCreateApplierByprovider();
					}
					return _applierByRelationshipIndex;
			  }
			  throw new System.NotSupportedException( "Unknown IEntity type " + command.EntityType );
		 }

		 private void LazyCreateApplierByprovider()
		 {
			  if ( ApplierByProvider.Count == 0 )
			  {
					ApplierByProvider = new Dictionary<string, TransactionApplier>();
			  }
		 }

		 public override void Close()
		 {
			  // Let other transactions in same batch run
			  // Last transaction notifies on the batch level, to let appliers close before-hand.
			  // Internal appliers are closed on the batch level (ExplicitIndexBatchApplier)
			  NotifyExplicitIndexOperationQueue();

		 }

		 private void NotifyExplicitIndexOperationQueue()
		 {
			  if ( _transactionId != -1 )
			  {
					_transactionOrdering.removeChecked( _transactionId );
					_transactionId = -1;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddNodeCommand(org.Neo4Net.kernel.impl.index.IndexCommand.AddNodeCommand command) throws java.io.IOException
		 public override bool VisitIndexAddNodeCommand( IndexCommand.AddNodeCommand command )
		 {
			  return Applier( command ).visitIndexAddNodeCommand( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddRelationshipCommand(org.Neo4Net.kernel.impl.index.IndexCommand.AddRelationshipCommand command) throws java.io.IOException
		 public override bool VisitIndexAddRelationshipCommand( IndexCommand.AddRelationshipCommand command )
		 {
			  return Applier( command ).visitIndexAddRelationshipCommand( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexRemoveCommand(org.Neo4Net.kernel.impl.index.IndexCommand.RemoveCommand command) throws java.io.IOException
		 public override bool VisitIndexRemoveCommand( IndexCommand.RemoveCommand command )
		 {
			  return Applier( command ).visitIndexRemoveCommand( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDeleteCommand(org.Neo4Net.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException
		 public override bool VisitIndexDeleteCommand( IndexCommand.DeleteCommand command )
		 {
			  return Applier( command ).visitIndexDeleteCommand( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexCreateCommand(org.Neo4Net.kernel.impl.index.IndexCommand.CreateCommand command) throws java.io.IOException
		 public override bool VisitIndexCreateCommand( IndexCommand.CreateCommand command )
		 {
			  _indexConfigStore.setIfNecessary( IndexEntityType.byId( command.EntityType ).entityClass(), _defineCommand.getIndexName(command.IndexNameId), command.Config );
			  return Applier( command ).visitIndexCreateCommand( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDefineCommand(org.Neo4Net.kernel.impl.index.IndexDefineCommand command) throws java.io.IOException
		 public override bool VisitIndexDefineCommand( IndexDefineCommand command )
		 {
			  this._defineCommand = command;
			  Forward( command, _applierByNodeIndex );
			  Forward( command, _applierByRelationshipIndex );
			  Forward( command, ApplierByProvider );
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forward(org.Neo4Net.kernel.impl.index.IndexDefineCommand definitions, java.util.Map<String,TransactionApplier> appliers) throws java.io.IOException
		 private void Forward( IndexDefineCommand definitions, IDictionary<string, TransactionApplier> appliers )
		 {
			  foreach ( CommandVisitor applier in appliers.Values )
			  {
					applier.VisitIndexDefineCommand( definitions );
			  }
		 }
	}

}