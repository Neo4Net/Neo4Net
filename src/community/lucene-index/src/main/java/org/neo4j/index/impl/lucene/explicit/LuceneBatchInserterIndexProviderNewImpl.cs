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
namespace Neo4Net.Index.impl.lucene.@explicit
{

	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.index;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserterIndex = Neo4Net.@unsafe.Batchinsert.BatchInserterIndex;
	using BatchInserterIndexProvider = Neo4Net.@unsafe.Batchinsert.BatchInserterIndexProvider;
	using BatchRelationship = Neo4Net.@unsafe.Batchinsert.BatchRelationship;
	using IndexConfigStoreProvider = Neo4Net.@unsafe.Batchinsert.Internal.IndexConfigStoreProvider;

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
//ORIGINAL LINE: public LuceneBatchInserterIndexProviderNewImpl(final org.Neo4Net.unsafe.batchinsert.BatchInserter inserter)
		 [Obsolete]
		 public LuceneBatchInserterIndexProviderNewImpl( BatchInserter inserter )
		 {
			  this._inserter = inserter;
			  this.IndexStore = ( ( IndexConfigStoreProvider ) inserter ).IndexStore;
			  this._relationshipLookup = id =>
			  {
				// TODO too may objects allocated here
				BatchRelationship rel = inserter.GetRelationshipById( id );
				return new IEntityId_RelationshipData( id, rel.StartNode, rel.EndNode );
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
					config = MapUtil.stringMap( new Dictionary<>( config ), Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER, LuceneIndexImplementation.SERVICE_NAME );
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