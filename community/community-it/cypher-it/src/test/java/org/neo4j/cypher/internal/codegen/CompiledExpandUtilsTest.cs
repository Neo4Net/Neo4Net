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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using Write = Org.Neo4j.@internal.Kernel.Api.Write;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cypher.@internal.codegen.CompiledExpandUtils.nodeGetDegreeIfDense;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;

	public class CompiledExpandUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "1");
		 public DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, "1");

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.Transaction transaction() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private Transaction Transaction()
		 {
			  DependencyResolver resolver = this.Db.DependencyResolver;
			  return resolver.ResolveDependency( typeof( Kernel ) ).beginTransaction( @implicit, LoginContext.AUTH_DISABLED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeDegreeWithoutType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComputeDegreeWithoutType()
		 {
			  // GIVEN
			  long node;
			  using ( Transaction tx = Transaction() )
			  {
					Write write = tx.DataWrite();
					node = write.NodeCreate();
					write.RelationshipCreate( node, tx.TokenWrite().relationshipTypeGetOrCreateForName("R1"), write.NodeCreate() );
					write.RelationshipCreate( node, tx.TokenWrite().relationshipTypeGetOrCreateForName("R2"), write.NodeCreate() );
					write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R3"), node );
					write.RelationshipCreate( node, tx.TokenWrite().relationshipTypeGetOrCreateForName("R4"), node );

					tx.Success();
			  }

			  using ( Transaction tx = Transaction() )
			  {
					Read read = tx.DataRead();
					CursorFactory cursors = tx.Cursors();
					using ( NodeCursor nodes = cursors.AllocateNodeCursor() )
					{
						 assertThat( CompiledExpandUtils.NodeGetDegreeIfDense( read, node, nodes, cursors, OUTGOING ), equalTo( 3 ) );
						 assertThat( CompiledExpandUtils.NodeGetDegreeIfDense( read, node, nodes, cursors, INCOMING ), equalTo( 2 ) );
						 assertThat( CompiledExpandUtils.NodeGetDegreeIfDense( read, node, nodes, cursors, BOTH ), equalTo( 4 ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeDegreeWithType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComputeDegreeWithType()
		 {
			  // GIVEN
			  long node;
			  int @in, @out, loop;
			  using ( Transaction tx = Transaction() )
			  {
					Write write = tx.DataWrite();
					node = write.NodeCreate();
					TokenWrite tokenWrite = tx.TokenWrite();
					@out = tokenWrite.RelationshipTypeGetOrCreateForName( "OUT" );
					@in = tokenWrite.RelationshipTypeGetOrCreateForName( "IN" );
					loop = tokenWrite.RelationshipTypeGetOrCreateForName( "LOOP" );
					write.RelationshipCreate( node, @out, write.NodeCreate() );
					write.RelationshipCreate( node, @out, write.NodeCreate() );
					write.RelationshipCreate( write.NodeCreate(), @in, node );
					write.RelationshipCreate( node, loop, node );

					tx.Success();
			  }

			  using ( Transaction tx = Transaction() )
			  {
					Read read = tx.DataRead();
					CursorFactory cursors = tx.Cursors();
					using ( NodeCursor nodes = cursors.AllocateNodeCursor() )
					{
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, OUTGOING, @out ), equalTo( 2 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, OUTGOING, @in ), equalTo( 0 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, OUTGOING, loop ), equalTo( 1 ) );

						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, INCOMING, @out ), equalTo( 0 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, INCOMING, @in ), equalTo( 1 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, INCOMING, loop ), equalTo( 1 ) );

						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, BOTH, @out ), equalTo( 2 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, BOTH, @in ), equalTo( 1 ) );
						 assertThat( nodeGetDegreeIfDense( read, node, nodes, cursors, BOTH, loop ), equalTo( 1 ) );
					}
			  }
		 }
	}

}