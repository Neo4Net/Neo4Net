using System;
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


	using Node = Neo4Net.Graphdb.Node;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using PropertyValueException = Neo4Net.Server.rest.web.PropertyValueException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using LABEL = Neo4Net.Test.GraphDescription.LABEL;
	using NODE = Neo4Net.Test.GraphDescription.NODE;
	using PROP = Neo4Net.Test.GraphDescription.PROP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.createJsonFrom;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.readJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.GraphDescription.PropType.ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.GraphDescription.PropType.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class LabelsIT : AbstractRestFunctionalTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Adding a label to a node.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true) }) public void adding_a_label_to_a_node()
		 [Documented("Adding a label to a node.")]
		 public virtual void AddingALabelToANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Clint Eastwood"] );

			  GenConflict.get().expectedStatus(204).payload(createJsonFrom("Person")).post(nodeUri + "/labels");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Adding multiple labels to a node.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true) }) public void adding_multiple_labels_to_a_node()
		 [Documented("Adding multiple labels to a node.")]
		 public virtual void AddingMultipleLabelsToANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Clint Eastwood"] );

			  GenConflict.get().expectedStatus(204).payload(createJsonFrom(new string[]{ "Person", "Actor" })).post(nodeUri + "/labels");

			  // Then
			  assertThat( nodes["Clint Eastwood"], inTx( Graphdb(), hasLabels("Person", "Actor") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Adding a label with an invalid name.\n" + "\n" + "Labels with empty names are not allowed, however, all other valid strings are accepted as label names.\n" + "Adding an invalid label to a node will lead to a HTTP 400 response.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true) }) public void adding_an_invalid_label_to_a_node()
		 [Documented("Adding a label with an invalid name.\n" + "\n" + "Labels with empty names are not allowed, however, all other valid strings are accepted as label names.\n" + "Adding an invalid label to a node will lead to a HTTP 400 response.")]
		 public virtual void AddingAnInvalidLabelToANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Clint Eastwood"] );

			  GenConflict.get().expectedStatus(400).payload(createJsonFrom("")).post(nodeUri + "/labels");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Replacing labels on a node.\n" + "\n" + "This removes any labels currently on a node, and replaces them with the labels passed in as the\n" + "request body.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true, labels = {@LABEL("Person")})}) public void replacing_labels_on_a_node()
		 [Documented("Replacing labels on a node.\n" + "\n" + "This removes any labels currently on a node, and replaces them with the labels passed in as the\n" + "request body.")]
		 public virtual void ReplacingLabelsOnANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Clint Eastwood"] );

			  // When
			  GenConflict.get().expectedStatus(204).payload(createJsonFrom(new string[]{ "Actor", "Director" })).put(nodeUri + "/labels");

			  // Then
			  assertThat( nodes["Clint Eastwood"], inTx( Graphdb(), hasLabels("Actor", "Director") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Listing labels for a node.") @Test @GraphDescription.Graph(nodes = {@NODE(name = "Clint Eastwood", labels = {@LABEL("Actor"), @LABEL("Director")}, setNameProperty = true)}) public void listing_node_labels() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Listing labels for a node.")]
		 public virtual void ListingNodeLabels()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Clint Eastwood"] );

			  string body = GenConflict.get().expectedStatus(200).get(nodeUri + "/labels").entity();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> labels = (java.util.List<String>) readJson(body);
			  IList<string> labels = ( IList<string> ) readJson( body );
			  assertEquals( asSet( "Actor", "Director" ), Iterables.asSet( labels ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Removing a label from a node.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true, labels = { @LABEL("Person") }) }) public void removing_a_label_from_a_node()
		 [Documented("Removing a label from a node.")]
		 public virtual void RemovingALabelFromANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  Node node = nodes["Clint Eastwood"];
			  string nodeUri = GetNodeUri( node );

			  string labelName = "Person";
			  GenConflict.get().expectedStatus(204).delete(nodeUri + "/labels/" + labelName);

			  assertThat( node, inTx( Graphdb(), not(hasLabel(label(labelName))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Removing a non-existent label from a node.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true) }) public void removing_a_non_existent_label_from_a_node()
		 [Documented("Removing a non-existent label from a node.")]
		 public virtual void RemovingANonExistentLabelFromANode()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  Node node = nodes["Clint Eastwood"];
			  string nodeUri = GetNodeUri( node );

			  string labelName = "Person";
			  GenConflict.get().expectedStatus(204).delete(nodeUri + "/labels/" + labelName);

			  assertThat( node, inTx( Graphdb(), not(hasLabel(label(labelName))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get all nodes with a label.") @Test @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true, labels = { @LABEL("Actor"), @LABEL("Director") }), @NODE(name = "Donald Sutherland", setNameProperty = true, labels = { @LABEL("Actor") }), @NODE(name = "Steven Spielberg", setNameProperty = true, labels = { @LABEL("Director") }) }) public void get_all_nodes_with_label() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all nodes with a label.")]
		 public virtual void GetAllNodesWithLabel()
		 {
			  Data.get();
			  string uri = GetNodesWithLabelUri( "Actor" );
			  string body = GenConflict.get().expectedStatus(200).get(uri).entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> parsed = (java.util.List<?>) readJson(body);
			  IList<object> parsed = ( IList<object> ) readJson( body );
			  assertEquals( asSet( "Clint Eastwood", "Donald Sutherland" ), Iterables.asSet( map( GetProperty( "name", typeof( string ) ), parsed ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Get nodes by label and property.\n" + "\n" + "You can retrieve all nodes with a given label and property by passing one property as a query parameter.\n" + "Notice that the property value is JSON-encoded and then URL-encoded.\n" + "\n" + "If there is an index available on the label/property combination you send, that index will be used. If no\n" + "index is available, all nodes with the given label will be filtered through to find matching nodes.\n" + "\n" + "Currently, it is not possible to search using multiple properties.") @GraphDescription.Graph(nodes = {@NODE(name = "Donald Sutherland", labels = {@LABEL("Person")}), @NODE(name = "Clint Eastwood", labels = {@LABEL("Person")}, properties = { @PROP(key = "name", value = "Clint Eastwood")}), @NODE(name = "Steven Spielberg", labels = {@LABEL("Person")}, properties = { @PROP(key = "name", value = "Steven Spielberg")})}) public void get_nodes_with_label_and_property() throws org.neo4j.server.rest.domain.JsonParseException, java.io.UnsupportedEncodingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get nodes by label and property.\n" + "\n" + "You can retrieve all nodes with a given label and property by passing one property as a query parameter.\n" + "Notice that the property value is JSON-encoded and then URL-encoded.\n" + "\n" + "If there is an index available on the label/property combination you send, that index will be used. If no\n" + "index is available, all nodes with the given label will be filtered through to find matching nodes.\n" + "\n" + "Currently, it is not possible to search using multiple properties.")]
		 public virtual void GetNodesWithLabelAndProperty()
		 {
			  Data.get();

			  string labelName = "Person";

			  string result = GenConflict.get().expectedStatus(200).get(GetNodesWithLabelAndPropertyUri(labelName, "name", "Clint Eastwood")).entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> parsed = (java.util.List<?>) readJson(result);
			  IList<object> parsed = ( IList<object> ) readJson( result );
			  assertEquals( asSet( "Clint Eastwood" ), Iterables.asSet( map( GetProperty( "name", typeof( string ) ), parsed ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Get nodes by label and array property.") @GraphDescription.Graph(nodes = {@NODE(name = "Donald Sutherland", labels = {@LABEL("Person")}), @NODE(name = "Clint Eastwood", labels = {@LABEL("Person")}, properties = { @PROP(key = "names", value = "Clint,Eastwood", type = ARRAY, componentType = STRING)}), @NODE(name = "Steven Spielberg", labels = {@LABEL("Person")}, properties = { @PROP(key = "names", value = "Steven,Spielberg", type = ARRAY, componentType = STRING)})}) public void get_nodes_with_label_and_array_property() throws org.neo4j.server.rest.domain.JsonParseException, java.io.UnsupportedEncodingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get nodes by label and array property.")]
		 public virtual void GetNodesWithLabelAndArrayProperty()
		 {
			  Data.get();

			  string labelName = "Person";

			  string uri = GetNodesWithLabelAndPropertyUri( labelName, "names", new string[] { "Clint", "Eastwood" } );

			  string result = GenConflict.get().expectedStatus(200).get(uri).entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> parsed = (java.util.List<?>) readJson(result);
			  IList<object> parsed = ( IList<object> ) readJson( result );
			  assertEquals( 1, parsed.Count );

			  //noinspection AssertEqualsBetweenInconvertibleTypes
			  assertEquals( Iterables.asSet( asList( asList( "Clint", "Eastwood" ) ) ), Iterables.asSet( map( GetProperty( "names", typeof( System.Collections.IList ) ), parsed ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("List all labels.\n" + " \n" + "By default, the server will return labels in use only. If you also want to return labels not in use,\n" + "append the \"in_use=0\" query parameter.") @GraphDescription.Graph(nodes = { @NODE(name = "Clint Eastwood", setNameProperty = true, labels = { @LABEL("Person"), @LABEL("Actor"), @LABEL("Director") }), @NODE(name = "Donald Sutherland", setNameProperty = true, labels = { @LABEL("Person"), @LABEL("Actor") }), @NODE(name = "Steven Spielberg", setNameProperty = true, labels = { @LABEL("Person"), @LABEL("Director") }) }) public void list_all_labels() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("List all labels.\n" + " \n" + "By default, the server will return labels in use only. If you also want to return labels not in use,\n" + "append the \"in_use=0\" query parameter.")]
		 public virtual void ListAllLabels()
		 {
			  Data.get();
			  string uri = LabelsUri;
			  string body = GenConflict.get().expectedStatus(200).get(uri).entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> parsed = org.neo4j.helpers.collection.Iterables.asSet((java.util.List<?>) readJson(body));
			  ISet<object> parsed = Iterables.asSet( ( IList<object> ) readJson( body ) );
			  assertTrue( parsed.Contains( "Person" ) );
			  assertTrue( parsed.Contains( "Actor" ) );
			  assertTrue( parsed.Contains( "Director" ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private <T> System.Func<Object, T> getProperty(final String propertyKey, final Class<T> propertyType)
		 private System.Func<object, T> GetProperty<T>( string propertyKey, Type propertyType )
		 {
				 propertyType = typeof( T );
			  return from =>
			  {
				IDictionary<object, ?> node = ( IDictionary<object, ?> ) from;
				IDictionary<object, ?> data1 = ( IDictionary<object, ?> ) node.get( "data" );
				return propertyType.cast( data1.get( propertyKey ) );
			  };
		 }
	}

}