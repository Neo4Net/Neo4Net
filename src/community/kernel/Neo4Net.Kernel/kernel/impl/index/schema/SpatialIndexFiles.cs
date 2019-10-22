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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using SpaceFillingCurveSettingsFactory = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	internal class SpatialIndexFiles
	{
		 private static readonly Pattern _crsDirPattern = Pattern.compile( "(\\d+)-(\\d+)" );
		 private readonly FileSystemAbstraction _fs;
		 private readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings;
		 private readonly File _indexDirectory;

		 internal SpatialIndexFiles( IndexDirectoryStructure directoryStructure, long indexId, FileSystemAbstraction fs, ConfiguredSpaceFillingCurveSettingsCache settingsCache )
		 {
			  this._fs = fs;
			  this._configuredSettings = settingsCache;
			  _indexDirectory = directoryStructure.DirectoryForIndex( indexId );
		 }

		 internal virtual IEnumerable<SpatialFile> Existing()
		 {
			  IList<SpatialFile> existing = new List<SpatialFile>();
			  AddExistingFiles( existing );
			  return existing;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T> void loadExistingIndexes(SpatialIndexCache<T> indexCache) throws java.io.IOException
		 internal virtual void LoadExistingIndexes<T>( SpatialIndexCache<T> indexCache )
		 {
			  foreach ( SpatialFile fileLayout in Existing() )
			  {
					indexCache.Select( fileLayout.Crs );
			  }
		 }

		 internal virtual SpatialFile ForCrs( CoordinateReferenceSystem crs )
		 {
			  return new SpatialFile( crs, _configuredSettings, _indexDirectory );
		 }

		 private void AddExistingFiles( IList<SpatialFile> existing )
		 {
			  File[] files = _fs.listFiles( _indexDirectory );
			  if ( files != null )
			  {
					foreach ( File file in files )
					{
						 string name = file.Name;
						 Matcher matcher = _crsDirPattern.matcher( name );
						 if ( matcher.matches() )
						 {
							  int tableId = int.Parse( matcher.group( 1 ) );
							  int code = int.Parse( matcher.group( 2 ) );
							  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( tableId, code );
							  existing.Add( ForCrs( crs ) );
						 }
					}
			  }
		 }

		 internal class SpatialFile
		 {
			  internal readonly File IndexFile;
			  internal readonly ConfiguredSpaceFillingCurveSettingsCache ConfiguredSettings;
			  internal readonly CoordinateReferenceSystem Crs;

			  internal SpatialFile( CoordinateReferenceSystem crs, ConfiguredSpaceFillingCurveSettingsCache configuredSettings, File indexDirectory )
			  {
					this.Crs = crs;
					this.ConfiguredSettings = configuredSettings;
					string s = crs.Table.TableId + "-" + Convert.ToString( crs.Code );
					this.IndexFile = new File( indexDirectory, s );
			  }

			  /// <summary>
			  /// If this is the first time an index is being created, get the layout settings from the config settings only
			  /// </summary>
			  internal virtual SpatialFileLayout LayoutForNewIndex
			  {
				  get
				  {
						return new SpatialFileLayout( this, ConfiguredSettings.forCRS( Crs ) );
				  }
			  }

			  /// <summary>
			  /// If we are loading a layout for an existing index, read the settings from the index header, and ignore config settings
			  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SpatialFileLayout getLayoutForExistingIndex(org.Neo4Net.io.pagecache.PageCache pageCache) throws java.io.IOException
			  internal virtual SpatialFileLayout GetLayoutForExistingIndex( PageCache pageCache )
			  {
					SpaceFillingCurveSettings settings = SpaceFillingCurveSettingsFactory.fromGBPTree( IndexFile, pageCache, NativeIndexHeaderReader.readFailureMessage );
					return new SpatialFileLayout( this, settings );
			  }
		 }

		 internal class SpatialFileLayout
		 {
			  internal readonly SpaceFillingCurveSettings Settings;
			  internal readonly SpatialFile SpatialFile;
			  internal readonly IndexLayout<SpatialIndexKey, NativeIndexValue> Layout;

			  internal SpatialFileLayout( SpatialFile spatialFile, SpaceFillingCurveSettings settings )
			  {
					this.SpatialFile = spatialFile;
					this.Settings = settings;
					this.Layout = new SpatialLayout( spatialFile.Crs, settings.Curve() );
			  }

			  public virtual File IndexFile
			  {
				  get
				  {
						return SpatialFile.indexFile;
				  }
			  }
		 }
	}

}