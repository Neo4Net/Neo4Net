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
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.domain.JsonHelper.createJsonFrom;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.domain.JsonHelper.jsonToList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.domain.JsonHelper.jsonToMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getConstraints;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.isEmpty;

	public class SchemaConstraintsIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create uniqueness constraint.\n" + "Create a uniqueness constraint on a property.") @Test @GraphDescription.Graph(nodes = {}) public void createPropertyUniquenessConstraint() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Create uniqueness constraint.\n" + "Create a uniqueness constraint on a property.")]
		 public virtual void CreatePropertyUniquenessConstraint()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  IDictionary<string, object> definition = map( "property_keys", singletonList( propertyKey ) );

			  string result = GenConflict.get().expectedStatus(200).payload(createJsonFrom(definition)).post(GetSchemaConstraintLabelUniquenessUri(labelName)).entity();

			  IDictionary<string, object> serialized = jsonToMap( result );

			  IDictionary<string, object> constraint = new Dictionary<string, object>();
			  constraint["type"] = ConstraintType.UNIQUENESS.name();
			  constraint["label"] = labelName;
			  constraint["property_keys"] = singletonList( propertyKey );

			  assertThat( serialized, equalTo( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get a specific uniqueness constraint.\n" + "Get a specific uniqueness constraint for a label and a property.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelUniquenessPropertyConstraint() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get a specific uniqueness constraint.\n" + "Get a specific uniqueness constraint for a label and a property.")]
		 public virtual void getLabelUniquenessPropertyConstraint()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey );

			  string result = GenConflict.get().expectedStatus(200).get(GetSchemaConstraintLabelUniquenessPropertyUri(labelName, propertyKey)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint = new Dictionary<string, object>();
			  constraint["type"] = ConstraintType.UNIQUENESS.name();
			  constraint["label"] = labelName;
			  constraint["property_keys"] = singletonList( propertyKey );

			  assertThat( serializedList, hasItem( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all uniqueness constraints for a label.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelUniquenessPropertyConstraints() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all uniqueness constraints for a label.")]
		 public virtual void getLabelUniquenessPropertyConstraints()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey1 );
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey2 );

			  string result = GenConflict.get().expectedStatus(200).get(GetSchemaConstraintLabelUniquenessUri(labelName)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.UNIQUENESS.name();
			  constraint1["label"] = labelName;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> constraint2 = new Dictionary<string, object>();
			  constraint2["type"] = ConstraintType.UNIQUENESS.name();
			  constraint2["label"] = labelName;
			  constraint2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( constraint1, constraint2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all constraints for a label.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelPropertyConstraints() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all constraints for a label.")]
		 public virtual void getLabelPropertyConstraints()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey1 );

			  string result = GenConflict.get().expectedStatus(200).get(GetSchemaConstraintLabelUri(labelName)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.UNIQUENESS.name();
			  constraint1["label"] = labelName;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  assertThat( serializedList, hasItems( constraint1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all constraints.") @Test @GraphDescription.Graph(nodes = {}) public void get_constraints() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all constraints.")]
		 public virtual void GetConstraints()
		 {
			  Data.get();

			  string labelName1 = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName1, propertyKey1 );

			  string result = GenConflict.get().expectedStatus(200).get(SchemaConstraintUri).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.UNIQUENESS.name();
			  constraint1["label"] = labelName1;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  assertThat( serializedList, hasItems( constraint1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Drop uniqueness constraint.\n" + "Drop uniqueness constraint for a label and a property.") @Test @GraphDescription.Graph(nodes = {}) public void drop_constraint()
		 [Documented("Drop uniqueness constraint.\n" + "Drop uniqueness constraint for a label and a property.")]
		 public virtual void DropConstraint()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  ConstraintDefinition constraintDefinition = CreateLabelUniquenessPropertyConstraint( labelName, propertyKey );
			  assertThat( getConstraints( Graphdb(), label(labelName) ), containsOnly(constraintDefinition) );

			  GenConflict.get().expectedStatus(204).delete(GetSchemaConstraintLabelUniquenessPropertyUri(labelName, propertyKey)).entity();

			  assertThat( getConstraints( Graphdb(), label(labelName) ), Empty );
		 }

		 /// <summary>
		 /// Create an index for a label and property key which already exists.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_existing_constraint()
		 public virtual void CreateExistingConstraint()
		 {
			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey );

			  IDictionary<string, object> definition = map( "property_keys", singletonList( propertyKey ) );
			  GenConflict.get().expectedStatus(409).payload(createJsonFrom(definition)).post(GetSchemaConstraintLabelUniquenessUri(labelName)).entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void drop_non_existent_constraint()
		 public virtual void DropNonExistentConstraint()
		 {
			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();

			  GenConflict.get().expectedStatus(404).delete(GetSchemaConstraintLabelUniquenessPropertyUri(labelName, propertyKey));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_compound_schema_index()
		 public virtual void CreateCompoundSchemaIndex()
		 {
			  IDictionary<string, object> definition = map( "property_keys", asList( _properties.newInstance(), _properties.newInstance() ) );

			  GenConflict.get().expectedStatus(200).payload(createJsonFrom(definition)).post(GetSchemaIndexLabelUri(_labels.newInstance()));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create_compound_schema_constraint()
		 public virtual void CreateCompoundSchemaConstraint()
		 {
			  IDictionary<string, object> definition = map( "property_keys", asList( _properties.newInstance(), _properties.newInstance() ) );

			  GenConflict.get().expectedStatus(405).payload(createJsonFrom(definition)).post(GetSchemaConstraintLabelUri(_labels.newInstance()));
		 }

		 private ConstraintDefinition CreateLabelUniquenessPropertyConstraint( string labelName, string propertyKey )
		 {
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					ConstraintDefinition constraintDefinition = Graphdb().schema().constraintFor(label(labelName)).assertPropertyIsUnique(propertyKey).create();
					tx.Success();
					return constraintDefinition;
			  }
		 }

		 private readonly IFactory<string> _labels = UniqueStrings.WithPrefix( "label" );
		 private readonly IFactory<string> _properties = UniqueStrings.WithPrefix( "property" );
	}

}