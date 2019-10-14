using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{
	using Test = org.junit.Test;


	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using Result = Neo4Net.Graphdb.Result;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using EnterpriseSecurityContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class EmbeddedBuiltInProceduresInteractionIT : BuiltInProceduresInteractionTestBase<EnterpriseLoginContext>
	{

		 protected internal override object ValueOf( object obj )
		 {
			  if ( obj is int? )
			  {
					return ( ( int? ) obj ).Value;
			  }
			  else
			  {
					return obj;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected NeoInteractionLevel<org.neo4j.kernel.enterprise.api.security.EnterpriseLoginContext> setUpNeoServer(java.util.Map<String, String> config) throws Throwable
		 protected internal override NeoInteractionLevel<EnterpriseLoginContext> setUpNeoServer( IDictionary<string, string> config )
		 {
			  return new EmbeddedInteraction( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListAnyQueriesIfNotAuthenticated()
		 public virtual void ShouldNotListAnyQueriesIfNotAuthenticated()
		 {
			  EnterpriseLoginContext unAuthSubject = CreateFakeAnonymousEnterpriseLoginContext();
			  GraphDatabaseFacade graph = Neo.LocalGraph;

			  using ( InternalTransaction tx = graph.BeginTransaction( KernelTransaction.Type.@explicit, unAuthSubject ) )
			  {
					Result result = graph.execute( tx, "CALL dbms.listQueries", EMPTY_MAP );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotKillQueryIfNotAuthenticated() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotKillQueryIfNotAuthenticated()
		 {
			  EnterpriseLoginContext unAuthSubject = CreateFakeAnonymousEnterpriseLoginContext();

			  GraphDatabaseFacade graph = Neo.LocalGraph;
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<EnterpriseLoginContext> read = new ThreadedTransaction<EnterpriseLoginContext>( Neo, latch );
			  string query = read.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );

			  latch.StartAndWaitForAllToStart();

			  string id = ExtractQueryId( query );

			  try
			  {
					  using ( InternalTransaction tx = graph.BeginTransaction( KernelTransaction.Type.@explicit, unAuthSubject ) )
					  {
						graph.execute( tx, "CALL dbms.killQuery('" + id + "')", EMPTY_MAP );
						throw new AssertionError( "Expected exception to be thrown" );
					  }
			  }
			  catch ( QueryExecutionException e )
			  {
					assertThat( e.Message, containsString( PERMISSION_DENIED ) );
			  }

			  latch.FinishAndWaitForAllToFinish();
			  read.CloseAndAssertSuccess();
		 }

		 private EnterpriseLoginContext CreateFakeAnonymousEnterpriseLoginContext()
		 {
			  return new EnterpriseLoginContextAnonymousInnerClass( this );
		 }

		 private class EnterpriseLoginContextAnonymousInnerClass : EnterpriseLoginContext
		 {
			 private readonly EmbeddedBuiltInProceduresInteractionIT _outerInstance;

			 public EnterpriseLoginContextAnonymousInnerClass( EmbeddedBuiltInProceduresInteractionIT outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 inner = AnonymousContext.none().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME);
			 }

			 public EnterpriseSecurityContext authorize( System.Func<string, int> propertyIdLookup, string dbName )
			 {
				  return new EnterpriseSecurityContext( subject(), inner.mode(), Collections.emptySet(), false );
			 }

			 public ISet<string> roles()
			 {
				  return Collections.emptySet();
			 }

			 internal SecurityContext inner;

			 public AuthSubject subject()
			 {
				  return inner.subject();
			 }
		 }
	}

}