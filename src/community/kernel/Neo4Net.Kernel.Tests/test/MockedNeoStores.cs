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
namespace Neo4Net.Test
{
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using DynamicArrayStore = Neo4Net.Kernel.impl.store.DynamicArrayStore;
	using DynamicStringStore = Neo4Net.Kernel.impl.store.DynamicStringStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class MockedNeoStores
	{
		 private MockedNeoStores()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "rawtypes"}) public static org.Neo4Net.kernel.impl.store.NeoStores basicMockedNeoStores()
		 public static NeoStores BasicMockedNeoStores()
		 {
			  NeoStores neoStores = mock( typeof( NeoStores ) );

			  // NodeStore - DynamicLabelStore
			  NodeStore nodeStore = mock( typeof( NodeStore ) );
			  when( neoStores.NodeStore ).thenReturn( nodeStore );

			  // NodeStore - DynamicLabelStore
			  DynamicArrayStore dynamicLabelStore = mock( typeof( DynamicArrayStore ) );
			  when( nodeStore.DynamicLabelStore ).thenReturn( dynamicLabelStore );

			  // RelationshipStore
			  RelationshipStore relationshipStore = mock( typeof( RelationshipStore ) );
			  when( neoStores.RelationshipStore ).thenReturn( relationshipStore );

			  // RelationshipGroupStore
			  RelationshipGroupStore relationshipGroupStore = mock( typeof( RelationshipGroupStore ) );
			  when( neoStores.RelationshipGroupStore ).thenReturn( relationshipGroupStore );

			  // PropertyStore
			  PropertyStore propertyStore = mock( typeof( PropertyStore ) );
			  when( neoStores.PropertyStore ).thenReturn( propertyStore );

			  // PropertyStore -- DynamicStringStore
			  DynamicStringStore propertyStringStore = mock( typeof( DynamicStringStore ) );
			  when( propertyStore.StringStore ).thenReturn( propertyStringStore );

			  // PropertyStore -- DynamicArrayStore
			  DynamicArrayStore propertyArrayStore = mock( typeof( DynamicArrayStore ) );
			  when( propertyStore.ArrayStore ).thenReturn( propertyArrayStore );

			  return neoStores;
		 }

		 public static TokenHolders MockedTokenHolders()
		 {
			  return new TokenHolders( mock( typeof( TokenHolder ) ), mock( typeof( TokenHolder ) ), mock( typeof( TokenHolder ) ) );
		 }
	}

}