using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.management
{

	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using BranchedStore = Neo4Net.management.BranchedStore;
	using BranchedStoreInfo = Neo4Net.management.BranchedStoreInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.storecopy.StoreUtil.getBranchedDataRootDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class BranchedStoreBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	public sealed class BranchedStoreBean : ManagementBeanProvider
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public BranchedStoreBean()
		 public BranchedStoreBean() : base(typeof(BranchedStore))
		 {
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
		 {
			  if ( !IsHA( management ) )
			  {
					return null;
			  }
			  return new BranchedStoreImpl( management, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.jmx.impl.Neo4NetMBean createMBean(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
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

		 private class BranchedStoreImpl : Neo4NetMBean, BranchedStore
		 {
			  internal readonly FileSystemAbstraction FileSystem;
			  internal readonly File StorePath;
			  internal readonly PageCache PageCache;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BranchedStoreImpl(final org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  internal BranchedStoreImpl( ManagementData management ) : base( management )
			  {
					FileSystem = GetFilesystem( management );
					StorePath = GetStorePath( management );
					PageCache = GetPageCache( management );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: BranchedStoreImpl(final org.Neo4Net.jmx.impl.ManagementData management, boolean isMXBean)
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
//ORIGINAL LINE: final java.io.File neoStoreFile = org.Neo4Net.io.layout.DatabaseLayout.of(branchedDatabase).metadataStore();
						 File neoStoreFile = DatabaseLayout.of( branchedDatabase ).metadataStore();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long txId = org.Neo4Net.kernel.impl.store.MetaDataStore.getRecord(pageCache, neoStoreFile, org.Neo4Net.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_ID);
						 long txId = MetaDataStore.getRecord( PageCache, neoStoreFile, MetaDataStore.Position.LAST_TRANSACTION_ID );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = long.Parse(branchedDatabase.getName());
						 long timestamp = long.Parse( branchedDatabase.Name );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long branchedStoreSize = org.Neo4Net.io.fs.FileUtils.size(fileSystem, branchedDatabase);
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