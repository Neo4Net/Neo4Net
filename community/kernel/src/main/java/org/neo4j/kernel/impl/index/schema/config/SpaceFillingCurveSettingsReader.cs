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

	using Envelope = Org.Neo4j.Gis.Spatial.Index.Envelope;
	using Org.Neo4j.Index.@internal.gbptree;
	using Header = Org.Neo4j.Index.@internal.gbptree.Header;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.config.SpaceFillingCurveSettingsWriter.VERSION;

	/// <summary>
	/// <seealso cref="GBPTree"/> header reader for reading <seealso cref="SpaceFillingCurveSettings"/>.
	/// </summary>
	/// <seealso cref= SpaceFillingCurveSettingsWriter </seealso>
	public class SpaceFillingCurveSettingsReader : Header.Reader
	{
		 private readonly IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> _settings;

		 public SpaceFillingCurveSettingsReader( IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> settings )
		 {
			  this._settings = settings;
		 }

		 public override void Read( ByteBuffer headerBytes )
		 {
			  sbyte version = headerBytes.get();
			  if ( version != VERSION )
			  {
					throw new System.NotSupportedException( "Invalid crs settings header version " + version + ", was expecting " + VERSION );
			  }

			  int count = headerBytes.Int;
			  for ( int i = 0; i < count; i++ )
			  {
					ReadNext( headerBytes );
			  }
		 }

		 private void ReadNext( ByteBuffer headerBytes )
		 {
			  int tableId = headerBytes.get() & 0xFF;
			  int code = headerBytes.Int;
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( tableId, code );

			  int maxLevels = headerBytes.Short & 0xFFFF;
			  int dimensions = headerBytes.Short & 0xFFFF;
			  double[] min = new double[dimensions];
			  double[] max = new double[dimensions];
			  for ( int i = 0; i < dimensions; i++ )
			  {
					min[i] = Double.longBitsToDouble( headerBytes.Long );
					max[i] = Double.longBitsToDouble( headerBytes.Long );
			  }
			  Envelope extents = new Envelope( min, max );
			  _settings[crs] = new SpaceFillingCurveSettings( dimensions, extents, maxLevels );
		 }
	}

}