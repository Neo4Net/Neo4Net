using System;

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
namespace Neo4Net.Index.lucene
{
	using Operator = org.apache.lucene.queryparser.classic.QueryParser.Operator;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using Sort = org.apache.lucene.search.Sort;
	using SortField = org.apache.lucene.search.SortField;
	using SortedNumericSortField = org.apache.lucene.search.SortedNumericSortField;
	using SortedSetSortField = org.apache.lucene.search.SortedSetSortField;

	using Neo4Net.GraphDb.Index;
	using Neo4Net.GraphDb.Index;
	using LuceneUtil = Neo4Net.Index.impl.lucene.@explicit.LuceneUtil;

	/// <summary>
	/// This class has the extra query configuration to use
	/// with <seealso cref="Index.query(object)"/> and <seealso cref="Index.query(string, object)"/>.
	/// It allows a query to have sorting, default operators, and allows the engine
	/// to turn off searching of modifications made inside a transaction,
	/// to gain performance. </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public class QueryContext
	{
		 private readonly object _queryOrQueryObject;
		 private Sort _sorting;
		 private Operator _defaultOperator;
		 private bool _tradeCorrectnessForSpeed;
		 private int _topHits;

		 [Obsolete]
		 public QueryContext( object queryOrQueryObject )
		 {
			  this._queryOrQueryObject = queryOrQueryObject;
		 }

		 /// <returns> the query (or query object) specified in the constructor. </returns>
		 [Obsolete]
		 public virtual object QueryOrQueryObject
		 {
			 get
			 {
				  return _queryOrQueryObject;
			 }
		 }

		 /// <summary>
		 /// Returns a QueryContext with sorting added to it.
		 /// </summary>
		 /// <param name="sorting"> The sorting to be used </param>
		 /// <returns> A QueryContext with the sorting applied. </returns>
		 [Obsolete]
		 public virtual QueryContext Sort( Sort sorting )
		 {
			  this._sorting = sorting;
			  return this;
		 }

		 /// <summary>
		 /// Returns a QueryContext with sorting added to it.
		 /// </summary>
		 /// <param name="key"> The key to sort on. </param>
		 /// <param name="additionalKeys"> Any additional keys to sort on. </param>
		 /// <returns> A QueryContext with sorting added to it. </returns>
		 [Obsolete]
		 public virtual QueryContext Sort( string key, params string[] additionalKeys )
		 {
			  SortField firstSortField = new SortedSetSortField( key, false );
			  if ( additionalKeys.Length == 0 )
			  {
					return Sort( new Sort( firstSortField ) );
			  }

			  SortField[] sortFields = new SortField[1 + additionalKeys.Length];
			  sortFields[0] = firstSortField;
			  for ( int i = 0; i < additionalKeys.Length; i++ )
			  {
					sortFields[1 + i] = new SortedSetSortField( additionalKeys[i], false );
			  }
			  return Sort( new Sort( sortFields ) );
		 }

		 /// <returns> a QueryContext with sorting by relevance, i.e. sorted after which
		 /// score each hit has. </returns>
		 [Obsolete]
		 public virtual QueryContext SortByScore()
		 {
			  return sort( Sort.RELEVANCE );
		 }

		 /// <summary>
		 /// Sort the results of a numeric range query if the query in this context
		 /// is a <seealso cref="NumericRangeQuery"/>, see <seealso cref="numericRange(string, Number, Number)"/>,
		 /// Otherwise an <seealso cref="System.InvalidOperationException"/> will be thrown.
		 /// </summary>
		 /// <param name="key"> the key to sort on. </param>
		 /// <param name="reversed"> if the sort order should be reversed or not. {@code true}
		 /// for lowest first (ascending), {@code false} for highest first (descending) </param>
		 /// <returns> a QueryContext with sorting by numeric value. </returns>
		 [Obsolete]
		 public virtual QueryContext SortNumeric( string key, bool reversed )
		 {
			  if ( !( _queryOrQueryObject is NumericRangeQuery ) )
			  {
					throw new System.InvalidOperationException( "Not a numeric range query" );
			  }

			  Number number = ( ( NumericRangeQuery )_queryOrQueryObject ).Min;
			  number = number != null ? number : ( ( NumericRangeQuery )_queryOrQueryObject ).Max;
			  SortField.Type fieldType = SortField.Type.INT;
			  if ( number is long? )
			  {
					fieldType = SortField.Type.LONG;
			  }
			  else if ( number is float? )
			  {
					fieldType = SortField.Type.FLOAT;
			  }
			  else if ( number is double? )
			  {
					fieldType = SortField.Type.DOUBLE;
			  }
			  Sort( new Sort( new SortedNumericSortField( key, fieldType, reversed ) ) );
			  return this;
		 }

		 /// <summary>
		 /// Returns the sorting setting for this context.
		 /// </summary>
		 /// <returns> the sorting set with one of the sort methods, f.ex
		 /// <seealso cref="sort(Sort)"/> or <seealso cref="sortByScore()"/> </returns>
		 [Obsolete]
		 public virtual Sort Sorting
		 {
			 get
			 {
				  return this._sorting;
			 }
		 }

		 /// <summary>
		 /// Changes the the default operator used between terms in compound queries,
		 /// default is OR.
		 /// </summary>
		 /// <param name="defaultOperator"> The new operator to use. </param>
		 /// <returns> A QueryContext with the new default operator applied. </returns>
		 [Obsolete]
		 public virtual QueryContext DefaultOperator( Operator defaultOperator )
		 {
			  this._defaultOperator = defaultOperator;
			  return this;
		 }

		 /// <summary>
		 /// Returns the default operator used between terms in compound queries.
		 /// </summary>
		 /// <returns> the default <seealso cref="Operator"/> specified with
		 ///         <seealso cref="defaultOperator"/> or "OR" if none specified. </returns>
		 [Obsolete]
		 public virtual Operator DefaultOperator
		 {
			 get
			 {
				  return this._defaultOperator;
			 }
		 }

		 /// <summary>
		 /// Adding to or removing from an index affects results from query methods
		 /// inside the same transaction, even before those changes are committed.
		 /// To let those modifications be visible in query results, some rather heavy
		 /// operations may have to be done, which can be slow to complete.
		 /// 
		 /// The default behavior is that these modifications are visible, but using
		 /// this method will tell the query to not strive to include the absolutely
		 /// latest modifications, so that such a performance penalty can be avoided.
		 /// </summary>
		 /// <returns> A QueryContext which doesn't necessarily include the latest
		 /// transaction modifications in the results, but may perform faster. </returns>
		 [Obsolete]
		 public virtual QueryContext TradeCorrectnessForSpeed()
		 {
			  this._tradeCorrectnessForSpeed = true;
			  return this;
		 }

		 /// <summary>
		 /// Returns {@code true} if this context is set to prioritize speed over
		 /// the inclusion of transactional state in the results. </summary>
		 /// <returns> whether or not <seealso cref="tradeCorrectnessForSpeed()"/> has been called. </returns>
		 [Obsolete]
		 public virtual bool TradeCorrectnessForSpeed
		 {
			 get
			 {
				  return _tradeCorrectnessForSpeed;
			 }
		 }

		 /// <summary>
		 /// Makes use of <seealso cref="IndexSearcher.search(org.apache.lucene.search.Query, int)"/>,
		 /// alternatively <seealso cref="IndexSearcher.search(org.apache.lucene.search.Query, org.apache.lucene.search.Filter, int, Sort)"/>
		 /// where only the top {@code numberOfTopHits} hits are returned. Default
		 /// behavior is to return all hits, although lazily retrieved from lucene all
		 /// the way up to the <seealso cref="IndexHits"/> iterator.
		 /// </summary>
		 /// <param name="numberOfTopHits"> the maximum number of top hits to return. </param>
		 /// <returns> A <seealso cref="QueryContext"/> with the number of top hits set. </returns>
		 [Obsolete]
		 public virtual QueryContext Top( int numberOfTopHits )
		 {
			  this._topHits = numberOfTopHits;
			  return this;
		 }

		 /// <summary>
		 /// Return the max number of results to be returned.
		 /// </summary>
		 /// <returns> the top hits set with <seealso cref="top(int)"/>. </returns>
		 [Obsolete]
		 public virtual int Top
		 {
			 get
			 {
				  return this._topHits;
			 }
		 }

		 /// <summary>
		 /// Will create a <seealso cref="QueryContext"/> with a query for numeric ranges, that is
		 /// values that have been indexed using <seealso cref="ValueContext.indexNumeric()"/>.
		 /// {@code from} (lower) and {@code to} (higher) bounds are inclusive.
		 /// It will match the type of numbers supplied to the type of values that
		 /// are indexed in the index, f.ex. long, int, float and double.
		 /// If both {@code from} and {@code to} is {@code null} then it will default
		 /// to int.
		 /// </summary>
		 /// <param name="key"> the property key to query. </param>
		 /// <param name="from"> the low end of the range (inclusive) </param>
		 /// <param name="to"> the high end of the range (inclusive) </param>
		 /// <returns> a <seealso cref="QueryContext"/> to do numeric range queries with. </returns>
		 [Obsolete]
		 public static QueryContext NumericRange( string key, Number from, Number to )
		 {
			  return NumericRange( key, from, to, true, true );
		 }

		 /// <summary>
		 /// Will create a <seealso cref="QueryContext"/> with a query for numeric ranges, that is
		 /// values that have been indexed using <seealso cref="ValueContext.indexNumeric()"/>.
		 /// It will match the type of numbers supplied to the type of values that
		 /// are indexed in the index, f.ex. long, int, float and double.
		 /// If both {@code from} and {@code to} is {@code null} then it will default
		 /// to int.
		 /// </summary>
		 /// <param name="key"> the property key to query. </param>
		 /// <param name="from"> the low end of the range (inclusive) </param>
		 /// <param name="to"> the high end of the range (inclusive) </param>
		 /// <param name="includeFrom"> whether or not {@code from} (the lower bound) is inclusive
		 /// or not. </param>
		 /// <param name="includeTo"> whether or not {@code to} (the higher bound) is inclusive
		 /// or not. </param>
		 /// <returns> a <seealso cref="QueryContext"/> to do numeric range queries with. </returns>
		 [Obsolete]
		 public static QueryContext NumericRange( string key, Number from, Number to, bool includeFrom, bool includeTo )
		 {
			  return new QueryContext( LuceneUtil.rangeQuery( key, from, to, includeFrom, includeTo ) );
		 }
	}

}