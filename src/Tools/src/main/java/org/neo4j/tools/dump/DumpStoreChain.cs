using System;
using System.Collections.Generic;

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

	using Args = Neo4Net.Helpers.Args;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	/// <summary>
	/// Tool to dump content of <seealso cref="StoreType.NODE"/>, <seealso cref="StoreType.PROPERTY"/>, <seealso cref="StoreType.RELATIONSHIP"/> stores
	/// into readable format. </summary>
	/// @param <RECORD> record type to dump </param>
	public abstract class DumpStoreChain<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private const string REVERSE = "reverse";
		 private const string NODE = "node";
		 private const string FIRST = "first";
		 private const string RELS = "relationships";
		 private const string PROPS = "properties";
		 private const string RELSTORE = "neostore.relationshipstore.db";
		 private const string PROPSTORE = "neostore.propertystore.db";
		 private const string NODESTORE = "neostore.nodestore.db";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String... args) throws Exception
		 public static void Main( params string[] args )
		 {
			  Args arguments = Args.withFlags( REVERSE, RELS, PROPS ).parse( args );
			  IList<string> orphans = arguments.Orphans();
			  if ( orphans.Count != 1 )
			  {
					throw InvalidUsage( "no store file given" );
			  }
			  File storeFile = new File( orphans[0] );
			  DumpStoreChain tool;
			  if ( storeFile.Directory )
			  {
					VerifyFilesExists( new File( storeFile, NODESTORE ), new File( storeFile, RELSTORE ), new File( storeFile, PROPSTORE ) );
					tool = ChainForNode( arguments );
			  }
			  else
			  {
					VerifyFilesExists( storeFile );
					if ( RELSTORE.Equals( storeFile.Name ) )
					{
						 tool = RelationshipChain( arguments );
					}
					else if ( PROPSTORE.Equals( storeFile.Name ) )
					{
						 tool = PropertyChain( arguments );
					}
					else
					{
						 throw InvalidUsage( "not a chain store: " + storeFile.Name );
					}
			  }
			  tool.Dump( DatabaseLayout.of( storeFile ) );
		 }

		 internal long FirstRecord;

		 private DumpStoreChain( long firstRecord )
		 {
			  this.FirstRecord = firstRecord;
		 }

		 private static LogProvider LogProvider()
		 {
			  return Boolean.getBoolean( "logger" ) ? FormattedLogProvider.toOutputStream( System.out ) : NullLogProvider.Instance;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dump(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 internal virtual void Dump( DatabaseLayout databaseLayout )
		 {
			  using ( DefaultFileSystemAbstraction fs = new DefaultFileSystemAbstraction(), PageCache pageCache = createPageCache(fs, createInitialisedScheduler()) )
			  {
					DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
					Config config = Config.defaults();
					StoreFactory storeFactory = new StoreFactory( databaseLayout, config, idGeneratorFactory, pageCache, fs, LogProvider(), EmptyVersionContextSupplier.EMPTY );

					using ( NeoStores neoStores = storeFactory.OpenNeoStores( StoreTypes ) )
					{
						 RecordStore<RECORD> store = store( neoStores );
						 RECORD record = store.NewRecord();
						 for ( long next = FirstRecord; next != -1; )
						 {
							  store.GetRecord( next, record, RecordLoad.FORCE );
							  Console.WriteLine( record );
							  next = next( record );
						 }
					}
			  }
		 }

		 private static StoreType[] StoreTypes
		 {
			 get
			 {
				  return new StoreType[]{ StoreType.NODE, StoreType.PROPERTY, StoreType.RELATIONSHIP };
			 }
		 }

		 internal abstract long Next( RECORD record );

		 internal abstract RecordStore<RECORD> Store( NeoStores neoStores );

		 private static DumpStoreChain PropertyChain( Args args )
		 {
			  bool reverse = VerifyParametersAndCheckReverse( args, FIRST );
			  return new DumpPropertyChain( long.Parse( args.Get( FIRST, null ) ), reverse );
		 }

		 private static DumpStoreChain RelationshipChain( Args args )
		 {
			  bool reverse = VerifyParametersAndCheckReverse( args, FIRST, NODE );
			  long node = long.Parse( args.Get( NODE, null ) );
			  return new DumpRelationshipChain( long.Parse( args.Get( FIRST, null ) ), node, reverse );
		 }

		 private static DumpStoreChain ChainForNode( Args args )
		 {
			  ISet<string> kwArgs = args.AsMap().Keys;
			  VerifyParameters( kwArgs, kwArgs.Contains( RELS ) ? RELS : PROPS, NODE );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long node = long.Parse(args.get(NODE, null));
			  long node = long.Parse( args.Get( NODE, null ) );
			  if ( args.GetBoolean( RELS, false, true ).Value )
			  {
					return new DumpRelationshipChainAnonymousInnerClass( node );
			  }
			  else if ( args.GetBoolean( PROPS, false, true ).Value )
			  {
					return new DumpPropertyChainAnonymousInnerClass( node );
			  }
			  else
			  {
					throw InvalidUsage( string.Format( "Must be either -{0} or -{1}", RELS, PROPS ) );
			  }
		 }

		 private class DumpRelationshipChainAnonymousInnerClass : DumpRelationshipChain
		 {
			 private long _node;

			 public DumpRelationshipChainAnonymousInnerClass( long node ) : base( -1, node, false )
			 {
				 this._node = node;
			 }

			 internal override RelationshipStore store( NeoStores neoStores )
			 {
				  NodeRecord nodeRecord = nodeRecord( neoStores, _node );
				  outerInstance.firstRecord = nodeRecord.Dense ? -1 : nodeRecord.NextRel;
				  return base.store( neoStores );
			 }
		 }

		 private class DumpPropertyChainAnonymousInnerClass : DumpPropertyChain
		 {
			 private long _node;

			 public DumpPropertyChainAnonymousInnerClass( long node ) : base( -1, false )
			 {
				 this._node = node;
			 }

			 internal override PropertyStore store( NeoStores neoStores )
			 {
				  outerInstance.firstRecord = NodeRecord( neoStores, _node ).NextProp;
				  return base.store( neoStores );
			 }
		 }

		 private static NodeRecord NodeRecord( NeoStores neoStores, long id )
		 {
			  NodeStore nodeStore = neoStores.NodeStore;
			  return nodeStore.GetRecord( id, nodeStore.NewRecord(), FORCE );
		 }

		 private static void VerifyFilesExists( params File[] files )
		 {
			  foreach ( File file in files )
			  {
					if ( !file.File )
					{
						 throw InvalidUsage( file + " does not exist" );
					}
			  }
		 }

		 private static bool VerifyParametersAndCheckReverse( Args args, params string[] parameters )
		 {
			  ISet<string> kwArgs = args.AsMap().Keys;
			  if ( kwArgs.Contains( REVERSE ) )
			  {
					parameters = Arrays.copyOf( parameters, parameters.Length + 1 );
					parameters[parameters.Length - 1] = REVERSE;
			  }
			  VerifyParameters( kwArgs, parameters );
			  return args.GetBoolean( REVERSE, false, true ).Value;
		 }

		 private static void VerifyParameters( ISet<string> args, params string[] parameters )
		 {
			  if ( args.Count != parameters.Length )
			  {
					throw InvalidUsage( "accepted/required parameters: " + Arrays.ToString( parameters ) );
			  }
			  foreach ( string parameter in parameters )
			  {
					if ( !args.Contains( parameter ) )
					{
						 throw InvalidUsage( "accepted/required parameters: " + Arrays.ToString( parameters ) );
					}
			  }
		 }

		 private static Exception InvalidUsage( string message )
		 {
			  Console.Error.WriteLine( "invalid usage: " + message );
			  Environment.Exit( 1 );
			  return null;
		 }

		 private class DumpPropertyChain : DumpStoreChain<PropertyRecord>
		 {
			  internal readonly bool Reverse;

			  internal DumpPropertyChain( long first, bool reverse ) : base( first )
			  {
					this.Reverse = reverse;
			  }

			  internal override PropertyStore Store( NeoStores neoStores )
			  {
					return neoStores.PropertyStore;
			  }

			  internal override long Next( PropertyRecord record )
			  {
					return Reverse ? record.PrevProp : record.NextProp;
			  }
		 }

		 private class DumpRelationshipChain : DumpStoreChain<RelationshipRecord>
		 {
			  internal readonly long Node;
			  internal readonly bool Reverse;

			  internal DumpRelationshipChain( long first, long node, bool reverse ) : base( first )
			  {
					this.Node = node;
					this.Reverse = reverse;
			  }

			  internal override RelationshipStore Store( NeoStores neoStores )
			  {
					return neoStores.RelationshipStore;
			  }

			  internal override long Next( RelationshipRecord record )
			  {
					if ( record.FirstNode == Node )
					{
						 return Reverse ? record.FirstPrevRel : record.FirstNextRel;
					}
					else if ( record.SecondNode == Node )
					{
						 return Reverse ? record.SecondPrevRel : record.SecondNextRel;
					}
					else
					{
						 return -1;
					}
			  }
		 }
	}

}