using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ExplicitIndexTransactionState = Org.Neo4j.Kernel.api.txstate.ExplicitIndexTransactionState;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

	public class CachingExplicitIndexTransactionState : ExplicitIndexTransactionState
	{
		 private IDictionary<string, ExplicitIndex> _nodeExplicitIndexChanges;
		 private IDictionary<string, ExplicitIndex> _relationshipExplicitIndexChanges;
		 private readonly ExplicitIndexTransactionState _txState;

		 public CachingExplicitIndexTransactionState( ExplicitIndexTransactionState txState )
		 {
			  this._txState = txState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndex nodeChanges(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndex NodeChanges( string indexName )
		 {
			  if ( _nodeExplicitIndexChanges == null )
			  {
					_nodeExplicitIndexChanges = new Dictionary<string, ExplicitIndex>();
			  }
			  ExplicitIndex changes = _nodeExplicitIndexChanges[indexName];
			  if ( changes == null )
			  {
					_nodeExplicitIndexChanges[indexName] = changes = _txState.nodeChanges( indexName );
			  }
			  return changes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndex relationshipChanges(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndex RelationshipChanges( string indexName )
		 {
			  if ( _relationshipExplicitIndexChanges == null )
			  {
					_relationshipExplicitIndexChanges = new Dictionary<string, ExplicitIndex>();
			  }
			  ExplicitIndex changes = _relationshipExplicitIndexChanges[indexName];
			  if ( changes == null )
			  {
					_relationshipExplicitIndexChanges[indexName] = changes = _txState.relationshipChanges( indexName );
			  }
			  return changes;
		 }

		 public override void CreateIndex( IndexEntityType node, string name, IDictionary<string, string> config )
		 {
			  _txState.createIndex( node, name, config );
		 }

		 public override void DeleteIndex( IndexEntityType entityType, string indexName )
		 {
			  _txState.deleteIndex( entityType, indexName );
		 }

		 public override bool HasChanges()
		 {
			  return _txState.hasChanges();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void extractCommands(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> target) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override void ExtractCommands( ICollection<StorageCommand> target )
		 {
			  _txState.extractCommands( target );
		 }

		 public override bool CheckIndexExistence( IndexEntityType entityType, string indexName, IDictionary<string, string> config )
		 {
			  return _txState.checkIndexExistence( entityType, indexName, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _txState.close();
		 }
	}

}