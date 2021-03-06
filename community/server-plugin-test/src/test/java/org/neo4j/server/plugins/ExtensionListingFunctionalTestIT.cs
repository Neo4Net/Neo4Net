﻿using System.Collections.Generic;

/*
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
namespace Org.Neo4j.Server.plugins
{
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using ServerHelper = Org.Neo4j.Server.helpers.ServerHelper;
	using JaxRsResponse = Org.Neo4j.Server.rest.JaxRsResponse;
	using RestRequest = Org.Neo4j.Server.rest.RestRequest;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using SharedServerTestBase = Org.Neo4j.Test.server.SharedServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ExtensionListingFunctionalTestIT : SharedServerTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( SharedServerTestBase.server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase()
		 public virtual void CleanTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( SharedServerTestBase.server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void datarootContainsReferenceToExtensions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatarootContainsReferenceToExtensions()
		 {
			  JaxRsResponse response = RestRequest.req().get(_functionalTestHelper.dataUri());
			  assertThat( response.Status, equalTo( 200 ) );
			  IDictionary<string, object> json = JsonHelper.jsonToMap( response.Entity );
			  new URI( ( string ) json["extensions_info"] ); // throws on error
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canListAllAvailableServerExtensions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanListAllAvailableServerExtensions()
		 {
			  JaxRsResponse response = RestRequest.req().get(_functionalTestHelper.extensionUri());
			  assertThat( response.Status, equalTo( 200 ) );
			  IDictionary<string, object> json = JsonHelper.jsonToMap( response.Entity );
			  assertFalse( json.Count == 0 );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void canListExtensionMethodsForServerExtension() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanListExtensionMethodsForServerExtension()
		 {
			  JaxRsResponse response = RestRequest.req().get(_functionalTestHelper.extensionUri());
			  assertThat( response.Status, equalTo( 200 ) );

			  IDictionary<string, object> json = JsonHelper.jsonToMap( response.Entity );
			  string refNodeService = ( string ) json[typeof( FunctionalTestPlugin ).Name];
			  response.Close();

			  response = RestRequest.req().get(refNodeService);
			  string result = response.Entity;

			  assertThat( response.Status, equalTo( 200 ) );

			  json = JsonHelper.jsonToMap( result );
			  json = ( IDictionary<string, object> ) json["graphdb"];
			  assertThat( json, hasKey( FunctionalTestPlugin.CREATE_NODE ) );
			  response.Close();
		 }
	}

}