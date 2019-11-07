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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using NeoStoresRule = Neo4Net.Test.rule.NeoStoresRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexProvider.EMPTY;

	public class HighIdTransactionApplierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.NeoStoresRule neoStoresRule = new Neo4Net.test.rule.NeoStoresRule(getClass());
		 public readonly NeoStoresRule NeoStoresRule = new NeoStoresRule( this.GetType() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateHighIdsOnExternalTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateHighIdsOnExternalTransaction()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  HighIdTransactionApplier tracker = new HighIdTransactionApplier( neoStores );

			  // WHEN
			  // Nodes
			  tracker.VisitNodeCommand( Commands.CreateNode( 10, 2, 3 ) );
			  tracker.VisitNodeCommand( Commands.CreateNode( 20, 4, 5 ) );

			  // Relationships
			  tracker.VisitRelationshipCommand( Commands.CreateRelationship( 4, 10, 20, 0 ) );
			  tracker.VisitRelationshipCommand( Commands.CreateRelationship( 45, 10, 20, 1 ) );

			  // Label tokens
			  tracker.VisitLabelTokenCommand( Commands.CreateLabelToken( 3, 0 ) );
			  tracker.VisitLabelTokenCommand( Commands.CreateLabelToken( 5, 1 ) );

			  // Property tokens
			  tracker.VisitPropertyKeyTokenCommand( Commands.CreatePropertyKeyToken( 3, 0 ) );
			  tracker.VisitPropertyKeyTokenCommand( Commands.CreatePropertyKeyToken( 5, 1 ) );

			  // Relationship type tokens
			  tracker.VisitRelationshipTypeTokenCommand( Commands.CreateRelationshipTypeToken( 3, 0 ) );
			  tracker.VisitRelationshipTypeTokenCommand( Commands.CreateRelationshipTypeToken( 5, 1 ) );

			  // Relationship groups
			  tracker.VisitRelationshipGroupCommand( Commands.CreateRelationshipGroup( 10, 1 ) );
			  tracker.VisitRelationshipGroupCommand( Commands.CreateRelationshipGroup( 20, 2 ) );

			  // Schema rules
			  tracker.VisitSchemaRuleCommand( Commands.CreateIndexRule( EMPTY.ProviderDescriptor, 10, SchemaDescriptorFactory.forLabel( 0, 1 ) ) );
			  tracker.VisitSchemaRuleCommand( Commands.CreateIndexRule( EMPTY.ProviderDescriptor, 20, SchemaDescriptorFactory.forLabel( 1, 2 ) ) );

			  // Properties
			  tracker.VisitPropertyCommand( Commands.CreateProperty( 10, PropertyType.STRING, 0, 6, 7 ) );
			  tracker.VisitPropertyCommand( Commands.CreateProperty( 20, PropertyType.ARRAY, 1, 8, 9 ) );

			  tracker.Close();

			  // THEN
			  assertEquals( "NodeStore", 20 + 1, neoStores.NodeStore.HighId );
			  assertEquals( "DynamicNodeLabelStore", 5 + 1, neoStores.NodeStore.DynamicLabelStore.HighId );
			  assertEquals( "RelationshipStore", 45 + 1, neoStores.RelationshipStore.HighId );
			  assertEquals( "RelationshipTypeStore", 5 + 1, neoStores.RelationshipTypeTokenStore.HighId );
			  assertEquals( "RelationshipType NameStore", 1 + 1, neoStores.RelationshipTypeTokenStore.NameStore.HighId );
			  assertEquals( "PropertyKeyStore", 5 + 1, neoStores.PropertyKeyTokenStore.HighId );
			  assertEquals( "PropertyKey NameStore", 1 + 1, neoStores.PropertyKeyTokenStore.NameStore.HighId );
			  assertEquals( "LabelStore", 5 + 1, neoStores.LabelTokenStore.HighId );
			  assertEquals( "Label NameStore", 1 + 1, neoStores.LabelTokenStore.NameStore.HighId );
			  assertEquals( "PropertyStore", 20 + 1, neoStores.PropertyStore.HighId );
			  assertEquals( "PropertyStore DynamicStringStore", 7 + 1, neoStores.PropertyStore.StringStore.HighId );
			  assertEquals( "PropertyStore DynamicArrayStore", 9 + 1, neoStores.PropertyStore.ArrayStore.HighId );
			  assertEquals( "SchemaStore", 20 + 1, neoStores.SchemaStore.HighId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackSecondaryUnitIdsAsWell() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackSecondaryUnitIdsAsWell()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  HighIdTransactionApplier tracker = new HighIdTransactionApplier( neoStores );

			  NodeRecord node = ( new NodeRecord( 5 ) ).initialize( true, 123, true, 456, 0 );
			  node.SecondaryUnitId = 6;
			  node.RequiresSecondaryUnit = true;

			  RelationshipRecord relationship = ( new RelationshipRecord( 10 ) ).initialize( true, 1, 2, 3, 4, 5, 6, 7, 8, true, true );
			  relationship.SecondaryUnitId = 12;
			  relationship.RequiresSecondaryUnit = true;

			  RelationshipGroupRecord relationshipGroup = ( new RelationshipGroupRecord( 8 ) ).initialize( true, 0, 1, 2, 3, 4, 5 );
			  relationshipGroup.SecondaryUnitId = 20;
			  relationshipGroup.RequiresSecondaryUnit = true;

			  // WHEN
			  tracker.VisitNodeCommand( new NodeCommand( new NodeRecord( node.Id ), node ) );
			  tracker.VisitRelationshipCommand( new RelationshipCommand( new RelationshipRecord( relationship.Id ), relationship ) );
			  tracker.VisitRelationshipGroupCommand( new RelationshipGroupCommand( new RelationshipGroupRecord( relationshipGroup.Id ), relationshipGroup ) );
			  tracker.Close();

			  // THEN
			  assertEquals( node.SecondaryUnitId + 1, neoStores.NodeStore.HighId );
			  assertEquals( relationship.SecondaryUnitId + 1, neoStores.RelationshipStore.HighId );
			  assertEquals( relationshipGroup.SecondaryUnitId + 1, neoStores.RelationshipGroupStore.HighId );
		 }
	}

}