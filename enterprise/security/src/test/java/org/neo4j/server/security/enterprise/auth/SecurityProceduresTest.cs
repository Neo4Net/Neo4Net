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
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using EnterpriseSecurityContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using UserResult = Org.Neo4j.Server.security.enterprise.auth.AuthProceduresBase.UserResult;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SecurityProceduresTest
	{

		 private SecurityProcedures _procedures;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  AuthSubject subject = mock( typeof( AuthSubject ) );
			  when( subject.Username() ).thenReturn("pearl");

			  EnterpriseSecurityContext ctx = mock( typeof( EnterpriseSecurityContext ) );
			  when( ctx.Subject() ).thenReturn(subject);
			  when( ctx.Roles() ).thenReturn(Collections.singleton("jammer"));

			  _procedures = new SecurityProcedures();
			  _procedures.securityContext = ctx;
			  _procedures.userManager = mock( typeof( EnterpriseUserManager ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSecurityContextRoles()
		 public virtual void ShouldReturnSecurityContextRoles()
		 {
			  IList<UserResult> infoList = _procedures.showCurrentUser().collect(Collectors.toList());
			  assertThat( infoList.Count, equalTo( 1 ) );

			  UserResult row = infoList[0];
			  assertThat( row.Username, equalTo( "pearl" ) );
			  assertThat( row.Roles, containsInAnyOrder( "jammer" ) );
			  assertThat( row.Flags, empty() );
		 }
	}

}