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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using GraphPropertiesProxy = Org.Neo4j.Kernel.impl.core.GraphPropertiesProxy;
	using NodeProxy = Org.Neo4j.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Org.Neo4j.Kernel.impl.core.RelationshipProxy;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Values = Org.Neo4j.Values.Storable.Values;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using NodeReference = Org.Neo4j.Values.@virtual.NodeReference;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using RelationshipReference = Org.Neo4j.Values.@virtual.RelationshipReference;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	public class CompiledMaterializeValueMapperTest
	{
		private bool InstanceFieldsInitialized = false;

		public CompiledMaterializeValueMapperTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			DirectRelationshipValue = VirtualValues.relationshipValue( 12L, NodeProxyValue, DirectNodeValue, Values.stringValue( "TYPE" ), VirtualValues.emptyMap() );
		}

		 private EmbeddedProxySPI spi = new EmbeddedProxySPIAnonymousInnerClass();

		 private class EmbeddedProxySPIAnonymousInnerClass : EmbeddedProxySPI
		 {
			 public RelationshipProxy newRelationshipProxy( long id )
			 {
				  return new RelationshipProxy( this, id );
			 }

			 public NodeProxy newNodeProxy( long nodeId )
			 {
				  return new NodeProxy( this, nodeId );
			 }

			 public Statement statement()
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public KernelTransaction kernelTransaction()
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public GraphDatabaseService GraphDatabase
			 {
				 get
				 {
					  throw new System.InvalidOperationException( "Should not be used" );
				 }
			 }

			 public void assertInUnterminatedTransaction()
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public void failTransaction()
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public RelationshipProxy newRelationshipProxy( long id, long startNodeId, int typeId, long endNodeId )
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public GraphPropertiesProxy newGraphPropertiesProxy()
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }

			 public RelationshipType getRelationshipTypeById( int type )
			 {
				  throw new System.InvalidOperationException( "Should not be used" );
			 }
		 }

		 internal NodeValue NodeProxyValue = ValueUtils.fromNodeProxy( new NodeProxy( spi, 1L ) );
		 internal NodeValue DirectNodeValue = VirtualValues.nodeValue( 2L, Values.stringArray(), VirtualValues.emptyMap() );
		 internal NodeReference NodeReference = VirtualValues.node( 1L ); // Should equal nodeProxyValue when converted

		 internal RelationshipValue RelationshipProxyValue = ValueUtils.fromRelationshipProxy( new RelationshipProxy( spi, 11L ) );
		 internal RelationshipValue DirectRelationshipValue;
		 internal RelationshipReference RelationshipReference = VirtualValues.relationship( 11L ); // Should equal relationshipProxyValue when converted

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTouchValuesThatDoNotNeedConversion()
		 public virtual void ShouldNotTouchValuesThatDoNotNeedConversion()
		 {
			  // Given
			  ListValue nodeList = VirtualValues.list( NodeProxyValue, DirectNodeValue );
			  ListValue relationshipList = VirtualValues.list( RelationshipProxyValue, DirectRelationshipValue );
			  MapValue nodeMap = VirtualValues.map( new string[]{ "a", "b" }, new AnyValue[]{ NodeProxyValue, DirectNodeValue } );
			  MapValue relationshipMap = VirtualValues.map( new string[]{ "a", "b" }, new AnyValue[]{ RelationshipProxyValue, DirectRelationshipValue } );

			  // Verify
			  VerifyDoesNotTouchValue( NodeProxyValue );
			  VerifyDoesNotTouchValue( RelationshipProxyValue );
			  VerifyDoesNotTouchValue( DirectNodeValue );
			  VerifyDoesNotTouchValue( DirectRelationshipValue );
			  VerifyDoesNotTouchValue( nodeList );
			  VerifyDoesNotTouchValue( relationshipList );
			  VerifyDoesNotTouchValue( nodeMap );
			  VerifyDoesNotTouchValue( relationshipMap );

			  // This is not an exhaustive test since the other cases are very uninteresting...
			  VerifyDoesNotTouchValue( Values.booleanValue( false ) );
			  VerifyDoesNotTouchValue( Values.stringValue( "Hello" ) );
			  VerifyDoesNotTouchValue( Values.longValue( 42L ) );
			  // ...
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertValuesWithVirtualEntities()
		 public virtual void ShouldConvertValuesWithVirtualEntities()
		 {
			  // Given
			  ListValue nodeList = VirtualValues.list( NodeProxyValue, DirectNodeValue, NodeReference );
			  ListValue expectedNodeList = VirtualValues.list( NodeProxyValue, DirectNodeValue, NodeProxyValue );

			  ListValue relationshipList = VirtualValues.list( RelationshipProxyValue, DirectRelationshipValue, RelationshipReference );
			  ListValue expectedRelationshipList = VirtualValues.list( RelationshipProxyValue, DirectRelationshipValue, RelationshipProxyValue );

			  MapValue nodeMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ NodeProxyValue, DirectNodeValue, NodeReference } );
			  MapValue expectedNodeMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ NodeProxyValue, DirectNodeValue, NodeProxyValue } );

			  MapValue relationshipMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ RelationshipProxyValue, DirectRelationshipValue, RelationshipReference } );
			  MapValue expectedRelationshipMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ RelationshipProxyValue, DirectRelationshipValue, RelationshipProxyValue } );

			  ListValue nestedNodeList = VirtualValues.list( nodeList, nodeMap, NodeReference );
			  ListValue expectedNestedNodeList = VirtualValues.list( expectedNodeList, expectedNodeMap, NodeProxyValue );

			  ListValue nestedRelationshipList = VirtualValues.list( relationshipList, relationshipMap, RelationshipReference );
			  ListValue expectedNestedRelationshipList = VirtualValues.list( expectedRelationshipList, expectedRelationshipMap, RelationshipProxyValue );

			  MapValue nestedNodeMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ nodeList, nodeMap, nestedNodeList } );
			  MapValue expectedNestedNodeMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ expectedNodeList, expectedNodeMap, expectedNestedNodeList } );

			  MapValue nestedRelationshipMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ relationshipList, relationshipMap, nestedRelationshipList } );
			  MapValue expectedNestedRelationshipMap = VirtualValues.map( new string[]{ "a", "b", "c" }, new AnyValue[]{ expectedRelationshipList, expectedRelationshipMap, expectedNestedRelationshipList } );

			  // Verify
			  VerifyConvertsValue( expectedNodeList, nodeList );
			  VerifyConvertsValue( expectedRelationshipList, relationshipList );

			  VerifyConvertsValue( expectedNodeMap, nodeMap );
			  VerifyConvertsValue( expectedRelationshipMap, relationshipMap );

			  VerifyConvertsValue( expectedNestedNodeList, nestedNodeList );
			  VerifyConvertsValue( expectedNestedRelationshipList, nestedRelationshipList );

			  VerifyConvertsValue( expectedNestedNodeMap, nestedNodeMap );
			  VerifyConvertsValue( expectedNestedRelationshipMap, nestedRelationshipMap );
		 }

		 private void VerifyConvertsValue( AnyValue expected, AnyValue valueToTest )
		 {
			  AnyValue actual = CompiledMaterializeValueMapper.MapAnyValue( spi, valueToTest );
			  assertEquals( expected, actual );
		 }

		 private void VerifyDoesNotTouchValue( AnyValue value )
		 {
			  AnyValue mappedValue = CompiledMaterializeValueMapper.MapAnyValue( spi, value );
			  assertSame( value, mappedValue ); // Test with reference equality since we should get the same reference back
		 }
	}

}