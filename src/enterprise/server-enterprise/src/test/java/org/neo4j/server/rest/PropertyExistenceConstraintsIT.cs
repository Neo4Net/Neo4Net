using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.rest
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using GraphHolder = Neo4Net.Test.GraphHolder;
	using Neo4Net.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_SCHEMA_CONSTRAINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.web.Surface_Fields.PATH_SCHEMA_RELATIONSHIP_CONSTRAINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.rule.SuppressOutput.suppressAll;

	public class PropertyExistenceConstraintsIT : GraphHolder
	{
		private bool InstanceFieldsInitialized = false;

		public PropertyExistenceConstraintsIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Data = TestData.producedThrough( GraphDescription.createGraphFor( this, true ) );
		}

		 private readonly IFactory<string> _labels = UniqueStrings.WithPrefix( "label" );
		 private readonly IFactory<string> _properties = UniqueStrings.WithPrefix( "property" );
		 private readonly IFactory<string> _relationshipTypes = UniqueStrings.WithPrefix( "relationshipType" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<java.util.Map<String,Neo4Net.graphdb.Node>> data = Neo4Net.test.TestData.producedThrough(Neo4Net.test.GraphDescription.createGraphFor(this, true));
		 public TestData<IDictionary<string, Node>> Data;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<RESTRequestGenerator> gen = Neo4Net.test.TestData.producedThrough(RESTRequestGenerator.PRODUCER);
		 public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

		 private static NeoServer _server;

		 public override IGraphDatabaseService Graphdb()
		 {
			  return _server.Database.Graph;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void InitServer()
		 {
			  suppressAll().call((Callable<Void>)() =>
			  {
				CommunityServerBuilder serverBuilder = EnterpriseServerBuilder.serverOnRandomPorts();

				PropertyExistenceConstraintsIT._server = ServerHelper.createNonPersistentServer( serverBuilder );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void StopServer()
		 {
			  if ( _server != null )
			  {
					suppressAll().call((Callable<Void>)() =>
					{
					 _server.stop();
					 return null;
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get a specific node property existence constraint.\n" + "Get a specific node property existence constraint for a label and a property.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelPropertyExistenceConstraint() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get a specific node property existence constraint.\n" + "Get a specific node property existence constraint for a label and a property.")]
		 public virtual void getLabelPropertyExistenceConstraint()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateLabelPropertyExistenceConstraint( labelName, propertyKey );

			  string result = Gen.get().expectedStatus(200).get(GetSchemaConstraintLabelExistencePropertyUri(labelName, propertyKey)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint = new Dictionary<string, object>();
			  constraint["type"] = ConstraintType.NODE_PROPERTY_EXISTENCE.name();
			  constraint["label"] = labelName;
			  constraint["property_keys"] = singletonList( propertyKey );

			  assertThat( serializedList, hasItem( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get a specific relationship property existence constraint.\n" + "Get a specific relationship property existence constraint for a label and a property.") @Test @GraphDescription.Graph(nodes = {}) public void getRelationshipTypePropertyExistenceConstraint() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get a specific relationship property existence constraint.\n" + "Get a specific relationship property existence constraint for a label and a property.")]
		 public virtual void getRelationshipTypePropertyExistenceConstraint()
		 {
			  Data.get();

			  string typeName = _relationshipTypes.newInstance();
			  string propertyKey = _properties.newInstance();
			  CreateRelationshipTypePropertyExistenceConstraint( typeName, propertyKey );

			  string result = Gen.get().expectedStatus(200).get(GetSchemaRelationshipConstraintTypeExistencePropertyUri(typeName, propertyKey)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint = new Dictionary<string, object>();
			  constraint["type"] = ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE.name();
			  constraint["relationshipType"] = typeName;
			  constraint["property_keys"] = singletonList( propertyKey );

			  assertThat( serializedList, hasItem( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all node property existence constraints for a label.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelPropertyExistenceConstraints() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all node property existence constraints for a label.")]
		 public virtual void getLabelPropertyExistenceConstraints()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateLabelPropertyExistenceConstraint( labelName, propertyKey1 );
			  CreateLabelPropertyExistenceConstraint( labelName, propertyKey2 );

			  string result = Gen.get().expectedStatus(200).get(GetSchemaConstraintLabelExistenceUri(labelName)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.NODE_PROPERTY_EXISTENCE.name();
			  constraint1["label"] = labelName;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> constraint2 = new Dictionary<string, object>();
			  constraint2["type"] = ConstraintType.NODE_PROPERTY_EXISTENCE.name();
			  constraint2["label"] = labelName;
			  constraint2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( constraint1, constraint2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all relationship property existence constraints for a type.") @Test @GraphDescription.Graph(nodes = {}) public void getRelationshipTypePropertyExistenceConstraints() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all relationship property existence constraints for a type.")]
		 public virtual void getRelationshipTypePropertyExistenceConstraints()
		 {
			  Data.get();

			  string typeName = _relationshipTypes.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateRelationshipTypePropertyExistenceConstraint( typeName, propertyKey1 );
			  CreateRelationshipTypePropertyExistenceConstraint( typeName, propertyKey2 );

			  string result = Gen.get().expectedStatus(200).get(GetSchemaRelationshipConstraintTypeExistenceUri(typeName)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE.name();
			  constraint1["relationshipType"] = typeName;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> constraint2 = new Dictionary<string, object>();
			  constraint2["type"] = ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE.name();
			  constraint2["relationshipType"] = typeName;
			  constraint2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( constraint1, constraint2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all constraints for a label.") @Test @GraphDescription.Graph(nodes = {}) public void getLabelPropertyConstraints() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all constraints for a label.")]
		 public virtual void getLabelPropertyConstraints()
		 {
			  Data.get();

			  string labelName = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName, propertyKey1 );
			  CreateLabelPropertyExistenceConstraint( labelName, propertyKey2 );

			  string result = Gen.get().expectedStatus(200).get(GetSchemaConstraintLabelUri(labelName)).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.UNIQUENESS.name();
			  constraint1["label"] = labelName;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> constraint2 = new Dictionary<string, object>();
			  constraint2["type"] = ConstraintType.NODE_PROPERTY_EXISTENCE.name();
			  constraint2["label"] = labelName;
			  constraint2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( constraint1, constraint2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Documented("Get all constraints.") @Test @GraphDescription.Graph(nodes = {}) public void get_constraints() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all constraints.")]
		 public virtual void GetConstraints()
		 {
			  Data.get();

			  string labelName1 = _labels.newInstance();
			  string propertyKey1 = _properties.newInstance();
			  string labelName2 = _labels.newInstance();
			  string propertyKey2 = _properties.newInstance();
			  CreateLabelUniquenessPropertyConstraint( labelName1, propertyKey1 );
			  CreateLabelPropertyExistenceConstraint( labelName2, propertyKey2 );

			  string result = Gen.get().expectedStatus(200).get(SchemaConstraintUri).entity();

			  IList<IDictionary<string, object>> serializedList = jsonToList( result );

			  IDictionary<string, object> constraint1 = new Dictionary<string, object>();
			  constraint1["type"] = ConstraintType.UNIQUENESS.name();
			  constraint1["label"] = labelName1;
			  constraint1["property_keys"] = singletonList( propertyKey1 );

			  IDictionary<string, object> constraint2 = new Dictionary<string, object>();
			  constraint2["type"] = ConstraintType.NODE_PROPERTY_EXISTENCE.name();
			  constraint2["label"] = labelName2;
			  constraint2["property_keys"] = singletonList( propertyKey2 );

			  assertThat( serializedList, hasItems( constraint1, constraint2 ) );
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

		 private string DataUri
		 {
			 get
			 {
				  return _server.baseUri() + "db/data/";
			 }
		 }

		 public virtual string SchemaConstraintUri
		 {
			 get
			 {
				  return DataUri + PATH_SCHEMA_CONSTRAINT;
			 }
		 }

		 public virtual string GetSchemaConstraintLabelUri( string label )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label;
		 }

		 private void CreateLabelPropertyExistenceConstraint( string labelName, string propertyKey )
		 {
			  string query = string.Format( "CREATE CONSTRAINT ON (n:{0}) ASSERT exists(n.{1})", labelName, propertyKey );
			  Graphdb().execute(query);
		 }

		 private void CreateRelationshipTypePropertyExistenceConstraint( string typeName, string propertyKey )
		 {
			  string query = string.Format( "CREATE CONSTRAINT ON ()-[r:{0}]-() ASSERT exists(r.{1})", typeName, propertyKey );
			  Graphdb().execute(query);
		 }

		 private string GetSchemaConstraintLabelExistenceUri( string label )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label + "/existence/";
		 }

		 private string GetSchemaRelationshipConstraintTypeExistenceUri( string type )
		 {
			  return DataUri + PATH_SCHEMA_RELATIONSHIP_CONSTRAINT + "/" + type + "/existence/";
		 }

		 private string GetSchemaConstraintLabelExistencePropertyUri( string label, string property )
		 {
			  return DataUri + PATH_SCHEMA_CONSTRAINT + "/" + label + "/existence/" + property;
		 }

		 private string GetSchemaRelationshipConstraintTypeExistencePropertyUri( string type, string property )
		 {
			  return DataUri + PATH_SCHEMA_RELATIONSHIP_CONSTRAINT + "/" + type + "/existence/" + property;
		 }
	}

}