using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using Test = org.junit.Test;


	using Notification = Org.Neo4j.Graphdb.Notification;
	using Result = Org.Neo4j.Graphdb.Result;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using SecuritySettings = Org.Neo4j.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.EDITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public abstract class ConfiguredAuthScenariosInteractionTestBase<S> : ProcedureInteractionTestBase<S>
	{
		private bool InstanceFieldsInitialized = false;

		public ConfiguredAuthScenariosInteractionTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_userList = map( "adminSubject", ListOf( ADMIN ), "readSubject", ListOf( READER ), "schemaSubject", ListOf( ARCHITECT ), "writeSubject", ListOf( PUBLISHER ), "editorSubject", ListOf( EDITOR ), "pwdSubject", ListOf(), "noneSubject", ListOf(), "neo4j", ListOf(ADMIN) );
		}

		 public override void SetUp()
		 {
			  // tests are required to setup database with specific configs
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRoleCallCreateNewTokensProceduresWhenConfigured() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRoleCallCreateNewTokensProceduresWhenConfigured()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.default_allowed.name(), "role1" ) );
			  UserManager.newRole( "role1", "noneSubject" );
			  AssertEmpty( NoneSubject, "CALL db.createLabel('MySpecialLabel')" );
			  AssertEmpty( NoneSubject, "CALL db.createRelationshipType('MySpecialRelationship')" );
			  AssertEmpty( NoneSubject, "CALL db.createProperty('MySpecialProperty')" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnWhenUsingInternalAndOtherProvider() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnWhenUsingInternalAndOtherProvider()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.auth_providers.name(), InternalSecurityName() + " ,LDAP" ) );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsers", r => assertKeyIsMap( r, "username", "roles", ValueOf( _userList ) ) );
			  GraphDatabaseFacade localGraph = Neo.LocalGraph;
			  InternalTransaction transaction = localGraph.BeginTransaction( KernelTransaction.Type.@explicit, StandardEnterpriseLoginContext.AUTH_DISABLED );
			  Result result = localGraph.execute( transaction, "EXPLAIN CALL dbms.security.listUsers", EMPTY_MAP );
			  string description = string.Format( "{0} ({1})", Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureWarning.code().description(), "dbms.security.listUsers only applies to native users." );
			  assertThat( ContainsNotification( result, description ), equalTo( true ) );
			  transaction.Success();
			  transaction.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWarnWhenOnlyUsingInternalProvider() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWarnWhenOnlyUsingInternalProvider()
		 {
			  ConfiguredSetup( stringMap( SecuritySettings.auth_provider.name(), InternalSecurityName() ) );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsers", r => assertKeyIsMap( r, "username", "roles", ValueOf( _userList ) ) );
			  GraphDatabaseFacade localGraph = Neo.LocalGraph;
			  InternalTransaction transaction = localGraph.BeginTransaction( KernelTransaction.Type.@explicit, StandardEnterpriseLoginContext.AUTH_DISABLED );
			  Result result = localGraph.execute( transaction, "EXPLAIN CALL dbms.security.listUsers", EMPTY_MAP );
			  string description = string.Format( "{0} ({1})", Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureWarning.code().description(), "dbms.security.listUsers only applies to native users." );
			  assertThat( ContainsNotification( result, description ), equalTo( false ) );
			  transaction.Success();
			  transaction.Close();
		 }

		 protected internal override object ValueOf( object obj )
		 {
			  return obj;
		 }

		 private IDictionary<string, object> _userList;

		 private bool ContainsNotification( Result result, string description )
		 {
			  IEnumerator<Notification> itr = result.Notifications.GetEnumerator();
			  bool found = false;
			  while ( itr.MoveNext() )
			  {
					found |= itr.Current.Description.Equals( description );
			  }
			  return found;
		 }

		 protected internal virtual string InternalSecurityName()
		 {
			  return SecuritySettings.NATIVE_REALM_NAME;
		 }
	}

}