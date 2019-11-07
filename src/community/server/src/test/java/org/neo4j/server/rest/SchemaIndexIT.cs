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


	using Neo4Net.Functions;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using Neo4NetMatchers = Neo4Net.Test.mockito.matcher.Neo4NetMatchers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.createJsonFrom;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.containsOnly;

	public class SchemaIndexIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create index.\n" + "\n" + "This will start a background job in the database that will create and populate the index.\n" + "You can check the status of your index by listing all the indexes for the relevant label.") @Test @GraphDescription.Graph(nodes = {}) public void create_index() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Create index.\n" + "\n" + "This will start a background job in the database that will create and populate the index.\n" + "You can check the status of your index by listing all the indexes for the relevant label.")]
		 public virtual void CreateIndex()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  IDictionary<string, object> definition = map( "property_keys", singletonList( propertyKey ) );

			  string result = GenConflict.get().expectedStatus(200).payload(createJsonFrom(definition)).post(GetSchemaIndexLabelUri(labelName)).entity();

			  IDictionary<string, object> serialized = jsonToMap( result );

			  IDictionary<string, object> index = new Dictionary<string, object>();
			  index["label"] = labelName;
			  index["labels"] = singletonList( labelName );
			  index["property_keys"] = singletonList( propertyKey );

			  assertThat( serialized, equalTo( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("List indexes for a label.") @Test @GraphDescription.Graph(nodes = {}) public void get_indexes_for_label() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("List indexes for a label.")]
		 public virtual void GetIndexesForLabel()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateIndex( labelName, propertyKey );
			  IDictionary<string, object> definition = map( "property_keys", singletonList( propertyKey ) );

			  IList<IDictionary<string, object>> serializedList = RetryOnStillPopulating( () => GenConflict.get().expectedStatus(200).payload(createJsonFrom(definition)).get(GetSchemaIndexLabelUri(labelName)).entity() );

			  IDictionary<string, object> index = new Dictionary<string, object>();
			  index["label"] = labelName;
			  index["labels"] = singletonList( labelName );
			  index["property_keys"] = singletonList( propertyKey );

			  assertThat( serializedList, hasItem( index ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<java.util.Map<String,Object>> retryOnStillPopulating(java.util.concurrent.Callable<String> callable) throws Exception
		 private IList<IDictionary<string, object>> RetryOnStillPopulating( Callable<string> callable )
		 {
			  long endTime = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.MINUTES.toMillis(1);
			  IList<IDictionary<string, object>> serializedList;
			  do
			  {
					string result = callable.call();
					serializedList = jsonToList( result );
					if ( DateTimeHelper.CurrentUnixTimeMillis() > endTime )
					{
						 fail( "Indexes didn't populate correctly, last result '" + result + "'" );
					}
			  } while ( StillPopulating( serializedList ) );
			  return serializedList;
		 }

		 private bool StillPopulating( IList<IDictionary<string, object>> serializedList )
		 {
			  // We've created an index. That HTTP call for creating the index will return
			  // immediately and indexing continue in the background. Querying the index endpoint
			  // while index is populating gives back additional information like population progress.
			  // This test below will look at the response of a "get index" result and if still populating
			  // then return true so that caller may retry the call later.
			  foreach ( IDictionary<string, object> map in serializedList )
			  {
					if ( map.ContainsKey( "population_progress" ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all indexes.") @Test @GraphDescription.Graph(nodes = {}) public void get_indexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all indexes.")]
		 public virtual void GetIndexes()
		 {
			  Data.get();

			  string labelName1 = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string labelName2 = _labels.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateIndex( labelName1, propertyKey1 );
			  CreateIndex( labelName2, propertyKey2 );

			  IList<IDictionary<string, object>> serializedList = RetryOnStillPopulating( () => GenConflict.get().expectedStatus(200).get(SchemaIndexUri).entity() );

			  IDictionary<string, object> index1 = new Dictionary<string, object>();
			  index1["label"] = labelName1;
			  index1["labels"] = singletonList( labelName1 );
			  index1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> index2 = new Dictionary<string, object>();
			  index2["label"] = labelName2;
			  index2["labels"] = singletonList( labelName2 );
			  index2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( index1, index2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Drop index") @Test @GraphDescription.Graph(nodes = {}) public void drop_index()
		 [Documented("Drop index")]
		 public virtual void DropIndex()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  IndexDefinition schemaIndex = CreateIndex( labelName, propertyKey );
			  assertThat( Neo4NetMatchers.getIndexes( Graphdb(), label(labelName) ), containsOnly(schemaIndex) );

			  GenConflict.get().expectedStatus(204).delete(GetSchemaIndexLabelPropertyUri(labelName, propertyKey)).entity();

			  assertThat( Neo4NetMatchers.getIndexes( Graphdb(), label(labelName) ), not(containsOnly(schemaIndex)) );
		 }

		 /// <summary>
		 /// Create an index for a label and property key which already exists.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_existing_index()
		 public virtual void CreateExistingIndex()
		 {
			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateIndex( labelName, propertyKey );
			  IDictionary<string, object> definition = map( "property_keys", singletonList( propertyKey ) );

			  GenConflict.get().expectedStatus(409).payload(createJsonFrom(definition)).post(GetSchemaIndexLabelUri(labelName));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void drop_non_existent_index()
		 public virtual void DropNonExistentIndex()
		 {
			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();

			  GenConflict.get().expectedStatus(404).delete(GetSchemaIndexLabelPropertyUri(labelName, propertyKey));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_compound_index()
		 public virtual void CreateCompoundIndex()
		 {
			  IDictionary<string, object> definition = map( "property_keys", asList( _properties.newInstance(), _properties.newInstance() ) );

			  GenConflict.get().expectedStatus(200).payload(createJsonFrom(definition)).post(GetSchemaIndexLabelUri(_labels.newInstance()));
		 }

		 private IndexDefinition CreateIndex( string labelName, string propertyKey )
		 {
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					IndexDefinition indexDefinition = Graphdb().schema().indexFor(label(labelName)).on(propertyKey).create();
					tx.Success();
					return indexDefinition;
			  }
		 }

		 private readonly IFactory<string> _labels = UniqueStrings.WithPrefix( "label" );
		 private readonly IFactory<string> _properties = UniqueStrings.WithPrefix( "property" );
	}

}