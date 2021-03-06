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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using JavaConversions = scala.collection.JavaConversions;


	using InternalExecutionResult = Org.Neo4j.Cypher.@internal.runtime.InternalExecutionResult;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using ExecutionPlanDescription = Org.Neo4j.Graphdb.ExecutionPlanDescription;
	using Notification = Org.Neo4j.Graphdb.Notification;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using QueryExecutionType = Org.Neo4j.Graphdb.QueryExecutionType;
	using QueryType = Org.Neo4j.Graphdb.QueryExecutionType.QueryType;
	using QueryStatistics = Org.Neo4j.Graphdb.QueryStatistics;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	/// <summary>
	/// Holds Cypher query result sets, in tabular form. Each row of the result is a map
	/// of column name to result object. Each column name correlates directly
	/// with the terms used in the "return" clause of the Cypher query.
	/// The result objects could be <seealso cref="org.neo4j.graphdb.Node Nodes"/>,
	/// <seealso cref="org.neo4j.graphdb.Relationship Relationships"/> or java primitives.
	/// 
	/// 
	/// Either iterate directly over the ExecutionResult to retrieve each row of the result
	/// set, or use <code>columnAs()</code> to access a single column with result objects
	/// cast to a type.
	/// </summary>
	public class ExecutionResult : ResourceIterable<IDictionary<string, object>>, Result, QueryResultProvider
	{
		 private readonly InternalExecutionResult _inner;

		 /// <summary>
		 /// Initialized lazily and should be accessed with <seealso cref="innerIterator()"/> method
		 /// because <seealso cref="accept(ResultVisitor)"/> does not require iterator.
		 /// </summary>
		 private ResourceIterator<IDictionary<string, object>> _innerIterator;

		 /// <summary>
		 /// Constructor used by the Cypher framework. End-users should not
		 /// create an ExecutionResult directly, but instead use the result
		 /// returned from calling <seealso cref="QueryExecutionEngine.executeQuery(string, MapValue, org.neo4j.kernel.impl.query.TransactionalContext)"/>.
		 /// </summary>
		 /// <param name="projection"> Execution result projection to use. </param>
		 public ExecutionResult( InternalExecutionResult projection )
		 {
			  _inner = Objects.requireNonNull( projection );
			  //if updating query we must fetch the iterator right away in order to eagerly perform updates
			  if ( projection.executionType().queryType() == QueryExecutionType.QueryType.WRITE )
			  {
					InnerIterator();
			  }
		 }

		 /// <summary>
		 /// Returns an iterator with the result objects from a single column of the result set. This method is best used for
		 /// single column results.
		 /// 
		 /// <para><b>To ensure that any resources, including transactions bound to it, are properly closed, the iterator must
		 /// either be fully exhausted, or the <seealso cref="ResourceIterator.close() close()"/> method must be
		 /// called.</b></para>
		 /// </summary>
		 /// <param name="n"> exact name of the column, as it appeared in the original query </param>
		 /// @param <T> desired type cast for the result objects </param>
		 /// <returns> an iterator of the result objects, possibly empty </returns>
		 /// <exception cref="ClassCastException"> when the result object can not be cast to the requested type </exception>
		 /// <exception cref="org.neo4j.graphdb.NotFoundException"> when the column name does not appear in the original query </exception>
		 public override ResourceIterator<T> ColumnAs<T>( string n )
		 {
			  // this method is both a legacy method, and a method on Result,
			  // prefer conforming to the new API and convert exceptions
			  try
			  {
					return new ExceptionConversion<T>( _inner.javaColumnAs<T>( n ) );
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public virtual QueryExecutionType QueryExecutionType
		 {
			 get
			 {
				  try
				  {
						return _inner.executionType();
				  }
				  catch ( CypherException e )
				  {
						throw Converted( e );
				  }
			 }
		 }

		 /// <summary>
		 /// The exact names used to represent each column in the result set.
		 /// </summary>
		 /// <returns> List of the column names. </returns>
		 public override IList<string> Columns()
		 {
			  // this method is both a legacy method, and a method on Result,
			  // prefer conforming to the new API and convert exceptions
			  try
			  {
					return _inner.javaColumns();
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public override string ToString()
		 {
			  return _inner.ToString(); // legacy method - don't convert exceptions...
		 }

		 /// <summary>
		 /// Provides a textual representation of the query result.
		 /// <para><b>
		 /// The execution result represented by this object will be consumed in its entirety after this method is called.
		 /// Calling any of the other iterating methods on it should not be expected to return any results.
		 /// </b></para> </summary>
		 /// <returns> Returns the execution result </returns>
		 public virtual string DumpToString()
		 {
			  return _inner.dumpToString(); // legacy method - don't convert exceptions...
		 }

		 /// <summary>
		 /// Returns statistics about this result. </summary>
		 /// <returns> statistics about this result </returns>
		 public virtual QueryStatistics QueryStatistics
		 {
			 get
			 {
				  try
				  {
						return _inner.queryStatistics();
				  }
				  catch ( CypherException e )
				  {
						throw Converted( e );
				  }
			 }
		 }

		 public virtual void ToString( PrintWriter writer )
		 {
			  _inner.dumpToString( writer ); // legacy method - don't convert exceptions...
			  foreach ( Notification notification in JavaConversions.asJavaIterable( _inner.notifications() ) )
			  {
					writer.println( notification.Description );
			  }
		 }

		 /// <summary>
		 /// Returns an iterator over the <i>return</i> clause of the query. The format is a map that has as keys the names
		 /// of the columns or their explicit names (set via 'as') and the value is the calculated value. Each iterator item
		 /// is one row of the query result.
		 /// 
		 /// <para><b>To ensure that any resources, including transactions bound to it, are properly closed, the iterator must
		 /// either be fully exhausted, or the <seealso cref="ResourceIterator.close() close()"/> method must be
		 /// called.</b></para>
		 /// </summary>
		 /// <returns> An iterator over the result of the query as a map from projected column name to value </returns>
		 public override ResourceIterator<IDictionary<string, object>> Iterator()
		 {
			  return InnerIterator(); // legacy method - don't convert exceptions...
		 }

		 /// <returns> this result as a <seealso cref="Stream"/> </returns>
		 public override Stream<IDictionary<string, object>> Stream()
		 {
			  // Need to implement to disambiguate Iterable/Iterator stream
			  return Iterator().stream();
		 }

		 public override bool HasNext()
		 {
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return InnerIterator().hasNext();
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public override IDictionary<string, object> Next()
		 {
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return InnerIterator().next();
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public override void Close()
		 {
			  try
			  {
					// inner iterator might be null if this result was consumed using visitor
					if ( _innerIterator != null )
					{
						 _innerIterator.close();
					}
					// but we still need to close the underlying extended execution result
					_inner.close();
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public virtual ExecutionPlanDescription ExecutionPlanDescription
		 {
			 get
			 {
				  try
				  {
						return _inner.executionPlanDescription();
				  }
				  catch ( CypherException e )
				  {
						throw Converted( e );
				  }
			 }
		 }

		 public override string ResultAsString()
		 {
			  try
			  {
					return DumpToString();
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public override void WriteAsStringTo( PrintWriter writer )
		 {
			  try
			  {
					ToString( writer );
			  }
			  catch ( CypherException e )
			  {
					throw Converted( e );
			  }
		 }

		 public override void Remove()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <VisitationException extends Exception> void accept(org.neo4j.graphdb.Result_ResultVisitor<VisitationException> visitor) throws VisitationException
		 public override void Accept<VisitationException>( Org.Neo4j.Graphdb.Result_ResultVisitor<VisitationException> visitor ) where VisitationException : Exception
		 {
			  _inner.accept( visitor );
		 }

		 public virtual IEnumerable<Notification> Notifications
		 {
			 get
			 {
				  return JavaConversions.asJavaIterable( _inner.notifications() );
			 }
		 }

		 private ResourceIterator<IDictionary<string, object>> InnerIterator()
		 {
			  if ( _innerIterator == null )
			  {
					_innerIterator = _inner.javaIterator();
			  }
			  return _innerIterator;
		 }

		 private class ExceptionConversion<T> : ResourceIterator<T>
		 {
			  internal readonly ResourceIterator<T> Inner;

			  internal ExceptionConversion( ResourceIterator<T> inner )
			  {
					this.Inner = inner;
			  }

			  public override void Close()
			  {
					try
					{
						 Inner.close();
					}
					catch ( CypherException e )
					{
						 throw Converted( e );
					}
			  }

			  public override bool HasNext()
			  {
					try
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return Inner.hasNext();
					}
					catch ( CypherException e )
					{
						 throw Converted( e );
					}
			  }

			  public override T Next()
			  {
					try
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return Inner.next();
					}
					catch ( CypherException e )
					{
						 throw Converted( e );
					}
			  }

			  public override void Remove()
			  {
					try
					{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 Inner.remove();
					}
					catch ( CypherException e )
					{
						 throw Converted( e );
					}
			  }
		 }

		 public virtual InternalExecutionResult InternalExecutionResult()
		 {
			  return _inner;
		 }

		 public override QueryResult QueryResult()
		 {
			  return _inner;
		 }

		 private static QueryExecutionException Converted( CypherException e )
		 {
			  return ( new QueryExecutionKernelException( e ) ).asUserException();
		 }
	}

}