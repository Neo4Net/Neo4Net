using System;
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
	using Test = org.junit.Test;

	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using RestfulGraphDatabase = Org.Neo4j.Server.rest.web.RestfulGraphDatabase;
	using Graph = Org.Neo4j.Test.GraphDescription.Graph;
	using NODE = Org.Neo4j.Test.GraphDescription.NODE;
	using PROP = Org.Neo4j.Test.GraphDescription.PROP;
	using REL = Org.Neo4j.Test.GraphDescription.REL;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class AutoIndexIT : AbstractRestFunctionalTestBase
	{
		 /// <summary>
		 /// Find node by query from an automatic index.
		 /// <p/>
		 /// See Find node by query for the actual query syntax.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void shouldRetrieveFromAutoIndexByQuery()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void ShouldRetrieveFromAutoIndexByQuery()
		 {
			  Data.get();
			  AssertSize( 1, GenConflict.get().expectedStatus(200).get(NodeAutoIndexUri() + "?query=name:I").entity() );
		 }

		 private string NodeAutoIndexUri()
		 {
			  return DataUri + "index/auto/node/";
		 }

		 /// <summary>
		 /// Automatic index nodes can be found via exact lookups with normal Index
		 /// REST syntax.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void find_node_by_exact_match_from_an_automatic_index()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void FindNodeByExactMatchFromAnAutomaticIndex()
		 {
			  Data.get();
			  AssertSize( 1, GenConflict.get().expectedStatus(200).get(NodeAutoIndexUri() + "name/I").entity() );
		 }

		 /// <summary>
		 /// The automatic relationship index can not be removed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void Relationship_AutoIndex_is_not_removable()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void RelationshipAutoIndexIsNotRemovable()
		 {
			  Data.get();
			  GenConflict.get().expectedStatus(405).delete(RelationshipAutoIndexUri()).entity();
		 }

		 /// <summary>
		 /// The automatic node index can not be removed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void AutoIndex_is_not_removable()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void AutoIndexIsNotRemovable()
		 {
			  GenConflict.get().expectedStatus(405).delete(NodeAutoIndexUri()).entity();
		 }

		 /// <summary>
		 /// It is not allowed to add items manually to automatic indexes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void items_can_not_be_added_manually_to_an_AutoIndex()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void ItemsCanNotBeAddedManuallyToAnAutoIndex()
		 {
			  Data.get();
			  string indexName;
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					indexName = Graphdb().index().NodeAutoIndexer.AutoIndex.Name;
					tx.Success();
			  }

			  GenConflict.get().expectedStatus(405).payload(CreateJsonStringFor(getNodeUri(Data.get()["I"]), "name", "I")).post(PostNodeIndexUri(indexName)).entity();

		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String createJsonStringFor(final String targetUri, final String key, final String value)
		 private string CreateJsonStringFor( string targetUri, string key, string value )
		 {
			  return "{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\": \"" + targetUri + "\"}";
		 }

		 /// <summary>
		 /// It is not allowed to add items manually to automatic indexes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I"), @NODE(name = "you")}, relationships = {@REL(start = "I", end = "you", type = "know", properties = {@PROP(key = "since", value = "today")})}, autoIndexRelationships = true) public void items_can_not_be_added_manually_to_a_Relationship_AutoIndex()
		 [Graph(nodes : {@NODE(name : "I"), @NODE(name : "you")}, relationships : {@REL(start : "I", end : "you", type : "know", properties : {@PROP(key : "since", value : "today")})}, autoIndexRelationships : true)]
		 public virtual void ItemsCanNotBeAddedManuallyToARelationshipAutoIndex()
		 {
			  Data.get();
			  string indexName;
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					indexName = Graphdb().index().RelationshipAutoIndexer.AutoIndex.Name;
					tx.Success();
			  }
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					ResourceIterable<Relationship> relationships = ( ResourceIterable<Relationship> ) Data.get()["I"].Relationships;
					using ( ResourceIterator<Relationship> resourceIterator = relationships.GetEnumerator() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 GenConflict.get().expectedStatus(405).payload(CreateJsonStringFor(GetRelationshipUri(resourceIterator.next()), "name", "I")).post(PostRelationshipIndexUri(indexName)).entity();
					}
			  }
		 }

		 /// <summary>
		 /// It is not allowed to remove entries manually from automatic indexes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true)}, autoIndexNodes = true) public void autoindexed_items_cannot_be_removed_manually()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true)}, autoIndexNodes : true)]
		 public virtual void AutoindexedItemsCannotBeRemovedManually()
		 {
			  long id = Data.get()["I"].Id;
			  string indexName;
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					indexName = Graphdb().index().NodeAutoIndexer.AutoIndex.Name;
					tx.Success();
			  }
			  GenConflict.get().expectedStatus(405).delete(DataUri + "index/node/" + indexName + "/name/I/" + id).entity();
			  GenConflict.get().expectedStatus(405).delete(DataUri + "index/node/" + indexName + "/name/" + id).entity();
			  GenConflict.get().expectedStatus(405).delete(DataUri + "index/node/" + indexName + "/" + id).entity();
		 }

		 /// <summary>
		 /// It is not allowed to remove entries manually from automatic indexes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I"), @NODE(name = "you")}, relationships = {@REL(start = "I", end = "you", type = "know", properties = {@PROP(key = "since", value = "today")})}, autoIndexRelationships = true) public void autoindexed_relationships_cannot_be_removed_manually()
		 [Graph(nodes : {@NODE(name : "I"), @NODE(name : "you")}, relationships : {@REL(start : "I", end : "you", type : "know", properties : {@PROP(key : "since", value : "today")})}, autoIndexRelationships : true)]
		 public virtual void AutoindexedRelationshipsCannotBeRemovedManually()
		 {
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					Data.get();
					tx.Success();
			  }

			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					ResourceIterable<Relationship> relationshipIterable = ( ResourceIterable<Relationship> ) Data.get()["I"].Relationships;
					long id;
					using ( ResourceIterator<Relationship> resourceIterator = relationshipIterable.GetEnumerator() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 id = resourceIterator.next().Id;
					}
					string indexName = Graphdb().index().RelationshipAutoIndexer.AutoIndex.Name;
					GenConflict.get().expectedStatus(405).delete(DataUri + "index/relationship/" + indexName + "/since/today/" + id).entity();
					GenConflict.get().expectedStatus(405).delete(DataUri + "index/relationship/" + indexName + "/since/" + id).entity();
					GenConflict.get().expectedStatus(405).delete(DataUri + "index/relationship/" + indexName + "/" + id).entity();
			  }
		 }

		 /// <summary>
		 /// See the example request.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I"), @NODE(name = "you")}, relationships = {@REL(start = "I", end = "you", type = "know", properties = {@PROP(key = "since", value = "today")})}, autoIndexRelationships = true) public void Find_relationship_by_query_from_an_automatic_index()
		 [Graph(nodes : {@NODE(name : "I"), @NODE(name : "you")}, relationships : {@REL(start : "I", end : "you", type : "know", properties : {@PROP(key : "since", value : "today")})}, autoIndexRelationships : true)]
		 public virtual void FindRelationshipByQueryFromAnAutomaticIndex()
		 {
			  Data.get();
			  AssertSize( 1, GenConflict.get().expectedStatus(200).get(RelationshipAutoIndexUri() + "?query=since:today").entity() );
		 }

		 /// <summary>
		 /// See the example request.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I"), @NODE(name = "you")}, relationships = {@REL(start = "I", end = "you", type = "know", properties = {@PROP(key = "since", value = "today")})}, autoIndexRelationships = true) public void Find_relationship_by_exact_match_from_an_automatic_index()
		 [Graph(nodes : {@NODE(name : "I"), @NODE(name : "you")}, relationships : {@REL(start : "I", end : "you", type : "know", properties : {@PROP(key : "since", value : "today")})}, autoIndexRelationships : true)]
		 public virtual void FindRelationshipByExactMatchFromAnAutomaticIndex()
		 {
			  Data.get();
			  AssertSize( 1, GenConflict.get().expectedStatus(200).get(RelationshipAutoIndexUri() + "since/today/").entity() );
		 }

		 /// <summary>
		 /// Get current status for autoindexing on nodes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCurrentStatusForNodes()
		 public virtual void getCurrentStatusForNodes()
		 {
			  SetEnabledAutoIndexingForType( "node", false );
			  CheckAndAssertAutoIndexerIsEnabled( "node", false );
		 }

		 /// <summary>
		 /// Enable node autoindexing.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableNodeAutoIndexing()
		 public virtual void EnableNodeAutoIndexing()
		 {
			  SetEnabledAutoIndexingForType( "node", true );
			  CheckAndAssertAutoIndexerIsEnabled( "node", true );
		 }

		 /// <summary>
		 /// Add a property for autoindexing on nodes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addAutoIndexingPropertyForNodes()
		 public virtual void AddAutoIndexingPropertyForNodes()
		 {
			  GenConflict.get().expectedStatus(204).payload("myProperty1").post(AutoIndexURI("node") + "/properties");
		 }

		 /// <summary>
		 /// Lookup list of properties being autoindexed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAutoIndexingPropertiesForNodes() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAutoIndexingPropertiesForNodes()
		 {
			  int initialPropertiesSize = GetAutoIndexedPropertiesForType( "node" ).Count;

			  string propName = "some-property" + DateTimeHelper.CurrentUnixTimeMillis();
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().NodeAutoIndexer.startAutoIndexingProperty(propName);
					tx.Success();
			  }

			  IList<string> properties = GetAutoIndexedPropertiesForType( "node" );

			  assertEquals( initialPropertiesSize + 1, properties.Count );
			  assertThat( properties, hasItem( propName ) );
		 }

		 /// <summary>
		 /// Remove a property for autoindexing on nodes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeAutoIndexingPropertyForNodes()
		 public virtual void RemoveAutoIndexingPropertyForNodes()
		 {
			  GenConflict.get().expectedStatus(204).delete(AutoIndexURI("node") + "/properties/myProperty1");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchOnOffAutoIndexingForNodes()
		 public virtual void SwitchOnOffAutoIndexingForNodes()
		 {
			  SwitchOnOffAutoIndexingForType( "node" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchOnOffAutoIndexingForRelationships()
		 public virtual void SwitchOnOffAutoIndexingForRelationships()
		 {
			  SwitchOnOffAutoIndexingForType( "relationship" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemoveAutoIndexedPropertyForNodes() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddRemoveAutoIndexedPropertyForNodes()
		 {
			  AddRemoveAutoIndexedPropertyForType( "node" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemoveAutoIndexedPropertyForRelationships() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddRemoveAutoIndexedPropertyForRelationships()
		 {
			  AddRemoveAutoIndexedPropertyForType( "relationship" );
		 }

		 private string RelationshipAutoIndexUri()
		 {
			  return DataUri + "index/auto/relationship/";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addRemoveAutoIndexedPropertyForType(String uriPartForType) throws org.neo4j.server.rest.domain.JsonParseException
		 private void AddRemoveAutoIndexedPropertyForType( string uriPartForType )
		 {
			  int initialPropertiesSize = GetAutoIndexedPropertiesForType( uriPartForType ).Count;

			  long millis = DateTimeHelper.CurrentUnixTimeMillis();
			  string myProperty1 = uriPartForType + "-myProperty1-" + millis;
			  string myProperty2 = uriPartForType + "-myProperty2-" + millis;

			  GenConflict.get().expectedStatus(204).payload(myProperty1).post(AutoIndexURI(uriPartForType) + "/properties");
			  GenConflict.get().expectedStatus(204).payload(myProperty2).post(AutoIndexURI(uriPartForType) + "/properties");

			  IList<string> properties = GetAutoIndexedPropertiesForType( uriPartForType );
			  assertEquals( initialPropertiesSize + 2, properties.Count );
			  assertTrue( properties.Contains( myProperty1 ) );
			  assertTrue( properties.Contains( myProperty2 ) );

			  GenConflict.get().expectedStatus(204).payload(null).delete(AutoIndexURI(uriPartForType) + "/properties/" + myProperty2);

			  properties = GetAutoIndexedPropertiesForType( uriPartForType );
			  assertEquals( initialPropertiesSize + 1, properties.Count );
			  assertTrue( properties.Contains( myProperty1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.List<String> getAutoIndexedPropertiesForType(String uriPartForType) throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private IList<string> GetAutoIndexedPropertiesForType( string uriPartForType )
		 {
			  string result = GenConflict.get().expectedStatus(200).get(AutoIndexURI(uriPartForType) + "/properties").entity();
			  return ( IList<string> ) JsonHelper.readJson( result );
		 }

		 private void SwitchOnOffAutoIndexingForType( string uriPartForType )
		 {
			  SetEnabledAutoIndexingForType( uriPartForType, true );
			  CheckAndAssertAutoIndexerIsEnabled( uriPartForType, true );
			  SetEnabledAutoIndexingForType( uriPartForType, false );
			  CheckAndAssertAutoIndexerIsEnabled( uriPartForType, false );
		 }

		 private void SetEnabledAutoIndexingForType( string uriPartForType, bool enabled )
		 {
			  GenConflict.get().expectedStatus(204).payload(Convert.ToString(enabled)).put(AutoIndexURI(uriPartForType) + "/status");
		 }

		 private void CheckAndAssertAutoIndexerIsEnabled( string uriPartForType, bool enabled )
		 {
			  string result = GenConflict.get().expectedStatus(200).get(AutoIndexURI(uriPartForType) + "/status").entity();
			  assertEquals( enabled, bool.Parse( result ) );
		 }

		 private string AutoIndexURI( string type )
		 {
			  return DataUri + RestfulGraphDatabase.PATH_AUTO_INDEX.Replace( "{type}", type );
		 }
	}

}