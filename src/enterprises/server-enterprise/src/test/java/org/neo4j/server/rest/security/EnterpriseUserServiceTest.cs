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
namespace Neo4Net.Server.rest.security
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using UserServiceTest = Neo4Net.Server.rest.dbms.UserServiceTest;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using MultiRealmAuthManagerRule = Neo4Net.Server.security.enterprise.auth.MultiRealmAuthManagerRule;
	using ShiroSubject = Neo4Net.Server.security.enterprise.auth.ShiroSubject;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class EnterpriseUserServiceTest : UserServiceTest
	{
		private bool InstanceFieldsInitialized = false;

		public EnterpriseUserServiceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			AuthManagerRule = new MultiRealmAuthManagerRule( UserRepository, mock( typeof( AuthenticationStrategy ) ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.server.security.enterprise.auth.MultiRealmAuthManagerRule authManagerRule = new org.neo4j.server.security.enterprise.auth.MultiRealmAuthManagerRule(userRepository, mock(org.neo4j.server.security.auth.AuthenticationStrategy.class));
		 public MultiRealmAuthManagerRule AuthManagerRule;

		 protected internal override void SetupAuthManagerAndSubject()
		 {
			  UserManagerSupplier = AuthManagerRule.Manager;

			  ShiroSubject shiroSubject = mock( typeof( ShiroSubject ) );
			  when( shiroSubject.Principal ).thenReturn( "neo4j" );
			  Neo4jContext = AuthManagerRule.makeLoginContext( shiroSubject );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogPasswordChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogPasswordChange()
		 {
			  ShouldChangePasswordAndReturnSuccess();

			  MultiRealmAuthManagerRule.FullSecurityLog fullLog = AuthManagerRule.getFullSecurityLog();
			  fullLog.AssertHasLine( "neo4j", "changed password" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailedPasswordChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailedPasswordChange()
		 {
			  ShouldReturn422IfPasswordIdentical();

			  MultiRealmAuthManagerRule.FullSecurityLog fullLog = AuthManagerRule.getFullSecurityLog();
			  fullLog.AssertHasLine( "neo4j", "tried to change password: Old password and new password cannot be the same." );
		 }
	}

}