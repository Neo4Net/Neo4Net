using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using Org.Neo4j.@internal.Kernel.Api;
	using NodeValueIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueTuple = Org.Neo4j.Values.Storable.ValueTuple;
	using ValueType = Org.Neo4j.Values.Storable.ValueType;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class IndexProvidedValuesNativeBTree10Test : KernelAPIReadTestBase<ReadTestSupport>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("FieldCanBeLocal") private static int N_NODES = 10000;
		 private static int _nNodes = 10000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.RandomRule randomRule = new org.neo4j.test.rule.RandomRule();
		 public RandomRule RandomRule = new RandomRule();

		 private IList<Value> _singlePropValues = new List<Value>();
		 private IList<ValueTuple> _doublePropValues = new List<ValueTuple>();

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

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					RandomValues randomValues = RandomRule.randomValues();

					ValueType[] allExceptNonSortable = RandomValues.excluding( ValueType.STRING, ValueType.STRING_ARRAY );

					for ( int i = 0; i < _nNodes; i++ )
					{
						 Node node = graphDb.CreateNode( label( "Node" ) );
						 Value propValue = randomValues.NextValueOfTypes( allExceptNonSortable );
						 node.SetProperty( "prop", propValue.AsObject() );
						 Value pripValue = randomValues.NextValue();
						 node.SetProperty( "prip", pripValue.AsObject() );

						 _singlePropValues.Add( propValue );
						 _doublePropValues.Add( ValueTuple.of( propValue, pripValue ) );
					}
					tx.Success();
			  }

			  _singlePropValues.sort( Values.COMPARATOR );
			  _doublePropValues.sort( ValueTuple.COMPARATOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllSinglePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetAllSinglePropertyValues()
		 {
			  int label = Token.nodeLabel( "Node" );
			  int prop = Token.propertyKey( "prop" );
			  IndexReference index = SchemaRead.index( label, prop );
			  using ( NodeValueIndexCursor node = Cursors.allocateNodeValueIndexCursor() )
			  {
					Read.nodeIndexScan( index, node, IndexOrder.NONE, true );

					IList<Value> values = new List<Value>();
					while ( node.Next() )
					{
						 values.Add( node.PropertyValue( 0 ) );
					}

					values.sort( Values.COMPARATOR );
					for ( int i = 0; i < _singlePropValues.Count; i++ )
					{
						 assertEquals( _singlePropValues[i], values[i] );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllDoublePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetAllDoublePropertyValues()
		 {
			  int label = Token.nodeLabel( "Node" );
			  int prop = Token.propertyKey( "prop" );
			  int prip = Token.propertyKey( "prip" );
			  IndexReference index = SchemaRead.index( label, prop, prip );
			  using ( NodeValueIndexCursor node = Cursors.allocateNodeValueIndexCursor() )
			  {
					Read.nodeIndexScan( index, node, IndexOrder.NONE, true );

					IList<ValueTuple> values = new List<ValueTuple>();
					while ( node.Next() )
					{
						 values.Add( ValueTuple.of( node.PropertyValue( 0 ), node.PropertyValue( 1 ) ) );
					}

					values.sort( ValueTuple.COMPARATOR );
					for ( int i = 0; i < _doublePropValues.Count; i++ )
					{
						 assertEquals( _doublePropValues[i], values[i] );
					}
			  }
		 }
	}

}