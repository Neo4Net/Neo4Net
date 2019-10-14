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
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class RemoveNodePropertiesIT : AbstractRestFunctionalDocTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getPropertiesUri(final long nodeId)
		 private string GetPropertiesUri( long nodeId )
		 {
			  return _functionalTestHelper.nodePropertiesUri( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn204WhenPropertiesAreRemoved()
		 public virtual void ShouldReturn204WhenPropertiesAreRemoved()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = "tobias";
			  _helper.setNodeProperties( nodeId, map );
			  JaxRsResponse response = RemoveNodePropertiesOnServer( nodeId );
			  assertEquals( 204, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Delete all properties from node.") @Test public void shouldReturn204WhenAllPropertiesAreRemoved()
		 [Documented("Delete all properties from node.")]
		 public virtual void ShouldReturn204WhenAllPropertiesAreRemoved()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = "tobias";
			  _helper.setNodeProperties( nodeId, map );
			  GenConflict.get().expectedStatus(204).delete(_functionalTestHelper.nodePropertiesUri(nodeId));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 public virtual void ShouldReturn404WhenPropertiesSentToANodeWhichDoesNotExist()
		 {
			  JaxRsResponse response = RestRequest.Req().delete(GetPropertiesUri(999999));
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse removeNodePropertiesOnServer(final long nodeId)
		 private JaxRsResponse RemoveNodePropertiesOnServer( long nodeId )
		 {
			  return RestRequest.Req().delete(GetPropertiesUri(nodeId));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("To delete a single property\n" + "from a node, see the example below") @Test public void delete_a_named_property_from_a_node()
		 [Documented("To delete a single property\n" + "from a node, see the example below")]
		 public virtual void DeleteANamedPropertyFromANode()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["name"] = "tobias";
			  _helper.setNodeProperties( nodeId, map );
			  GenConflict.get().expectedStatus(204).delete(_functionalTestHelper.nodePropertyUri(nodeId, "name"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenRemovingNonExistingNodeProperty()
		 public virtual void ShouldReturn404WhenRemovingNonExistingNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["jim"] = "tobias";
			  _helper.setNodeProperties( nodeId, map );
			  JaxRsResponse response = RemoveNodePropertyOnServer( nodeId, "foo" );
			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 public virtual void ShouldReturn404WhenPropertySentToANodeWhichDoesNotExist()
		 {
			  JaxRsResponse response = RestRequest.Req().delete(GetPropertyUri(999999, "foo"));
			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getPropertyUri(final long nodeId, final String key)
		 private string GetPropertyUri( long nodeId, string key )
		 {
			  return _functionalTestHelper.nodePropertyUri( nodeId, key );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse removeNodePropertyOnServer(final long nodeId, final String key)
		 private JaxRsResponse RemoveNodePropertyOnServer( long nodeId, string key )
		 {
			  return RestRequest.Req().delete(GetPropertyUri(nodeId, key));
		 }
	}

}