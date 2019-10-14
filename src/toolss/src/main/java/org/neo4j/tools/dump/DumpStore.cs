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
namespace Neo4Net.tools.dump
{

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.store;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using Neo4Net.Kernel.impl.store;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using Neo4Net.Kernel.impl.store;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using TokenRecord = Neo4Net.Kernel.impl.store.record.TokenRecord;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PrintStreamLogger = Neo4Net.Logging.PrintStreamLogger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	/// <summary>
	/// Tool to dump content of specified store into readable format for further analysis. </summary>
	/// @param <RECORD> type of record to dump </param>
	/// @param <STORE> type of store to dump </param>
	public class DumpStore<RECORD, STORE> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where STORE : Neo4Net.Kernel.impl.store.RecordStore<RECORD>
	{
		 private class IdRange
		 {
			  internal readonly long StartId;
			  internal readonly long EndId;

			  internal IdRange( long startId, long endId )
			  {
					this.StartId = startId;
					this.EndId = endId;
			  }

			  internal static IdRange Parse( string idString )
			  {
					if ( idString.Contains( "-" ) )
					{
						 string[] parts = idString.Split( "-", true );
						 return new IdRange( parseLong( parts[0] ), parseLong( parts[1] ) );
					}

					long id = parseLong( idString );
					return new IdRange( id, id + 1 );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String... args) throws Exception
		 public static void Main( params string[] args )
		 {
			  if ( args == null || args.Length == 0 )
			  {
					Console.Error.WriteLine( "SYNTAX: [file[:id[,id]*]]+" );
					Console.Error.WriteLine( "where 'id' can be single id or range like: lowId-highId" );
					return;
			  }

			  using ( DefaultFileSystemAbstraction fs = new DefaultFileSystemAbstraction(), PageCache pageCache = createPageCache(fs, createInitializedScheduler()) )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.id.DefaultIdGeneratorFactory idGeneratorFactory = new org.neo4j.kernel.impl.store.id.DefaultIdGeneratorFactory(fs);
					DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
					System.Func<File, StoreFactory> createStoreFactory = file => new StoreFactory( DatabaseLayout.of( file.ParentFile ), Config.defaults(), idGeneratorFactory, pageCache, fs, LogProvider(), EmptyVersionContextSupplier.EMPTY );

					foreach ( string arg in args )
					{
						 DumpFile( createStoreFactory, arg );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpFile(System.Func<java.io.File, org.neo4j.kernel.impl.store.StoreFactory> createStoreFactory, String fileName) throws Exception
		 private static void DumpFile( System.Func<File, StoreFactory> createStoreFactory, string fileName )
		 {
			  File file = new File( fileName );
			  IdRange[] ids = null; // null means all possible ids

			  if ( file.File )
			  {
						 /* If file exists, even with : in its path, then accept it straight off. */
			  }
			  else if ( !file.Directory && file.Name.IndexOf( ':' ) != -1 )
			  {
						 /* Now we know that it is not a directory either, and that the last component
						    of the path contains a colon, thus it is very likely an attempt to use the
						    id-specifying syntax. */

					int idStart = fileName.LastIndexOf( ':' );

					string[] idStrings = fileName.Substring( idStart + 1 ).Split( ",", true );
					ids = new IdRange[idStrings.Length];
					for ( int i = 0; i < ids.Length; i++ )
					{
						 ids[i] = IdRange.Parse( idStrings[i] );
					}
					file = new File( fileName.Substring( 0, idStart ) );

					if ( !file.File )
					{
						 throw new System.ArgumentException( "No such file: " + fileName );
					}
			  }
			  DatabaseFile databaseFile = DatabaseFile.fileOf( file.Name ).orElseThrow( IllegalArgumentExceptionSupplier( fileName ) );
			  StoreType storeType = StoreType.typeOf( databaseFile ).orElseThrow( IllegalArgumentExceptionSupplier( fileName ) );
			  using ( NeoStores neoStores = createStoreFactory( file ).openNeoStores( storeType ) )
			  {
					switch ( storeType.innerEnumValue )
					{
					case StoreType.InnerEnum.META_DATA:
						 DumpMetaDataStore( neoStores );
						 break;
					case StoreType.InnerEnum.NODE:
						 DumpNodeStore( neoStores, ids );
						 break;
					case StoreType.InnerEnum.RELATIONSHIP:
						 DumpRelationshipStore( neoStores, ids );
						 break;
					case StoreType.InnerEnum.PROPERTY:
						 DumpPropertyStore( neoStores, ids );
						 break;
					case StoreType.InnerEnum.SCHEMA:
						 DumpSchemaStore( neoStores, ids );
						 break;
					case StoreType.InnerEnum.PROPERTY_KEY_TOKEN:
						 DumpPropertyKeys( neoStores, ids );
						 break;
					case StoreType.InnerEnum.LABEL_TOKEN:
						 DumpLabels( neoStores, ids );
						 break;
					case StoreType.InnerEnum.RELATIONSHIP_TYPE_TOKEN:
						 DumpRelationshipTypes( neoStores, ids );
						 break;
					case StoreType.InnerEnum.RELATIONSHIP_GROUP:
						 DumpRelationshipGroups( neoStores, ids );
						 break;
					default:
						 throw new System.ArgumentException( "Unsupported store type: " + storeType );
					}
			  }
		 }

		 private static System.Func<System.ArgumentException> IllegalArgumentExceptionSupplier( string fileName )
		 {
			  return () => new System.ArgumentException("Not a store file: " + fileName);
		 }

		 private static void DumpMetaDataStore( NeoStores neoStores )
		 {
			  neoStores.MetaDataStore.logRecords( new PrintStreamLogger( System.out ) );
		 }

		 private static LogProvider LogProvider()
		 {
			  return Boolean.getBoolean( "logger" ) ? FormattedLogProvider.toOutputStream( System.out ) : NullLogProvider.Instance;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <R extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord, S extends org.neo4j.kernel.impl.store.RecordStore<R>> void dump(IdRange[] ids, S store) throws Exception
		 private static void Dump<R, S>( IdRange[] ids, S store ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where S : Neo4Net.Kernel.impl.store.RecordStore<R>
		 {
			  ( new DumpStore<R, S>( System.out ) ).Dump( store, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpPropertyKeys(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpPropertyKeys( NeoStores neoStores, IdRange[] ids )
		 {
			  DumpTokens( neoStores.PropertyKeyTokenStore, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpLabels(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpLabels( NeoStores neoStores, IdRange[] ids )
		 {
			  DumpTokens( neoStores.LabelTokenStore, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpRelationshipTypes(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpRelationshipTypes( NeoStores neoStores, IdRange[] ids )
		 {
			  DumpTokens( neoStores.RelationshipTypeTokenStore, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <R extends org.neo4j.kernel.impl.store.record.TokenRecord> void dumpTokens(final org.neo4j.kernel.impl.store.TokenStore<R> store, IdRange[] ids) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static void DumpTokens<R>( TokenStore<R> store, IdRange[] ids ) where R : Neo4Net.Kernel.impl.store.record.TokenRecord
		 {
			  try
			  {
					new DumpStoreAnonymousInnerClass( store )
					.dump( store, ids );
			  }
			  finally
			  {
					store.close();
			  }
		 }

		 private class DumpStoreAnonymousInnerClass : DumpStore<R, TokenStore<R>>
		 {
			 private TokenStore<R> _store;

			 public DumpStoreAnonymousInnerClass( TokenStore<R> store ) : base( System.out )
			 {
				 this._store = store;
			 }

			 protected internal override object transform( R record )
			 {
				  if ( record.inUse() )
				  {
						_store.ensureHeavy( record );
						return record.Id + ": \"" + _store.getStringFor( record ) + "\": " + record;
				  }
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpRelationshipGroups(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpRelationshipGroups( NeoStores neoStores, IdRange[] ids )
		 {
			  Dump( ids, neoStores.RelationshipGroupStore );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpRelationshipStore(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpRelationshipStore( NeoStores neoStores, IdRange[] ids )
		 {
			  Dump( ids, neoStores.RelationshipStore );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpPropertyStore(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpPropertyStore( NeoStores neoStores, IdRange[] ids )
		 {
			  Dump( ids, neoStores.PropertyStore );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpSchemaStore(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpSchemaStore( NeoStores neoStores, IdRange[] ids )
		 {
			  using ( SchemaStore store = neoStores.SchemaStore )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.SchemaStorage storage = new org.neo4j.kernel.impl.store.SchemaStorage(store);
					SchemaStorage storage = new SchemaStorage( store );
					new DumpStoreAnonymousInnerClass2( storage )
					.dump( store, ids );
			  }
		 }

		 private class DumpStoreAnonymousInnerClass2 : DumpStore<DynamicRecord, SchemaStore>
		 {
			 private SchemaStorage _storage;

			 public DumpStoreAnonymousInnerClass2( SchemaStorage storage ) : base( System.out )
			 {
				 this._storage = storage;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Object transform(org.neo4j.kernel.impl.store.record.DynamicRecord record) throws Exception
			 protected internal override object transform( DynamicRecord record )
			 {
				  return record.InUse() && record.StartRecord ? _storage.loadSingleSchemaRule(record.Id) : null;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void dumpNodeStore(org.neo4j.kernel.impl.store.NeoStores neoStores, IdRange[] ids) throws Exception
		 private static void DumpNodeStore( NeoStores neoStores, IdRange[] ids )
		 {
			  new DumpStoreAnonymousInnerClass3()
			  .dump( neoStores.NodeStore, ids );
		 }

		 private class DumpStoreAnonymousInnerClass3 : DumpStore<NodeRecord, NodeStore>
		 {
			 public DumpStoreAnonymousInnerClass3() : base(System.out)
			 {
			 }

			 protected internal override object transform( NodeRecord record )
			 {
				  return record.InUse() ? record : "";
			 }
		 }

		 private readonly PrintStream @out;
		 private readonly HexPrinter _printer;

		 protected internal DumpStore( PrintStream @out )
		 {
			  this.@out = @out;
			  this._printer = ( new HexPrinter( @out ) ).withBytesGroupingFormat( 16, 4, "  " ).withLineNumberDigits( 8 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void dump(STORE store, IdRange[] ids) throws Exception
		 public void Dump( STORE store, IdRange[] ids )
		 {
			  int size = store.RecordSize;
			  long highId = store.HighId;
			  @out.println( "store.getRecordSize() = " + size );
			  @out.println( "store.getHighId() = " + highId );
			  @out.println( "<dump>" );
			  long used = 0;

			  if ( ids == null )
			  {
					for ( long id = 0; id < highId; id++ )
					{
						 bool inUse = DumpRecord( store, size, id );

						 if ( inUse )
						 {
							  used++;
						 }
					}
			  }
			  else
			  {
					foreach ( IdRange range in ids )
					{
						 for ( long id = range.StartId; id < range.EndId; id++ )
						 {
							  DumpRecord( store, size, id );
						 }
					}
			  }
			  @out.println( "</dump>" );

			  if ( ids == null )
			  {
					@out.printf( "used = %s / highId = %s (%.2f%%)%n", used, highId, used * 100.0 / highId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean dumpRecord(STORE store, int size, long id) throws Exception
		 private bool DumpRecord( STORE store, int size, long id )
		 {
			  RECORD record = store.getRecord( id, store.newRecord(), FORCE );
			  object transform = transform( record );
			  if ( transform != null )
			  {
					if ( !"".Equals( transform ) )
					{
						 @out.println( transform );
					}
			  }
			  else
			  {
					@out.print( record );
					// TODO Hmm, please don't do this
					sbyte[] rawRecord = ( ( CommonAbstractStore )store ).getRawRecordData( id );
					DumpHex( record, ByteBuffer.wrap( rawRecord ), id, size );
			  }
			  return record.inUse();
		 }

		 internal virtual void DumpHex( RECORD record, ByteBuffer buffer, long id, int size )
		 {
			  _printer.withLineNumberOffset( id * size );
			  if ( record.inUse() )
			  {
					_printer.append( buffer );
			  }
			  else if ( AllZero( buffer ) )
			  {
					@out.printf( ": all zeros @ 0x%x - 0x%x", id * size, ( id + 1 ) * size );
			  }
			  else
			  {
					_printer.append( buffer );
			  }
			  @out.printf( "%n" );
		 }

		 private bool AllZero( ByteBuffer buffer )
		 {
			  for ( int i = 0; i < buffer.limit(); i++ )
			  {
					if ( buffer.get( i ) != 0 )
					{
						 return false;
					}
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Object transform(RECORD record) throws Exception
		 protected internal virtual object Transform( RECORD record )
		 {
			  return record.inUse() ? record : null;
		 }
	}

}