using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Kernel.ha.management
{

	using Service = Org.Neo4j.Helpers.Service;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using Position = Org.Neo4j.Kernel.impl.store.MetaDataStore.Position;
	using BranchedStore = Org.Neo4j.management.BranchedStore;
	using BranchedStoreInfo = Org.Neo4j.management.BranchedStoreInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.StoreUtil.getBranchedDataRootDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class BranchedStoreBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	public sealed class BranchedStoreBean : ManagementBeanProvider
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public BranchedStoreBean()
		 public BranchedStoreBean() : base(typeof(BranchedStore))
		 {
		 }

		 protected internal override Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  if ( !IsHA( management ) )
			  {
					return null;
			  }
			  return new BranchedStoreImpl( management, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  if ( !IsHA( management ) )
			  {
					return null;
			  }
			  return new BranchedStoreImpl( management );
		 }

		 private static bool IsHA( ManagementData management )
		 {
			  return OperationalMode.ha == management.ResolveDependency( typeof( DatabaseInfo ) ).operationalMode;
		 }

		 private class BranchedStoreImpl : Neo4jMBean, BranchedStore
		 {
			  internal readonly FileSystemAbstraction FileSystem;
			  internal readonly File StorePath;
			  internal readonly PageCache PageCache;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BranchedStoreImpl(final org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  internal BranchedStoreImpl( ManagementData management ) : base( management )
			  {
					FileSystem = GetFilesystem( management );
					StorePath = GetStorePath( management );
					PageCache = GetPageCache( management );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: BranchedStoreImpl(final org.neo4j.jmx.impl.ManagementData management, boolean isMXBean)
			  internal BranchedStoreImpl( ManagementData management, bool isMXBean ) : base( management, isMXBean )
			  {
					FileSystem = GetFilesystem( management );
					StorePath = GetStorePath( management );
					PageCache = GetPageCache( management );
			  }

			  public virtual BranchedStoreInfo[] BranchedStores
			  {
				  get
				  {
						if ( StorePath == null )
						{
							 return new BranchedStoreInfo[0];
						}
   
						IList<BranchedStoreInfo> toReturn = new LinkedList<BranchedStoreInfo>();
						foreach ( File branchDirectory in FileSystem.listFiles( getBranchedDataRootDirectory( StorePath ) ) )
						{
							 if ( !branchDirectory.Directory )
							 {
								  continue;
							 }
							 toReturn.Add( ParseBranchedStore( branchDirectory ) );
						}
						return toReturn.ToArray();
				  }
			  }

			  internal virtual BranchedStoreInfo ParseBranchedStore( File branchedDatabase )
			  {
					try
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File neoStoreFile = org.neo4j.io.layout.DatabaseLayout.of(branchedDatabase).metadataStore();
						 File neoStoreFile = DatabaseLayout.of( branchedDatabase ).metadataStore();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long txId = org.neo4j.kernel.impl.store.MetaDataStore.getRecord(pageCache, neoStoreFile, org.neo4j.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_ID);
						 long txId = MetaDataStore.getRecord( PageCache, neoStoreFile, MetaDataStore.Position.LAST_TRANSACTION_ID );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = long.Parse(branchedDatabase.getName());
						 long timestamp = long.Parse( branchedDatabase.Name );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long branchedStoreSize = org.neo4j.io.fs.FileUtils.size(fileSystem, branchedDatabase);
						 long branchedStoreSize = FileUtils.size( FileSystem, branchedDatabase );

						 return new BranchedStoreInfo( branchedDatabase.Name, txId, timestamp, branchedStoreSize );
					}
					catch ( IOException e )
					{
						 throw new System.InvalidOperationException( "Cannot read branched neostore", e );
					}
			  }

			  internal virtual PageCache GetPageCache( ManagementData management )
			  {
					return management.KernelData.PageCache;
			  }

			  internal virtual FileSystemAbstraction GetFilesystem( ManagementData management )
			  {
					return management.KernelData.FilesystemAbstraction;
			  }

			  internal virtual File GetStorePath( ManagementData management )
			  {
					return management.KernelData.StoreDir;
			  }
		 }
	}

}