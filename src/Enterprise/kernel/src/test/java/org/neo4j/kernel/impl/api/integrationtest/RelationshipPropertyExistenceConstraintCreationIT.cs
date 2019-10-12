using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class RelationshipPropertyExistenceConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, RelationTypeSchemaDescriptor>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(org.neo4j.internal.kernel.api.TokenWrite tokenWrite, String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.RelationshipTypeGetOrCreateForName( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, RelationTypeSchemaDescriptor descriptor )
		 {
			  return writeOps.RelationshipPropertyExistenceConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( GraphDatabaseService db, string type, string property )
		 {
			  SchemaHelper.createRelPropertyExistenceConstraint( db, type, property );
		 }

		 internal override RelExistenceConstraintDescriptor NewConstraintObject( RelationTypeSchemaDescriptor descriptor )
		 {
			  return ConstraintDescriptorFactory.existsForSchema( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dropConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws Exception
		 internal override void DropConstraint( SchemaWrite writeOps, ConstraintDescriptor constraint )
		 {
			  writeOps.ConstraintDrop( constraint );
		 }

		 internal override void CreateOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  Node start = Db.createNode();
			  Node end = Db.createNode();
			  start.CreateRelationshipTo( end, withName( KEY ) );
		 }

		 internal override void RemoveOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  IEnumerable<Relationship> relationships = Db.AllRelationships;
			  foreach ( Relationship relationship in relationships )
			  {
					relationship.Delete();
			  }
		 }

		 internal override RelationTypeSchemaDescriptor MakeDescriptor( int typeId, int propertyKeyId )
		 {
			  return SchemaDescriptorFactory.forRelType( typeId, propertyKeyId );
		 }
	}

}