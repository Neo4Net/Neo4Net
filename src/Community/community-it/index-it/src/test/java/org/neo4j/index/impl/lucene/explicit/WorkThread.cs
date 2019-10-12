using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Index.impl.lucene.@explicit
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Test;

	public class WorkThread : OtherThreadExecutor<CommandState>
	{
		 private volatile bool _txOngoing;

		 public WorkThread( string name, Index<Node> index, GraphDatabaseService graphDb, Node node ) : base( name, new CommandState( index, graphDb, node ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void createNodeAndIndexBy(String key, String value) throws Exception
		 public virtual void CreateNodeAndIndexBy( string key, string value )
		 {
			  Execute( new CreateNodeAndIndexByCommand( key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteIndex() throws Exception
		 public virtual void DeleteIndex()
		 {
			  Execute( new DeleteIndexCommand() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Node> queryIndex(String key, Object value) throws Exception
		 public virtual IndexHits<Node> QueryIndex( string key, object value )
		 {
			  return Execute( new QueryIndexCommand( key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void commit() throws Exception
		 public virtual void Commit()
		 {
			  Execute( new CommitCommand() );
			  _txOngoing = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginTransaction() throws Exception
		 public virtual void BeginTransaction()
		 {
			  Debug.Assert( !_txOngoing );
			  Execute( new BeginTransactionCommand() );
			  _txOngoing = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeFromIndex(String key, String value) throws Exception
		 public virtual void RemoveFromIndex( string key, string value )
		 {
			  Execute( new RemoveFromIndexCommand( key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void rollback() throws Exception
		 public virtual void Rollback()
		 {
			  if ( !_txOngoing )
			  {
					return;
			  }
			  Execute( new RollbackCommand() );
			  _txOngoing = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void die() throws Exception
		 public virtual void Die()
		 {
			  Execute( new DieCommand() );
		 }

		 public virtual Future<Node> PutIfAbsent( Node node, string key, object value )
		 {
			  return ExecuteDontWait( new PutIfAbsentCommand( node, key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(final org.neo4j.graphdb.Node node, final String key, final Object value) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual void Add( Node node, string key, object value )
		 {
			  Execute((WorkerCommand<CommandState, Void>) StateConflict =>
			  {
				StateConflict.index.add( node, key, value );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.concurrent.Future<org.neo4j.graphdb.Node> getOrCreate(final String key, final Object value, final Object initialValue)
		 public virtual Future<Node> GetOrCreate( string key, object value, object initialValue )
		 {
			  return ExecuteDontWait(StateConflict =>
			  {
				UniqueFactory.UniqueNodeFactory factory = new UniqueNodeFactoryAnonymousInnerClass( this, StateConflict.index, key, initialValue );
				return factory.getOrCreate( key, value );
			  });
		 }

		 private class UniqueNodeFactoryAnonymousInnerClass : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly WorkThread _outerInstance;

			 private string _key;
			 private object _initialValue;

			 public UniqueNodeFactoryAnonymousInnerClass( WorkThread outerInstance, Index<Node> index, string key, object initialValue ) : base( index )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._initialValue = initialValue;
			 }

			 protected internal override void initialize( Node node, IDictionary<string, object> properties )
			 {
				  node.SetProperty( _key, _initialValue );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object getProperty(final org.neo4j.graphdb.PropertyContainer entity, final String key) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual object GetProperty( PropertyContainer entity, string key )
		 {
			  return Execute( StateConflict => entity.GetProperty( key ) );
		 }
	}

}