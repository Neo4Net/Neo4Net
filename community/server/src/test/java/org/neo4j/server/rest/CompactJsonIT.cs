﻿using System;

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
namespace Org.Neo4j.Server.rest
{
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Org.Neo4j.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using CompactJsonFormat = Org.Neo4j.Server.rest.repr.formats.CompactJsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CompactJsonIT : AbstractRestFunctionalTestBase
	{
		 private long _thomasAnderson;
		 private long _trinity;
		 private long _thomasAndersonLovesTrinity;

		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  CreateTheMatrix();
		 }

		 private void CreateTheMatrix()
		 {
			  // Create the matrix example
			  _thomasAnderson = CreateAndIndexNode( "Thomas Anderson" );
			  _trinity = CreateAndIndexNode( "Trinity" );
			  long tank = CreateAndIndexNode( "Tank" );

			  long knowsRelationshipId = _helper.createRelationship( "KNOWS", _thomasAnderson, _trinity );
			  _thomasAndersonLovesTrinity = _helper.createRelationship( "LOVES", _thomasAnderson, _trinity );
			  _helper.setRelationshipProperties( _thomasAndersonLovesTrinity, Collections.singletonMap( "strength", 100 ) );
			  _helper.createRelationship( "KNOWS", _thomasAnderson, tank );
			  _helper.createRelationship( "KNOWS", _trinity, tank );

			  // index a relationship
			  _helper.createRelationshipIndex( "relationships" );
			  _helper.addRelationshipToIndex( "relationships", "key", "value", knowsRelationshipId );

			  // index a relationship
			  _helper.createRelationshipIndex( "relationships2" );
			  _helper.addRelationshipToIndex( "relationships2", "key2", "value2", knowsRelationshipId );
		 }

		 private long CreateAndIndexNode( string name )
		 {
			  long id = _helper.createNode();
			  _helper.setNodeProperties( id, Collections.singletonMap( "name", name ) );
			  _helper.addNodeToIndex( "node", "name", name, id );
			  return id;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetThomasAndersonDirectly()
		 public virtual void ShouldGetThomasAndersonDirectly()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.nodeUri(_thomasAnderson), CompactJsonFormat.MEDIA_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string entity = response.Entity;
			  assertTrue( entity.Contains( "Thomas Anderson" ) );
			  AssertValidJson( entity );
			  response.Close();
		 }

		 private void AssertValidJson( string entity )
		 {
			  try
			  {
					assertTrue( JsonHelper.jsonToMap( entity ).ContainsKey( "self" ) );
					assertFalse( JsonHelper.jsonToMap( entity ).ContainsKey( "properties" ) );
			  }
			  catch ( JsonParseException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
		 }
	}

}