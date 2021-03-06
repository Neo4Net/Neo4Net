﻿/*
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
namespace Schema
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Helpers.Collection;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SpatialIndexValueTestUtil = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpatialIndexValueTestUtil;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestLabels = Org.Neo4j.Test.TestLabels;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class UniqueSpatialIndexIT
	internal class UniqueSpatialIndexIT
	{
		 private const string KEY = "prop";
		 private const TestLabels LABEL = TestLabels.LABEL_ONE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;
		 private GraphDatabaseService _db;
		 private PointValue _point1;
		 private PointValue _point2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  Pair<PointValue, PointValue> collidingPoints = SpatialIndexValueTestUtil.pointsWithSameValueOnSpaceFillingCurve( Config.defaults() );
			  _point1 = collidingPoints.First();
			  _point2 = collidingPoints.Other();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("providerSettings") void shouldPopulateIndexWithUniquePointsThatCollideOnSpaceFillingCurve(org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex)
		 internal virtual void ShouldPopulateIndexWithUniquePointsThatCollideOnSpaceFillingCurve( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  // given
			  SetupDb( schemaIndex );
			  Pair<long, long> nodeIds = CreateUniqueNodes();

			  // when
			  CreateUniquenessConstraint();

			  // then
			  AssertBothNodesArePresent( nodeIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("providerSettings") void shouldAddPointsThatCollideOnSpaceFillingCurveToUniqueIndexInSameTx(org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex)
		 internal virtual void ShouldAddPointsThatCollideOnSpaceFillingCurveToUniqueIndexInSameTx( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  // given
			  SetupDb( schemaIndex );
			  CreateUniquenessConstraint();

			  // when
			  Pair<long, long> nodeIds = CreateUniqueNodes();

			  // then
			  AssertBothNodesArePresent( nodeIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("providerSettings") void shouldThrowWhenPopulatingWithNonUniquePoints(org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex)
		 internal virtual void ShouldThrowWhenPopulatingWithNonUniquePoints( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  // given
			  SetupDb( schemaIndex );
			  CreateNonUniqueNodes();

			  // then
			  assertThrows( typeof( ConstraintViolationException ), this.createUniquenessConstraint );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("providerSettings") void shouldThrowWhenAddingNonUniquePoints(org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex schemaIndex)
		 internal virtual void ShouldThrowWhenAddingNonUniquePoints( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  // given
			  SetupDb( schemaIndex );
			  CreateUniquenessConstraint();

			  // when
			  assertThrows( typeof( ConstraintViolationException ), this.createNonUniqueNodes );
		 }

		 private static Stream<GraphDatabaseSettings.SchemaIndex> ProviderSettings()
		 {
			  return Arrays.stream( GraphDatabaseSettings.SchemaIndex.values() );
		 }

		 private void CreateNonUniqueNodes()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node originNode = _db.createNode( LABEL );
					originNode.SetProperty( KEY, _point1 );
					Node centerNode = _db.createNode( LABEL );
					centerNode.SetProperty( KEY, _point1 );
					tx.Success();
			  }
		 }

		 private Pair<long, long> CreateUniqueNodes()
		 {
			  Pair<long, long> nodeIds;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node originNode = _db.createNode( LABEL );
					originNode.SetProperty( KEY, _point1 );
					Node centerNode = _db.createNode( LABEL );
					centerNode.SetProperty( KEY, _point2 );

					nodeIds = Pair.of( originNode.Id, centerNode.Id );
					tx.Success();
			  }
			  return nodeIds;
		 }

		 private void AssertBothNodesArePresent( Pair<long, long> nodeIds )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					ResourceIterator<Node> origin = _db.findNodes( LABEL, KEY, _point1 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( origin.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( nodeIds.First(), origin.next().Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( origin.hasNext() );

					ResourceIterator<Node> center = _db.findNodes( LABEL, KEY, _point2 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( center.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( nodeIds.Other(), center.next().Id );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( center.hasNext() );

					tx.Success();
			  }
		 }

		 private void CreateUniquenessConstraint()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().constraintFor(TestLabels.LABEL_ONE).assertPropertyIsUnique(KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private void SetupDb( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  TestGraphDatabaseFactory dbFactory = new TestGraphDatabaseFactory();
			  GraphDatabaseBuilder builder = dbFactory.NewEmbeddedDatabaseBuilder( _directory.storeDir() ).setConfig(GraphDatabaseSettings.default_schema_provider, schemaIndex.providerName());
			  _db = builder.NewGraphDatabase();
		 }
	}

}