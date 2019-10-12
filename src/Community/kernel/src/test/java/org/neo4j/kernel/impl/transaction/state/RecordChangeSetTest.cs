/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Test = org.junit.Test;

	using Loaders = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RecordChangeSetTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithSetsInitializedAndEmpty()
		 public virtual void ShouldStartWithSetsInitializedAndEmpty()
		 {
			  // GIVEN
			  RecordChangeSet changeSet = new RecordChangeSet( mock( typeof( Loaders ) ) );

			  // WHEN
			  // nothing really

			  // THEN
			  assertEquals( 0, changeSet.NodeRecords.changeSize() );
			  assertEquals( 0, changeSet.PropertyRecords.changeSize() );
			  assertEquals( 0, changeSet.RelRecords.changeSize() );
			  assertEquals( 0, changeSet.SchemaRuleChanges.changeSize() );
			  assertEquals( 0, changeSet.RelGroupRecords.changeSize() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearStateOnClose()
		 public virtual void ShouldClearStateOnClose()
		 {
			  // GIVEN
			  NeoStores mockStore = mock( typeof( NeoStores ) );
			  NodeStore store = mock( typeof( NodeStore ) );
			  when( mockStore.NodeStore ).thenReturn( store );
			  RelationshipStore relationshipStore = mock( typeof( RelationshipStore ) );
			  when( mockStore.RelationshipStore ).thenReturn( relationshipStore );
			  PropertyStore propertyStore = mock( typeof( PropertyStore ) );
			  when( mockStore.PropertyStore ).thenReturn( propertyStore );
			  SchemaStore schemaStore = mock( typeof( SchemaStore ) );
			  when( mockStore.SchemaStore ).thenReturn( schemaStore );
			  RelationshipGroupStore groupStore = mock( typeof( RelationshipGroupStore ) );
			  when( mockStore.RelationshipGroupStore ).thenReturn( groupStore );

			  RecordChangeSet changeSet = new RecordChangeSet( new Loaders( mockStore ) );

			  // WHEN
			  /*
			   * We need to make sure some stuff is stored in the sets being managed. That is why forChangingLinkage() is
			   * called - otherwise, no changes will be stored and changeSize() would return 0 anyway.
			   */
			  changeSet.NodeRecords.create( 1L, null ).forChangingLinkage();
			  changeSet.PropertyRecords.create( 1L, null ).forChangingLinkage();
			  changeSet.RelRecords.create( 1L, null ).forChangingLinkage();
			  changeSet.SchemaRuleChanges.create( 1L, null ).forChangingLinkage();
			  changeSet.RelGroupRecords.create( 1L, 1 ).forChangingLinkage();

			  changeSet.Close();

			  // THEN
			  assertEquals( 0, changeSet.NodeRecords.changeSize() );
			  assertEquals( 0, changeSet.PropertyRecords.changeSize() );
			  assertEquals( 0, changeSet.RelRecords.changeSize() );
			  assertEquals( 0, changeSet.SchemaRuleChanges.changeSize() );
			  assertEquals( 0, changeSet.RelGroupRecords.changeSize() );
		 }
	}

}