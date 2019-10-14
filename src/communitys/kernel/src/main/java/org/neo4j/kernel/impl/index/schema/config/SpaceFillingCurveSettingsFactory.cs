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

	using PartialOverlapConfiguration = Neo4Net.Gis.Spatial.Index.curves.PartialOverlapConfiguration;
	using SpaceFillingCurveConfiguration = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using Neo4Net.Index.@internal.gbptree;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;

	/// <summary>
	/// <para>
	/// This factory can be used to create new space filling curve settings for use in configuring the curves.
	/// These settings can be created either by defaults from the neo4j.conf file (see ConfiguredSpaceFullCurveSettingsCache)
	/// or from reading the header of an existing GBPTree based index.
	/// </para>
	/// </summary>
	public sealed class SpaceFillingCurveSettingsFactory
	{
		 private SpaceFillingCurveSettingsFactory()
		 {
		 }

		 /// <summary>
		 /// This method builds the default index configuration object for the specified CRS and other config options.
		 /// Currently we only support a SingleSpaceFillingCurveSettings which is the best option for cartesian, but
		 /// not necessarily the best for geographic coordinate systems.
		 /// </summary>
		 internal static SpaceFillingCurveSettings FromConfig( int maxBits, EnvelopeSettings envelopeSettings )
		 {
			  // Currently we support only one type of index, but in future we could support different types for different CRS
			  return new SpaceFillingCurveSettings.SettingsFromConfig( envelopeSettings.Crs.Dimension, maxBits, envelopeSettings.AsEnvelope() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SpaceFillingCurveSettings fromGBPTree(java.io.File indexFile, org.neo4j.io.pagecache.PageCache pageCache, System.Func<ByteBuffer,String> onError) throws java.io.IOException
		 public static SpaceFillingCurveSettings FromGBPTree( File indexFile, PageCache pageCache, System.Func<ByteBuffer, string> onError )
		 {
			  SpaceFillingCurveSettings.SettingsFromIndexHeader settings = new SpaceFillingCurveSettings.SettingsFromIndexHeader();
			  GBPTree.readHeader( pageCache, indexFile, settings.HeaderReader( onError ) );
			  if ( settings.Failed )
			  {
					throw new IOException( settings.FailureMessage );
			  }
			  return settings;
		 }

		 /// <summary>
		 /// Extracts settings from <seealso cref="Config"/> about how to optimize the 2D (or 3D) to 1D mapping of the space filling curve which will be
		 /// used when querying geometry ranges.
		 /// </summary>
		 /// <param name="config"> <seealso cref="Config"/> containing space filling curve settings. </param>
		 /// <returns> <seealso cref="SpaceFillingCurveConfiguration"/> from the settings found in <seealso cref="Config"/>. </returns>
		 public static SpaceFillingCurveConfiguration GetConfiguredSpaceFillingCurveConfiguration( Config config )
		 {
			  int extraLevels = config.Get( SpatialIndexSettings.SpaceFillingCurveExtraLevels );
			  double topThreshold = config.Get( SpatialIndexSettings.SpaceFillingCurveTopThreshold );
			  double bottomThreshold = config.Get( SpatialIndexSettings.SpaceFillingCurveBottomThreshold );

			  if ( topThreshold == 0.0 || bottomThreshold == 0.0 )
			  {
					return new StandardConfiguration( extraLevels );
			  }
			  else
			  {
					return new PartialOverlapConfiguration( extraLevels, topThreshold, bottomThreshold );
			  }
		 }
	}

}