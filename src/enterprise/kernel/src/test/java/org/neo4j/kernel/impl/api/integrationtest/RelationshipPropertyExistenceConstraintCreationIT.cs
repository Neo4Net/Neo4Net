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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;

	public class RelationshipPropertyExistenceConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, RelationTypeSchemaDescriptor>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(org.Neo4Net.Kernel.Api.Internal.TokenWrite tokenWrite, String name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.RelationshipTypeGetOrCreateForName( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(org.Neo4Net.Kernel.Api.Internal.SchemaWrite writeOps, org.Neo4Net.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, RelationTypeSchemaDescriptor descriptor )
		 {
			  return writeOps.RelationshipPropertyExistenceConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( IGraphDatabaseService db, string type, string property )
		 {
			  SchemaHelper.createRelPropertyExistenceConstraint( db, type, property );
		 }

		 internal override RelExistenceConstraintDescriptor NewConstraintObject( RelationTypeSchemaDescriptor descriptor )
		 {
			  return ConstraintDescriptorFactory.existsForSchema( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dropConstraint(org.Neo4Net.Kernel.Api.Internal.SchemaWrite writeOps, org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor constraint) throws Exception
		 internal override void DropConstraint( SchemaWrite writeOps, ConstraintDescriptor constraint )
		 {
			  writeOps.ConstraintDrop( constraint );
		 }

		 internal override void CreateOffendingDataInRunningTx( IGraphDatabaseService db )
		 {
			  Node start = Db.createNode();
			  Node end = Db.createNode();
			  start.CreateRelationshipTo( end, withName( KEY ) );
		 }

		 internal override void RemoveOffendingDataInRunningTx( IGraphDatabaseService db )
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