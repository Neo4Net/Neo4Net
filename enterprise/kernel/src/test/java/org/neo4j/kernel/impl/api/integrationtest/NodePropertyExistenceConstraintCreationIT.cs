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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using DropConstraintFailureException = Org.Neo4j.Kernel.Api.Exceptions.schema.DropConstraintFailureException;
	using NoSuchConstraintException = Org.Neo4j.Kernel.Api.Exceptions.schema.NoSuchConstraintException;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeExistenceConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeExistenceConstraintDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;

	public class NodePropertyExistenceConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, LabelSchemaDescriptor>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(org.neo4j.internal.kernel.api.TokenWrite tokenWrite, String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.LabelGetOrCreateForName( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, LabelSchemaDescriptor descriptor )
		 {
			  return writeOps.NodePropertyExistenceConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( GraphDatabaseService db, string label, string property )
		 {
			  SchemaHelper.createNodePropertyExistenceConstraint( db, label, property );
		 }

		 internal override NodeExistenceConstraintDescriptor NewConstraintObject( LabelSchemaDescriptor descriptor )
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
			  Db.createNode( label( KEY ) );
		 }

		 internal override void RemoveOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  using ( ResourceIterator<Node> nodes = Db.findNodes( label( KEY ) ) )
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDropPropertyExistenceConstraintThatDoesNotExistWhenThereIsAUniquePropertyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropPropertyExistenceConstraintThatDoesNotExistWhenThereIsAUniquePropertyConstraint()
		 {
			  // given
			  ConstraintDescriptor constraint;
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					constraint = statement.UniquePropertyConstraintCreate( Descriptor );
					commit();
			  }

			  // when
			  try
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					statement.ConstraintDrop( ConstraintDescriptorFactory.existsForSchema( constraint.Schema() ) );

					fail( "expected exception" );
			  }
			  // then
			  catch ( DropConstraintFailureException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( NoSuchConstraintException ) ) );
			  }
			  finally
			  {
					rollback();
			  }

			  {
			  // then
					Transaction transaction = newTransaction();

					IEnumerator<ConstraintDescriptor> constraints = transaction.SchemaRead().constraintsGetForSchema(Descriptor);

					assertEquals( constraint, single( constraints ) );
					commit();
			  }
		 }
	}

}