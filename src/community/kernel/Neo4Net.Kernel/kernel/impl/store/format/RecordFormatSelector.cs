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
namespace Neo4Net.Kernel.impl.store.format
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MetaDataRecordFormat = Neo4Net.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.concat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;

	/// <summary>
	/// Selects record format that will be used in a database.
	/// Supports selection based on the existing store and given configuration.
	/// <para>
	/// Automatic selection is used by various tools and tests that should pretend being format independent (for
	/// example backup).
	/// </para>
	/// </summary>
	public class RecordFormatSelector
	{
		 private static readonly RecordFormats _defaultFormat = Standard.LATEST_RECORD_FORMATS;

		 private static readonly IList<RecordFormats> _knownFormats = new IList<RecordFormats> { StandardV2_3.RECORD_FORMATS, StandardV3_0.RECORD_FORMATS, StandardV3_2.RECORD_FORMATS, StandardV3_4.RECORD_FORMATS };

		 private RecordFormatSelector()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 /// <summary>
		 /// Select <seealso cref="DEFAULT_FORMAT"/> record format.
		 /// </summary>
		 /// <returns> default record format. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static RecordFormats defaultFormat()
		 public static RecordFormats DefaultFormat()
		 {
			  return _defaultFormat;
		 }

		 /// <summary>
		 /// Select record formats for provided store version.
		 /// </summary>
		 /// <param name="storeVersion"> store version to find format for </param>
		 /// <returns> record formats </returns>
		 /// <exception cref="IllegalArgumentException"> if format for specified store version not found </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static RecordFormats selectForVersion(String storeVersion)
		 public static RecordFormats SelectForVersion( string storeVersion )
		 {
			  foreach ( RecordFormats format in AllFormats() )
			  {
					if ( format.StoreVersion().Equals(storeVersion) )
					{
						 return format;
					}
			  }
			  throw new System.ArgumentException( "Unknown store version '" + storeVersion + "'" );
		 }

		 /// <summary>
		 /// Select configured record format based on available services in class path.
		 /// Specific format can be specified by <seealso cref="GraphDatabaseSettings.record_format"/> property.
		 /// <para>
		 /// If format is not specified <seealso cref="DEFAULT_FORMAT"/> will be used.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="config"> configuration parameters </param>
		 /// <param name="logProvider"> logging provider </param>
		 /// <returns> selected record format </returns>
		 /// <exception cref="IllegalArgumentException"> if requested format not found </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static RecordFormats selectForConfig(org.neo4j.kernel.configuration.Config config, org.neo4j.logging.LogProvider logProvider)
		 public static RecordFormats SelectForConfig( Config config, LogProvider logProvider )
		 {
			  string recordFormat = ConfiguredRecordFormat( config );
			  if ( StringUtils.isEmpty( recordFormat ) )
			  {
					Info( logProvider, "Record format not configured, selected default: " + DefaultFormat() );
					return DefaultFormat();
			  }
			  RecordFormats format = SelectSpecificFormat( recordFormat );
			  Info( logProvider, "Selected record format based on config: " + format );
			  return format;
		 }

		 /// <summary>
		 /// Select record format for the given store directory.
		 /// <para>
		 /// <b>Note:</b> package private only for testing.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="databaseLayout"> directory with the store </param>
		 /// <param name="fs"> file system used to access store files </param>
		 /// <param name="pageCache"> page cache to read store files </param>
		 /// <returns> record format of the given store or <code>null</code> if <seealso cref="DatabaseLayout.metadataStore()"/> file not
		 /// found or can't be read </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable static RecordFormats selectForStore(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.LogProvider logProvider)
		 internal static RecordFormats SelectForStore( DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, LogProvider logProvider )
		 {
			  File neoStoreFile = databaseLayout.MetadataStore();
			  if ( fs.FileExists( neoStoreFile ) )
			  {
					try
					{
						 long value = MetaDataStore.getRecord( pageCache, neoStoreFile, STORE_VERSION );
						 if ( value != MetaDataRecordFormat.FIELD_NOT_PRESENT )
						 {
							  string storeVersion = MetaDataStore.versionLongToString( value );

							  foreach ( RecordFormats format in AllFormats() )
							  {
									if ( format.StoreVersion().Equals(storeVersion) )
									{
										 Info( logProvider, "Selected " + format + " record format from store " + databaseLayout.DatabaseDirectory() );
										 return format;
									}
							  }
						 }
					}
					catch ( IOException e )
					{
						 Info( logProvider, "Unable to read store format: " + e.Message );
					}
			  }
			  return null;
		 }

		 /// <summary>
		 /// Select record format for the given store (if exists) or from the given configuration. If there is no store and
		 /// record format is not configured than <seealso cref="DEFAULT_FORMAT"/> is selected.
		 /// </summary>
		 /// <param name="config"> configuration parameters </param>
		 /// <param name="databaseLayout"> database directory structure </param>
		 /// <param name="fs"> file system used to access store files </param>
		 /// <param name="pageCache"> page cache to read store files </param>
		 /// <returns> record format from the store (if it can be read) or configured record format or <seealso cref="DEFAULT_FORMAT"/> </returns>
		 /// <exception cref="IllegalArgumentException"> when configured format is different from the format present in the store </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static RecordFormats selectForStoreOrConfig(org.neo4j.kernel.configuration.Config config, org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.LogProvider logProvider)
		 public static RecordFormats SelectForStoreOrConfig( Config config, DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, LogProvider logProvider )
		 {
			  RecordFormats configuredFormat = LoadRecordFormat( ConfiguredRecordFormat( config ) );
			  bool formatConfigured = configuredFormat != null;

			  RecordFormats currentFormat = SelectForStore( databaseLayout, fs, pageCache, logProvider );
			  bool storeWithFormatExists = currentFormat != null;

			  if ( formatConfigured && storeWithFormatExists )
			  {
					if ( currentFormat.FormatFamily.Equals( configuredFormat.FormatFamily ) && ( currentFormat.Generation() == configuredFormat.Generation() ) )
					{
						 Info( logProvider, "Configured format matches format in the store. Selected: " + currentFormat );
						 return currentFormat;
					}
					throw new System.ArgumentException( string.Format( "Configured format '{0}' is different from the actual format in the store '{1}'", configuredFormat, currentFormat ) );
			  }

			  if ( !formatConfigured && storeWithFormatExists )
			  {
					Info( logProvider, "Format not configured. Selected format from the store: " + currentFormat );
					return currentFormat;
			  }

			  if ( formatConfigured )
			  {
					Info( logProvider, "Selected configured format: " + configuredFormat );
					return configuredFormat;
			  }

			  return _defaultFormat;
		 }

		 /// <summary>
		 /// Check if store and configured formats are compatible. In case if format is not configured or store does not
		 /// exist yet - we consider formats as compatible. </summary>
		 /// <param name="config"> configuration parameters </param>
		 /// <param name="databaseLayout"> database directory structure </param>
		 /// <param name="fs"> file system used to access store files </param>
		 /// <param name="pageCache"> page cache to read store files </param>
		 /// <param name="logProvider"> log provider </param>
		 /// <returns> true if configured and actual format is compatible, false otherwise. </returns>
		 public static bool IsStoreAndConfigFormatsCompatible( Config config, DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, LogProvider logProvider )
		 {
			  RecordFormats configuredFormat = LoadRecordFormat( ConfiguredRecordFormat( config ) );

			  RecordFormats currentFormat = SelectForStore( databaseLayout, fs, pageCache, logProvider );

			  return ( configuredFormat == null ) || ( currentFormat == null ) || ( currentFormat.FormatFamily.Equals( configuredFormat.FormatFamily ) && ( currentFormat.Generation() == configuredFormat.Generation() ) );
		 }

		 /// <summary>
		 /// Select explicitly configured record format (via given {@code config}) or format from the store. If store does
		 /// not exist or has old format (<seealso cref="RecordFormats.generation()"/>) than this method returns
		 /// <seealso cref="DEFAULT_FORMAT"/>.
		 /// </summary>
		 /// <param name="config"> configuration parameters </param>
		 /// <param name="databaseLayout"> database directory structure </param>
		 /// <param name="fs"> file system used to access store files </param>
		 /// <param name="pageCache"> page cache to read store files </param>
		 /// <returns> record format from the store (if it can be read) or configured record format or <seealso cref="DEFAULT_FORMAT"/> </returns>
		 /// <seealso cref= RecordFormats#generation() </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static RecordFormats selectNewestFormat(org.neo4j.kernel.configuration.Config config, org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.LogProvider logProvider)
		 public static RecordFormats SelectNewestFormat( Config config, DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, LogProvider logProvider )
		 {
			  bool formatConfigured = StringUtils.isNotEmpty( ConfiguredRecordFormat( config ) );
			  if ( formatConfigured )
			  {
					// format was explicitly configured so select it
					return SelectForConfig( config, logProvider );
			  }
			  else
			  {
					RecordFormats result = SelectForStore( databaseLayout, fs, pageCache, logProvider );
					if ( result == null )
					{
						 // format was not explicitly configured and store does not exist, select default format
						 Info( logProvider, "Selected format '" + _defaultFormat + "' for the new store" );
						 result = _defaultFormat;
					}
					else if ( FormatFamily.IsHigherFamilyFormat( _defaultFormat, result ) || ( FormatFamily.IsSameFamily( result, _defaultFormat ) && ( result.Generation() < _defaultFormat.generation() ) ) )
					{
						 // format was not explicitly configured and store has lower format
						 // select default format, upgrade is intended
						 Info( logProvider, "Selected format '" + _defaultFormat + "' for existing store with format '" + result + "'" );
						 result = _defaultFormat;
					}
					return result;
			  }
		 }

		 /// <summary>
		 /// Finds which format, if any, succeeded the specified format. Only formats in the same family are considered.
		 /// </summary>
		 /// <param name="format"> to find successor to. </param>
		 /// <returns> the format with the lowest generation > format.generation, or None if no such format is known. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static java.util.Optional<RecordFormats> findSuccessor(@Nonnull final RecordFormats format)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Optional<RecordFormats> FindSuccessor( RecordFormats format )
		 {
			  return StreamSupport.stream( RecordFormatSelector.AllFormats().spliterator(), false ).filter(candidate => FormatFamily.IsSameFamily(format, candidate)).filter(candidate => candidate.generation() > format.Generation()).reduce((a, b) => a.generation() < b.generation() ? a : b);
		 }

		 /// <summary>
		 /// Gets all <seealso cref="RecordFormats"/> that the selector is aware of. </summary>
		 /// <returns> An iterable over all known record formats. </returns>
		 public static IEnumerable<RecordFormats> AllFormats()
		 {
			  IEnumerable<RecordFormats_Factory> loadableFormatFactories = Service.load( typeof( RecordFormats_Factory ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IEnumerable<RecordFormats> loadableFormats = map( RecordFormats_Factory::newInstance, loadableFormatFactories );
			  return concat( _knownFormats, loadableFormats );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private static RecordFormats selectSpecificFormat(String recordFormat)
		 private static RecordFormats SelectSpecificFormat( string recordFormat )
		 {
			  RecordFormats formats = LoadRecordFormat( recordFormat );
			  if ( formats == null )
			  {
					throw new System.ArgumentException( "No record format found with the name '" + recordFormat + "'." );
			  }
			  return formats;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private static RecordFormats loadRecordFormat(String recordFormat)
		 private static RecordFormats LoadRecordFormat( string recordFormat )
		 {
			  if ( StringUtils.isNotEmpty( recordFormat ) )
			  {
					if ( Standard.LATEST_NAME.Equals( recordFormat ) )
					{
						 return Standard.LATEST_RECORD_FORMATS;
					}
					foreach ( RecordFormats knownFormat in _knownFormats )
					{
						 if ( recordFormat.Equals( knownFormat.Name() ) )
						 {
							  return knownFormat;
						 }
					}
					RecordFormats_Factory formatFactory = Service.loadSilently( typeof( RecordFormats_Factory ), recordFormat );
					if ( formatFactory != null )
					{
						 return formatFactory.NewInstance();
					}
			  }
			  return null;
		 }

		 private static void Info( LogProvider logProvider, string message )
		 {
			  logProvider.GetLog( typeof( RecordFormatSelector ) ).info( message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private static String configuredRecordFormat(org.neo4j.kernel.configuration.Config config)
		 private static string ConfiguredRecordFormat( Config config )
		 {
			  return config.Get( GraphDatabaseSettings.record_format );
		 }
	}

}