using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using Neo4Net.Internal.Kernel.Api;
	using NodeValueIndexCursor = Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using ValueType = Neo4Net.Values.Storable.ValueType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueTuple.COMPARATOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("FieldCanBeLocal") public class IndexProvidedOrderNativeBTree10Test extends org.neo4j.internal.kernel.api.KernelAPIReadTestBase<ReadTestSupport>
	public class IndexProvidedOrderNativeBTree10Test : KernelAPIReadTestBase<ReadTestSupport>
	{
		 private static int _nNodes = 10000;
		 private static int _nIterations = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.RandomRule randomRule = new org.neo4j.test.rule.RandomRule();
		 public RandomRule RandomRule = new RandomRule();

		 private SortedSet<NodeValueTuple> _singlePropValues = new SortedSet<NodeValueTuple>( COMPARATOR );
		 private SortedSet<NodeValueTuple> _doublePropValues = new SortedSet<NodeValueTuple>( COMPARATOR );
		 private ValueType[] _targetedTypes;

		 public override ReadTestSupport NewTestSupport()
		 {
			  ReadTestSupport readTestSupport = new ReadTestSupport();
			  readTestSupport.AddSetting( GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10.providerName() );
			  return readTestSupport;
		 }

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().indexFor(label("Node")).on("prop").create();
					graphDb.Schema().indexFor(label("Node")).on("prop").on("prip").create();
					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().awaitIndexesOnline(5, MINUTES);
					tx.Success();
			  }

			  RandomValues randomValues = RandomRule.randomValues();

			  ValueType[] allExceptNonOrderable = RandomValues.excluding( ValueType.STRING, ValueType.STRING_ARRAY, ValueType.GEOGRAPHIC_POINT, ValueType.GEOGRAPHIC_POINT_ARRAY, ValueType.GEOGRAPHIC_POINT_3D, ValueType.GEOGRAPHIC_POINT_3D_ARRAY, ValueType.CARTESIAN_POINT, ValueType.CARTESIAN_POINT_ARRAY, ValueType.CARTESIAN_POINT_3D, ValueType.CARTESIAN_POINT_3D_ARRAY );
			  _targetedTypes = randomValues.Selection( allExceptNonOrderable, 1, allExceptNonOrderable.Length, false );
			  _targetedTypes = EnsureHighEnoughCardinality( _targetedTypes );
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					for ( int i = 0; i < _nNodes; i++ )
					{
						 Node node = graphDb.CreateNode( label( "Node" ) );
						 Value propValue;
						 Value pripValue;
						 NodeValueTuple singleValue;
						 NodeValueTuple doubleValue;
						 do
						 {
							  propValue = randomValues.NextValueOfTypes( _targetedTypes );
							  pripValue = randomValues.NextValueOfTypes( _targetedTypes );
							  singleValue = new NodeValueTuple( this, node.Id, propValue );
							  doubleValue = new NodeValueTuple( this, node.Id, propValue, pripValue );
						 } while ( _singlePropValues.Contains( singleValue ) || _doublePropValues.Contains( doubleValue ) );
						 _singlePropValues.Add( singleValue );
						 _doublePropValues.Add( doubleValue );

						 node.SetProperty( "prop", propValue.AsObject() );
						 node.SetProperty( "prip", pripValue.AsObject() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideResultInOrderIfCapable() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideResultInOrderIfCapable()
		 {
			  int label = Token.nodeLabel( "Node" );
			  int prop = Token.propertyKey( "prop" );

			  RandomValues randomValues = RandomRule.randomValues();
			  IndexReference index = SchemaRead.index( label, prop );
			  for ( int i = 0; i < _nIterations; i++ )
			  {
					ValueType type = randomValues.Among( _targetedTypes );
					IndexOrder[] order = index.OrderCapability( type.valueGroup.category() );
					foreach ( IndexOrder indexOrder in order )
					{
						 if ( indexOrder == IndexOrder.NONE )
						 {
							  continue;
						 }
						 NodeValueTuple from = new NodeValueTuple( this, long.MinValue, randomValues.NextValueOfType( type ) );
						 NodeValueTuple to = new NodeValueTuple( this, long.MaxValue, randomValues.NextValueOfType( type ) );
						 if ( COMPARATOR.compare( from, to ) > 0 )
						 {
							  NodeValueTuple tmp = from;
							  from = to;
							  to = tmp;
						 }
						 bool fromInclusive = randomValues.NextBoolean();
						 bool toInclusive = randomValues.NextBoolean();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> range = org.neo4j.internal.kernel.api.IndexQuery.range(prop, from.getOnlyValue(), fromInclusive, to.getOnlyValue(), toInclusive);
						 IndexQuery.RangePredicate<object> range = IndexQuery.range( prop, from.OnlyValue, fromInclusive, to.OnlyValue, toInclusive );

						 using ( NodeValueIndexCursor node = Cursors.allocateNodeValueIndexCursor() )
						 {
							  Read.nodeIndexSeek( index, node, indexOrder, false, range );

							  IList<long> expectedIdsInOrder = expectedIdsInOrder( from, fromInclusive, to, toInclusive, indexOrder );
							  IList<long> actualIdsInOrder = new List<long>();
							  while ( node.Next() )
							  {
									actualIdsInOrder.Add( node.NodeReference() );
							  }

							  assertEquals( expectedIdsInOrder, actualIdsInOrder, "actual node ids not in same order as expected" );

						 }
					}
			  }
		 }

		 private IList<long> ExpectedIdsInOrder( NodeValueTuple from, bool fromInclusive, NodeValueTuple to, bool toInclusive, IndexOrder indexOrder )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<long> expectedIdsInOrder = _singlePropValues.subSet( from, fromInclusive, to, toInclusive ).Select( NodeValueTuple::nodeId ).ToList();
			  if ( indexOrder == IndexOrder.DESCENDING )
			  {
					expectedIdsInOrder.Reverse();
			  }
			  return expectedIdsInOrder;
		 }

		 /// <summary>
		 /// If targetedTypes only contain types that has very low cardinality, then add one random high cardinality value type to the array.
		 /// This is to prevent createTestGraph from looping forever when trying to generate unique values.
		 /// </summary>
		 private ValueType[] EnsureHighEnoughCardinality( ValueType[] targetedTypes )
		 {
			  ValueType[] lowCardinalityArray = new ValueType[]{ ValueType.BOOLEAN, ValueType.BYTE, ValueType.BOOLEAN_ARRAY };
			  IList<ValueType> typesOfLowCardinality = new List<ValueType>( Arrays.asList( lowCardinalityArray ) );
			  foreach ( ValueType targetedType in targetedTypes )
			  {
					if ( !typesOfLowCardinality.Contains( targetedType ) )
					{
						 return targetedTypes;
					}
			  }
			  List<ValueType> result = new List<ValueType>( Arrays.asList( targetedTypes ) );
			  ValueType highCardinalityType = RandomRule.randomValues().among(RandomValues.excluding(lowCardinalityArray));
			  result.Add( highCardinalityType );
			  return result.ToArray();
		 }

		 private class NodeValueTuple : ValueTuple
		 {
			 private readonly IndexProvidedOrderNativeBTree10Test _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long NodeIdConflict;

			  internal NodeValueTuple( IndexProvidedOrderNativeBTree10Test outerInstance, long nodeId, params Value[] values ) : base( values )
			  {
				  this._outerInstance = outerInstance;
					this.NodeIdConflict = nodeId;
			  }

			  internal virtual long NodeId()
			  {
					return NodeIdConflict;
			  }
		 }
	}

}