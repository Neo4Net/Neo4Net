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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{
	using Test = org.junit.jupiter.api.Test;


	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;

	internal class IndexSpecificSpaceFillingCurveSettingsCacheTest
	{
		 private readonly ConfiguredSpaceFillingCurveSettingsCache _globalSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveInitialIndexSpecificSetting()
		 internal virtual void ShouldHaveInitialIndexSpecificSetting()
		 {
			  // given
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> initialSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  initialSettings[WGS84] = _globalSettings.forCRS( WGS84 );
			  initialSettings[Cartesian] = _globalSettings.forCRS( Cartesian );
			  IndexSpecificSpaceFillingCurveSettingsCache indexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( _globalSettings, initialSettings );

			  // when
			  ToMapSettingVisitor visitor = new ToMapSettingVisitor();
			  indexSettings.VisitIndexSpecificSettings( visitor );

			  // then
			  assertEquals( initialSettings, visitor.Map );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveInitialIndexSpecificSettingsPlusRequestedOnes()
		 internal virtual void ShouldHaveInitialIndexSpecificSettingsPlusRequestedOnes()
		 {
			  // given
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> initialSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  initialSettings[WGS84] = _globalSettings.forCRS( WGS84 );
			  initialSettings[Cartesian] = _globalSettings.forCRS( Cartesian );
			  IndexSpecificSpaceFillingCurveSettingsCache indexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( _globalSettings, initialSettings );

			  // when
			  indexSettings.ForCrs( Cartesian_3D, true );

			  // then
			  ToMapSettingVisitor visitor = new ToMapSettingVisitor();
			  indexSettings.VisitIndexSpecificSettings( visitor );
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> expectedSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>( initialSettings );
			  assertNull( expectedSettings.put( Cartesian_3D, _globalSettings.forCRS( Cartesian_3D ) ) );
			  assertEquals( expectedSettings, visitor.Map );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCreateIndexSpecificSettingForReadRequest()
		 internal virtual void ShouldNotCreateIndexSpecificSettingForReadRequest()
		 {
			  // given
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> initialSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  initialSettings[WGS84] = _globalSettings.forCRS( WGS84 );
			  initialSettings[Cartesian] = _globalSettings.forCRS( Cartesian );
			  IndexSpecificSpaceFillingCurveSettingsCache indexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( _globalSettings, initialSettings );

			  // when
			  indexSettings.ForCrs( Cartesian_3D, false );

			  // then
			  ToMapSettingVisitor visitor = new ToMapSettingVisitor();
			  indexSettings.VisitIndexSpecificSettings( visitor );
			  assertEquals( initialSettings, visitor.Map );
		 }
	}

}