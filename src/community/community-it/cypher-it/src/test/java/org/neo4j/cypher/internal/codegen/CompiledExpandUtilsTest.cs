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
namespace Neo4Net.Cypher.Internal.codegen
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using Write = Neo4Net.Internal.Kernel.Api.Write;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cypher.Internal.codegen.CompiledExpandUtils.nodeGetDegreeIfDense;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@implicit;

	public class CompiledExpandUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.EmbeddedDatabaseRule().withSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "1");
		 public DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, "1");

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.internal.kernel.api.Transaction transaction() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
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