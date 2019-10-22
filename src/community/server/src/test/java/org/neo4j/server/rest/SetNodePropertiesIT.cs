using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest
{
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Graph = Neo4Net.Test.GraphDescription.Graph;
	using NODE = Neo4Net.Test.GraphDescription.NODE;
	using PROP = Neo4Net.Test.GraphDescription.PROP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class SetNodePropertiesIT : AbstractRestFunctionalTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Graph("jim knows joe") @Documented("Update node properties.\n" + "\n" + "This will replace all existing properties on the node with the new set\n" + "of attributes.") @Test public void shouldReturn204WhenPropertiesAreUpdated()
		 [Graph("jim knows joe"), Documented("Update node properties.\n" + "\n" + "This will replace all existing properties on the node with the new set\n" + "of attributes.")]
		 public virtual void ShouldReturn204WhenPropertiesAreUpdated()
		 {
			  Node jim = Data.get()["jim"];
			  assertThat( jim, inTx( Graphdb(), not(hasProperty("age")) ) );
			  GenConflict.get().payload(JsonHelper.createJsonFrom(MapUtil.map("age", "18"))).expectedStatus(204).put(GetPropertiesUri(jim));
			  assertThat( jim, inTx( Graphdb(), hasProperty("age").withValue("18") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Graph("jim knows joe") @Test public void set_node_properties_in_Unicode()
		 [Graph("jim knows joe")]
		 public virtual void SetNodePropertiesInUnicode()
		 {
			  Node jim = Data.get()["jim"];
			  GenConflict.get().payload(JsonHelper.createJsonFrom(MapUtil.map("name", "\u4f8b\u5b50"))).expectedStatus(204).put(GetPropertiesUri(jim));
			  assertThat( jim, inTx( Graphdb(), hasProperty("name").withValue("\u4f8b\u5b50") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("jim knows joe") public void shouldReturn400WhenSendinIncompatibleJsonProperties()
		 [Graph("jim knows joe")]
		 public virtual void ShouldReturn400WhenSendinIncompatibleJsonProperties()
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = new Dictionary<string, object>();
			  GenConflict.get().payload(JsonHelper.createJsonFrom(map)).expectedStatus(400).put(GetPropertiesUri(Data.get()["jim"]));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("jim knows joe") public void shouldReturn400WhenSendingCorruptJsonProperties()
		 [Graph("jim knows joe")]
		 public virtual void ShouldReturn400WhenSendingCorruptJsonProperties()
		 {
			  JaxRsResponse response = RestRequest.Req().put(GetPropertiesUri(Data.get()["jim"]), "this:::Is::notJSON}");
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("jim knows joe") public void shouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 [Graph("jim knows joe")]
		 public virtual void ShouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 {
			  GenConflict.get().payload(JsonHelper.createJsonFrom(MapUtil.map("key", "val"))).expectedStatus(404).put(DataUri + "node/12345/properties");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URI getPropertyUri(org.Neo4Net.graphdb.Node node, String key) throws Exception
		 private URI GetPropertyUri( Node node, string key )
		 {
			  return new URI( GetPropertiesUri( node ) + "/" + key );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Set property on node.\n" + "\n" + "Setting different properties will retain the existing ones for this node.\n" + "Note that a single value are submitted not as a map but just as a value\n" + "(which is valid JSON) like in the example\n" + "below.") @Graph(nodes = {@NODE(name = "jim", properties = {@PROP(key = "foo2", value = "bar2")})}) @Test public void shouldReturn204WhenPropertyIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Set property on node.\n" + "\n" + "Setting different properties will retain the existing ones for this node.\n" + "Note that a single value are submitted not as a map but just as a value\n" + "(which is valid JSON) like in the example\n" + "below."), Graph(nodes : {@NODE(name : "jim", properties : {@PROP(key : "foo2", value : "bar2")})})]
		 public virtual void ShouldReturn204WhenPropertyIsSet()
		 {
			  Node jim = Data.get()["jim"];
			  GenConflict.get().payload(JsonHelper.createJsonFrom("bar")).expectedStatus(204).put(GetPropertyUri(jim, "foo").ToString());
			  assertThat( jim, inTx( Graphdb(), hasProperty("foo") ) );
			  assertThat( jim, inTx( Graphdb(), hasProperty("foo2") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Property values can not be nested.\n" + "\n" + "Nesting properties is not supported. You could for example store the\n" + "nested JSON as a string instead.") @Test public void shouldReturn400WhenSendinIncompatibleJsonProperty()
		 [Documented("Property values can not be nested.\n" + "\n" + "Nesting properties is not supported. You could for example store the\n" + "nested JSON as a string instead.")]
		 public virtual void ShouldReturn400WhenSendinIncompatibleJsonProperty()
		 {
			  GenConflict.get().payload("{\"foo\" : {\"bar\" : \"baz\"}}").expectedStatus(400).post(DataUri + "node/");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("jim knows joe") public void shouldReturn400WhenSendingCorruptJsonProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("jim knows joe")]
		 public virtual void ShouldReturn400WhenSendingCorruptJsonProperty()
		 {
			  JaxRsResponse response = RestRequest.Req().put(GetPropertyUri(Data.get()["jim"], "foo"), "this:::Is::notJSON}");
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("jim knows joe") public void shouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 [Graph("jim knows joe")]
		 public virtual void ShouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 {
			  JaxRsResponse response = RestRequest.Req().put(DataUri + "node/1234/foo", JsonHelper.createJsonFrom("bar"));
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

	}

}