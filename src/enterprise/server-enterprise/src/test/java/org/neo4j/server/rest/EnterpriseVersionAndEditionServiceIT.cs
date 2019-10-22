﻿/*
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
namespace Neo4Net.Server.rest
{
	using Test = org.junit.Test;

	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using VersionAndEditionService = Neo4Net.Server.rest.management.VersionAndEditionService;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	/*
	Note that when running this test from within an IDE, the version field will be an empty string. This is because the
	code that generates the version identifier is written by Maven as part of the build process(!). The tests will pass
	both in the IDE (where the empty string will be correctly compared).
	 */
	public class EnterpriseVersionAndEditionServiceIT : EnterpriseVersionIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportEnterpriseEdition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportEnterpriseEdition()
		 {
			  // Given
			  string releaseVersion = Server.Database.Graph.DependencyResolver.resolveDependency( typeof( KernelData ) ).version().ReleaseVersion;

			  // When
			  HTTP.Response res = HTTP.GET( FunctionalTestHelper.managementUri() + "/" + VersionAndEditionService.SERVER_PATH );

			  // Then
			  assertEquals( 200, res.Status() );
			  assertThat( res.Get( "edition" ).asText(), equalTo("enterprise") );
			  assertThat( res.Get( "version" ).asText(), equalTo(releaseVersion) );
		 }
	}

}