﻿/*
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
namespace Org.Neo4j.Server
{
	using Test = org.junit.Test;

	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using AbstractRestFunctionalTestBase = Org.Neo4j.Server.rest.AbstractRestFunctionalTestBase;
	using JaxRsResponse = Org.Neo4j.Server.rest.JaxRsResponse;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class NeoServerDefaultPortAndHostnameIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultToSensiblePortIfNoneSpecifiedInConfig()
		 public virtual void ShouldDefaultToSensiblePortIfNoneSpecifiedInConfig()
		 {
			  FunctionalTestHelper functionalTestHelper = new FunctionalTestHelper( Server() );

			  JaxRsResponse response = functionalTestHelper.Get( functionalTestHelper.ManagementUri() );

			  assertThat( response.Status, @is( 200 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultToLocalhostOfNoneSpecifiedInConfig()
		 public virtual void ShouldDefaultToLocalhostOfNoneSpecifiedInConfig()
		 {
			  assertThat( Server().baseUri().Host, @is("localhost") );
		 }
	}

}