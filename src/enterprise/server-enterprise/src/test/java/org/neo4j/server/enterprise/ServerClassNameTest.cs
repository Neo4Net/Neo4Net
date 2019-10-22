using System;

/// <summary>
/// See: https://raw.githubusercontent.com/Neo4Net/Neo4Net/3.4/enterprise/server-enterprise/src/test/java/org/Neo4Net/server/enterprise/ServerClassNameTest.java
/// </summary>
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

namespace Neo4Net.Server.enterprise
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// The classes that extend AbstractNeoServer are currently known to be:
	/// CommunityNeoServer and EnterpriseNeoServer
	/// <para>
	/// This test asserts that those names won't change, for example during an
	/// otherwise perfectly reasonable refactoring. Changing those names will cause
	/// problems for the server which relies on those names to yield the correct
	/// Neo4Net edition (community, enterprise) to the Web UI and other clients.
	/// </para>
	/// <para>
	/// Although this test asserts naming against classes in other modules (Neo4Net),
	/// it lives in Neo4Net-enterprise because otherwise the CommunityNeoServer
	/// and EnterpriseNeoServer would not be visible.
	/// </para>
	/// </summary>
	public class ServerClassNameTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainNamingOfCommunityNeoServerSoThatTheNeo4NetEditionIsCorrectlyShownToRESTAPICallers()
		 public virtual void ShouldMaintainNamingOfCommunityNeoServerSoThatTheNeo4NetEditionIsCorrectlyShownToRESTAPICallers()
		 {
			  assertEquals( GetErrorMessage( typeof( CommunityNeoServer ) ), "communityneoserver", typeof( CommunityNeoServer ).Name.ToLower() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainNamingOfEnterpriseNeoServerSoThatTheNeo4NetEditionIsCorrectlyShownToRESTAPICallers()
		 public virtual void ShouldMaintainNamingOfEnterpriseNeoServerSoThatTheNeo4NetEditionIsCorrectlyShownToRESTAPICallers()
		 {
			  assertEquals( GetErrorMessage( typeof( EnterpriseNeoServer ) ), "enterpriseneoserver", typeof( EnterpriseNeoServer ).Name.ToLower() );
		 }

		 private string GetErrorMessage( Type neoServerClass )
		 {
			  return "The " + neoServerClass.Name + " class appears to have been renamed. There is a strict " +
						 "dependency from the REST API VersionAndEditionService on the name of that class. If you want " +
						 "to change the name of that class, then remember to change VersionAndEditionService, " +
						 "VersionAndEditionServiceTest and, of course, this test. ";
		 }
	}

}