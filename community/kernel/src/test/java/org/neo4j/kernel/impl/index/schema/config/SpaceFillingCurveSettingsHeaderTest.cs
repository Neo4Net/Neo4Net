﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.config
{
	using Test = org.junit.jupiter.api.Test;


	using ByteArrayPageCursor = Org.Neo4j.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;

	internal class SpaceFillingCurveSettingsHeaderTest
	{
		private bool InstanceFieldsInitialized = false;

		public SpaceFillingCurveSettingsHeaderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_pageCursor = ByteArrayPageCursor.wrap( _data );
		}

		 private readonly sbyte[] _data = new sbyte[PAGE_SIZE];
		 private PageCursor _pageCursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadZeroSettings()
		 internal virtual void ShouldWriteAndReadZeroSettings()
		 {
			  ShouldWriteAndReadSettings();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadSingleSetting()
		 internal virtual void ShouldWriteAndReadSingleSetting()
		 {
			  ShouldWriteAndReadSettings( CoordinateReferenceSystem.WGS84 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadMultipleSettings()
		 internal virtual void ShouldWriteAndReadMultipleSettings()
		 {
			  ShouldWriteAndReadSettings( CoordinateReferenceSystem.WGS84, CoordinateReferenceSystem.Cartesian, CoordinateReferenceSystem.Cartesian_3D );
		 }

		 private void ShouldWriteAndReadSettings( params CoordinateReferenceSystem[] crss )
		 {
			  // given
			  IndexSpecificSpaceFillingCurveSettingsCache indexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
			  foreach ( CoordinateReferenceSystem crs in crss )
			  {
					indexSettings.ForCrs( crs, true );
			  }
			  SpaceFillingCurveSettingsWriter writer = new SpaceFillingCurveSettingsWriter( indexSettings );

			  // when
			  writer.Accept( _pageCursor );
			  _pageCursor.rewind();

			  // then
			  IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> read = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
			  SpaceFillingCurveSettingsReader reader = new SpaceFillingCurveSettingsReader( read );
			  reader.Read( ByteBuffer.wrap( _data ) );
			  assertEquals( AsMap( indexSettings ), read );
		 }

		 private IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> AsMap( IndexSpecificSpaceFillingCurveSettingsCache indexSettings )
		 {
			  ToMapSettingVisitor visitor = new ToMapSettingVisitor();
			  indexSettings.VisitIndexSpecificSettings( visitor );
			  return visitor.Map;
		 }
	}

}