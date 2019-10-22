/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using Test = org.junit.Test;

	using Driver = Neo4Net.driver.v1.Driver;

	public abstract class AuthTestBase : EnterpriseAuthenticationTestBase
	{
		 internal const string NONE_USER = "smith";
		 internal const string READ_USER = "neo";
		 internal const string WRITE_USER = "tank";
		 internal const string PROC_USER = "jane";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoginWithCorrectInformation()
		 public virtual void ShouldLoginWithCorrectInformation()
		 {
			  AssertAuth( READ_USER, Password );
			  AssertAuth( READ_USER, Password );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailLoginWithIncorrectCredentials()
		 public virtual void ShouldFailLoginWithIncorrectCredentials()
		 {
			  AssertAuthFail( READ_USER, "WRONG" );
			  AssertAuthFail( READ_USER, "ALSO WRONG" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailLoginWithInvalidCredentialsFollowingSuccessfulLogin()
		 public virtual void ShouldFailLoginWithInvalidCredentialsFollowingSuccessfulLogin()
		 {
			  AssertAuth( READ_USER, Password );
			  AssertAuthFail( READ_USER, "WRONG" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoginFollowingFailedLogin()
		 public virtual void ShouldLoginFollowingFailedLogin()
		 {
			  AssertAuthFail( READ_USER, "WRONG" );
			  AssertAuth( READ_USER, Password );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectAuthorizationNoPermission()
		 public virtual void ShouldGetCorrectAuthorizationNoPermission()
		 {
			  using ( Driver driver = ConnectDriver( NONE_USER, Password ) )
			  {
					AssertReadFails( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectAuthorizationReaderUser()
		 public virtual void ShouldGetCorrectAuthorizationReaderUser()
		 {
			  using ( Driver driver = ConnectDriver( READ_USER, Password ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectAuthorizationWriteUser()
		 public virtual void ShouldGetCorrectAuthorizationWriteUser()
		 {
			  using ( Driver driver = ConnectDriver( WRITE_USER, Password ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteSucceeds( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectAuthorizationAllowedProcedure()
		 public virtual void ShouldGetCorrectAuthorizationAllowedProcedure()
		 {
			  using ( Driver driver = ConnectDriver( PROC_USER, Password ) )
			  {
					AssertProcSucceeds( driver );
					AssertReadFails( driver );
					AssertWriteFails( driver );
			  }
		 }

		 protected internal abstract string Password { get; }
	}

}