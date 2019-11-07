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
namespace Neo4Net.Kernel.impl.coreapi
{

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.GraphDb.Index;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using ExplicitIndexWrite = Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite;
	using NodeExplicitIndexCursor = Neo4Net.Kernel.Api.Internal.NodeExplicitIndexCursor;
	using RelationshipExplicitIndexCursor = Neo4Net.Kernel.Api.Internal.RelationshipExplicitIndexCursor;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Statement = Neo4Net.Kernel.Api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.locking.ResourceTypes.explicitIndexResourceId;

	public class ExplicitIndexProxy<T> : Index<T> where T : Neo4Net.GraphDb.PropertyContainer
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

			 public Node IEntity( long id, IGraphDatabaseService IGraphDatabaseService )
			 {
				  return IGraphDatabaseService.GetNodeById( id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Node> get(Neo4Net.kernel.api.KernelTransaction ktx, String name, String key, Object value, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> get( KernelTransaction ktx, string name, string key, object value, IGraphDatabaseService IGraphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexLookup(cursor, name, key, value);
				  return new CursorWrappingNodeIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Node> query(Neo4Net.kernel.api.KernelTransaction ktx, String name, String key, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> query( KernelTransaction ktx, string name, string key, object queryOrQueryObject, IGraphDatabaseService IGraphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexQuery(cursor, name, key, queryOrQueryObject);
				  return new CursorWrappingNodeIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Node> query(Neo4Net.kernel.api.KernelTransaction ktx, String name, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Node> query( KernelTransaction ktx, string name, object queryOrQueryObject, IGraphDatabaseService IGraphDatabaseService )
			 {
				  NodeExplicitIndexCursor cursor = ktx.Cursors().allocateNodeExplicitIndexCursor();
				  ktx.IndexRead().nodeExplicitIndexQuery(cursor, name, queryOrQueryObject);
				  return new CursorWrappingNodeIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void add( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.NodeAddToExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id, key );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id )
			 {
				  operations.NodeRemoveFromExplicitIndex( name, id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void drop(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void drop( ExplicitIndexWrite operations, string name )
			 {
				  operations.NodeExplicitIndexDrop( name );
			 }

			 public long id( IPropertyContainer IEntity )
			 {
				  return ( ( Node ) IEntity ).Id;
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

			 public Relationship IEntity( long id, IGraphDatabaseService IGraphDatabaseService )
			 {
				  return IGraphDatabaseService.GetRelationshipById( id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Relationship> get(Neo4Net.kernel.api.KernelTransaction ktx, String name, String key, Object value, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> get( KernelTransaction ktx, string name, string key, object value, IGraphDatabaseService IGraphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexLookup(cursor, name, key, value, -1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Relationship> query(Neo4Net.kernel.api.KernelTransaction ktx, String name, String key, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> query( KernelTransaction ktx, string name, string key, object queryOrQueryObject, IGraphDatabaseService IGraphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexQuery(cursor, name, key, queryOrQueryObject,-1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.GraphDb.Index.IndexHits<Neo4Net.graphdb.Relationship> query(Neo4Net.kernel.api.KernelTransaction ktx, String name, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public IndexHits<Relationship> query( KernelTransaction ktx, string name, object queryOrQueryObject, IGraphDatabaseService IGraphDatabaseService )
			 {
				  RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
				  ktx.IndexRead().relationshipExplicitIndexQuery(cursor, name, queryOrQueryObject, -1, -1);
				  return new CursorWrappingRelationshipIndexHits( cursor, IGraphDatabaseService, ktx, name );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException, Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException
			 public void add( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.RelationshipAddToExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key, object value )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id, key, value );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id, string key )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id, key );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void remove( ExplicitIndexWrite operations, string name, long id )
			 {
				  operations.RelationshipRemoveFromExplicitIndex( name, id );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void drop(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
			 public void drop( ExplicitIndexWrite operations, string name )
			 {
				  operations.RelationshipExplicitIndexDrop( name );
			 }

			 public long id( IPropertyContainer IEntity )
			 {
				  return ( ( Relationship ) IEntity ).Id;
			 }
		 }

		 internal interface Type<T> where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  Type<T> EntityType { get; }

			  T IEntity( long id, IGraphDatabaseService IGraphDatabaseService );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.GraphDb.Index.IndexHits<T> get(Neo4Net.kernel.api.KernelTransaction operations, String name, String key, Object value, Neo4Net.graphdb.GraphDatabaseService gds) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Get( KernelTransaction operations, string name, string key, object value, IGraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.GraphDb.Index.IndexHits<T> query(Neo4Net.kernel.api.KernelTransaction operations, String name, String key, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService gds) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Query( KernelTransaction operations, string name, string key, object queryOrQueryObject, IGraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.GraphDb.Index.IndexHits<T> query(Neo4Net.kernel.api.KernelTransaction transaction, String name, Object queryOrQueryObject, Neo4Net.graphdb.GraphDatabaseService gds) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  IndexHits<T> Query( KernelTransaction transaction, string name, object queryOrQueryObject, IGraphDatabaseService gds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException, Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
			  void Add( ExplicitIndexWrite operations, string name, long id, string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id, string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id, String key) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id, string key );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void remove(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name, long id) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Remove( ExplicitIndexWrite operations, string name, long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void drop(Neo4Net.Kernel.Api.Internal.ExplicitIndexWrite operations, String name) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
			  void Drop( ExplicitIndexWrite operations, string name );

			  long Id( IPropertyContainer IEntity );
		 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string NameConflict;
		 protected internal readonly Type<T> Type;
		 protected internal readonly System.Func<KernelTransaction> TxBridge;
		 private readonly IGraphDatabaseService _gds;

		 public ExplicitIndexProxy( string name, Type<T> type, IGraphDatabaseService gds, System.Func<KernelTransaction> txBridge )
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
//ORIGINAL LINE: private Neo4Net.GraphDb.Index.IndexHits<T> internalGet(String key, Object value, Neo4Net.kernel.api.KernelTransaction ktx) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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

		 public override IGraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return _gds;
			 }
		 }

		 public override void Add( T IEntity, string key, object value )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						InternalAdd( IEntity, key, value, ktx );
					  }
			  }
			  catch ( IEntityNotFoundException e )
			  {
					throw new NotFoundException( format( "%s %d not found", Type, Type.id( IEntity ) ), e );
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

		 public override void Remove( T IEntity, string key, object value )
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

		 public override void Remove( T IEntity, string key )
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

		 public override void Remove( T IEntity )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						InternalRemove( ktx, Type.id( IEntity ) );
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
//ORIGINAL LINE: private void internalRemove(Neo4Net.kernel.api.KernelTransaction ktx, long id) throws Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException, Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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

		 public override T PutIfAbsent( T IEntity, string key, object value )
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
      
						InternalAdd( IEntity, key, value, ktx );
						return default( T );
					  }
			  }
			  catch ( IEntityNotFoundException e )
			  {
					throw new NotFoundException( format( "%s %d not found", Type, Type.id( IEntity ) ), e );
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
//ORIGINAL LINE: private void internalAdd(T IEntity, String key, Object value, Neo4Net.kernel.api.KernelTransaction transaction) throws Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException, Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException, Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private void InternalAdd( T IEntity, string key, object value, KernelTransaction transaction )
		 {
			  Type.add( transaction.IndexWrite(), NameConflict, Type.id(entity), key, value );
		 }

		 public override string ToString()
		 {
			  return "Index[" + Type + ", " + NameConflict + "]";
		 }

		 private abstract class AbstractCursorWrappingIndexHits<T> : IndexHits<T> where T : Neo4Net.GraphDb.PropertyContainer
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

			  public override IResourceIterator<T> Iterator()
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
			  internal readonly IGraphDatabaseService IGraphDatabaseService;
			  internal readonly KernelTransaction Ktx;
			  internal readonly string Name;

			  internal CursorWrappingNodeIndexHits( NodeExplicitIndexCursor cursor, IGraphDatabaseService IGraphDatabaseService, KernelTransaction ktx, string name ) : base( cursor.ExpectedTotalNumberOfResults(), cursor.Score() )
			  {
					this.Cursor = cursor;
					this.GraphDatabaseService = IGraphDatabaseService;
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
					return IGraphDatabaseService.getNodeById( id );
			  }
		 }

		 protected internal class CursorWrappingRelationshipIndexHits : AbstractCursorWrappingIndexHits<Relationship>
		 {
			  internal readonly RelationshipExplicitIndexCursor Cursor;
			  internal readonly IGraphDatabaseService IGraphDatabaseService;
			  internal readonly KernelTransaction Ktx;
			  internal readonly string Name;

			  internal CursorWrappingRelationshipIndexHits( RelationshipExplicitIndexCursor cursor, IGraphDatabaseService IGraphDatabaseService, KernelTransaction ktx, string name ) : base( cursor.ExpectedTotalNumberOfResults(), cursor.Score() )
			  {
					this.Cursor = cursor;
					this.GraphDatabaseService = IGraphDatabaseService;
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
					return IGraphDatabaseService.getRelationshipById( id );
			  }
		 }
	}

}