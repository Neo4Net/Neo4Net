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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{

	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using AddNodeCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using CreateCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.CreateCommand;
	using DeleteCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.DeleteCommand;
	using RemoveCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using IndexImplementation = Org.Neo4j.Kernel.spi.explicitindex.IndexImplementation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.Index.impl.lucene.@explicit.EntityId_IdData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.Index.impl.lucene.@explicit.EntityId_RelationshipData;

	/// <summary>
	/// Applies changes from <seealso cref="IndexCommand commands"/> onto one or more indexes from the same
	/// <seealso cref="IndexImplementation provider"/>.
	/// </summary>
	public class LuceneCommandApplier : Org.Neo4j.Kernel.Impl.Api.TransactionApplier_Adapter
	{
		 private readonly LuceneDataSource _dataSource;
		 private readonly IDictionary<string, CommitContext> _nodeContexts = new Dictionary<string, CommitContext>();
		 private readonly IDictionary<string, CommitContext> _relationshipContexts = new Dictionary<string, CommitContext>();
		 private readonly bool _recovery;
		 private IndexDefineCommand _definitions;

		 public LuceneCommandApplier( LuceneDataSource dataSource, bool recovery )
		 {
			  this._dataSource = dataSource;
			  this._recovery = recovery;
		 }

		 public override bool VisitIndexAddNodeCommand( IndexCommand.AddNodeCommand command )
		 {
			  IdData entityId = new IdData( command.EntityId );
			  return VisitIndexAddCommand( command, entityId );
		 }

		 public override bool VisitIndexAddRelationshipCommand( IndexCommand.AddRelationshipCommand command )
		 {
			  RelationshipData entityId = new RelationshipData( command.EntityId, command.StartNode, command.EndNode );
			  return VisitIndexAddCommand( command, entityId );
		 }

		 private bool VisitIndexAddCommand( IndexCommand command, EntityId entityId )
		 {
			  try
			  {
					CommitContext context = CommitContext( command );
					string key = _definitions.getKey( command.KeyId );
					object value = command.Value;

					// Below is a check for a null value where such a value is ignored. This may look strange, but the
					// reason is that there was this bug where adding a null value to an index would be fine and written
					// into the log as a command, to later fail during application of that command, i.e. here.
					// There was a fix introduced to throw IllegalArgumentException out to user right away if passing in
					// null or object that had toString() produce null. Although databases already affected by this would
					// not be able to recover, which is why this check is here.
					if ( value != null )
					{
						 context.EnsureWriterInstantiated();
						 context.IndexType.addToDocument( context.GetDocument( entityId, true ).Document, key, value );
					}
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					// Pretend the index never existed.
			  }
			  return false;
		 }

		 public override bool VisitIndexRemoveCommand( IndexCommand.RemoveCommand command )
		 {
			  try
			  {
					CommitContext context = CommitContext( command );
					string key = _definitions.getKey( command.KeyId );
					object value = command.Value;
					context.EnsureWriterInstantiated();
					CommitContext.DocumentContext document = context.GetDocument( new IdData( command.EntityId ), false );
					if ( document != null )
					{
						 context.IndexType.removeFromDocument( document.Document, key, value );
					}
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					// Pretend the index never existed.
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDeleteCommand(org.neo4j.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException
		 public override bool VisitIndexDeleteCommand( IndexCommand.DeleteCommand command )
		 {
			  try
			  {
					CommitContext context = CommitContext( command );
					context.Documents.clear();
					context.DataSource.deleteIndex( context.Identifier, context.Recovery );
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					// Pretend the index never existed.
			  }
			  return false;
		 }

		 public override bool VisitIndexCreateCommand( IndexCommand.CreateCommand createCommand )
		 {
			  return false;
		 }

		 public override bool VisitIndexDefineCommand( IndexDefineCommand indexDefineCommand )
		 {
			  _definitions = indexDefineCommand;
			  return false;
		 }

		 public override void Close()
		 {
			  try
			  {
					if ( _definitions != null )
					{
						 _dataSource.WriteLock;
						 foreach ( CommitContext context in _nodeContexts.Values )
						 {
							  context.Dispose();
						 }
						 foreach ( CommitContext context in _relationshipContexts.Values )
						 {
							  context.Dispose();
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failure to commit changes to lucene", e );
			  }
			  finally
			  {
					if ( _definitions != null )
					{
						 _dataSource.releaseWriteLock();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private CommitContext commitContext(org.neo4j.kernel.impl.index.IndexCommand command) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private CommitContext CommitContext( IndexCommand command )
		 {
			  IDictionary<string, CommitContext> contextMap = CommitContextMap( command.EntityType );
			  string indexName = _definitions.getIndexName( command.IndexNameId );
			  CommitContext context = contextMap[indexName];
			  if ( context == null )
			  {
					IndexIdentifier identifier = new IndexIdentifier( IndexEntityType.byId( command.EntityType ), indexName );

					// TODO the fact that we look up index type from config here using the index store
					// directly should be avoided. But how can we do it in, say recovery?
					// The `dataSource.getType()` call can throw an exception if the index is concurrently deleted.
					// To avoid bubbling an exception during commit, we instead ignore the commands related to that index,
					// and proceed as if the index never existed, and thus cannot accept any modifications.
					IndexType type = _dataSource.getType( identifier, _recovery );
					context = new CommitContext( _dataSource, identifier, type, _recovery );
					contextMap[indexName] = context;
			  }
			  return context;
		 }

		 private IDictionary<string, CommitContext> CommitContextMap( sbyte entityType )
		 {
			  if ( entityType == IndexEntityType.Node.id() )
			  {
					return _nodeContexts;
			  }
			  if ( entityType == IndexEntityType.Relationship.id() )
			  {
					return _relationshipContexts;
			  }
			  throw new System.ArgumentException( "Unknown entity type " + entityType );
		 }
	}

}