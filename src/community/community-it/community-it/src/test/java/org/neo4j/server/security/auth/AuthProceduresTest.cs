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
namespace Neo4Net.Server.Security.Auth
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
	using ResourceTracker = Neo4Net.Kernel.Api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.Api.StubResourceManager;
	using AnonymousContext = Neo4Net.Kernel.Api.security.AnonymousContext;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureName;

	public class AuthProceduresTest : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDeprecatedChangePasswordWithStaticAccessModeInDbmsMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenDeprecatedChangePasswordWithStaticAccessModeInDbmsMode()
		 {
			  // Given
			  object[] inputArray = new object[1];
			  inputArray[0] = "newPassword";

			  // Then
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Anonymous cannot change password" );

			  // When
			  DbmsOperations().procedureCallDbms(procedureName("dbms", "changePassword"), inputArray, DependencyResolver, AnonymousContext.none().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), _resourceTracker, ProcedureCallContext.EMPTY);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenChangePasswordWithStaticAccessModeInDbmsMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenChangePasswordWithStaticAccessModeInDbmsMode()
		 {
			  // Given
			  object[] inputArray = new object[1];
			  inputArray[0] = "newPassword";

			  // Then
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Anonymous cannot change password" );

			  // When
			  DbmsOperations().procedureCallDbms(procedureName("dbms", "security", "changePassword"), inputArray, DependencyResolver, AnonymousContext.none().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), _resourceTracker, ProcedureCallContext.EMPTY);
		 }

		 protected internal override GraphDatabaseBuilder Configure( GraphDatabaseBuilder graphDatabaseBuilder )
		 {
			  graphDatabaseBuilder.SetConfig( GraphDatabaseSettings.auth_enabled, "true" );
			  return graphDatabaseBuilder;
		 }
	}

}