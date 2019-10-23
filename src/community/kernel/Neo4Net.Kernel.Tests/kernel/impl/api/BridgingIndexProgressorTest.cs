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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;

	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using BridgingIndexProgressor = Neo4Net.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class BridgingIndexProgressorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustCloseAll()
		 public virtual void CloseMustCloseAll()
		 {
			  IndexDescriptor index = TestIndexDescriptorFactory.forLabel( 1, 2, 3 );
			  BridgingIndexProgressor progressor = new BridgingIndexProgressor( null, index.Schema().PropertyIds );

			  IndexProgressor[] parts = new IndexProgressor[] { mock( typeof( IndexProgressor ) ), mock( typeof( IndexProgressor ) ) };

			  // Given
			  foreach ( IndexProgressor part in parts )
			  {
					progressor.Initialize( index, part, null, IndexOrder.NONE, false );
			  }

			  // When
			  progressor.Close();

			  // Then
			  foreach ( IndexProgressor part in parts )
			  {
					verify( part, times( 1 ) ).close();
			  }
		 }
	}

}