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
namespace Neo4Net.Jmx.impl
{

	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) @Deprecated public final class StoreFileBean extends ManagementBeanProvider
	[Obsolete]
	public sealed class StoreFileBean : ManagementBeanProvider
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public StoreFileBean()
		 public StoreFileBean() : base(typeof(StoreFile))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4jMBean createMBean(ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new StoreFileImpl( management );
		 }

		 internal class StoreFileImpl : Neo4jMBean, StoreFile
		 {
			  internal File DatabaseDirectory;
			  internal LogFiles LogFiles;
			  internal FileSystemAbstraction Fs;
			  internal DatabaseLayout DatabaseLayout;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreFileImpl(ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal StoreFileImpl( ManagementData management ) : base( management )
			  {

					Fs = management.KernelData.FilesystemAbstraction;

					DataSourceManager dataSourceManager = management.KernelData.DataSourceManager;
					dataSourceManager.AddListener( new ListenerAnonymousInnerClass( this ) );
			  }

			  private class ListenerAnonymousInnerClass : DataSourceManager.Listener
			  {
				  private readonly StoreFileImpl _outerInstance;

				  public ListenerAnonymousInnerClass( StoreFileImpl outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public void registered( NeoStoreDataSource ds )
				  {
						_outerInstance.logFiles = resolveDependency( ds, typeof( LogFiles ) );
						_outerInstance.databaseLayout = ds.DatabaseLayout;
						_outerInstance.databaseDirectory = resolveDatabaseDirectory();
				  }

				  private T resolveDependency<T>( NeoStoreDataSource ds, Type clazz )
				  {
						  clazz = typeof( T );
						return ds.DependencyResolver.resolveDependency( clazz );
				  }

				  public void unregistered( NeoStoreDataSource ds )
				  {
						_outerInstance.logFiles = null;
						_outerInstance.databaseDirectory = null;
						_outerInstance.databaseLayout = null;
				  }

				  private File resolveDatabaseDirectory()
				  {
						return _outerInstance.databaseLayout.databaseDirectory();
				  }
			  }

			  public virtual long TotalStoreSize
			  {
				  get
				  {
						return DatabaseDirectory == null ? 0 : FileUtils.size( Fs, DatabaseDirectory );
				  }
			  }

			  public virtual long LogicalLogSize
			  {
				  get
				  {
						return LogFiles == null ? 0 : FileUtils.size( Fs, LogFiles.HighestLogFile );
				  }
			  }

			  public virtual long ArrayStoreSize
			  {
				  get
				  {
						return SizeOf( DatabaseLayout.propertyArrayStore() );
				  }
			  }

			  public virtual long NodeStoreSize
			  {
				  get
				  {
						return SizeOf( DatabaseLayout.nodeStore() );
				  }
			  }

			  public virtual long PropertyStoreSize
			  {
				  get
				  {
						return SizeOf( DatabaseLayout.propertyStore() );
				  }
			  }

			  public virtual long RelationshipStoreSize
			  {
				  get
				  {
						return SizeOf( DatabaseLayout.relationshipStore() );
				  }
			  }

			  public virtual long StringStoreSize
			  {
				  get
				  {
						return SizeOf( DatabaseLayout.propertyStringStore() );
				  }
			  }

			  internal virtual long SizeOf( File file )
			  {
					return DatabaseDirectory == null ? 0 : FileUtils.size( Fs, file );
			  }
		 }
	}

}