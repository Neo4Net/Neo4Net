using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;


	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using LayoutBootstrapper = Neo4Net.Index.Internal.gbptree.LayoutBootstrapper;
	using Meta = Neo4Net.Index.Internal.gbptree.Meta;
	using MetadataMismatchException = Neo4Net.Index.Internal.gbptree.MetadataMismatchException;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LabelScanLayout = Neo4Net.Kernel.impl.index.labelscan.LabelScanLayout;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using SpaceFillingCurveSettingsFactory = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using SpaceFillingCurveSettingsReader = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsReader;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	public class SchemaLayouts : LayoutBootstrapper
	{
		 private readonly IList<LayoutBootstrapper> _allSchemaLayout;

		 public SchemaLayouts()
		 {
			  _allSchemaLayout = new List<LayoutBootstrapper>();
			  ( ( IList<LayoutBootstrapper> )_allSchemaLayout ).AddRange( Arrays.asList( SpatialLayoutFactory( CoordinateReferenceSystem.WGS84 ), SpatialLayoutFactory( CoordinateReferenceSystem.WGS84_3D ), SpatialLayoutFactory( CoordinateReferenceSystem.Cartesian ), SpatialLayoutFactory( CoordinateReferenceSystem.Cartesian_3D ), ( indexFile, pageCache, meta, targetLayout ) => new LocalTimeLayout(), (indexFile, pageCache, meta, targetLayout) => new ZonedDateTimeLayout(), (indexFile, pageCache, meta, targetLayout) => new DurationLayout(), (indexFile, pageCache, meta, targetLayout) => new ZonedTimeLayout(), (indexFile, pageCache, meta, targetLayout) => new DateLayout(), (indexFile, pageCache, meta, targetLayout) => new LocalDateTimeLayout(), (indexFile, pageCache, meta, targetLayout) => new StringLayout(), (indexFile, pageCache, meta, targetLayout) => new NumberLayoutUnique(), (indexFile, pageCache, meta, targetLayout) => new NumberLayoutNonUnique(), GenericLayout(), (indexFile, pageCache, meta, targetLayout) => new LabelScanLayout() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.index.internal.gbptree.Layout<?,?> create(java.io.File indexFile, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.index.internal.gbptree.Meta meta, String targetLayout) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public override Layout<object, ?> Create( File indexFile, PageCache pageCache, Meta meta, string targetLayout )
		 {
			  foreach ( LayoutBootstrapper factory in _allSchemaLayout )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.index.internal.gbptree.Layout<?,?> layout = factory.create(indexFile, pageCache, meta, targetLayout);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					Layout<object, ?> layout = factory.Create( indexFile, pageCache, meta, targetLayout );
					if ( layout != null && MatchingLayout( meta, layout ) )
					{
						 // Verify spatial and generic
						 return layout;
					}
			  }
			  throw new Exception( "Layout with identifier \"" + targetLayout + "\" did not match meta " + meta );
		 }

		 private static bool MatchingLayout( Meta meta, Layout layout )
		 {
			  try
			  {
					meta.Verify( layout );
					return true;
			  }
			  catch ( MetadataMismatchException )
			  {
					return false;
			  }
		 }

		 private static LayoutBootstrapper GenericLayout()
		 {
			  return ( indexFile, pageCache, meta, targetLayout ) =>
			  {
				if ( targetLayout.contains( "generic" ) )
				{
					 string numberOfSlotsString = targetLayout.replace( "generic", "" );
					 int numberOfSlots = int.Parse( numberOfSlotsString );
					 IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> settings = new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();
					 GBPTree.readHeader( pageCache, indexFile, new NativeIndexHeaderReader( new SpaceFillingCurveSettingsReader( settings ) ) );
					 ConfiguredSpaceFillingCurveSettingsCache configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );
					 return new GenericLayout( numberOfSlots, new IndexSpecificSpaceFillingCurveSettingsCache( configuredSettings, settings ) );
				}
				return null;
			  };
		 }

		 private static LayoutBootstrapper SpatialLayoutFactory( CoordinateReferenceSystem crs )
		 {
			  return ( indexFile, pageCache, meta, targetLayout ) =>
			  {
				if ( targetLayout.Equals( crs.Name ) )
				{
					 MutableBoolean failure = new MutableBoolean( false );
					 Function<ByteBuffer, string> onError = byteBuffer =>
					 {
						  failure.setTrue();
						  return "";
					 };
					 SpaceFillingCurveSettings curveSettings = SpaceFillingCurveSettingsFactory.fromGBPTree( indexFile, pageCache, onError );
					 if ( !failure.Value )
					 {
						  return new SpatialLayout( crs, curveSettings.curve() );
					 }
				}
				return null;
			  };
		 }
	}

}