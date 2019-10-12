using System;
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

	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.index;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using BatchInserter = Org.Neo4j.@unsafe.Batchinsert.BatchInserter;
	using BatchInserterIndex = Org.Neo4j.@unsafe.Batchinsert.BatchInserterIndex;
	using BatchInserterIndexProvider = Org.Neo4j.@unsafe.Batchinsert.BatchInserterIndexProvider;
	using BatchRelationship = Org.Neo4j.@unsafe.Batchinsert.BatchRelationship;
	using IndexConfigStoreProvider = Org.Neo4j.@unsafe.Batchinsert.@internal.IndexConfigStoreProvider;

	/// <summary>
	/// The <seealso cref="BatchInserter"/> version of <seealso cref="LuceneIndexImplementation"/>. Indexes
	/// created and populated using <seealso cref="BatchInserterIndex"/>s from this provider
	/// are compatible with <seealso cref="Index"/>s from <seealso cref="LuceneIndexImplementation"/>. </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public class LuceneBatchInserterIndexProviderNewImpl : BatchInserterIndexProvider
	{
		 private readonly BatchInserter _inserter;
		 private readonly IDictionary<IndexIdentifier, LuceneBatchInserterIndex> _indexes = new Dictionary<IndexIdentifier, LuceneBatchInserterIndex>();
		 internal readonly IndexConfigStore IndexStore;
		 private LuceneBatchInserterIndex.RelationshipLookup _relationshipLookup;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LuceneBatchInserterIndexProviderNewImpl(final org.neo4j.unsafe.batchinsert.BatchInserter inserter)
		 [Obsolete]
		 public LuceneBatchInserterIndexProviderNewImpl( BatchInserter inserter )
		 {
			  this._inserter = inserter;
			  this.IndexStore = ( ( IndexConfigStoreProvider ) inserter ).IndexStore;
			  this._relationshipLookup = id =>
			  {
				// TODO too may objects allocated here
				BatchRelationship rel = inserter.GetRelationshipById( id );
				return new EntityId_RelationshipData( id, rel.StartNode, rel.EndNode );
			  };
		 }

		 public override BatchInserterIndex NodeIndex( string indexName, IDictionary<string, string> config )
		 {
			  config( typeof( Node ), indexName, config );
			  return Index( new IndexIdentifier( IndexEntityType.Node, indexName ), config );
		 }

		 private IDictionary<string, string> Config( Type cls, string indexName, IDictionary<string, string> config )
		 {
			  // TODO Doesn't look right
			  if ( config != null )
			  {
					config = MapUtil.stringMap( new Dictionary<>( config ), Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER, LuceneIndexImplementation.SERVICE_NAME );
					IndexStore.setIfNecessary( cls, indexName, config );
					return config;
			  }
			  else
			  {
					return IndexStore.get( cls, indexName );
			  }
		 }

		 public override BatchInserterIndex RelationshipIndex( string indexName, IDictionary<string, string> config )
		 {
			  config( typeof( Relationship ), indexName, config );
			  return Index( new IndexIdentifier( IndexEntityType.Relationship, indexName ), config );
		 }

		 private BatchInserterIndex Index( IndexIdentifier identifier, IDictionary<string, string> config )
		 {
			  // We don't care about threads here... c'mon... it's a
			  // single-threaded batch inserter
			  return _indexes.computeIfAbsent( identifier, k => GetLuceneBatchInserterIndex( identifier, config ) );
		 }

		 private LuceneBatchInserterIndex GetLuceneBatchInserterIndex( IndexIdentifier identifier, IDictionary<string, string> config )
		 {
			  return new LuceneBatchInserterIndex( new File( _inserter.StoreDir ), identifier, config, _relationshipLookup );
		 }

		 public override void Shutdown()
		 {
			  foreach ( LuceneBatchInserterIndex index in _indexes.Values )
			  {
					index.Shutdown();
			  }
		 }
	}

}