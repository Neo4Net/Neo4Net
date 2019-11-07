using System;

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
namespace Neo4Net.Kernel.impl.store.counts
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Headers = Neo4Net.Kernel.impl.store.kvstore.Headers;
	using MetadataVisitor = Neo4Net.Kernel.impl.store.kvstore.MetadataVisitor;
	using ReadableBuffer = Neo4Net.Kernel.impl.store.kvstore.ReadableBuffer;
	using Rotation = Neo4Net.Kernel.impl.store.kvstore.Rotation;
	using RotationMonitor = Neo4Net.Kernel.impl.store.kvstore.RotationMonitor;
	using RotationTimerFactory = Neo4Net.Kernel.impl.store.kvstore.RotationTimerFactory;
	using UnknownKey = Neo4Net.Kernel.impl.store.kvstore.UnknownKey;
	using WritableBuffer = Neo4Net.Kernel.impl.store.kvstore.WritableBuffer;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Register = Neo4Net.Register.Register;
	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.counts_store_rotation_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.indexSampleKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.indexStatisticsKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;

	/// <summary>
	/// This is the main class for the counts store.
	/// 
	/// The counts store is a key/value store, where key/value entries are stored sorted by the key in ascending unsigned
	/// (big endian) order. These store files are immutable, and on store-flush the implementation swaps the read and write
	/// file in a <seealso cref="Rotation.Strategy.LEFT_RIGHT left/right pattern"/>.
	/// 
	/// This class defines <seealso cref="KeyFormat the key serialisation format"/>,
	/// <seealso cref="CountsUpdater the value serialisation format"/>, and
	/// <seealso cref="HEADER_FIELDS the header fields"/>.
	/// 
	/// The <seealso cref="AbstractKeyValueStore parent class"/> defines the life cycle of the store.
	/// 
	/// The pattern of immutable store files, and rotation strategy, et.c. is defined in the
	/// {@code kvstore}-package, see <seealso cref="Neo4Net.kernel.impl.store.kvstore.KeyValueStoreFile"/> for a good entry point.
	/// </summary>
	[Rotation(value : Rotation.Strategy.LEFT_RIGHT)]
	public class CountsTracker : AbstractKeyValueStore<CountsKey>, Neo4Net.Kernel.Impl.Api.CountsVisitor_Visitable, CountsAccessor
	{
		 /// <summary>
		 /// The format specifier for the current version of the store file format. </summary>
		 private static readonly sbyte[] _format = new sbyte[] { ( sbyte )'N', ( sbyte )'e', ( sbyte )'o', ( sbyte )'C', ( sbyte )'o', ( sbyte )'u', ( sbyte )'n', ( sbyte )'t', ( sbyte )'S', ( sbyte )'t', ( sbyte )'o', ( sbyte )'r', ( sbyte )'e', 0, 2, ( sbyte )'V' };
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static final Neo4Net.kernel.impl.store.kvstore.HeaderField<?>[] HEADER_FIELDS = new Neo4Net.kernel.impl.store.kvstore.HeaderField[]{FileVersion.FILE_VERSION};
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static readonly HeaderField<object>[] _headerFields = new HeaderField[]{ FileVersion.FILE_VERSION };
		 public const string TYPE_DESCRIPTOR = "CountsStore";

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CountsTracker(final Neo4Net.logging.LogProvider logProvider, Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.pagecache.PageCache pages, Neo4Net.kernel.configuration.Config config, Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier)
		 public CountsTracker( LogProvider logProvider, FileSystemAbstraction fs, PageCache pages, Config config, DatabaseLayout databaseLayout, VersionContextSupplier versionContextSupplier ) : this( logProvider, fs, pages, config, databaseLayout, Clocks.nanoClock(), versionContextSupplier )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CountsTracker(final Neo4Net.logging.LogProvider logProvider, Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.pagecache.PageCache pages, Neo4Net.kernel.configuration.Config config, Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.time.SystemNanoClock clock, Neo4Net.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier)
		 public CountsTracker( LogProvider logProvider, FileSystemAbstraction fs, PageCache pages, Config config, DatabaseLayout databaseLayout, SystemNanoClock clock, VersionContextSupplier versionContextSupplier ) : base( fs, pages, databaseLayout, new CountsTrackerRotationMonitor( logProvider ), logProvider.GetLog( typeof( CountsTracker ) ).infoLogger(), new RotationTimerFactory(clock, config.Get(counts_store_rotation_timeout).toMillis()), versionContextSupplier, 16, 16, _headerFields )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CountsTracker setInitializer(final Neo4Net.kernel.impl.store.kvstore.DataInitializer<Neo4Net.kernel.impl.api.CountsAccessor_Updater> initializer)
		 public virtual CountsTracker setInitializer( DataInitializer<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater> initializer )
		 {
			  EntryUpdaterInitializer = new DataInitializerAnonymousInnerClass( this, initializer );
			  return this;
		 }

		 private class DataInitializerAnonymousInnerClass : DataInitializer<EntryUpdater<CountsKey>>
		 {
			 private readonly CountsTracker _outerInstance;

			 private DataInitializer<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater> _initializer;

			 public DataInitializerAnonymousInnerClass( CountsTracker outerInstance, DataInitializer<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater> initializer )
			 {
				 this.outerInstance = outerInstance;
				 this._initializer = initializer;
			 }

			 public void initialize( EntryUpdater<CountsKey> updater )
			 {
				  _initializer.initialize( new CountsUpdater( updater ) );
			 }

			 public long initialVersion()
			 {
				  return _initializer.initialVersion();
			 }
		 }

		 /// <param name="txId"> the lowest transaction id that must be included in the snapshot created by the rotation. </param>
		 /// <returns> the highest transaction id that was included in the snapshot created by the rotation. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long rotate(long txId) throws java.io.IOException
		 public virtual long Rotate( long txId )
		 {
			  return PrepareRotation( txId ).rotate();
		 }

		 public virtual long TxId()
		 {
			  return Headers().get(FileVersion.FILE_VERSION).txId;
		 }

		 public virtual long MinorVersion()
		 {
			  return Headers().get(FileVersion.FILE_VERSION).minorVersion;
		 }

		 public virtual Neo4Net.Register.Register_DoubleLongRegister Get( CountsKey key, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  try
			  {
					return Lookup( key, new ValueRegister( target ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Neo4Net.register.Register_DoubleLongRegister nodeCount(int labelId, final Neo4Net.register.Register_DoubleLongRegister target)
		 public override Neo4Net.Register.Register_DoubleLongRegister NodeCount( int labelId, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  return Get( nodeKey( labelId ), target );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister RelationshipCount( int startLabelId, int typeId, int endLabelId, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  return Get( relationshipKey( startLabelId, typeId, endLabelId ), target );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  return Get( indexStatisticsKey( indexId ), target );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexSample( long indexId, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  return Get( indexSampleKey( indexId ), target );
		 }

		 public virtual Optional<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater> Apply( long txId )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Updater( txId ).map( CountsUpdater::new );
		 }

		 public virtual Neo4Net.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater UpdateIndexCounts()
		 {
			  return new CountsUpdater( Updater() );
		 }

		 public virtual Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater Reset( long txId )
		 {
			  return new CountsUpdater( Resetter( txId ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void accept(final Neo4Net.kernel.impl.api.CountsVisitor visitor)
		 public override void Accept( CountsVisitor visitor )
		 {
			  try
			  {
					VisitAll( new DelegatingVisitor( this, visitor ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void visitFile(java.io.File path, Neo4Net.kernel.impl.api.CountsVisitor visitor) throws java.io.IOException
		 protected internal virtual void VisitFile( File path, CountsVisitor visitor )
		 {
			  base.VisitFile( path, new DelegatingVisitor( this, visitor ) );
		 }

		 protected internal override Headers InitialHeaders( long txId )
		 {
			  return Headers.headersBuilder().put(FileVersion.FILE_VERSION, new FileVersion(txId)).headers();
		 }

		 protected internal override int CompareHeaders( Headers lhs, Headers rhs )
		 {
			  return Compare( lhs.Get( FileVersion.FILE_VERSION ), rhs.Get( FileVersion.FILE_VERSION ) );
		 }

		 internal static int Compare( FileVersion lhs, FileVersion rhs )
		 {
			  int cmp = Long.compare( lhs.TxId, rhs.TxId );
			  if ( cmp == 0 )
			  {
					cmp = Long.compare( lhs.MinorVersion, rhs.MinorVersion );
			  }
			  return cmp;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void writeKey(Neo4Net.kernel.impl.store.counts.keys.CountsKey key, final Neo4Net.kernel.impl.store.kvstore.WritableBuffer buffer)
		 protected internal override void WriteKey( CountsKey key, WritableBuffer buffer )
		 {
			  key.Accept( new KeyFormat( buffer ), 0, 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.kernel.impl.store.counts.keys.CountsKey readKey(Neo4Net.kernel.impl.store.kvstore.ReadableBuffer key) throws Neo4Net.kernel.impl.store.kvstore.UnknownKey
		 protected internal override CountsKey ReadKey( ReadableBuffer key )
		 {
			  return KeyFormat.ReadKey( key );
		 }

		 protected internal override bool Include( CountsKey countsKey, ReadableBuffer value )
		 {
			  return !value.AllZeroes();
		 }

		 protected internal override void UpdateHeaders( Headers.Builder headers, long version )
		 {
			  headers.Put( FileVersion.FILE_VERSION, headers.Get( FileVersion.FILE_VERSION ).update( version ) );
		 }

		 protected internal override long Version( Headers headers )
		 {
			  return headers == null ? FileVersion.InitialTxId : headers.Get( FileVersion.FILE_VERSION ).txId;
		 }

		 protected internal override void WriteFormatSpecifier( WritableBuffer formatSpecifier )
		 {
			  formatSpecifier.Put( 0, _format );
		 }

		 private class CountsTrackerRotationMonitor : RotationMonitor
		 {
			  internal readonly Log Log;

			  internal CountsTrackerRotationMonitor( LogProvider logProvider )
			  {
					Log = logProvider.GetLog( typeof( CountsTracker ) );
			  }

			  public override void FailedToOpenStoreFile( File path, Exception error )
			  {
					Log.error( "Failed to open counts store file: " + path, error );
			  }

			  public override void BeforeRotation( File source, File target, Headers headers )
			  {
			  }

			  public override void RotationSucceeded( File source, File target, Headers headers )
			  {
					Log.info( format( "Rotated counts store at transaction %d to [%s], from [%s].", headers.Get( FileVersion.FILE_VERSION ).txId, target, source ) );
			  }

			  public override void RotationFailed( File source, File target, Headers headers, Exception e )
			  {
					Log.error( format( "Failed to rotate counts store at transaction %d to [%s], from [%s].", headers.Get( FileVersion.FILE_VERSION ).txId, target, source ), e );
			  }
		 }

		 private class DelegatingVisitor : Visitor, MetadataVisitor
		 {
			 private readonly CountsTracker _outerInstance;

			  internal readonly CountsVisitor Visitor;

			  internal DelegatingVisitor( CountsTracker outerInstance, CountsVisitor visitor ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.Visitor = visitor;
			  }

			  protected internal override bool VisitKeyValuePair( CountsKey key, ReadableBuffer value )
			  {
					key.Accept( Visitor, value.GetLong( 0 ), value.GetLong( 8 ) );
					return true;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void visitMetadata(java.io.File path, Neo4Net.kernel.impl.store.kvstore.Headers headers, int entryCount)
			  public override void VisitMetadata( File path, Headers headers, int entryCount )
			  {
					if ( Visitor is MetadataVisitor )
					{
						 ( ( MetadataVisitor ) Visitor ).visitMetadata( path, headers, entryCount );
					}
			  }

			  protected internal override bool VisitUnknownKey( UnknownKey exception, ReadableBuffer key, ReadableBuffer value )
			  {
					if ( Visitor is UnknownKey.Visitor )
					{
						 return ( ( UnknownKey.Visitor ) Visitor ).visitUnknownKey( key, value );
					}
					else
					{
						 return base.VisitUnknownKey( exception, key, value );
					}
			  }
		 }
	}

}