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
namespace Neo4Net.Jmx.impl
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogVersionVisitor = Neo4Net.Kernel.impl.transaction.log.files.LogVersionVisitor;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.COUNTS_STORE_A;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.COUNTS_STORE_B;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.LABEL_TOKEN_NAMES_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.LABEL_TOKEN_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.NODE_LABEL_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.NODE_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.PROPERTY_ARRAY_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.PROPERTY_KEY_TOKEN_NAMES_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.PROPERTY_KEY_TOKEN_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.PROPERTY_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.PROPERTY_STRING_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_GROUP_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_TYPE_TOKEN_NAMES_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_TYPE_TOKEN_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.layout.DatabaseFile.SCHEMA_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.jmx.impl.ThrottlingBeanSnapshotProxy.newThrottlingBeanSnapshotProxy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) @Deprecated public final class StoreSizeBean extends ManagementBeanProvider
	[Obsolete]
	public sealed class StoreSizeBean : ManagementBeanProvider
	{
		 private const long UPDATE_INTERVAL = 60000;
		 private static readonly StoreSize NO_STORE_SIZE = new StoreSizeAnonymousInnerClass();

		 private class StoreSizeAnonymousInnerClass : StoreSize
		 {
			 public long TransactionLogsSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long NodeStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long RelationshipStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long PropertyStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long StringStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long ArrayStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long LabelStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long CountStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long SchemaStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long IndexStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 public long TotalStoreSize
			 {
				 get
				 {
					  return 0;
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public StoreSizeBean()
		 public StoreSizeBean() : base(typeof(StoreSize))
		 {
		 }

		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return CreateBean( management, false, UPDATE_INTERVAL, Clock.systemUTC() );
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
		 {
			  return CreateBean( management, true, UPDATE_INTERVAL, Clock.systemUTC() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting static StoreSizeMBean createBean(ManagementData management, boolean isMxBean, long updateInterval, java.time.Clock clock)
		 internal static StoreSizeMBean CreateBean( ManagementData management, bool isMxBean, long updateInterval, Clock clock )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StoreSizeMBean bean = new StoreSizeMBean(management, isMxBean, updateInterval, clock);
			  StoreSizeMBean bean = new StoreSizeMBean( management, isMxBean, updateInterval, clock );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.state.DataSourceManager dataSourceManager = management.getKernelData().getDataSourceManager();
			  DataSourceManager dataSourceManager = management.KernelData.DataSourceManager;
			  dataSourceManager.AddListener( bean );
			  return bean;
		 }

		 private class StoreSizeMBean : Neo4NetMBean, StoreSize, DataSourceManager.Listener
		 {
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly long UpdateInterval;
			  internal readonly Clock Clock;
			  internal volatile StoreSize Delegate = NO_STORE_SIZE;

			  internal StoreSizeMBean( ManagementData management, bool isMXBean, long updateInterval, Clock clock ) : base( management, isMXBean )
			  {
					this.Fs = management.KernelData.FilesystemAbstraction;
					this.UpdateInterval = updateInterval;
					this.Clock = clock;
			  }

			  public override void Registered( NeoStoreDataSource ds )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StoreSizeProvider dataProvider = new StoreSizeProvider(fs, ds);
					StoreSizeProvider dataProvider = new StoreSizeProvider( Fs, ds );
					this.Delegate = newThrottlingBeanSnapshotProxy( typeof( StoreSize ), dataProvider, UpdateInterval, Clock );
			  }

			  public override void Unregistered( NeoStoreDataSource ds )
			  {
					this.Delegate = NO_STORE_SIZE;
			  }

			  public virtual long TransactionLogsSize
			  {
				  get
				  {
						return Delegate.TransactionLogsSize;
				  }
			  }

			  public virtual long NodeStoreSize
			  {
				  get
				  {
						return Delegate.NodeStoreSize;
				  }
			  }

			  public virtual long RelationshipStoreSize
			  {
				  get
				  {
						return Delegate.RelationshipStoreSize;
				  }
			  }

			  public virtual long PropertyStoreSize
			  {
				  get
				  {
						return Delegate.PropertyStoreSize;
				  }
			  }

			  public virtual long StringStoreSize
			  {
				  get
				  {
						return Delegate.StringStoreSize;
				  }
			  }

			  public virtual long ArrayStoreSize
			  {
				  get
				  {
						return Delegate.ArrayStoreSize;
				  }
			  }

			  public virtual long LabelStoreSize
			  {
				  get
				  {
						return Delegate.LabelStoreSize;
				  }
			  }

			  public virtual long CountStoreSize
			  {
				  get
				  {
						return Delegate.CountStoreSize;
				  }
			  }

			  public virtual long SchemaStoreSize
			  {
				  get
				  {
						return Delegate.SchemaStoreSize;
				  }
			  }

			  public virtual long IndexStoreSize
			  {
				  get
				  {
						return Delegate.IndexStoreSize;
				  }
			  }

			  public virtual long TotalStoreSize
			  {
				  get
				  {
						return Delegate.TotalStoreSize;
				  }
			  }
		 }

		 private class StoreSizeProvider : StoreSize
		 {
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly LogFiles LogFiles;
			  internal readonly ExplicitIndexProvider ExplicitIndexProviderLookup;
			  internal readonly IndexProviderMap IndexProviderMap;
			  internal readonly LabelScanStore LabelScanStore;
			  internal readonly DatabaseLayout DatabaseLayout;

			  internal StoreSizeProvider( FileSystemAbstraction fs, NeoStoreDataSource ds )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.DependencyResolver deps = ds.getDependencyResolver();
					DependencyResolver deps = ds.DependencyResolver;
					this.Fs = requireNonNull( fs );
					this.LogFiles = deps.ResolveDependency( typeof( LogFiles ) );
					this.ExplicitIndexProviderLookup = deps.ResolveDependency( typeof( ExplicitIndexProvider ) );
					this.IndexProviderMap = deps.ResolveDependency( typeof( IndexProviderMap ) );
					this.LabelScanStore = deps.ResolveDependency( typeof( LabelScanStore ) );
					this.DatabaseLayout = ds.DatabaseLayout;
			  }

			  public virtual long TransactionLogsSize
			  {
				  get
				  {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final TotalSizeVersionVisitor logVersionVisitor = new TotalSizeVersionVisitor();
						TotalSizeVersionVisitor logVersionVisitor = new TotalSizeVersionVisitor( this );
						LogFiles.accept( logVersionVisitor );
						return logVersionVisitor.TotalSize;
				  }
			  }

			  public virtual long NodeStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( NODE_STORE, NODE_LABEL_STORE );
				  }
			  }

			  public virtual long RelationshipStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( RELATIONSHIP_STORE, RELATIONSHIP_GROUP_STORE, RELATIONSHIP_TYPE_TOKEN_STORE, RELATIONSHIP_TYPE_TOKEN_NAMES_STORE );
				  }
			  }

			  public virtual long PropertyStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( PROPERTY_STORE, PROPERTY_KEY_TOKEN_STORE, PROPERTY_KEY_TOKEN_NAMES_STORE );
				  }
			  }

			  public virtual long StringStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( PROPERTY_STRING_STORE );
				  }
			  }

			  public virtual long ArrayStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( PROPERTY_ARRAY_STORE );
				  }
			  }

			  public virtual long LabelStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( LABEL_TOKEN_STORE, LABEL_TOKEN_NAMES_STORE );
				  }
			  }

			  public virtual long CountStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( COUNTS_STORE_A, COUNTS_STORE_B );
				  }
			  }

			  public virtual long SchemaStoreSize
			  {
				  get
				  {
						return SizeOfStoreFiles( SCHEMA_STORE );
				  }
			  }

			  public virtual long IndexStoreSize
			  {
				  get
				  {
						long size = 0L;
   
						// Add explicit indices
						foreach ( IndexImplementation index in ExplicitIndexProviderLookup.allIndexProviders() )
						{
							 size += FileUtils.size( Fs, index.GetIndexImplementationDirectory( DatabaseLayout ) );
						}
   
						// Add schema index
						MutableLong schemaSize = new MutableLong();
						IndexProviderMap.accept(provider =>
						{
						 File rootDirectory = provider.directoryStructure().rootDirectory();
						 if ( rootDirectory != null )
						 {
							  schemaSize.add( FileUtils.size( Fs, rootDirectory ) );
						 }
						 // else this provider didn't have any persistent storage
						});
						size += schemaSize.longValue();
   
						// Add label index
						size += FileUtils.size( Fs, LabelScanStore.LabelScanStoreFile );
   
						return size;
				  }
			  }

			  public virtual long TotalStoreSize
			  {
				  get
				  {
						return FileUtils.size( Fs, DatabaseLayout.databaseDirectory() );
				  }
			  }

			  internal virtual long SizeOf( File file )
			  {
					return FileUtils.size( Fs, file );
			  }

			  private class TotalSizeVersionVisitor : LogVersionVisitor
			  {
				  private readonly StoreSizeBean.StoreSizeProvider _outerInstance;

				  public TotalSizeVersionVisitor( StoreSizeBean.StoreSizeProvider outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
					internal long TotalSizeConflict;

					internal virtual long TotalSize
					{
						get
						{
							 return TotalSizeConflict;
						}
					}

					public override void Visit( File file, long logVersion )
					{
						 TotalSizeConflict += FileUtils.size( outerInstance.Fs, file );
					}
			  }

			  /// <summary>
			  /// Count the total file size, including id files, of <seealso cref="DatabaseFile"/>s.
			  /// Missing files will be counted as 0 bytes.
			  /// </summary>
			  /// <param name="databaseFiles"> the store types to count </param>
			  /// <returns> the total size in bytes of the files </returns>
			  internal virtual long SizeOfStoreFiles( params DatabaseFile[] databaseFiles )
			  {
					long size = 0L;
					foreach ( DatabaseFile store in databaseFiles )
					{
						 size += DatabaseLayout.file( store ).mapToLong( this.sizeOf ).sum();
						 size += DatabaseLayout.idFile( store ).map( this.sizeOf ).orElse( 0L );
					}
					return size;
			  }
		 }
	}

}