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
namespace Neo4Net.Kernel.impl.enterprise
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class PropertyExistenceEnforcerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constraintPropertyIdsNotUpdatedByConstraintEnforcer()
		 public virtual void ConstraintPropertyIdsNotUpdatedByConstraintEnforcer()
		 {
			  UniquenessConstraintDescriptor uniquenessConstraint = ConstraintDescriptorFactory.uniqueForLabel( 1, 1, 70, 8 );
			  NodeKeyConstraintDescriptor nodeKeyConstraint = ConstraintDescriptorFactory.nodeKeyForLabel( 2, 12, 7, 13 );
			  RelExistenceConstraintDescriptor relTypeConstraint = ConstraintDescriptorFactory.existsForRelType( 3, 5, 13, 8 );
			  IList<ConstraintDescriptor> descriptors = Arrays.asList( uniquenessConstraint, nodeKeyConstraint, relTypeConstraint );

			  StorageReader storageReader = PrepareStorageReaderMock( descriptors );

			  PropertyExistenceEnforcer.GetOrCreatePropertyExistenceEnforcerFrom( storageReader );

			  assertArrayEquals( "Property ids should remain untouched.", new int[]{ 1, 70, 8 }, uniquenessConstraint.Schema().PropertyIds );
			  assertArrayEquals( "Property ids should remain untouched.", new int[]{ 12, 7, 13 }, nodeKeyConstraint.Schema().PropertyIds );
			  assertArrayEquals( "Property ids should remain untouched.", new int[]{ 5, 13, 8 }, relTypeConstraint.Schema().PropertyIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.storageengine.api.StorageReader prepareStorageReaderMock(java.util.List<org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor> descriptors)
		 private StorageReader PrepareStorageReaderMock( IList<ConstraintDescriptor> descriptors )
		 {
			  StorageReader storageReader = Mockito.mock( typeof( StorageReader ) );
			  when( storageReader.ConstraintsGetAll() ).thenReturn(descriptors.GetEnumerator());
			  when( storageReader.GetOrCreateSchemaDependantState( eq( typeof( PropertyExistenceEnforcer ) ), any( typeof( System.Func ) ) ) ).thenAnswer(invocation =>
			  {
				Function<StorageReader, PropertyExistenceEnforcer> function = invocation.getArgument( 1 );
				return function.apply( storageReader );
			  });
			  return storageReader;
		 }
	}

}