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
namespace Org.Neo4j.Kernel.impl.coreapi
{

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb.index;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;

	public class IndexProviderImpl : IndexProvider
	{
		 private readonly System.Func<KernelTransaction> _transactionBridge;
		 private readonly GraphDatabaseService _gds;

		 public IndexProviderImpl( GraphDatabaseService gds, System.Func<KernelTransaction> transactionBridge )
		 {
			  this._gds = gds;
			  this._transactionBridge = transactionBridge;
		 }

		 public override Index<Node> GetOrCreateNodeIndex( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  KernelTransaction ktx = _transactionBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						if ( !ktx.IndexRead().nodeExplicitIndexExists(indexName, customConfiguration) )
						{
							 // There's a sub-o-meta thing here where we create index config,
							 // and the index will itself share the same IndexConfigStore as us and pick up and use
							 // that. We should pass along config somehow with calls.
							 ktx.IndexWrite().nodeExplicitIndexCreateLazily(indexName, customConfiguration);
						}
						return new ExplicitIndexProxy<Node>( indexName, ExplicitIndexProxy.NODE, _gds, _transactionBridge );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override RelationshipIndex GetOrCreateRelationshipIndex( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  KernelTransaction ktx = _transactionBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						if ( !ktx.IndexRead().relationshipExplicitIndexExists(indexName, customConfiguration) )
						{
							 // There's a sub-o-meta thing here where we create index config,
							 // and the index will itself share the same IndexConfigStore as us and pick up and use
							 // that. We should pass along config somehow with calls.
							 ktx.IndexWrite().relationshipExplicitIndexCreateLazily(indexName, customConfiguration);
						}
						return new RelationshipExplicitIndexProxy( indexName, _gds, _transactionBridge );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }
	}

}