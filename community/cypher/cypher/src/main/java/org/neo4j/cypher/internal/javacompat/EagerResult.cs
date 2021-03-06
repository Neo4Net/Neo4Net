﻿using System;
using System.Collections.Generic;
using System.Text;

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

	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using ExecutionPlanDescription = Org.Neo4j.Graphdb.ExecutionPlanDescription;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Notification = Org.Neo4j.Graphdb.Notification;
	using QueryExecutionType = Org.Neo4j.Graphdb.QueryExecutionType;
	using QueryStatistics = Org.Neo4j.Graphdb.QueryStatistics;
	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using VersionContext = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;

	/// <summary>
	/// Result produced as result of eager query execution for cases when <seealso cref="SnapshotExecutionEngine"/> is used.
	/// </summary>
	internal class EagerResult : Result, QueryResultProvider
	{
		 private const string ITEM_SEPARATOR = ", ";
		 private readonly Result _originalResult;
		 private readonly VersionContext _versionContext;
		 private readonly IList<IDictionary<string, object>> _queryResult = new List<IDictionary<string, object>>();
		 private int _cursor;

		 internal EagerResult( Result result, VersionContext versionContext )
		 {
			  this._originalResult = result;
			  this._versionContext = versionContext;
		 }

		 public virtual void Consume()
		 {
			  while ( _originalResult.MoveNext() )
			  {
					_queryResult.Add( _originalResult.Current );
			  }
		 }

		 public virtual QueryExecutionType QueryExecutionType
		 {
			 get
			 {
				  return _originalResult.QueryExecutionType;
			 }
		 }

		 public override IList<string> Columns()
		 {
			  return _originalResult.columns();
		 }

		 public override ResourceIterator<T> ColumnAs<T>( string name )
		 {
			  return new EagerResultResourceIterator<T>( this, name );
		 }

		 public override bool HasNext()
		 {
			  return _cursor < _queryResult.Count;
		 }

		 public override IDictionary<string, object> Next()
		 {
			  return _queryResult[_cursor++];
		 }

		 public override void Close()
		 {
			  // nothing to close. Original result is already closed at this point
		 }

		 public virtual QueryStatistics QueryStatistics
		 {
			 get
			 {
				  return _originalResult.QueryStatistics;
			 }
		 }

		 public virtual ExecutionPlanDescription ExecutionPlanDescription
		 {
			 get
			 {
				  return _originalResult.ExecutionPlanDescription;
			 }
		 }

		 public override QueryResult QueryResult()
		 {
			  return new EagerQueryResult( this );
		 }

		 public override string ResultAsString()
		 {
			  IList<string> columns = _originalResult.columns();
			  StringBuilder builder = new StringBuilder();
			  builder.Append( string.join( ITEM_SEPARATOR, columns ) );
			  if ( _queryResult.Count > 0 )
			  {
					builder.Append( lineSeparator() );
					int numberOfColumns = columns.Count;
					foreach ( IDictionary<string, object> row in _queryResult )
					{
						 WriteRow( columns, builder, numberOfColumns, row );
						 builder.Append( lineSeparator() );
					}
			  }
			  return builder.ToString();
		 }

		 public override void WriteAsStringTo( PrintWriter writer )
		 {
			  writer.print( ResultAsString() );
		 }

		 public override void Remove()
		 {
			  throw new System.NotSupportedException( "Not supported" );
		 }

		 public virtual IEnumerable<Notification> Notifications
		 {
			 get
			 {
				  return _originalResult.Notifications;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <VisitationException extends Exception> void accept(org.neo4j.graphdb.Result_ResultVisitor<VisitationException> visitor) throws VisitationException
		 public override void Accept<VisitationException>( Org.Neo4j.Graphdb.Result_ResultVisitor<VisitationException> visitor ) where VisitationException : Exception
		 {
			  try
			  {
					foreach ( IDictionary<string, object> map in _queryResult )
					{
						 visitor.Visit( new MapRow( map ) );
					}
					CheckIfDirty();
			  }
			  catch ( NotFoundException e )
			  {
					CheckIfDirty();
					throw e;
			  }
		 }

		 private void CheckIfDirty()
		 {
			  if ( _versionContext.Dirty )
			  {
					throw ( new QueryExecutionKernelException( new UnstableSnapshotException( "Unable to get clean data snapshot for query serialisation." ) ) ).asUserException();
			  }
		 }

		 private void WriteRow( IList<string> columns, StringBuilder builder, int numberOfColumns, IDictionary<string, object> row )
		 {
			  for ( int i = 0; i < numberOfColumns; i++ )
			  {
					builder.Append( row[columns[i]] );
					if ( i != numberOfColumns - 1 )
					{
						 builder.Append( ITEM_SEPARATOR );
					}
			  }
		 }

		 private class EagerResultResourceIterator<T> : ResourceIterator<T>
		 {
			 private readonly EagerResult _outerInstance;

			  internal readonly string Column;
			  internal int Cursor;

			  internal EagerResultResourceIterator( EagerResult outerInstance, string column )
			  {
				  this._outerInstance = outerInstance;
					this.Column = column;
			  }

			  public override bool HasNext()
			  {
					return Cursor < outerInstance.queryResult.Count;
			  }

			  public override T Next()
			  {
					return ( T ) outerInstance.queryResult[Cursor++][Column];
			  }

			  public override void Close()
			  {
					// Nothing to close.
			  }
		 }

		 private class EagerQueryResult : QueryResult
		 {
			 private readonly EagerResult _outerInstance;


			  internal readonly string[] Fields;

			  internal EagerQueryResult( EagerResult outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Fields = outerInstance.originalResult.Columns().ToArray();
			  }

			  public override string[] FieldNames()
			  {
					return Fields;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void accept(org.neo4j.cypher.result.QueryResult_QueryResultVisitor<E> visitor) throws E
			  public override void Accept<E>( Org.Neo4j.Cypher.result.QueryResult_QueryResultVisitor<E> visitor ) where E : Exception
			  {
					while ( outerInstance.HasNext() )
					{
						 IDictionary<string, object> row = outerInstance.Next();
						 AnyValue[] anyValues = new AnyValue[Fields.Length];

						 for ( int i = 0; i < Fields.Length; i++ )
						 {
							  anyValues[i] = ValueUtils.of( row[Fields[i]] );
						 }

						 visitor.Visit( () => anyValues );
					}
			  }

			  public override QueryExecutionType ExecutionType()
			  {
					return outerInstance.originalResult.QueryExecutionType;
			  }

			  public override QueryStatistics QueryStatistics()
			  {
					return outerInstance.originalResult.QueryStatistics;
			  }

			  public override ExecutionPlanDescription ExecutionPlanDescription()
			  {
					return outerInstance.originalResult.ExecutionPlanDescription;
			  }

			  public virtual IEnumerable<Notification> Notifications
			  {
				  get
				  {
						return outerInstance.originalResult.Notifications;
				  }
			  }

			  public override void Close()
			  {
					// nothing to close
			  }
		 }
	}

}