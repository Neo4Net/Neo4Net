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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{

	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using ExplicitIndexProviderTransaction = Org.Neo4j.Kernel.spi.explicitindex.ExplicitIndexProviderTransaction;
	using IndexCommandFactory = Org.Neo4j.Kernel.spi.explicitindex.IndexCommandFactory;

	public class LuceneExplicitIndexTransaction : ExplicitIndexProviderTransaction
	{
		 private readonly LuceneDataSource _dataSource;
		 private readonly IDictionary<string, LuceneExplicitIndex> _nodeIndexes = new Dictionary<string, LuceneExplicitIndex>();
		 private readonly IDictionary<string, LuceneExplicitIndex> _relationshipIndexes = new Dictionary<string, LuceneExplicitIndex>();
		 private readonly LuceneTransactionState _luceneTransaction;
		 private readonly IndexCommandFactory _commandFactory;

		 public LuceneExplicitIndexTransaction( LuceneDataSource dataSource, IndexCommandFactory commandFactory )
		 {
			  this._dataSource = dataSource;
			  this._commandFactory = commandFactory;
			  this._luceneTransaction = new LuceneTransactionState();
		 }

		 public override ExplicitIndex NodeIndex( string indexName, IDictionary<string, string> configuration )
		 {
			  LuceneExplicitIndex index = _nodeIndexes[indexName];
			  if ( index == null )
			  {
					IndexIdentifier identifier = new IndexIdentifier( IndexEntityType.Node, indexName );
					index = new LuceneExplicitIndex.NodeExplicitIndex( _dataSource, identifier, _luceneTransaction, IndexType.GetIndexType( configuration ), _commandFactory );
					_nodeIndexes[indexName] = index;
			  }
			  return index;
		 }

		 public override ExplicitIndex RelationshipIndex( string indexName, IDictionary<string, string> configuration )
		 {
			  LuceneExplicitIndex index = _relationshipIndexes[indexName];
			  if ( index == null )
			  {
					IndexIdentifier identifier = new IndexIdentifier( IndexEntityType.Relationship, indexName );
					index = new LuceneExplicitIndex.RelationshipExplicitIndex( _dataSource, identifier, _luceneTransaction, IndexType.GetIndexType( configuration ), _commandFactory );
					_relationshipIndexes[indexName] = index;
			  }
			  return index;
		 }

		 public override void Close()
		 {
			  _luceneTransaction.Dispose();
		 }
	}

}