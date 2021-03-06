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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using SpaceFillingCurve = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Org.Neo4j.@internal.Kernel.Api;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

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