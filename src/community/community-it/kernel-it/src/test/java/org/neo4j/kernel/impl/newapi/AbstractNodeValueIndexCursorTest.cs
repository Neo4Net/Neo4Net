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
	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Neo4Net.Internal.Kernel.Api;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	internal abstract class AbstractNodeValueIndexCursorTest : NodeValueIndexCursorTestBase<ReadTestSupport>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void createCompositeIndex(org.neo4j.graphdb.GraphDatabaseService graphDb, String label, String... properties) throws Exception
		 protected internal override void CreateCompositeIndex( GraphDatabaseService graphDb, string label, params string[] properties )
		 {
			  GraphDatabaseAPI @internal = ( GraphDatabaseAPI ) graphDb;
			  KernelTransaction ktx = @internal.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
			  SchemaWrite schemaWrite = ktx.SchemaWrite();
			  TokenWrite token = ktx.TokenWrite();
			  schemaWrite.IndexCreate( SchemaDescriptorFactory.forLabel( token.LabelGetOrCreateForName( "Person" ), token.PropertyKeyGetOrCreateForName( "firstname" ), token.PropertyKeyGetOrCreateForName( "surname" ) ) );
		 }

		 protected internal override void AssertSameDerivedValue( PointValue p1, PointValue p2 )
		 {
			  ConfiguredSpaceFillingCurveSettingsCache settingsFactory = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );
			  SpaceFillingCurveSettings spaceFillingCurveSettings = settingsFactory.ForCRS( CoordinateReferenceSystem.WGS84 );
			  SpaceFillingCurve curve = spaceFillingCurveSettings.Curve();
			  assertEquals( curve.DerivedValueFor( p1.Coordinate() ), curve.DerivedValueFor(p2.Coordinate()) );
		 }
	}

}