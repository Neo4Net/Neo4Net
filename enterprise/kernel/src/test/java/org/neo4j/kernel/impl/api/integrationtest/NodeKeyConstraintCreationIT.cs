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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;

	public class NodeKeyConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, LabelSchemaDescriptor>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(org.neo4j.internal.kernel.api.TokenWrite tokenWrite, String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.LabelGetOrCreateForName( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.kernel.api.schema.LabelSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, LabelSchemaDescriptor descriptor )
		 {
			  return writeOps.NodeKeyConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( GraphDatabaseService db, string type, string property )
		 {
			  SchemaHelper.createNodeKeyConstraint( db, type, property );
		 }

		 internal override NodeKeyConstraintDescriptor NewConstraintObject( LabelSchemaDescriptor descriptor )
		 {
			  return ConstraintDescriptorFactory.nodeKeyForSchema( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dropConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws Exception
		 internal override void DropConstraint( SchemaWrite writeOps, ConstraintDescriptor constraint )
		 {
			  writeOps.ConstraintDrop( constraint );
		 }

		 internal override void CreateOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  Db.createNode( Label.label( KEY ) );
		 }

		 internal override void RemoveOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  using ( ResourceIterator<Node> nodes = Db.findNodes( Label.label( KEY ) ) )
			  {
					while ( nodes.MoveNext() )
					{
						 nodes.Current.delete();
					}
			  }
		 }

		 internal override LabelSchemaDescriptor MakeDescriptor( int typeId, int propertyKeyId )
		 {
			  return SchemaDescriptorFactory.forLabel( typeId, propertyKeyId );
		 }
	}

}