using System.Collections.Generic;

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


	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Org.Neo4j.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using Graph = Org.Neo4j.Test.GraphDescription.Graph;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class SetRelationshipPropertiesIT : AbstractRestFunctionalDocTestBase
	{
		 private URI _propertiesUri;
		 private URI _badUri;

		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupTheDatabase()
		 {
			  long relationshipId = ( new GraphDbHelper( Server().Database ) ).createRelationship("KNOWS");
			  _propertiesUri = new URI( _functionalTestHelper.relationshipPropertiesUri( relationshipId ) );
			  _badUri = new URI( _functionalTestHelper.relationshipPropertiesUri( relationshipId + 1 * 99999 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Update relationship properties.") @Test @Graph public void shouldReturn204WhenPropertiesAreUpdated()
		 [Documented("Update relationship properties.")]
		 public virtual void ShouldReturn204WhenPropertiesAreUpdated()
		 {
			  Data.get();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = "tobias";
			  GenConflict.get().payload(JsonHelper.createJsonFrom(map)).expectedStatus(204).put(_propertiesUri.ToString());
			  JaxRsResponse response = UpdatePropertiesOnServer( map );
			  assertEquals( 204, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn400WhenSendinIncompatibleJsonProperties()
		 public virtual void ShouldReturn400WhenSendinIncompatibleJsonProperties()
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = new Dictionary<string, object>();
			  JaxRsResponse response = UpdatePropertiesOnServer( map );
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn400WhenSendingCorruptJsonProperties()
		 public virtual void ShouldReturn400WhenSendingCorruptJsonProperties()
		 {
			  JaxRsResponse response = RestRequest.Req().put(_propertiesUri.ToString(), "this:::Is::notJSON}");
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 public virtual void ShouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = "tobias";

			  JaxRsResponse response = RestRequest.Req().put(_badUri.ToString(), JsonHelper.createJsonFrom(map));
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse updatePropertiesOnServer(final java.util.Map<String,Object> map)
		 private JaxRsResponse UpdatePropertiesOnServer( IDictionary<string, object> map )
		 {
			  return RestRequest.Req().put(_propertiesUri.ToString(), JsonHelper.createJsonFrom(map));
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getPropertyUri(final String key)
		 private string GetPropertyUri( string key )
		 {
			  return _propertiesUri.ToString() + "/" + key;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn204WhenPropertyIsSet()
		 public virtual void ShouldReturn204WhenPropertyIsSet()
		 {
			  JaxRsResponse response = SetPropertyOnServer( "foo", "bar" );
			  assertEquals( 204, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn400WhenSendinIncompatibleJsonProperty()
		 public virtual void ShouldReturn400WhenSendinIncompatibleJsonProperty()
		 {
			  JaxRsResponse response = SetPropertyOnServer( "jim", new Dictionary<string, object>() );
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn400WhenSendingCorruptJsonProperty()
		 public virtual void ShouldReturn400WhenSendingCorruptJsonProperty()
		 {
			  JaxRsResponse response = RestRequest.Req().put(GetPropertyUri("foo"), "this:::Is::notJSON}");
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 public virtual void ShouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 {
			  JaxRsResponse response = RestRequest.Req().put(_badUri.ToString() + "/foo", JsonHelper.createJsonFrom("bar"));
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse setPropertyOnServer(final String key, final Object value)
		 private JaxRsResponse SetPropertyOnServer( string key, object value )
		 {
			  return RestRequest.Req().put(GetPropertyUri(key), JsonHelper.createJsonFrom(value));
		 }
	}

}