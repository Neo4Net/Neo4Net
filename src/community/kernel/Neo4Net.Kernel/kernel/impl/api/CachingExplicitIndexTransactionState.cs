﻿using System.Collections.Generic;

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

	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ExplicitIndex = Neo4Net.Kernel.Api.ExplicitIndex;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.Api.txstate.ExplicitIndexTransactionState;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

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
//ORIGINAL LINE: public Neo4Net.kernel.api.ExplicitIndex nodeChanges(String indexName) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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
//ORIGINAL LINE: public Neo4Net.kernel.api.ExplicitIndex relationshipChanges(String indexName) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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

		 public override void DeleteIndex( IndexEntityType EntityType, string indexName )
		 {
			  _txState.deleteIndex( EntityType, indexName );
		 }

		 public override bool HasChanges()
		 {
			  return _txState.hasChanges();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void extractCommands(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StorageCommand> target) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override void ExtractCommands( ICollection<StorageCommand> target )
		 {
			  _txState.extractCommands( target );
		 }

		 public override bool CheckIndexExistence( IndexEntityType EntityType, string indexName, IDictionary<string, string> config )
		 {
			  return _txState.checkIndexExistence( EntityType, indexName, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _txState.close();
		 }
	}

}