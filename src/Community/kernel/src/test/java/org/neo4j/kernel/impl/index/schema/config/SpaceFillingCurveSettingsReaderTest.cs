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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{
	using Test = org.junit.jupiter.api.Test;


	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84_3D;

	internal class SpaceFillingCurveSettingsReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadMultipleSettings()
		 internal virtual void ShouldReadMultipleSettings()
		 {
			  // given
			  ConfiguredSpaceFillingCurveSettingsCache globalSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> expectedSpecificSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  RememberSettings( globalSettings, expectedSpecificSettings, WGS84, WGS84_3D, Cartesian );
			  IndexSpecificSpaceFillingCurveSettingsCache specificSettings = new IndexSpecificSpaceFillingCurveSettingsCache( globalSettings, expectedSpecificSettings );
			  SpaceFillingCurveSettingsWriter writer = new SpaceFillingCurveSettingsWriter( specificSettings );
			  sbyte[] bytes = new sbyte[Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE];
			  writer.Accept( new ByteArrayPageCursor( bytes ) );

			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> readExpectedSettings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  SpaceFillingCurveSettingsReader reader = new SpaceFillingCurveSettingsReader( readExpectedSettings );

			  // when
			  reader.Read( ByteBuffer.wrap( bytes ) );

			  // then
			  assertEquals( expectedSpecificSettings, readExpectedSettings );
		 }

		 private void RememberSettings( ConfiguredSpaceFillingCurveSettingsCache globalSettings, IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> expectedSpecificSettings, params CoordinateReferenceSystem[] crss )
		 {
			  foreach ( CoordinateReferenceSystem crs in crss )
			  {
					expectedSpecificSettings[crs] = globalSettings.ForCRS( crs );
			  }
		 }
	}

}