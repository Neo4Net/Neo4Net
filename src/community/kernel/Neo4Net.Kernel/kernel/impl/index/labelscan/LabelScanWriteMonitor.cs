using System;
using System.IO;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{

	using Args = Neo4Net.Helpers.Args;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using Neo4Net.Kernel.impl.transaction.log;
	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.ByteUnit.mebiBytes;

	/// <summary>
	/// A <seealso cref="NativeLabelScanWriter.WriteMonitor"/> which writes all interactions to a .writelog file, which has configurable rotation and pruning.
	/// This class also has a <seealso cref="main(string[])"/> method for dumping the contents of such a write log to console or file, as text.
	/// </summary>
	public class LabelScanWriteMonitor : NativeLabelScanWriter.WriteMonitor
	{
		 // configuration for this monitor
		 internal static readonly bool Enabled = FeatureToggles.flag( typeof( LabelScanWriteMonitor ), "enabled", false );
		 private static readonly long _rotationSizeThreshold = FeatureToggles.getLong( typeof( LabelScanWriteMonitor ), "rotationThreshold", mebiBytes( 200 ) );
		 private static readonly long _pruneThreshold = FeatureToggles.getLong( typeof( LabelScanWriteMonitor ), "pruneThreshold", TimeUnit.DAYS.toMillis( 2 ) );

		 private const sbyte TYPE_PREPARE_ADD = 0;
		 private const sbyte TYPE_PREPARE_REMOVE = 1;
		 private const sbyte TYPE_MERGE_ADD = 2;
		 private const sbyte TYPE_MERGE_REMOVE = 3;
		 private const sbyte TYPE_RANGE = 4;
		 private const sbyte TYPE_FLUSH = 5;
		 private const sbyte TYPE_SESSION_END = 6;

		 private const string ARG_TOFILE = "tofile";
		 private const string ARG_TXFILTER = "txfilter";

		 private readonly FileSystemAbstraction _fs;
		 private readonly File _storeDir;
		 private readonly File _file;
		 private FlushableChannel _channel;
		 private Lock @lock = new ReentrantLock();
		 private LongAdder _position = new LongAdder();
		 private long _rotationThreshold;
		 private long _pruneThreshold;

		 internal LabelScanWriteMonitor( FileSystemAbstraction fs, DatabaseLayout databaseLayout ) : this( fs, databaseLayout, _rotationSizeThreshold, ByteUnit.Byte, _pruneThreshold, TimeUnit.MILLISECONDS )
		 {
		 }

		 internal LabelScanWriteMonitor( FileSystemAbstraction fs, DatabaseLayout databaseLayout, long rotationThreshold, ByteUnit rotationThresholdUnit, long pruneThreshold, TimeUnit pruneThresholdUnit )
		 {
			  this._fs = fs;
			  this._rotationThreshold = rotationThresholdUnit.toBytes( rotationThreshold );
			  this._pruneThreshold = pruneThresholdUnit.toMillis( pruneThreshold );
			  this._storeDir = databaseLayout.DatabaseDirectory();
			  this._file = WriteLogBaseFile( databaseLayout );
			  try
			  {
					if ( fs.FileExists( _file ) )
					{
						 MoveAwayFile();
					}
					this._channel = InstantiateChannel();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 internal static File WriteLogBaseFile( DatabaseLayout databaseLayout )
		 {
			  return new File( databaseLayout.LabelScanStore() + ".writelog" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.impl.transaction.log.PhysicalFlushableChannel instantiateChannel() throws java.io.IOException
		 private PhysicalFlushableChannel InstantiateChannel()
		 {
			  return new PhysicalFlushableChannel( _fs.open( _file, OpenMode.READ_WRITE ) );
		 }

		 public override void Range( long range, int labelId )
		 {
			  try
			  {
					_channel.put( TYPE_RANGE );
					_channel.putLong( range );
					_channel.putInt( labelId );
					_position.add( 1 + 8 + 4 );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void PrepareAdd( long txId, int offset )
		 {
			  Prepare( TYPE_PREPARE_ADD, txId, offset );
		 }

		 public override void PrepareRemove( long txId, int offset )
		 {
			  Prepare( TYPE_PREPARE_REMOVE, txId, offset );
		 }

		 private void Prepare( sbyte type, long txId, int offset )
		 {
			  try
			  {
					_channel.put( type );
					_channel.putLong( txId );
					_channel.put( ( sbyte ) offset );
					_position.add( 1 + 8 + 1 );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void MergeAdd( LabelScanValue existingValue, LabelScanValue newValue )
		 {
			  Merge( TYPE_MERGE_ADD, existingValue, newValue );
		 }

		 public override void MergeRemove( LabelScanValue existingValue, LabelScanValue newValue )
		 {
			  Merge( TYPE_MERGE_REMOVE, existingValue, newValue );
		 }

		 private void Merge( sbyte type, LabelScanValue existingValue, LabelScanValue newValue )
		 {
			  try
			  {
					_channel.put( type );
					_channel.putLong( existingValue.Bits );
					_channel.putLong( newValue.Bits );
					_position.add( 1 + 8 + 8 );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void FlushPendingUpdates()
		 {
			  try
			  {
					_channel.put( TYPE_FLUSH );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void WriteSessionEnded()
		 {
			  try
			  {
					_channel.put( TYPE_SESSION_END );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }

			  _position.add( 1 );
			  if ( _position.sum() > _rotationThreshold )
			  {
					// Rotate
					@lock.@lock();
					try
					{
						 _channel.prepareForFlush().flush();
						 _channel.Dispose();
						 MoveAwayFile();
						 _position.reset();
						 _channel = InstantiateChannel();
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
					finally
					{
						 @lock.unlock();
					}

					// Prune
					long time = currentTimeMillis();
					long threshold = time - _pruneThreshold;
					foreach ( File file in _fs.listFiles( _storeDir, ( dir, name ) => name.StartsWith( file.Name + "-" ) ) )
					{
						 if ( MillisOf( file ) < threshold )
						 {
							  _fs.deleteFile( file );
						 }
					}
			  }
		 }

		 internal static long MillisOf( File file )
		 {
			  string name = file.Name;
			  int dashIndex = name.LastIndexOf( '-' );
			  if ( dashIndex == -1 )
			  {
					return 0;
			  }
			  return long.Parse( name.Substring( dashIndex + 1 ) );
		 }

		 public override void Force()
		 {
			  // checkpoint does this
			  @lock.@lock();
			  try
			  {
					_channel.prepareForFlush().flush();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

		 public override void Close()
		 {
			  try
			  {
					_channel.Dispose();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void moveAwayFile() throws java.io.IOException
		 private void MoveAwayFile()
		 {
			  File to;
			  do
			  {
					to = TimestampedFile();
			  } while ( _fs.fileExists( to ) );
			  _fs.renameFile( _file, to );
		 }

		 private File TimestampedFile()
		 {
			  return new File( _storeDir, _file.Name + "-" + currentTimeMillis() );
		 }

		 /// <summary>
		 /// Dumps a label scan write log as plain text. Arguments:
		 /// <ul>
		 ///     <li>{@value #ARG_TOFILE}: dumps to a .txt file next to the writelog</li>
		 ///     <li>{@value #ARG_TXFILTER}: filter for which tx ids to include in the dump.
		 ///     <para>
		 ///     Consists of one or more groups separated by comma.
		 /// </para>
		 ///     <para>
		 ///     Each group is either a txId, or a txId range, e.g. 123-456
		 ///     </li>
		 /// </ul>
		 /// </para>
		 /// <para>
		 /// How to interpret the dump, e.g:
		 /// <pre>
		 /// === ..../neostore.labelscanstore.db.writelog ===
		 /// [1,1]+tx:6,node:0,label:0
		 /// [1,1]+tx:3,node:20,label:0
		 /// [1,1]+tx:4,node:40,label:0
		 /// [1,1]+tx:5,node:60,label:0
		 /// [2,1]+tx:8,node:80,label:1
		 /// [3,1]+tx:10,node:41,label:1
		 /// [4,1]+tx:9,node:21,label:1
		 /// [4,1]+tx:11,node:61,label:1
		 /// [4,1]+range:0,labelId:1
		 ///  [00000000 00000000 00000010 00000000 00000000 00000000 00000000 00000000]
		 ///  [00100000 00000000 00000000 00000000 00000000 00100000 00000000 00000000]
		 /// [5,1]+tx:12,node:81,label:1
		 /// [5,1]+range:1,labelId:1
		 ///  [00000000 00000000 00000000 00000000 00000000 00000001 00000000 00000000]
		 ///  [00000000 00000000 00000000 00000000 00000000 00000010 00000000 00000000]
		 /// [6,1]+tx:13,node:1,label:1
		 /// [6,1]+range:0,labelId:1
		 ///  [00100000 00000000 00000010 00000000 00000000 00100000 00000000 00000000]
		 ///  [00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000010]
		 /// [7,1]+tx:14,node:62,label:1
		 /// [7,1]+range:0,labelId:1
		 /// </pre>
		 /// How to interpret a message like:
		 /// <pre>
		 /// [1,1]+tx:6,node:0,label:0
		 ///  ▲ ▲ ▲   ▲      ▲       ▲
		 ///  │ │ │   │      │       └──── label id of the change
		 ///  │ │ │   │      └──────────── node id of the change
		 ///  │ │ │   └─────────────────── id of transaction making this particular change
		 ///  │ │ └─────────────────────── addition, a minus means removal
		 ///  │ └───────────────────────── flush, local to each write session, incremented when a batch of changes is flushed internally in a writer session
		 ///  └─────────────────────────── write session, incremented for each <seealso cref="LabelScanStore.newWriter()"/>
		 /// </pre>
		 /// How to interpret a message like:
		 /// <pre>
		 /// [4,1]+range:0,labelId:1
		 ///  [00000000 00000000 00000010 00000000 00000000 00000000 00000000 00000000]
		 ///  [00100000 00000000 00000000 00000000 00000000 00100000 00000000 00000000]
		 /// </pre>
		 /// First the first line (parts within bracket same as above):
		 /// <pre>
		 /// [4,1]+range:0,labelId:1
		 ///             ▲         ▲
		 ///             │         └── label id of the changed bitset to apply
		 ///             └──────────── range, i.e. which bitset to apply this change for
		 /// </pre>
		 /// Then the bitsets are printed
		 /// <pre>
		 ///  [00000000 00000000 00000010 00000000 00000000 00000000 00000000 00000000] : state of the bitset for this label id before the change
		 ///  [00100000 00000000 00000000 00000000 00000000 00100000 00000000 00000000] : bits that applied to this bitset
		 ///                                                                              for addition the 1-bits denotes bits to be added
		 ///                                                                              for removal the 1-bits denotes bits to be removed
		 /// </pre>
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  Args arguments = Args.withFlags( ARG_TOFILE ).parse( args );
			  if ( arguments.Orphans().Count == 0 )
			  {
					Console.Error.WriteLine( "Please supply database directory" );
					return;
			  }

			  DatabaseLayout databaseLayout = DatabaseLayout.of( new File( arguments.Orphans()[0] ) );
			  FileSystemAbstraction fs = new DefaultFileSystemAbstraction();
			  TxFilter txFilter = ParseTxFilter( arguments.Get( ARG_TXFILTER, null ) );
			  PrintStream @out = System.out;
			  bool redirectsToFile = arguments.GetBoolean( ARG_TOFILE );
			  if ( redirectsToFile )
			  {
					File outFile = new File( WriteLogBaseFile( databaseLayout ).AbsolutePath + ".txt" );
					Console.WriteLine( "Redirecting output to " + outFile );
					@out = new PrintStream( new BufferedOutputStream( new FileStream( outFile, FileMode.Create, FileAccess.Write ) ) );
			  }
			  Dumper dumper = new PrintStreamDumper( @out );
			  Dump( fs, databaseLayout, dumper, txFilter );
			  if ( redirectsToFile )
			  {
					@out.close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void dump(org.Neo4Net.io.fs.FileSystemAbstraction fs, org.Neo4Net.io.layout.DatabaseLayout databaseLayout, Dumper dumper, TxFilter txFilter) throws java.io.IOException
		 public static void Dump( FileSystemAbstraction fs, DatabaseLayout databaseLayout, Dumper dumper, TxFilter txFilter )
		 {
			  File writeLogFile = WriteLogBaseFile( databaseLayout );
			  string writeLogFileBaseName = writeLogFile.Name;
			  File[] files = fs.ListFiles( databaseLayout.DatabaseDirectory(), (dir, name) => name.StartsWith(writeLogFileBaseName) );
			  Arrays.sort( files, comparing( _file => _file.Name.Equals( writeLogFileBaseName ) ? 0 : MillisOf( _file ) ) );
			  long session = 0;
			  foreach ( File file in files )
			  {
					dumper.File( file );
					session = DumpFile( fs, file, dumper, txFilter, session );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long dumpFile(org.Neo4Net.io.fs.FileSystemAbstraction fs, java.io.File file, Dumper dumper, TxFilter txFilter, long session) throws java.io.IOException
		 private static long DumpFile( FileSystemAbstraction fs, File file, Dumper dumper, TxFilter txFilter, long session )
		 {
			  try
			  {
					  using ( ReadableChannel channel = new ReadAheadChannel<>( fs.Open( file, OpenMode.READ ) ) )
					  {
						long range = -1;
						int labelId = -1;
						long flush = 0;
						while ( true )
						{
							 sbyte type = channel.Get();
							 switch ( type )
							 {
							 case TYPE_RANGE:
								  range = channel.Long;
								  labelId = channel.Int;
								  if ( txFilter != null )
								  {
										txFilter.Clear();
								  }
								  break;
							 case TYPE_PREPARE_ADD:
							 case TYPE_PREPARE_REMOVE:
								  DumpPrepare( dumper, type, channel, range, labelId, txFilter, session, flush );
								  break;
							 case TYPE_MERGE_ADD:
							 case TYPE_MERGE_REMOVE:
								  DumpMerge( dumper, type, channel, range, labelId, txFilter, session, flush );
								  break;
							 case TYPE_FLUSH:
								  flush++;
								  break;
							 case TYPE_SESSION_END:
								  session++;
								  flush = 0;
								  break;
							 default:
								  Console.WriteLine( "Unknown type " + type + " at " + ( ( ReadAheadChannel ) channel ).position() );
								  break;
							 }
						}
					  }
			  }
			  catch ( ReadPastEndException )
			  {
					// This is OK. we're done with this file
			  }
			  return session;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpMerge(Dumper dumper, byte type, org.Neo4Net.storageengine.api.ReadableChannel channel, long range, int labelId, TxFilter txFilter, long session, long flush) throws java.io.IOException
		 private static void DumpMerge( Dumper dumper, sbyte type, ReadableChannel channel, long range, int labelId, TxFilter txFilter, long session, long flush )
		 {
			  long existingBits = channel.Long;
			  long newBits = channel.Long;
			  if ( txFilter == null || txFilter.Contains() )
			  {
					dumper.Merge( type == TYPE_MERGE_ADD, session, flush, range, labelId, existingBits, newBits );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpPrepare(Dumper dumper, byte type, org.Neo4Net.storageengine.api.ReadableChannel channel, long range, int labelId, TxFilter txFilter, long session, long flush) throws java.io.IOException
		 private static void DumpPrepare( Dumper dumper, sbyte type, ReadableChannel channel, long range, int labelId, TxFilter txFilter, long session, long flush )
		 {
			  long txId = channel.Long;
			  int offset = channel.Get();
			  long nodeId = range * 64 + offset;
			  if ( txFilter == null || txFilter.Contains( txId ) )
			  {
					// I.e. if the txId this update comes from is within the txFilter
					dumper.Prepare( type == TYPE_PREPARE_ADD, session, flush, txId, nodeId, labelId );
			  }
		 }

		 internal static TxFilter ParseTxFilter( string txFilter )
		 {
			  if ( string.ReferenceEquals( txFilter, null ) )
			  {
					return null;
			  }

			  string[] tokens = txFilter.Split( ",", true );
			  long[][] filters = new long[tokens.Length][];
			  for ( int i = 0; i < tokens.Length; i++ )
			  {
					string token = tokens[i];
					int index = token.LastIndexOf( '-' );
					long low, high;
					if ( index == -1 )
					{
						 low = high = long.Parse( token );
					}
					else
					{
						 low = long.Parse( token.Substring( 0, index ) );
						 high = long.Parse( token.Substring( index + 1 ) );
					}
					filters[i] = new long[]{ low, high };
			  }
			  return new TxFilter( filters );
		 }

		 internal class TxFilter
		 {
			  internal readonly long[][] LowsAndHighs;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ContainsConflict;

			  internal TxFilter( params long[][] lowsAndHighs )
			  {
					this.LowsAndHighs = lowsAndHighs;
			  }

			  internal virtual void Clear()
			  {
					ContainsConflict = false;
			  }

			  internal virtual bool Contains( long txId )
			  {
					foreach ( long[] filter in LowsAndHighs )
					{
						 if ( txId >= filter[0] && txId <= filter[1] )
						 {
							  ContainsConflict = true;
							  return true;
						 }
					}
					return false;
			  }

			  internal virtual bool Contains()
			  {
					return ContainsConflict;
			  }
		 }

		 public interface Dumper
		 {
			  void File( File file );

			  void Prepare( bool add, long session, long flush, long txId, long nodeId, int labelId );

			  void Merge( bool add, long session, long flush, long range, int labelId, long existingBits, long newBits );
		 }

		 public class PrintStreamDumper : Dumper
		 {
			  internal readonly PrintStream Out;
			  internal readonly char[] BitsAsChars = new char[64 + 7];

			  internal PrintStreamDumper( PrintStream @out )
			  {
					this.Out = @out;
					Arrays.fill( BitsAsChars, ' ' );
			  }

			  public override void File( File file )
			  {
					Out.println( "=== " + file.AbsolutePath + " ===" );
			  }

			  public override void Prepare( bool add, long session, long flush, long txId, long nodeId, int labelId )
			  {
					Out.println( format( "[%d,%d]%stx:%d,node:%d,label:%d", session, flush, add ? '+' : '-', txId, nodeId, labelId ) );
			  }

			  public override void Merge( bool add, long session, long flush, long range, int labelId, long existingBits, long newBits )
			  {
					Out.println( format( "[%d,%d]%srange:%d,labelId:%d%n [%s]%n [%s]", session, flush, add ? '+' : '-', range, labelId, Bits( existingBits, BitsAsChars ), Bits( newBits, BitsAsChars ) ) );
			  }

			  internal static string Bits( long bits, char[] bitsAsChars )
			  {
					long mask = 1;
					for ( int i = 0, c = 0; i < 64; i++, c++ )
					{
						 if ( i % 8 == 0 )
						 {
							  c++;
						 }
						 bool set = ( bits & mask ) != 0;
						 bitsAsChars[bitsAsChars.Length - c] = set ? '1' : '0';
						 mask <<= 1;
					}
					return new string( bitsAsChars );
			  }
		 }
	}

}