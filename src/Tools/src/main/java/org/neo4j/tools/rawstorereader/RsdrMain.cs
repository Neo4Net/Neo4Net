using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.rawstorereader
{

	using Neo4Net.Cursors;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using InvalidRecordException = Neo4Net.Kernel.impl.store.InvalidRecordException;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using HexString = Neo4Net.@string.HexString;
	using TransactionLogUtils = Neo4Net.tools.util.TransactionLogUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionBridge_Fields.NO_MORE_CHANNELS;

	/// <summary>
	/// Tool to read raw data from various stores.
	/// </summary>
	public class RsdrMain
	{

		 private static readonly Console _console = System.console();
		 private static readonly Pattern _readCommandPattern = Pattern.compile( "r" + "((?<lower>\\d+)?,(?<upper>\\d+)?)?\\s+" + "(?<fname>[\\w.]+)" + "(\\s*\\|\\s*(?<regex>.+))?" );

		 private RsdrMain()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					_console.printf( "Neo4j Raw Store Diagnostics Reader%n" );

					if ( args.Length != 1 || !fileSystem.IsDirectory( new File( args[0] ) ) )
					{
						 _console.printf( "Usage: rsdr <store directory>%n" );
						 return;
					}

					File databaseDirectory = new File( args[0] );
					DatabaseLayout databaseLayout = DatabaseLayout.of( databaseDirectory );

					Config config = BuildConfig();
					JobScheduler jobScheduler = createInitialisedScheduler();
					using ( PageCache pageCache = createPageCache( fileSystem, config, jobScheduler ) )
					{
						 File neoStore = databaseLayout.MetadataStore();
						 StoreFactory factory = OpenStore( fileSystem, neoStore, config, pageCache );
						 NeoStores neoStores = factory.OpenAllNeoStores();
						 Interact( fileSystem, neoStores, databaseLayout );
					}
			  }
		 }

		 private static Config BuildConfig()
		 {
			  return Config.defaults( MapUtil.stringMap( GraphDatabaseSettings.read_only.name(), "true", GraphDatabaseSettings.pagecache_memory.name(), "64M" ) );
		 }

		 private static StoreFactory OpenStore( FileSystemAbstraction fileSystem, File storeDir, Config config, PageCache pageCache )
		 {
			  IdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fileSystem );
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  return new StoreFactory( DatabaseLayout.of( storeDir ), config, idGeneratorFactory, pageCache, fileSystem, logProvider, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void interact(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.kernel.impl.store.NeoStores neoStores, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static void Interact( FileSystemAbstraction fileSystem, NeoStores neoStores, DatabaseLayout databaseLayout )
		 {
			  PrintHelp();

			  string cmd;
			  do
			  {
					cmd = _console.readLine( "neo? " );
			  } while ( Execute( fileSystem, cmd, neoStores, databaseLayout ) );
			  Environment.Exit( 0 );
		 }

		 private static void PrintHelp()
		 {
			  _console.printf( "Usage:%n" + "  h            print this message%n" + "  l            list store files in store%n" + "  r f          read all records in store file 'f'%n" + "  r5,10 f      read record 5 through 10 in store file 'f'%n" + "  r f | rx     read records and filter through regex 'rx'%n" + "  q            quit%n" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean execute(org.neo4j.io.fs.FileSystemAbstraction fileSystem, String cmd, org.neo4j.kernel.impl.store.NeoStores neoStores, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static bool Execute( FileSystemAbstraction fileSystem, string cmd, NeoStores neoStores, DatabaseLayout databaseLayout )
		 {
			  if ( string.ReferenceEquals( cmd, null ) || cmd.Equals( "q" ) )
			  {
					return false;
			  }
			  else if ( cmd.Equals( "h" ) )
			  {
					PrintHelp();
			  }
			  else if ( cmd.Equals( "l" ) )
			  {
					ListFiles( fileSystem, databaseLayout );
			  }
			  else if ( cmd.StartsWith( "r", StringComparison.Ordinal ) )
			  {
					Read( fileSystem, cmd, neoStores, databaseLayout );
			  }
			  else if ( cmd.Trim().Length > 0 )
			  {
					_console.printf( "unrecognized command%n" );
			  }
			  return true;
		 }

		 private static void ListFiles( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout )
		 {
			  File databaseDirectory = databaseLayout.DatabaseDirectory();
			  File[] listing = fileSystem.ListFiles( databaseDirectory );
			  foreach ( File file in listing )
			  {
					_console.printf( "%s%n", file.Name );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void read(org.neo4j.io.fs.FileSystemAbstraction fileSystem, String cmd, org.neo4j.kernel.impl.store.NeoStores neoStores, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static void Read( FileSystemAbstraction fileSystem, string cmd, NeoStores neoStores, DatabaseLayout databaseLayout )
		 {
			  Matcher matcher = _readCommandPattern.matcher( cmd );
			  if ( matcher.find() )
			  {
					string lower = matcher.group( "lower" );
					string upper = matcher.group( "upper" );
					string fname = matcher.group( "fname" );
					string regex = matcher.group( "regex" );
					Pattern pattern = !string.ReferenceEquals( regex, null ) ? Pattern.compile( regex ) : null;
					long fromId = !string.ReferenceEquals( lower, null ) ? long.Parse( lower ) : 0L;
					long toId = !string.ReferenceEquals( upper, null ) ? long.Parse( upper ) : long.MaxValue;

					RecordStore store = GetStore( fname, neoStores );
					if ( store != null )
					{
						 ReadStore( fileSystem, store, fromId, toId, pattern );
						 return;
					}

					IOCursor<LogEntry> cursor = GetLogCursor( fileSystem, fname, databaseLayout );
					if ( cursor != null )
					{
						 ReadLog( cursor, fromId, toId, pattern );
						 cursor.close();
						 return;
					}

					_console.printf( "don't know how to read '%s'%n", fname );
			  }
			  else
			  {
					_console.printf( "bad read command format%n" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void readStore(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.kernel.impl.store.RecordStore store, long fromId, long toId, java.util.regex.Pattern pattern) throws java.io.IOException
		 private static void ReadStore( FileSystemAbstraction fileSystem, RecordStore store, long fromId, long toId, Pattern pattern )
		 {
			  toId = Math.Min( toId, store.HighId );
			  using ( StoreChannel channel = fileSystem.Open( store.StorageFile, OpenMode.READ ) )
			  {
					int recordSize = store.RecordSize;
					ByteBuffer buf = ByteBuffer.allocate( recordSize );
					for ( long i = fromId; i <= toId; i++ )
					{
						 buf.clear();
						 long offset = recordSize * i;
						 int count = channel.Read( buf, offset );
						 if ( count == -1 )
						 {
							  break;
						 }
						 sbyte[] bytes = new sbyte[count];
						 buf.clear();
						 buf.get( bytes );
						 string hex = HexString.encodeHexString( bytes );
						 int paddingNeeded = ( recordSize * 2 - Math.Max( count * 2, 0 ) ) + 1;
						 string format = "%s %6s 0x%08X %s%" + paddingNeeded + "s%s%n";
						 string str;
						 string use;

						 try
						 {
							  AbstractBaseRecord record = RecordStore.getRecord( store, i, CHECK );
							  use = record.InUse() ? "+" : "-";
							  str = record.ToString();
						 }
						 catch ( InvalidRecordException )
						 {
							  str = StringHelper.NewString( bytes, 0, count, "ASCII" );
							  use = "?";
						 }

						 if ( pattern == null || pattern.matcher( str ).find() )
						 {
							  _console.printf( format, use, i, offset, hex, " ", str );
						 }
					}
			  }
		 }

		 private static RecordStore GetStore( string fname, NeoStores neoStores )
		 {
			  switch ( fname )
			  {
			  case "neostore.nodestore.db":
					return neoStores.NodeStore;
			  case "neostore.labeltokenstore.db":
					return neoStores.LabelTokenStore;
			  case "neostore.propertystore.db.index":
					return neoStores.PropertyKeyTokenStore;
			  case "neostore.propertystore.db":
					return neoStores.PropertyStore;
			  case "neostore.relationshipgroupstore.db":
					return neoStores.RelationshipGroupStore;
			  case "neostore.relationshipstore.db":
					return neoStores.RelationshipStore;
			  case "neostore.relationshiptypestore.db":
					return neoStores.RelationshipTypeTokenStore;
			  case "neostore.schemastore.db":
					return neoStores.SchemaStore;
			  default:
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.cursor.IOCursor<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> getLogCursor(org.neo4j.io.fs.FileSystemAbstraction fileSystem, String fname, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static IOCursor<LogEntry> GetLogCursor( FileSystemAbstraction fileSystem, string fname, DatabaseLayout databaseLayout )
		 {
			  return TransactionLogUtils.openLogEntryCursor( fileSystem, new File( databaseLayout.DatabaseDirectory(), fname ), NO_MORE_CHANNELS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void readLog(org.neo4j.cursor.IOCursor<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> cursor, final long fromLine, final long toLine, final java.util.regex.Pattern pattern) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static void ReadLog( IOCursor<LogEntry> cursor, long fromLine, long toLine, Pattern pattern )
		 {
			  TimeZone timeZone = TimeZone.Default;
			  long lineCount = -1;
			  while ( cursor.next() )
			  {
					LogEntry logEntry = cursor.get();
					lineCount++;
					if ( lineCount > toLine )
					{
						 return;
					}
					if ( lineCount < fromLine )
					{
						 continue;
					}
					string str = logEntry.ToString( timeZone );
					if ( pattern == null || pattern.matcher( str ).find() )
					{
						 _console.printf( "%s%n", str );
					}
			  }
		 }
	}

}