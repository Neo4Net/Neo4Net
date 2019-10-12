using System;

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
namespace Org.Neo4j.Commandline.dbms
{

	using AdminCommand = Org.Neo4j.Commandline.admin.AdminCommand;
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;
	using OptionalNamedArg = Org.Neo4j.Commandline.arguments.OptionalNamedArg;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using OsBeanUtil = Org.Neo4j.Io.os.OsBeanUtil;
	using FailureStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.FailureStorage;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using NativeIndexFileFilter = Org.Neo4j.Kernel.@internal.NativeIndexFileFilter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.configuration.ExternalSettings.initialHeapSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.configuration.ExternalSettings.maxHeapSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.active_database;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.ONE_GIBI_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.ONE_KIBI_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.ONE_MEBI_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.gibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.tebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.baseSchemaIndexFolder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;

	public class MemoryRecommendationsCommand : AdminCommand
	{
		 // Fields: {System Memory in GiBs; OS memory reserve in GiBs; JVM Heap memory in GiBs}.
		 // And the page cache gets what's left, though always at least 100 MiB.
		 // Heap never goes beyond 31 GiBs.
		 private static readonly Bracket[] _datapoints = new Bracket[]
		 {
			 new Bracket( 0.01, 0.007, 0.002 ),
			 new Bracket( 1.0, 0.65, 0.3 ),
			 new Bracket( 2.0, 1, 0.5 ),
			 new Bracket( 4.0, 1.5, 2 ),
			 new Bracket( 6.0, 2, 3 ),
			 new Bracket( 8.0, 2.5, 3.5 ),
			 new Bracket( 10.0, 3, 4 ),
			 new Bracket( 12.0, 3.5, 4.5 ),
			 new Bracket( 16.0, 4, 5 ),
			 new Bracket( 24.0, 6, 8 ),
			 new Bracket( 32.0, 8, 12 ),
			 new Bracket( 64.0, 12, 24 ),
			 new Bracket( 128.0, 16, 31 ),
			 new Bracket( 256.0, 20, 31 ),
			 new Bracket( 512.0, 24, 31 ),
			 new Bracket( 1024.0, 30, 31 )
		 };
		 private const string ARG_MEMORY = "memory";
		 private readonly Path _homeDir;
		 private readonly OutsideWorld _outsideWorld;
		 private readonly Path _configDir;

		 internal static long RecommendOsMemory( long totalMemoryBytes )
		 {
			  Brackets brackets = FindMemoryBrackets( totalMemoryBytes );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return brackets.Recommend( Bracket::osMemory );
		 }

		 internal static long RecommendHeapMemory( long totalMemoryBytes )
		 {
			  Brackets brackets = FindMemoryBrackets( totalMemoryBytes );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return brackets.Recommend( Bracket::heapMemory );
		 }

		 internal static long RecommendPageCacheMemory( long totalMemoryBytes )
		 {
			  long osMemory = RecommendOsMemory( totalMemoryBytes );
			  long heapMemory = RecommendHeapMemory( totalMemoryBytes );
			  long recommendation = totalMemoryBytes - osMemory - heapMemory;
			  recommendation = Math.Max( mebiBytes( 8 ), recommendation );
			  recommendation = Math.Min( tebiBytes( 16 ), recommendation );
			  return recommendation;
		 }

		 private static Brackets FindMemoryBrackets( long totalMemoryBytes )
		 {
			  double totalMemoryGB = ( ( double ) totalMemoryBytes ) / ( ( double ) gibiBytes( 1 ) );
			  Bracket lower = null;
			  Bracket upper = null;
			  for ( int i = 1; i < _datapoints.Length; i++ )
			  {
					if ( totalMemoryGB < _datapoints[i].totalMemory )
					{
						 lower = _datapoints[i - 1];
						 upper = _datapoints[i];
						 break;
					}
			  }
			  if ( lower == null )
			  {
					lower = _datapoints[_datapoints.Length - 1];
					upper = _datapoints[_datapoints.Length - 1];
			  }
			  return new Brackets( totalMemoryGB, lower, upper );
		 }

		 public static Arguments BuildArgs()
		 {
			  string memory = BytesToString( OsBeanUtil.TotalPhysicalMemory );
			  return ( new Arguments() ).withArgument(new OptionalNamedArg(ARG_MEMORY, memory, memory, "Recommend memory settings with respect to the given amount of memory, " + "instead of the total memory of the system running the command.")).withDatabase("Name of specific database to calculate page cache memory requirement for. " + "The generic calculation is still a good generic recommendation for this machine, " + "but there will be an additional calculation for minimal required page cache memory " + "for mapping all store and index files that are managed by the page cache.");
		 }

		 internal static string BytesToString( double bytes )
		 {
			  double gibi1 = ONE_GIBI_BYTE;
			  double mebi1 = ONE_MEBI_BYTE;
			  double mebi100 = 100 * mebi1;
			  double kibi1 = ONE_KIBI_BYTE;
			  double kibi100 = 100 * kibi1;
			  if ( bytes >= gibi1 )
			  {
					double gibibytes = bytes / gibi1;
					double modMebi = bytes % gibi1;
					if ( modMebi >= mebi100 )
					{
						 return format( Locale.ROOT, "%dm", ( long )Math.Round( bytes / mebi100, MidpointRounding.AwayFromZero ) * 100 );
					}
					else
					{
						 return format( Locale.ROOT, "%.0fg", gibibytes );
					}
			  }
			  else if ( bytes >= mebi1 )
			  {
					double mebibytes = bytes / mebi1;
					double modKibi = bytes % mebi1;
					if ( modKibi >= kibi100 )
					{
						 return format( Locale.ROOT, "%dk", ( long )Math.Round( bytes / kibi100, MidpointRounding.AwayFromZero ) * 100 );
					}
					else
					{
						 return format( Locale.ROOT, "%.0fm", mebibytes );
					}
			  }
			  else
			  {
					// For kilobytes there's no need to bother with decimals, just print a rough figure rounded upwards
					double kibiBytes = bytes / kibi1;
					return format( Locale.ROOT, "%dk", ( long ) Math.Ceiling( kibiBytes ) );
			  }
		 }

		 internal MemoryRecommendationsCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this._homeDir = homeDir;
			  this._outsideWorld = outsideWorld;
			  this._configDir = configDir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  Arguments arguments = BuildArgs().parse(args);

			  string mem = arguments.Get( ARG_MEMORY );
			  long memory = buildSetting( ARG_MEMORY, BYTES ).build().apply(arguments.get);
			  string os = BytesToString( RecommendOsMemory( memory ) );
			  string heap = BytesToString( RecommendHeapMemory( memory ) );
			  string pagecache = BytesToString( RecommendPageCacheMemory( memory ) );
			  bool specificDb = arguments.Has( ARG_DATABASE );

			  Print( "# Memory settings recommendation from neo4j-admin memrec:" );
			  Print( "#" );
			  Print( "# Assuming the system is dedicated to running Neo4j and has " + mem + " of memory," );
			  Print( "# we recommend a heap size of around " + heap + ", and a page cache of around " + pagecache + "," );
			  Print( "# and that about " + os + " is left for the operating system, and the native memory" );
			  Print( "# needed by Lucene and Netty." );
			  Print( "#" );
			  Print( "# Tip: If the indexing storage use is high, e.g. there are many indexes or most" );
			  Print( "# data indexed, then it might advantageous to leave more memory for the" );
			  Print( "# operating system." );
			  Print( "#" );
			  Print( "# Tip: The more concurrent transactions your workload has and the more updates" );
			  Print( "# they do, the more heap memory you will need. However, don't allocate more" );
			  Print( "# than 31g of heap, since this will disable pointer compression, also known as" );
			  Print( "# \"compressed oops\", in the JVM and make less effective use of the heap." );
			  Print( "#" );
			  Print( "# Tip: Setting the initial and the max heap size to the same value means the" );
			  Print( "# JVM will never need to change the heap size. Changing the heap size otherwise" );
			  Print( "# involves a full GC, which is desirable to avoid." );
			  Print( "#" );
			  Print( "# Based on the above, the following memory settings are recommended:" );
			  Print( initialHeapSize.name() + "=" + heap );
			  Print( maxHeapSize.name() + "=" + heap );
			  Print( pagecache_memory.name() + "=" + pagecache );

			  if ( !specificDb )
			  {
					return;
			  }
			  string databaseName = arguments.get( ARG_DATABASE );
			  File configFile = _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ).toFile();
			  File databaseDirectory = GetConfig( configFile, databaseName ).get( database_path );
			  DatabaseLayout layout = DatabaseLayout.of( databaseDirectory );
			  long pageCacheSize = DbSpecificPageCacheSize( layout );
			  long luceneSize = DbSpecificLuceneSize( databaseDirectory );

			  Print( "#" );
			  Print( "# The numbers below have been derived based on your current data volume in database and index configuration of database '" + databaseName + "'." );
			  Print( "# They can be used as an input into more detailed memory analysis." );
			  Print( "# Lucene indexes: " + BytesToString( luceneSize ) );
			  Print( "# Data volume and native indexes: " + BytesToString( pageCacheSize ) );
		 }

		 private long DbSpecificPageCacheSize( DatabaseLayout databaseLayout )
		 {
			  return SumStoreFiles( databaseLayout ) + SumIndexFiles( baseSchemaIndexFolder( databaseLayout.DatabaseDirectory() ), GetNativeIndexFileFilter(databaseLayout.DatabaseDirectory(), false) );
		 }

		 private long DbSpecificLuceneSize( File databaseDirectory )
		 {
			  return SumIndexFiles( baseSchemaIndexFolder( databaseDirectory ), GetNativeIndexFileFilter( databaseDirectory, true ) );
		 }

		 private FilenameFilter GetNativeIndexFileFilter( File storeDir, bool inverse )
		 {
			  FileFilter nativeIndexFilter = new NativeIndexFileFilter( storeDir );
			  return ( dir, name ) =>
			  {
				File file = new File( dir, name );
				if ( _outsideWorld.fileSystem().isDirectory(file) )
				{
					 // Always go down directories
					 return true;
				}
				if ( name.Equals( FailureStorage.DEFAULT_FAILURE_FILE_NAME ) )
				{
					 // Never include failure-storage files
					 return false;
				}

				return inverse != nativeIndexFilter.accept( file );
			  };
		 }

		 private long SumStoreFiles( DatabaseLayout databaseLayout )
		 {
			  long total = 0;
			  // Include store files
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.RecordStore )
					{
						 FileSystemAbstraction fileSystem = _outsideWorld.fileSystem();
						 total += databaseLayout.file( type.DatabaseFile ).filter( fileSystem.fileExists ).mapToLong( fileSystem.getFileSize ).sum();
					}
			  }
			  // Include label index
			  total += SizeOfFileIfExists( databaseLayout.LabelScanStore() );
			  return total;
		 }

		 private long SizeOfFileIfExists( File file )
		 {
			  FileSystemAbstraction fileSystem = _outsideWorld.fileSystem();
			  return fileSystem.FileExists( file ) ? fileSystem.GetFileSize( file ) : 0;
		 }

		 private long SumIndexFiles( File file, FilenameFilter filter )
		 {
			  long total = 0;
			  if ( _outsideWorld.fileSystem().isDirectory(file) )
			  {
					File[] children = _outsideWorld.fileSystem().listFiles(file, filter);
					if ( children != null )
					{
						 foreach ( File child in children )
						 {
							  total += SumIndexFiles( child, filter );
						 }
					}
			  }
			  else
			  {
					total += _outsideWorld.fileSystem().getFileSize(file);
			  }
			  return total;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.configuration.Config getConfig(java.io.File configFile, String databaseName) throws org.neo4j.commandline.admin.CommandFailed
		 private Config GetConfig( File configFile, string databaseName )
		 {
			  if ( !_outsideWorld.fileSystem().fileExists(configFile) )
			  {
					throw new CommandFailed( "Unable to find config file, tried: " + configFile.AbsolutePath );
			  }
			  try
			  {
					return Config.fromFile( configFile ).withHome( _homeDir ).withSetting( active_database, databaseName ).withConnectorsDisabled().build();
			  }
			  catch ( Exception e )
			  {
					throw new CommandFailed( "Failed to read config file: " + configFile.AbsolutePath, e );
			  }
		 }

		 private void Print( string text )
		 {
			  _outsideWorld.stdOutLine( text );
		 }

		 private sealed class Bracket
		 {
			  internal readonly double TotalMemory;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly double OsMemoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly double HeapMemoryConflict;

			  internal Bracket( double totalMemory, double osMemory, double heapMemory )
			  {
					this.TotalMemory = totalMemory;
					this.OsMemoryConflict = osMemory;
					this.HeapMemoryConflict = heapMemory;
			  }

			  internal double OsMemory()
			  {
					return OsMemoryConflict;
			  }

			  internal double HeapMemory()
			  {
					return HeapMemoryConflict;
			  }
		 }

		 private sealed class Brackets
		 {
			  internal readonly double TotalMemoryGB;
			  internal readonly Bracket Lower;
			  internal readonly Bracket Upper;

			  internal Brackets( double totalMemoryGB, Bracket lower, Bracket upper )
			  {
					this.TotalMemoryGB = totalMemoryGB;
					this.Lower = lower;
					this.Upper = upper;
			  }

			  internal double DifferenceFactor()
			  {
					if ( Lower == Upper )
					{
						 return 0;
					}
					return ( TotalMemoryGB - Lower.totalMemory ) / ( Upper.totalMemory - Lower.totalMemory );
			  }

			  public long Recommend( System.Func<Bracket, double> parameter )
			  {
					double factor = DifferenceFactor();
					double lowerParam = parameter( Lower );
					double upperParam = parameter( Upper );
					double diff = upperParam - lowerParam;
					double recommend = lowerParam + ( diff * factor );
					return mebiBytes( ( long )( recommend * 1024.0 ) );
			  }
		 }
	}

}