﻿using System;
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
	using Document = org.apache.lucene.document.Document;
	using MultiReader = Org.Apache.Lucene.Index.MultiReader;
	using Term = Org.Apache.Lucene.Index.Term;
	using Occur = org.apache.lucene.search.BooleanClause.Occur;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using Sort = org.apache.lucene.search.Sort;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;
	using QueryContext = Org.Neo4j.Index.lucene.QueryContext;
	using ValueContext = Org.Neo4j.Index.lucene.ValueContext;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using ExplicitIndexHits = Org.Neo4j.Kernel.api.ExplicitIndexHits;
	using DocValuesCollector = Org.Neo4j.Kernel.Api.Impl.Index.collector.DocValuesCollector;
	using ExplicitIndexValueValidator = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexValueValidator;
	using IoPrimitiveUtils = Org.Neo4j.Kernel.impl.util.IoPrimitiveUtils;
	using IndexCommandFactory = Org.Neo4j.Kernel.spi.explicitindex.IndexCommandFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.Index.impl.lucene.@explicit.EntityId_IdData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.Index.impl.lucene.@explicit.EntityId_LongCostume;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.Index.impl.lucene.@explicit.EntityId_RelationshipData;

	public abstract class LuceneExplicitIndex : ExplicitIndex
	{
		public abstract void RemoveRelationship( long entity, long startNode, long endNode );
		public abstract void RemoveRelationship( long entity, string key, long startNode, long endNode );
		public abstract void RemoveRelationship( long entity, string key, object value, long startNode, long endNode );
		public abstract void AddRelationship( long entity, string key, object value, long startNode, long endNode );
		public abstract ExplicitIndexHits Query( object queryOrQueryObject, long startNode, long endNode );
		public abstract ExplicitIndexHits Query( string key, object queryOrQueryObject, long startNode, long endNode );
		public abstract ExplicitIndexHits Get( string key, object value, long startNode, long endNode );
		 internal const string KEY_DOC_ID = "_id_";
		 internal const string KEY_START_NODE_ID = "_start_node_id_";
		 internal const string KEY_END_NODE_ID = "_end_node_id_";

		 private static ISet<string> _forbiddenKeys = new HashSet<string>( Arrays.asList( null, KEY_DOC_ID, KEY_START_NODE_ID, KEY_END_NODE_ID ) );

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly IndexIdentifier IdentifierConflict;
		 internal readonly IndexType Type;

		 protected internal readonly LuceneTransactionState Transaction;
		 protected internal readonly LuceneDataSource DataSource;
		 protected internal readonly IndexCommandFactory CommandFactory;

		 internal LuceneExplicitIndex( LuceneDataSource dataSource, IndexIdentifier identifier, LuceneTransactionState transaction, IndexType type, IndexCommandFactory commandFactory )
		 {
			  this.DataSource = dataSource;
			  this.IdentifierConflict = identifier;
			  this.Transaction = transaction;
			  this.Type = type;
			  this.CommandFactory = commandFactory;
		 }

		 /// <summary>
		 /// See <seealso cref="Index.add(PropertyContainer, string, object)"/> for more generic
		 /// documentation.
		 /// 
		 /// Adds key/value to the {@code entity} in this index. Added values are
		 /// searchable within the transaction, but composite {@code AND}
		 /// queries aren't guaranteed to return added values correctly within that
		 /// transaction. When the transaction has been committed all such queries
		 /// are guaranteed to return correct results.
		 /// </summary>
		 /// <param name="key"> the key in the key/value pair to associate with the entity. </param>
		 /// <param name="value"> the value in the key/value pair to associate with the
		 /// entity. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addNode(long entityId, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void AddNode( long entityId, string key, object value )
		 {
			  AssertValidKey( key );
			  AssertValidValue( value );
			  EntityId entity = new IdData( entityId );
			  foreach ( object oneValue in IoPrimitiveUtils.asArray( value ) )
			  {
					oneValue = GetCorrectValue( oneValue );
					DataSource.assertValidType( key, oneValue, IdentifierConflict );
					Transaction.add( this, entity, key, oneValue );
					CommandFactory.addNode( IdentifierConflict.indexName, entityId, key, oneValue );
			  }
		 }

		 protected internal virtual object GetCorrectValue( object value )
		 {
			  AssertValidValue( value );
			  object result = value is ValueContext ? ( ( ValueContext ) value ).CorrectValue : value.ToString();
			  AssertValidValue( result );
			  return result;
		 }

		 internal static bool IsValidKey( string key )
		 {
			  return !_forbiddenKeys.Contains( key );
		 }

		 private static void AssertValidKey( string key )
		 {
			  if ( !IsValidKey( key ) )
			  {
					throw new System.ArgumentException( "Key " + key + " forbidden" );
			  }
		 }

		 private static void AssertValidValue( object value )
		 {
			  ExplicitIndexValueValidator.INSTANCE.validate( value );
		 }

		 /// <summary>
		 /// See <seealso cref="Index.remove(PropertyContainer, string, object)"/> for more
		 /// generic documentation.
		 /// 
		 /// Removes key/value to the {@code entity} in this index. Removed values
		 /// are excluded within the transaction, but composite {@code AND}
		 /// queries aren't guaranteed to exclude removed values correctly within
		 /// that transaction. When the transaction has been committed all such
		 /// queries are guaranteed to return correct results.
		 /// </summary>
		 /// <param name="entityId"> the entity (i.e <seealso cref="Node"/> or <seealso cref="Relationship"/>)
		 /// to dissociate the key/value pair from. </param>
		 /// <param name="key"> the key in the key/value pair to dissociate from the entity. </param>
		 /// <param name="value"> the value in the key/value pair to dissociate from the
		 /// entity. </param>
		 public override void Remove( long entityId, string key, object value )
		 {
			  AssertValidKey( key );
			  EntityId entity = new IdData( entityId );
			  foreach ( object oneValue in IoPrimitiveUtils.asArray( value ) )
			  {
					oneValue = GetCorrectValue( oneValue );
					Transaction.remove( this, entity, key, oneValue );
					AddRemoveCommand( entityId, key, oneValue );
			  }
		 }

		 public override void Remove( long entityId, string key )
		 {
			  AssertValidKey( key );
			  EntityId entity = new IdData( entityId );
			  Transaction.remove( this, entity, key );
			  AddRemoveCommand( entityId, key, null );
		 }

		 public override void Remove( long entityId )
		 {
			  EntityId entity = new IdData( entityId );
			  Transaction.remove( this, entity );
			  AddRemoveCommand( entityId, null, null );
		 }

		 public override void Drop()
		 {
			  Transaction.delete( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits get(String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndexHits Get( string key, object value )
		 {
			  return Query( Type.get( key, value ), key, value, null );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// 
		 /// {@code queryOrQueryObject} can be a <seealso cref="string"/> containing the query
		 /// in Lucene syntax format, http://lucene.apache.org/java/3_0_2/queryparsersyntax.html.
		 /// Or it can be a <seealso cref="Query"/> object. If can even be a <seealso cref="QueryContext"/>
		 /// object which can contain a query (<seealso cref="string"/> or <seealso cref="Query"/>) and
		 /// additional parameters, such as <seealso cref="Sort"/>.
		 /// 
		 /// Because of performance issues, including uncommitted transaction modifications
		 /// in the result is disabled by default, but can be enabled using
		 /// <seealso cref="QueryContext.tradeCorrectnessForSpeed()"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits query(String key, Object queryOrQueryObject) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndexHits Query( string key, object queryOrQueryObject )
		 {
			  QueryContext context = queryOrQueryObject is QueryContext ? ( QueryContext ) queryOrQueryObject : null;
			  return Query( Type.query( key, context != null ? context.QueryOrQueryObject : queryOrQueryObject, context ), null, null, context );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 /// <seealso cref= #query(String, Object) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits query(Object queryOrQueryObject) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override ExplicitIndexHits Query( object queryOrQueryObject )
		 {
			  return Query( null, queryOrQueryObject );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.kernel.api.ExplicitIndexHits query(org.apache.lucene.search.Query query, String keyForDirectLookup, Object valueForDirectLookup, org.neo4j.index.lucene.QueryContext additionalParametersOrNull) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 protected internal virtual ExplicitIndexHits Query( Query query, string keyForDirectLookup, object valueForDirectLookup, QueryContext additionalParametersOrNull )
		 {
			  IList<EntityId> simpleTransactionStateIds = new List<EntityId>();
			  ICollection<EntityId> removedIdsFromTransactionState = Collections.emptySet();
			  IndexSearcher fulltextTransactionStateSearcher = null;
			  if ( Transaction != null )
			  {
					if ( !string.ReferenceEquals( keyForDirectLookup, null ) )
					{
						 ( ( IList<EntityId> )simpleTransactionStateIds ).AddRange( Transaction.getAddedIds( this, keyForDirectLookup, valueForDirectLookup ) );
					}
					else
					{
						 fulltextTransactionStateSearcher = Transaction.getAdditionsAsSearcher( this, additionalParametersOrNull );
					}
					removedIdsFromTransactionState = !string.ReferenceEquals( keyForDirectLookup, null ) ? Transaction.getRemovedIds( this, keyForDirectLookup, valueForDirectLookup ) : Transaction.getRemovedIds( this, query );
			  }
			  ExplicitIndexHits idIterator = null;
			  IndexReference searcher;
			  DataSource.ReadLock;
			  try
			  {
					searcher = DataSource.getIndexSearcher( IdentifierConflict );
			  }
			  finally
			  {
					DataSource.releaseReadLock();
			  }

			  if ( searcher != null )
			  {
					try
					{
						 // Gather all added ids from fulltextTransactionStateSearcher and simpleTransactionStateIds.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet idsModifiedInTransactionState = gatherIdsModifiedInTransactionState(simpleTransactionStateIds, fulltextTransactionStateSearcher, query);
						 LongSet idsModifiedInTransactionState = GatherIdsModifiedInTransactionState( simpleTransactionStateIds, fulltextTransactionStateSearcher, query );

						 // Do the combined search from store and fulltext tx state
						 DocToIdIterator hits = new DocToIdIterator( Search( searcher, fulltextTransactionStateSearcher, query, additionalParametersOrNull, removedIdsFromTransactionState ), removedIdsFromTransactionState, searcher, idsModifiedInTransactionState );

						 idIterator = simpleTransactionStateIds.Count == 0 ? hits : new CombinedIndexHits( Arrays.asList( hits, new ConstantScoreIterator( simpleTransactionStateIds, Float.NaN ) ) );
					}
					catch ( IOException e )
					{
						 throw new Exception( "Unable to query " + this + " with " + query, e );
					}
			  }

			  // We've only got transaction state
			  idIterator = idIterator == null ? new ConstantScoreIterator( simpleTransactionStateIds, 0 ) : idIterator;
			  return idIterator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.eclipse.collections.api.set.primitive.LongSet gatherIdsModifiedInTransactionState(java.util.List<EntityId> simpleTransactionStateIds, org.apache.lucene.search.IndexSearcher fulltextTransactionStateSearcher, org.apache.lucene.search.Query query) throws java.io.IOException
		 private LongSet GatherIdsModifiedInTransactionState( IList<EntityId> simpleTransactionStateIds, IndexSearcher fulltextTransactionStateSearcher, Query query )
		 {
			  // If there's no state them don't bother gathering it
			  if ( simpleTransactionStateIds.Count == 0 && fulltextTransactionStateSearcher == null )
			  {
					return LongSets.immutable.empty();
			  }
			  // There's potentially some state
			  DocValuesCollector docValuesCollector = null;
			  int fulltextSize = 0;
			  if ( fulltextTransactionStateSearcher != null )
			  {
					docValuesCollector = new DocValuesCollector();
					fulltextTransactionStateSearcher.search( query, docValuesCollector );
					fulltextSize = docValuesCollector.TotalHits;
					// Nah, no state
					if ( simpleTransactionStateIds.Count == 0 && fulltextSize == 0 )
					{
						 return LongSets.immutable.empty();
					}
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet set = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet(simpleTransactionStateIds.size() + fulltextSize);
			  MutableLongSet set = new LongHashSet( simpleTransactionStateIds.Count + fulltextSize );

			  // Add from simple tx state
			  foreach ( EntityId id in simpleTransactionStateIds )
			  {
					set.add( id.Id() );
			  }

			  if ( docValuesCollector != null )
			  {
					// Add from fulltext tx state
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator valuesIterator = docValuesCollector.getValuesIterator(LuceneExplicitIndex.KEY_DOC_ID);
					LongIterator valuesIterator = docValuesCollector.GetValuesIterator( LuceneExplicitIndex.KEY_DOC_ID );
					while ( valuesIterator.hasNext() )
					{
						 set.add( valuesIterator.next() );
					}
			  }
			  return set;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.index.IndexHits<org.apache.lucene.document.Document> search(IndexReference searcherRef, org.apache.lucene.search.IndexSearcher fulltextTransactionStateSearcher, org.apache.lucene.search.Query query, org.neo4j.index.lucene.QueryContext additionalParametersOrNull, java.util.Collection<EntityId> removed) throws java.io.IOException
		 private IndexHits<Document> Search( IndexReference searcherRef, IndexSearcher fulltextTransactionStateSearcher, Query query, QueryContext additionalParametersOrNull, ICollection<EntityId> removed )
		 {
			  if ( fulltextTransactionStateSearcher != null && removed.Count > 0 )
			  {
					LetThroughAdditions( fulltextTransactionStateSearcher, query, removed );
			  }

			  IndexSearcher searcher = fulltextTransactionStateSearcher == null ? searcherRef.Searcher : new IndexSearcher( new MultiReader( searcherRef.Searcher.IndexReader, fulltextTransactionStateSearcher.IndexReader ) );
			  IndexHits<Document> result;
			  if ( additionalParametersOrNull != null && additionalParametersOrNull.Top > 0 )
			  {
					result = new TopDocsIterator( query, additionalParametersOrNull, searcher );
			  }
			  else
			  {
					Sort sorting = additionalParametersOrNull != null ? additionalParametersOrNull.Sorting : null;
					bool forceScore = additionalParametersOrNull == null || !additionalParametersOrNull.TradeCorrectnessForSpeed;
					DocValuesCollector collector = new DocValuesCollector( forceScore );
					searcher.search( query, collector );
					return collector.GetIndexHits( sorting );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void letThroughAdditions(org.apache.lucene.search.IndexSearcher additionsSearcher, org.apache.lucene.search.Query query, java.util.Collection<EntityId> removed) throws java.io.IOException
		 private void LetThroughAdditions( IndexSearcher additionsSearcher, Query query, ICollection<EntityId> removed )
		 {
			  // This could be improved further by doing a term-dict lookup for every term in removed
			  // and retaining only those that did not match.
			  // This is getting quite low-level though
			  DocValuesCollector collector = new DocValuesCollector( false );
			  additionsSearcher.search( query, collector );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator valuesIterator = collector.getValuesIterator(KEY_DOC_ID);
			  LongIterator valuesIterator = collector.GetValuesIterator( KEY_DOC_ID );
			  LongCostume id = new LongCostume();
			  while ( valuesIterator.hasNext() )
			  {
					long value = valuesIterator.next();
					removed.remove( id.setId( value ) );
			  }
		 }

		 internal virtual IndexIdentifier Identifier
		 {
			 get
			 {
				  return this.IdentifierConflict;
			 }
		 }

		 protected internal abstract void AddRemoveCommand( long entity, string key, object value );

		 internal class NodeExplicitIndex : LuceneExplicitIndex
		 {
			  internal NodeExplicitIndex( LuceneDataSource dataSource, IndexIdentifier identifier, LuceneTransactionState transaction, IndexType type, IndexCommandFactory commandFactory ) : base( dataSource, identifier, transaction, type, commandFactory )
			  {
			  }

			  public override ExplicitIndexHits Get( string key, object value, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override ExplicitIndexHits Query( string key, object queryOrQueryObject, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override ExplicitIndexHits Query( object queryOrQueryObject, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void AddRelationship( long entity, string key, object value, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void RemoveRelationship( long entity, string key, object value, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void RemoveRelationship( long entity, string key, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void RemoveRelationship( long entity, long startNode, long endNode )
			  {
					throw new System.NotSupportedException();
			  }

			  protected internal override void AddRemoveCommand( long entity, string key, object value )
			  {
					CommandFactory.removeNode( IdentifierConflict.indexName, entity, key, value );
			  }
		 }

		 internal class RelationshipExplicitIndex : LuceneExplicitIndex
		 {
			  internal RelationshipExplicitIndex( LuceneDataSource dataSource, IndexIdentifier identifier, LuceneTransactionState transaction, IndexType type, IndexCommandFactory commandFactory ) : base( dataSource, identifier, transaction, type, commandFactory )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addRelationship(long entityId, String key, Object value, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			  public override void AddRelationship( long entityId, string key, object value, long startNode, long endNode )
			  {
					AssertValidKey( key );
					AssertValidValue( value );
					RelationshipData entity = new RelationshipData( entityId, startNode, endNode );
					foreach ( object oneValue in IoPrimitiveUtils.asArray( value ) )
					{
						 oneValue = GetCorrectValue( oneValue );
						 DataSource.assertValidType( key, oneValue, IdentifierConflict );
						 Transaction.add( this, entity, key, oneValue );
						 CommandFactory.addRelationship( IdentifierConflict.indexName, entityId, key, oneValue, startNode, endNode );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits get(String key, Object valueOrNull, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			  public override ExplicitIndexHits Get( string key, object valueOrNull, long startNode, long endNode )
			  {
					BooleanQuery.Builder builder = new BooleanQuery.Builder();
					if ( !string.ReferenceEquals( key, null ) && valueOrNull != null )
					{
						 builder.add( Type.get( key, valueOrNull ), Occur.MUST );
					}
					AddIfAssigned( builder, startNode, KEY_START_NODE_ID );
					AddIfAssigned( builder, endNode, KEY_END_NODE_ID );
					return Query( builder.build(), null, null, null );
			  }

			  protected internal override void AddRemoveCommand( long entity, string key, object value )
			  {
					CommandFactory.removeRelationship( IdentifierConflict.indexName, entity, key, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits query(String key, Object queryOrQueryObjectOrNull, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			  public override ExplicitIndexHits Query( string key, object queryOrQueryObjectOrNull, long startNode, long endNode )
			  {
					QueryContext context = queryOrQueryObjectOrNull != null && queryOrQueryObjectOrNull is QueryContext ? ( QueryContext ) queryOrQueryObjectOrNull : null;

					BooleanQuery.Builder builder = new BooleanQuery.Builder();
					if ( ( context != null && context.QueryOrQueryObject != null ) || ( context == null && queryOrQueryObjectOrNull != null ) )
					{
						 builder.add( Type.query( key, context != null ? context.QueryOrQueryObject : queryOrQueryObjectOrNull, context ), Occur.MUST );
					}
					AddIfAssigned( builder, startNode, KEY_START_NODE_ID );
					AddIfAssigned( builder, endNode, KEY_END_NODE_ID );
					return Query( builder.build(), null, null, context );
			  }

			  public override void RemoveRelationship( long entityId, string key, object value, long startNode, long endNode )
			  {
					AssertValidKey( key );
					RelationshipData entity = new RelationshipData( entityId, startNode, endNode );
					foreach ( object oneValue in IoPrimitiveUtils.asArray( value ) )
					{
						 oneValue = GetCorrectValue( oneValue );
						 Transaction.remove( this, entity, key, oneValue );
						 AddRemoveCommand( entityId, key, oneValue );
					}
			  }

			  public override void RemoveRelationship( long entityId, string key, long startNode, long endNode )
			  {
					AssertValidKey( key );
					RelationshipData entity = new RelationshipData( entityId, startNode, endNode );
					Transaction.remove( this, entity, key );
					AddRemoveCommand( entityId, key, null );
			  }

			  public override void RemoveRelationship( long entityId, long startNode, long endNode )
			  {
					RelationshipData entity = new RelationshipData( entityId, startNode, endNode );
					Transaction.remove( this, entity );
					AddRemoveCommand( entityId, null, null );
			  }

			  internal static void AddIfAssigned( BooleanQuery.Builder builder, long node, string field )
			  {
					if ( node != -1 )
					{
						 builder.add( new TermQuery( new Term( field, "" + node ) ), Occur.MUST );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.ExplicitIndexHits query(Object queryOrQueryObjectOrNull, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			  public override ExplicitIndexHits Query( object queryOrQueryObjectOrNull, long startNode, long endNode )
			  {
					return Query( null, queryOrQueryObjectOrNull, startNode, endNode );
			  }
		 }
	}

}