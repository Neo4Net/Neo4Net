using System;

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
namespace Org.Neo4j.Kernel.impl.coreapi
{

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using ExplicitIndexWrite = Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite;
	using NodeExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeExplicitIndexCursor;
	using RelationshipExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipExplicitIndexCursor;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.explicitIndexResourceId;

	public class ExplicitIndexProxy<T> : Index<T> where T : Org.Neo4j.Graphdb.PropertyContainer
	{
		 public static readonly Type NODE = new TypeAnonymousInnerClass();

		 private class TypeAnonymousInnerClass : Type<Node>
		 {
			 public Type<Node> EntityType
			 {
				 get
				 {
					  return typeof( Node );
				 }
			 }

			 public Node entity( long id, GraphDatabaseService graphDatabaseService )
			 {
				  return graphDatabaseService.GetNodeById( id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Node> get(org.neo4j.kernel.api.KernelTransaction ktx, String name, String key, Object value, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> get( KernelTransaction ktx, string name, string key, object value, GraphDatabaseService graphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexLookup(cursor, name, key, value);
				  return new CursorWrappingNodeIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Node> query(org.neo4j.kernel.api.KernelTransaction ktx, String name, String key, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> query( KernelTransaction ktx, string name, string key, object queryOrQueryObject, GraphDatabaseService graphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexQuery(cursor, name, key, queryOrQueryObject);
				  return new CursorWrappingNodeIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Node> query(org.neo4j.kernel.api.KernelTransaction ktx, String name, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> query( KernelTransaction ktx, string name, object queryOrQueryObject, GraphDatabaseService graphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexQuery(cursor, name, queryOrQueryObject);
				  return new CursorWrappingNodeIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void add( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.NodeAddToExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id, key );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void drop(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void drop( ExplicitIndexWrite operations, string name )
			 {
				  operations.NodeExplicitIndexDrop( name );
			 }

			 public long id( PropertyContainer entity )
			 {
				  return ( ( Node ) entity ).Id;
			 }
		 }

		 public static readonly Type RELATIONSHIP = new TypeAnonymousInnerClass2();

		 private class TypeAnonymousInnerClass2 : Type<Relationship>
		 {
			 public Type<Relationship> EntityType
			 {
				 get
				 {
					  return typeof( Relationship );
				 }
			 }

			 public Relationship entity( long id, GraphDatabaseService graphDatabaseService )
			 {
				  return graphDatabaseService.GetRelationshipById( id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Relationship> get(org.neo4j.kernel.api.KernelTransaction ktx, String name, String key, Object value, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> get( KernelTransaction ktx, string name, string key, object value, GraphDatabaseService graphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexLookup(cursor, name, key, value, -1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Relationship> query(org.neo4j.kernel.api.KernelTransaction ktx, String name, String key, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> query( KernelTransaction ktx, string name, string key, object queryOrQueryObject, GraphDatabaseService graphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexQuery(cursor, name, key, queryOrQueryObject,-1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.index.IndexHits<org.neo4j.graphdb.Relationship> query(org.neo4j.kernel.api.KernelTransaction ktx, String name, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService graphDatabaseService) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> query( KernelTransaction ktx, string name, object queryOrQueryObject, GraphDatabaseService graphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexQuery(cursor, name, queryOrQueryObject, -1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, graphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
			 public void add( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.RelationshipAddToExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id, key );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void drop(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void drop( ExplicitIndexWrite operations, string name )
			 {
				  operations.RelationshipExplicitIndexDrop( name );
			 }

			 public long id( PropertyContainer entity )
			 {
				  return ( ( Relationship ) entity ).Id;
			 }
		 }

		 internal interface Type<T> where T : Org.Neo4j.Graphdb.PropertyContainer
		 {
			  Type<T> EntityType { get; }

			  T Entity( long id, GraphDatabaseService graphDatabaseService );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.index.IndexHits<T> get(org.neo4j.kernel.api.KernelTransaction operations, String name, String key, Object value, org.neo4j.graphdb.GraphDatabaseService gds) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Get( KernelTransaction operations, string name, string key, object value, GraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.index.IndexHits<T> query(org.neo4j.kernel.api.KernelTransaction operations, String name, String key, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService gds) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Query( KernelTransaction operations, string name, string key, object queryOrQueryObject, GraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.index.IndexHits<T> query(org.neo4j.kernel.api.KernelTransaction transaction, String name, Object queryOrQueryObject, org.neo4j.graphdb.GraphDatabaseService gds) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Query( KernelTransaction transaction, string name, object queryOrQueryObject, GraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException;
			  void Add( ExplicitIndexWrite operations, string name, long id, string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id, string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id, string key );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name, long id) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void drop(org.neo4j.internal.kernel.api.ExplicitIndexWrite operations, String name) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Drop( ExplicitIndexWrite operations, string name );

			  long Id( PropertyContainer entity );
		 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string NameConflict;
		 protected internal readonly Type<T> Type;
		 protected internal readonly System.Func<KernelTransaction> TxBridge;
		 private readonly GraphDatabaseService _gds;

		 public ExplicitIndexProxy( string name, Type<T> type, GraphDatabaseService gds, System.Func<KernelTransaction> txBridge )
		 {
			  this.NameConflict = name;
			  this.Type = type;
			  this._gds = gds;
			  this.TxBridge = txBridge;
		 }

		 public override string Name
		 {
			 get
			 {
				  return NameConflict;
			 }
		 }

		 public override Type<T> EntityType
		 {
			 get
			 {
				  return Type.EntityType;
			 }
		 }

		 public override IndexHits<T> Get( string key, object value )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						return InternalGet( key, value, ktx );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.index.IndexHits<T> internalGet(String key, Object value, org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private IndexHits<T> InternalGet( string key, object value, KernelTransaction ktx )
		 {
			  return Type.get( ktx, NameConflict, key, value, _gds );
		 }

		 public override IndexHits<T> Query( string key, object queryOrQueryObject )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						return Type.query( ktx, NameConflict, key, queryOrQueryObject, _gds );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override IndexHits<T> Query( object queryOrQueryObject )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						return Type.query( ktx, NameConflict, queryOrQueryObject, _gds );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override bool Writeable
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override GraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return _gds;
			 }
		 }

		 public override void Add( T entity, string key, object value )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						InternalAdd( entity, key, value, ktx );
					  }
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( format( "%s %d not found", Type, Type.id( entity ) ), e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override void Remove( T entity, string key, object value )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						Type.remove( ktx.IndexWrite(), NameConflict, Type.id(entity), key, value );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override void Remove( T entity, string key )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						Type.remove( ktx.IndexWrite(), NameConflict, Type.id(entity), key );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override void Remove( T entity )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						InternalRemove( ktx, Type.id( entity ) );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void internalRemove(org.neo4j.kernel.api.KernelTransaction ktx, long id) throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private void InternalRemove( KernelTransaction ktx, long id )
		 {
			  Type.remove( ktx.IndexWrite(), NameConflict, id );
		 }

		 public override void Delete()
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						Type.drop( ktx.IndexWrite(), NameConflict );
					  }
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override T PutIfAbsent( T entity, string key, object value )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						// Does it already exist?
						T existing = Iterators.single( InternalGet( key, value, ktx ), null );
						if ( existing != null )
						{
							 return existing;
						}
      
						// No, OK so Grab lock
						ktx.Locks().acquireExclusiveExplicitIndexLock(explicitIndexResourceId(NameConflict, key));
						// and check again -- now holding an exclusive lock
						existing = Iterators.single( InternalGet( key, value, ktx ), null );
						if ( existing != null )
						{
							 // Someone else created this entry before us just before we got the lock,
							 // release the lock as we won't be needing it
							 ktx.Locks().releaseExclusiveExplicitIndexLock(explicitIndexResourceId(NameConflict, key));
							 return existing;
						}
      
						InternalAdd( entity, key, value, ktx );
						return default( T );
					  }
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( format( "%s %d not found", Type, Type.id( entity ) ), e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( ExplicitIndexNotFoundKernelException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void internalAdd(T entity, String key, Object value, org.neo4j.kernel.api.KernelTransaction transaction) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException, org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private void InternalAdd( T entity, string key, object value, KernelTransaction transaction )
		 {
			  Type.add( transaction.IndexWrite(), NameConflict, Type.id(entity), key, value );
		 }

		 public override string ToString()
		 {
			  return "Index[" + Type + ", " + NameConflict + "]";
		 }

		 private abstract class AbstractCursorWrappingIndexHits<T> : IndexHits<T> where T : Org.Neo4j.Graphdb.PropertyContainer
		 {
			 public abstract java.util.stream.Stream<T> Stream();
			 public abstract void Close();
			  internal const long NOT_INITIALIZED = -2L;
			  internal const long NO_ID = -1L;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long NextConflict = NOT_INITIALIZED;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal int SizeConflict;
			  protected internal float Score;

			  internal AbstractCursorWrappingIndexHits( int size, float score )
			  {
					this.SizeConflict = size;
					this.Score = score;
			  }

			  public override ResourceIterator<T> Iterator()
			  {
					return this;
			  }

			  public override int Size()
			  {
					return SizeConflict;
			  }

			  public override float CurrentScore()
			  {
					return Score;
			  }

			  public virtual T Single
			  {
				  get
				  {
						if ( !HasNext() )
						{
							 return default( T );
						}
   
						T item = Next();
						if ( HasNext() )
						{
							 throw new NoSuchElementException();
						}
						return item;
				  }
			  }

			  public override bool HasNext()
			  {
					if ( NextConflict == NOT_INITIALIZED )
					{
						 NextConflict = FetchNext();
					}
					return NextConflict != NO_ID;
			  }

			  public override T Next()
			  {
					if ( !HasNext() )
					{
						 Close();
						 throw new NoSuchElementException();
					}
					T item = Materialize( NextConflict );
					NextConflict = NOT_INITIALIZED;
					return item;
			  }

			  protected internal abstract long FetchNext();

			  protected internal abstract T Materialize( long id );
		 }

		 private class CursorWrappingNodeIndexHits : AbstractCursorWrappingIndexHits<Node>
		 {
			  internal readonly NodeExplicitIndexCursor Cursor;
			  internal readonly GraphDatabaseService GraphDatabaseService;
			  internal readonly KernelTransaction Ktx;
			  internal readonly string Name;

			  internal CursorWrappingNodeIndexHits( NodeExplicitIndexCursor cursor, GraphDatabaseService graphDatabaseService, KernelTransaction ktx, string name ) : base( cursor.ExpectedTotalNumberOfResults(), cursor.Score() )
			  {
					this.Cursor = cursor;
					this.GraphDatabaseService = graphDatabaseService;
					this.Ktx = ktx;
					this.Name = name;
			  }

			  public override void Close()
			  {
					Cursor.close();
			  }

			  protected internal override long FetchNext()
			  {
					Ktx.assertOpen();
					while ( Cursor.next() )
					{
						 long reference = Cursor.nodeReference();
						 if ( Ktx.dataRead().nodeExists(reference) )
						 {
							  return reference;
						 }
						 else if ( Ktx.securityContext().mode().allowsWrites() )
						 {
							  //remove it from index so it doesn't happen again
							  try
							  {
									NODE.remove( Ktx.indexWrite(), Name, reference );
							  }
							  catch ( Exception e ) when ( e is ExplicitIndexNotFoundKernelException || e is InvalidTransactionTypeKernelException )
							  {
									//ignore
							  }
						 }
					}
					Close();
					return NO_ID;
			  }

			  protected internal override Node Materialize( long id )
			  {
					this.Score = Cursor.score();
					this.SizeConflict = Cursor.expectedTotalNumberOfResults();
					return GraphDatabaseService.getNodeById( id );
			  }
		 }

		 protected internal class CursorWrappingRelationshipIndexHits : AbstractCursorWrappingIndexHits<Relationship>
		 {
			  internal readonly RelationshipExplicitIndexCursor Cursor;
			  internal readonly GraphDatabaseService GraphDatabaseService;
			  internal readonly KernelTransaction Ktx;
			  internal readonly string Name;

			  internal CursorWrappingRelationshipIndexHits( RelationshipExplicitIndexCursor cursor, GraphDatabaseService graphDatabaseService, KernelTransaction ktx, string name ) : base( cursor.ExpectedTotalNumberOfResults(), cursor.Score() )
			  {
					this.Cursor = cursor;
					this.GraphDatabaseService = graphDatabaseService;
					this.Ktx = ktx;
					this.Name = name;
			  }

			  public override void Close()
			  {
					Cursor.close();
			  }

			  protected internal override long FetchNext()
			  {
					Ktx.assertOpen();
					while ( Cursor.next() )
					{
						 long reference = Cursor.relationshipReference();
						 if ( Ktx.dataRead().relationshipExists(reference) )
						 {
							  return reference;
						 }
						 else if ( Ktx.securityContext().mode().allowsWrites() )
						 {
							  //remove it from index so it doesn't happen again
							  try
							  {
									RELATIONSHIP.remove( Ktx.indexWrite(), Name, reference );
							  }
							  catch ( Exception e ) when ( e is ExplicitIndexNotFoundKernelException || e is InvalidTransactionTypeKernelException )
							  {
									//ignore
							  }
						 }
					}
					Close();
					return NO_ID;
			  }

			  protected internal override Relationship Materialize( long id )
			  {
					this.Score = Cursor.score();
					this.SizeConflict = Cursor.expectedTotalNumberOfResults();
					return GraphDatabaseService.getRelationshipById( id );
			  }
		 }
	}

}