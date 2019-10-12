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

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.index;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;
	using RelationshipExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipExplicitIndexCursor;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_NODE;

	public class RelationshipExplicitIndexProxy : ExplicitIndexProxy<Relationship>, RelationshipIndex
	{
		 public RelationshipExplicitIndexProxy( string name, GraphDatabaseService gds, System.Func<KernelTransaction> txBridge ) : base( name, RELATIONSHIP, gds, txBridge )
		 {
		 }

		 public override IndexHits<Relationship> Get( string key, object valueOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
						long source = startNodeOrNull == null ? NO_SUCH_NODE : startNodeOrNull.Id;
						long target = endNodeOrNull == null ? NO_SUCH_NODE : endNodeOrNull.Id;
						ktx.IndexRead().relationshipExplicitIndexLookup(cursor, NameConflict, key, valueOrNull, source, target);
						return new CursorWrappingRelationshipIndexHits( cursor, GraphDatabase, ktx, NameConflict );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override IndexHits<Relationship> Query( string key, object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
						long source = startNodeOrNull == null ? NO_SUCH_NODE : startNodeOrNull.Id;
						long target = endNodeOrNull == null ? NO_SUCH_NODE : endNodeOrNull.Id;
						ktx.IndexRead().relationshipExplicitIndexQuery(cursor, NameConflict, key, queryOrQueryObjectOrNull, source, target);
						return new CursorWrappingRelationshipIndexHits( cursor, GraphDatabase, ktx, NameConflict );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }

		 public override IndexHits<Relationship> Query( object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  KernelTransaction ktx = TxBridge.get();
			  try
			  {
					  using ( Statement ignore = ktx.AcquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = ktx.Cursors().allocateRelationshipExplicitIndexCursor();
						long source = startNodeOrNull == null ? NO_SUCH_NODE : startNodeOrNull.Id;
						long target = endNodeOrNull == null ? NO_SUCH_NODE : endNodeOrNull.Id;
						ktx.IndexRead().relationshipExplicitIndexQuery(cursor, NameConflict, queryOrQueryObjectOrNull, source, target);
						return new CursorWrappingRelationshipIndexHits( cursor, GraphDatabase, ktx, NameConflict );
					  }
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					throw new NotFoundException( Type + " index '" + NameConflict + "' doesn't exist" );
			  }
		 }
	}

}