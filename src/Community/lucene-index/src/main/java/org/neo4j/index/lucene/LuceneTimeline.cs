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
namespace Neo4Net.Index.lucene
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.MAX_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.search.NumericRangeQuery.newLongRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.lucene.ValueContext.numeric;

	using Sort = org.apache.lucene.search.Sort;
	using SortField = org.apache.lucene.search.SortField;
	using SortedNumericSortField = org.apache.lucene.search.SortedNumericSortField;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;

	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public class LuceneTimeline<T> : TimelineIndex<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 private const string FIELD = "timestamp";
		 private readonly Index<T> _index;

		 [Obsolete]
		 public LuceneTimeline( GraphDatabaseService db, Index<T> index )
		 {
			  AssertIsLuceneIndex( db, index );
			  this._index = index;
		 }

		 private void AssertIsLuceneIndex( GraphDatabaseService db, Index<T> index )
		 {
			  IDictionary<string, string> config = Db.index().getConfiguration(index);
			  if ( !config[Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER].Equals( "lucene" ) ) // Not so hard coded please
			  {
					throw new System.ArgumentException( index + " isn't a Lucene index" );
			  }
		 }

		 private T GetSingle( bool reversed )
		 {
			  IndexHits<T> hits = _index.query( Sort( EverythingQuery().top(1), reversed ) );
			  return hits.Single;
		 }

		 private QueryContext EverythingQuery()
		 {
			  return new QueryContext( newLongRange( FIELD, 0L, MAX_VALUE, true, true ) );
		 }

		 private QueryContext RangeQuery( long? startTimestampOrNull, long? endTimestampOrNull )
		 {
			  long start = startTimestampOrNull != null ? startTimestampOrNull : -long.MaxValue;
			  long end = endTimestampOrNull != null ? endTimestampOrNull.Value : MAX_VALUE;
			  return new QueryContext( newLongRange( FIELD, start, end, true, true ) );
		 }

		 private QueryContext Sort( QueryContext query, bool reversed )
		 {
			  return query.Sort( new Sort( new SortedNumericSortField( FIELD, SortField.Type.LONG, reversed ) ) );
		 }

		 [Obsolete]
		 public virtual T Last
		 {
			 get
			 {
				  return GetSingle( true );
			 }
		 }

		 [Obsolete]
		 public virtual T First
		 {
			 get
			 {
				  return GetSingle( false );
			 }
		 }

		 [Obsolete]
		 public override void Remove( T entity, long timestamp )
		 {
			  _index.remove( entity, FIELD, timestamp );
		 }

		 [Obsolete]
		 public override void Add( T entity, long timestamp )
		 {
			  _index.add( entity, FIELD, numeric( timestamp ) );
		 }

		 [Obsolete]
		 public override IndexHits<T> GetBetween( long? startTimestampOrNull, long? endTimestampOrNull )
		 {
			  return GetBetween( startTimestampOrNull, endTimestampOrNull, false );
		 }

		 [Obsolete]
		 public override IndexHits<T> GetBetween( long? startTimestampOrNull, long? endTimestampOrNull, bool reversed )
		 {
			  return _index.query( Sort( RangeQuery( startTimestampOrNull, endTimestampOrNull ), reversed ) );
		 }
	}

}