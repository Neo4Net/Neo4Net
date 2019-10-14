using System;
using System.Collections.Generic;
using System.Text;

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

	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using Neo4Net.Kernel.impl.store;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Headers = Neo4Net.Kernel.impl.store.kvstore.Headers;
	using MetadataVisitor = Neo4Net.Kernel.impl.store.kvstore.MetadataVisitor;
	using ReadableBuffer = Neo4Net.Kernel.impl.store.kvstore.ReadableBuffer;
	using UnknownKey = Neo4Net.Kernel.impl.store.kvstore.UnknownKey;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	/// <summary>
	/// Tool that will dump content of count store content into a simple string representation for further analysis.
	/// </summary>
	public class DumpCountsStore : CountsVisitor, MetadataVisitor, UnknownKey.Visitor
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String... args) throws Exception
		 public static void Main( params string[] args )
		 {
			  if ( args.Length != 1 )
			  {
					Console.Error.WriteLine( "Expecting exactly one argument describing the path to the store" );
					Environment.Exit( 1 );
			  }
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					DumpCountsStoreConflict( fileSystem, new File( args[0] ), System.out );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void dumpCountsStore(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File path, java.io.PrintStream out) throws Exception
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static void DumpCountsStoreConflict( FileSystemAbstraction fs, File path, PrintStream @out )
		 {
			  using ( JobScheduler jobScheduler = createInitializedScheduler(), PageCache pages = createPageCache(fs, jobScheduler), Lifespan life = new Lifespan() )
			  {
					NullLogProvider logProvider = NullLogProvider.Instance;
					Config config = Config.defaults();
					if ( fs.IsDirectory( path ) )
					{
						 DatabaseLayout databaseLayout = DatabaseLayout.of( path );
						 StoreFactory factory = new StoreFactory( databaseLayout, Config.defaults(), new DefaultIdGeneratorFactory(fs), pages, fs, logProvider, EmptyVersionContextSupplier.EMPTY );

						 NeoStores neoStores = factory.OpenAllNeoStores();
						 SchemaStorage schemaStorage = new SchemaStorage( neoStores.SchemaStore );
						 neoStores.Counts.accept( new DumpCountsStore( @out, neoStores, schemaStorage ) );
					}
					else
					{
						 VisitableCountsTracker tracker = new VisitableCountsTracker( logProvider, fs, pages, config, DatabaseLayout.of( path.ParentFile ) );
						 if ( fs.FileExists( path ) )
						 {
							  tracker.VisitFile( path, new DumpCountsStore( @out ) );
						 }
						 else
						 {
							  life.Add( tracker ).accept( new DumpCountsStore( @out ) );
						 }
					}
			  }
		 }

		 internal DumpCountsStore( PrintStream @out ) : this( @out, Collections.emptyMap(), Collections.emptyList(), Collections.emptyList(), Collections.emptyList() )
		 {
		 }

		 internal DumpCountsStore( PrintStream @out, NeoStores neoStores, SchemaStorage schemaStorage ) : this( @out, GetAllIndexesFrom( schemaStorage ), AllTokensFrom( neoStores.LabelTokenStore ), AllTokensFrom( neoStores.RelationshipTypeTokenStore ), AllTokensFrom( neoStores.PropertyKeyTokenStore ) )
		 {
		 }

		 private readonly PrintStream @out;
		 private readonly IDictionary<long, IndexDescriptor> _indexes;
		 private readonly IList<NamedToken> _labels;
		 private readonly IList<NamedToken> _relationshipTypes;
		 private readonly IList<NamedToken> _propertyKeys;

		 private DumpCountsStore( PrintStream @out, IDictionary<long, IndexDescriptor> indexes, IList<NamedToken> labels, IList<NamedToken> relationshipTypes, IList<NamedToken> propertyKeys )
		 {
			  this.@out = @out;
			  this._indexes = indexes;
			  this._labels = labels;
			  this._relationshipTypes = relationshipTypes;
			  this._propertyKeys = propertyKeys;
		 }

		 public override void VisitMetadata( File file, Headers headers, int entryCount )
		 {
			  @out.printf( "Counts Store:\t%s%n", file );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.impl.store.kvstore.HeaderField<?> headerField : headers.fields())
			  foreach ( HeaderField<object> headerField in headers.Fields() )
			  {
					@out.printf( "%s:\t%s%n", headerField.ToString(), headers.Get(headerField) );
			  }
			  @out.printf( "\tentries:\t%d%n", entryCount );
			  @out.println( "Entries:" );
		 }

		 public override void VisitNodeCount( int labelId, long count )
		 {
			  @out.printf( "\tNode[(%s)]:\t%d%n", Labels( new int[]{ labelId } ), count );
		 }

		 public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
		 {
			  @out.printf( "\tRelationship[(%s)-%s->(%s)]:\t%d%n", Labels( new int[]{ startLabelId } ), RelationshipType( typeId ), Labels( new int[]{ endLabelId } ), count );
		 }

		 public override void VisitIndexStatistics( long indexId, long updates, long size )
		 {
			  SchemaDescriptor schema = _indexes[indexId].schema();
			  string tokenIds;
			  switch ( Schema.entityType() )
			  {
			  case NODE:
					tokenIds = Labels( Schema.EntityTokenIds );
					break;
			  case RELATIONSHIP:
					tokenIds = RelationshipTypes( Schema.EntityTokenIds );
					break;
			  default:
					throw new System.InvalidOperationException( "Indexing is not supported for EntityType: " + Schema.entityType() );
			  }
			  @out.printf( "\tIndexStatistics[(%s {%s})]:\tupdates=%d, size=%d%n", tokenIds, PropertyKeys( Schema.PropertyIds ), updates, size );
		 }

		 public override void VisitIndexSample( long indexId, long unique, long size )
		 {
			  SchemaDescriptor schema = _indexes[indexId].schema();
			  string tokenIds;
			  switch ( Schema.entityType() )
			  {
			  case NODE:
					tokenIds = Labels( Schema.EntityTokenIds );
					break;
			  case RELATIONSHIP:
					tokenIds = RelationshipTypes( Schema.EntityTokenIds );
					break;
			  default:
					throw new System.InvalidOperationException( "Indexing is not supported for EntityType: " + Schema.entityType() );
			  }
			  @out.printf( "\tIndexSample[(%s {%s})]:\tunique=%d, size=%d%n", tokenIds, PropertyKeys( Schema.PropertyIds ), unique, size );
		 }

		 public override bool VisitUnknownKey( ReadableBuffer key, ReadableBuffer value )
		 {
			  @out.printf( "\t%s:\t%s%n", key, value );
			  return true;
		 }

		 private string Labels( int[] ids )
		 {
			  if ( ids.Length == 1 )
			  {
					if ( ids[0] == StatementConstants.ANY_LABEL )
					{
						 return "";
					}
			  }
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					if ( i > 0 )
					{
						 builder.Append( "," );
					}
					Token( builder, _labels, ":", "label", ids[i] ).ToString();
			  }
			  return builder.ToString();
		 }

		 private string PropertyKeys( int[] ids )
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					if ( i > 0 )
					{
						 builder.Append( "," );
					}
					Token( builder, _propertyKeys, "", "key", ids[i] );
			  }
			  return builder.ToString();
		 }

		 private string RelationshipTypes( int[] ids )
		 {
			  if ( ids.Length == 1 )
			  {
					if ( ids[0] == StatementConstants.ANY_LABEL )
					{
						 return "";
					}
			  }
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					if ( i > 0 )
					{
						 builder.Append( "," );
					}
					return Token( ( new StringBuilder() ).Append('['), _relationshipTypes, ":", "type", i ).Append(']').ToString();
			  }
			  return builder.ToString();
		 }

		 private string RelationshipType( int id )
		 {
			  if ( id == StatementConstants.ANY_RELATIONSHIP_TYPE )
			  {
					return "";
			  }
			  return Token( ( new StringBuilder() ).Append('['), _relationshipTypes, ":", "type", id ).Append(']').ToString();
		 }

		 private static StringBuilder Token( StringBuilder result, IList<NamedToken> tokens, string pre, string handle, int id )
		 {
			  NamedToken token = null;
			  // search backwards for the token
			  for ( int i = ( id < tokens.Count ) ? id : tokens.Count - 1; i >= 0; i-- )
			  {
					token = tokens[i];
					if ( token.Id() == id )
					{
						 break; // found
					}
					if ( token.Id() < id )
					{
						 token = null; // not found
						 break;
					}
			  }
			  if ( token != null )
			  {
					string name = token.Name();
					result.Append( pre ).Append( name ).Append( " [" ).Append( handle ).Append( "Id=" ).Append( token.Id() ).Append(']');
			  }
			  else
			  {
					result.Append( handle ).Append( "Id=" ).Append( id );
			  }
			  return result;
		 }

		 private static IList<NamedToken> AllTokensFrom<T1>( TokenStore<T1> store )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (org.neo4j.kernel.impl.store.TokenStore<?> tokens = store)
			  using ( TokenStore<object> tokens = store )
			  {
					return tokens.Tokens;
			  }
		 }

		 private static IDictionary<long, IndexDescriptor> GetAllIndexesFrom( SchemaStorage storage )
		 {
			  Dictionary<long, IndexDescriptor> indexes = new Dictionary<long, IndexDescriptor>();
			  IEnumerator<StoreIndexDescriptor> indexRules = storage.IndexesGetAll();
			  while ( indexRules.MoveNext() )
			  {
					StoreIndexDescriptor rule = indexRules.Current;
					indexes[rule.Id] = rule;
			  }
			  return indexes;
		 }

		 private class VisitableCountsTracker : CountsTracker
		 {

			  internal VisitableCountsTracker( LogProvider logProvider, FileSystemAbstraction fs, PageCache pages, Config config, DatabaseLayout databaseLayout ) : base( logProvider, fs, pages, config, databaseLayout, EmptyVersionContextSupplier.EMPTY )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitFile(java.io.File path, org.neo4j.kernel.impl.api.CountsVisitor visitor) throws java.io.IOException
			  public override void VisitFile( File path, CountsVisitor visitor )
			  {
					base.VisitFile( path, visitor );
			  }
		 }
	}

}