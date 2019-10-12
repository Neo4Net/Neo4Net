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
namespace Org.Neo4j.Graphdb
{

	/// <summary>
	/// Represents the result of <seealso cref="GraphDatabaseService.execute(string, System.Collections.IDictionary) executing"/> a query.
	/// <para>
	/// The result is comprised of a number of rows, potentially computed lazily, with this result object being an iterator
	/// over those rows. Each row is represented as a <code><seealso cref="System.Collections.IDictionary"/>&lt;<seealso cref="string"/>, <seealso cref="object"/>&gt;</code>, the
	/// keys in this map are the names of the columns in the row, as specified by the {@code return} clause of the query,
	/// and the values of the map is the corresponding computed value of the expression in the {@code return} clause. Each
	/// row will thus have the same set of keys, and these keys can be retrieved using the
	/// <seealso cref="columns() columns-method"/>.
	/// </para>
	/// <para>
	/// To ensure that any resource, including transactions bound to the query, are properly freed, the result must either
	/// be fully exhausted, by means of the <seealso cref="System.Collections.IEnumerator iterator protocol"/>, or the result has to be
	/// explicitly closed, by invoking the <seealso cref="close() close-method"/>.
	/// </para>
	/// <para>
	/// Idiomatic use of the Result object would look like this:
	/// <pre><code>
	/// try ( Result result = graphDatabase.execute( query, parameters ) )
	/// {
	///     while ( result.hasNext() )
	///     {
	///         Map&lt;String, Object&gt; row = result.next();
	///         for ( String key : result.columns() )
	///         {
	///             System.out.printf( "%s = %s%n", key, row.get( key ) );
	///         }
	///     }
	/// }
	/// </code></pre>
	/// If the result consists of only a single column, or if only one of the columns is of interest, a projection can be
	/// extracted using <seealso cref="columnAs(string)"/>. This produces a new iterator over the values of the named column. It
	/// should be noted that this iterator consumes the rows of the result in the same way as invoking <seealso cref="next()"/> on
	/// this object would, and that the <seealso cref="close() close-method"/> on either iterator has the same effect. It is thus
	/// safe to either close the projected column iterator, or this iterator, or both if all rows have not been consumed.
	/// </para>
	/// <para>
	/// In addition to the <seealso cref="next() iteration methods"/> on this interface, <seealso cref="close()"/>, and the
	/// <seealso cref="columnAs(string) column projection method"/>, there are two methods for getting a string representation of the
	/// result that also consumes the entire result if invoked. <seealso cref="resultAsString()"/> returns a single string
	/// representation of all (remaining) rows in the result, and <seealso cref="writeAsStringTo(PrintWriter)"/> does the same, but
	/// streams the result to the provided <seealso cref="PrintWriter"/> instead, without allocating large string objects.
	/// </para>
	/// <para>
	/// The methods that do not consume any rows from the result, or in other ways alter the state of the result are safe to
	/// invoke at any time, even after the result has been <seealso cref="close() closed"/> or fully exhausted. These methods
	/// are:
	/// <ul>
	/// <li><seealso cref="columns()"/></li>
	/// <li><seealso cref="getQueryStatistics()"/></li>
	/// <li><seealso cref="getQueryExecutionType()"/></li>
	/// <li><seealso cref="getExecutionPlanDescription()"/></li>
	/// </ul>
	/// </para>
	/// <para>
	/// Not all queries produce an actual result, and some queries that do might yield an empty result set. In order to
	/// distinguish between these cases the <seealso cref="QueryExecutionType"/> <seealso cref="getQueryExecutionType() of this result"/>
	/// can be queried.
	/// </para>
	/// </summary>
	public interface Result : ResourceIterator<IDictionary<string, object>>
	{
		 /// <summary>
		 /// Indicates what kind of query execution produced this result.
		 /// </summary>
		 /// <returns> an object that indicates what kind of query was executed to produce this result. </returns>
		 QueryExecutionType QueryExecutionType { get; }

		 /// <summary>
		 /// The exact names used to represent each column in the result set.
		 /// </summary>
		 /// <returns> List of the column names. </returns>
		 IList<string> Columns();

		 /// <summary>
		 /// Returns an iterator with the result objects from a single column of the result set. This method is best used for
		 /// single column results.
		 /// <para>
		 /// <strong>To ensure that any resources, including transactions bound to it, are properly closed, the iterator must
		 /// either be fully exhausted, or the <seealso cref="org.neo4j.graphdb.ResourceIterator.close() close()"/> method must be
		 /// called.</strong>
		 /// </para>
		 /// <para>
		 /// As an example, if we are only interested in a column {@code n} that contains node values, we can extract it like
		 /// this:
		 /// <pre><code>
		 ///     try ( ResourceIterator&lt;Node&gt; nodes = result.columnAs( "n" ) )
		 ///     {
		 ///         while ( nodes.hasNext() )
		 ///         {
		 ///             Node node = nodes.next();
		 ///             // Do some work with the 'node' instance.
		 ///         }
		 ///     }
		 /// </code></pre>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="name"> exact name of the column, as it appeared in the original query </param>
		 /// @param <T> desired type cast for the result objects </param>
		 /// <returns> an iterator of the result objects, possibly empty </returns>
		 /// <exception cref="ClassCastException"> when the result object can not be cast to the requested type </exception>
		 /// <exception cref="org.neo4j.graphdb.NotFoundException"> when the column name does not appear in the original query </exception>
		 ResourceIterator<T> columnAs<T>( string name );

		 /// <summary>
		 /// Denotes there being more rows available in this result. These rows must either be consumed, by invoking
		 /// <seealso cref="next()"/>, or the result has to be <seealso cref="close() closed"/>.
		 /// </summary>
		 /// <returns> {@code true} if there is more rows available in this result, {@code false} otherwise. </returns>
		 bool HasNext();

		 /// <summary>
		 /// Returns the next row in this result.
		 /// </summary>
		 /// <returns> the next row in this result. </returns>
		 IDictionary<string, object> Next();

		 /// <summary>
		 /// Closes the result, freeing up any resources held by the result.
		 /// <para>
		 /// This is an idempotent operation, invoking it multiple times has the same effect as invoking it exactly once.
		 /// It is thus safe (and even encouraged, for style and simplicity) to invoke this method even after consuming all
		 /// rows in the result through the <seealso cref="next() next-method"/>.
		 /// </para>
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Statistics about the effects of the query.
		 /// </summary>
		 /// <returns> statistics about the effects of the query. </returns>
		 QueryStatistics QueryStatistics { get; }

		 /// <summary>
		 /// Returns a description of the query plan used to produce this result.
		 /// <para>
		 /// Retrieving a description of the execution plan that was executed is always possible, regardless of whether the
		 /// query requested a plan or not. For implementing a client with the ability to present the plan to the user, it is
		 /// useful to be able to tell if the query requested a description of the plan or not. For these purposes the
		 /// <seealso cref="QueryExecutionType.requestedExecutionPlanDescription()"/>-method is used.
		 /// </para>
		 /// <para>
		 /// Being able to invoke this method, regardless of whether the user requested the plan or not is useful for
		 /// purposes of debugging queries in applications.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a description of the query plan used to produce this result. </returns>
		 ExecutionPlanDescription ExecutionPlanDescription { get; }

		 /// <summary>
		 /// Provides a textual representation of the query result.
		 /// <para><b>
		 /// The execution result represented by this object will be consumed in its entirety after this method is called.
		 /// Calling any of the other iterating methods on it should not be expected to return any results.
		 /// </b></para>
		 /// </summary>
		 /// <returns> the execution result formatted as a string </returns>
		 string ResultAsString();

		 /// <summary>
		 /// Provides a textual representation of the query result to the provided <seealso cref="java.io.PrintWriter"/>.
		 /// <para><b>
		 /// The execution result represented by this object will be consumed in its entirety after this method is called.
		 /// Calling any of the other iterating methods on it should not be expected to return any results.
		 /// </b></para>
		 /// </summary>
		 /// <param name="writer"> the <seealso cref="java.io.PrintWriter"/> to receive the textual representation of the query result. </param>
		 void WriteAsStringTo( PrintWriter writer );

		 /// <summary>
		 /// Removing rows from the result is not supported. </summary>
		 void Remove();

		 /// <summary>
		 /// Provides notifications about the query producing this result.
		 /// <para>
		 /// Notifications can be warnings about problematic queries or other valuable information that can be
		 /// presented in a client.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> an iterable of all notifications created when running the query. </returns>
		 IEnumerable<Notification> Notifications { get; }

		 /// <summary>
		 /// Visits all rows in this Result by iterating over them.
		 /// <para>
		 /// This is an alternative to using the iterator form of Result. Using the visitor is better from a object
		 /// creation perspective.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="visitor"> the ResultVisitor instance that will see the results of the visit. </param>
		 /// @param <VisitationException> the type of the exception that might get thrown </param>
		 /// <exception cref="VisitationException"> if the {@code visit(ResultRow)} method of <seealso cref="ResultVisitor"/> throws such an
		 /// exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <VisitationException extends Exception> void accept(Result_ResultVisitor<VisitationException> visitor) throws VisitationException;
		 void accept<VisitationException>( Result_ResultVisitor<VisitationException> visitor );

		 /// <summary>
		 /// Describes a row of a result. The contents of this object is only stable during the
		 /// call to the {@code visit(ResultRow)} method of <seealso cref="ResultVisitor"/>.
		 /// The data it contains can change between calls to the {@code visit(ResultRow)} method.
		 /// Instances of this type should thus not be saved
		 /// for later use, or shared with other threads, rather the content should be copied.
		 /// </summary>

		 /// <summary>
		 /// This is the visitor interface you need to implement to use the <seealso cref="Result.accept(ResultVisitor)"/> method.
		 /// </summary>
	}

	 public interface Result_ResultRow
	 {
		  // TODO: Type safe getters for collections and maps?
		  Node GetNode( string key );

		  Relationship GetRelationship( string key );

		  object Get( string key );

		  string GetString( string key );

		  Number GetNumber( string key );

		  bool? GetBoolean( string key );

		  Path GetPath( string key );
	 }

	 public interface Result_ResultVisitor<VisitationException> where VisitationException : Exception
	 {
		  /// <summary>
		  /// Visits the specified row.
		  /// </summary>
		  /// <param name="row"> the row to visit. The row object is only guaranteed to be stable until flow of control has
		  /// returned from this method. </param>
		  /// <returns> true if the next row should also be visited. Returning false will terminate the iteration of
		  /// result rows. </returns>
		  /// <exception cref="VisitationException"> if there is a problem in the execution of this method. This exception will close
		  /// the result being visited, and the exception will propagate out through the
		  /// <seealso cref="accept(ResultVisitor) accept method"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visit(Result_ResultRow row) throws VisitationException;
		  bool Visit( Result_ResultRow row );
	 }

}