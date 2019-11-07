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
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.Api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;

	public class NodeKeyConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, LabelSchemaDescriptor>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(Neo4Net.Kernel.Api.Internal.TokenWrite tokenWrite, String name) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.LabelGetOrCreateForName( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(Neo4Net.Kernel.Api.Internal.SchemaWrite writeOps, Neo4Net.kernel.api.schema.LabelSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, LabelSchemaDescriptor descriptor )
		 {
			  return writeOps.NodeKeyConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( IGraphDatabaseService db, string type, string property )
		 {
			  SchemaHelper.createNodeKeyConstraint( db, type, property );
		 }

		 internal override NodeKeyConstraintDescriptor NewConstraintObject( LabelSchemaDescriptor descriptor )
		 {
			  return ConstraintDescriptorFactory.nodeKeyForSchema( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dropConstraint(Neo4Net.Kernel.Api.Internal.SchemaWrite writeOps, Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor constraint) throws Exception
		 internal override void DropConstraint( SchemaWrite writeOps, ConstraintDescriptor constraint )
		 {
			  writeOps.ConstraintDrop( constraint );
		 }

		 internal override void CreateOffendingDataInRunningTx( IGraphDatabaseService db )
		 {
			  Db.createNode( Label.label( KEY ) );
		 }

		 internal override void RemoveOffendingDataInRunningTx( IGraphDatabaseService db )
		 {
			  using ( IResourceIterator<Node> nodes = Db.findNodes( Label.label( KEY ) ) )
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