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

	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

	[Obsolete]
	public class KernelBean : Neo4NetMBean, Kernel
	{
		 private readonly long _kernelStartTime;
		 private readonly string _kernelVersion;
		 private readonly ObjectName _query;
		 private readonly string _instanceId;

		 private bool _isReadOnly;
		 private long _storeCreationDate = -1;
		 private long _storeId = -1;
		 private string _databaseName;
		 private long _storeLogVersion;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KernelBean(Neo4Net.kernel.internal.KernelData kernel, Neo4Net.kernel.impl.transaction.state.DataSourceManager dataSourceManager, ManagementSupport support) throws javax.management.NotCompliantMBeanException
		 internal KernelBean( KernelData kernel, DataSourceManager dataSourceManager, ManagementSupport support ) : base( typeof( Kernel ), kernel, support )
		 {
			  dataSourceManager.AddListener( new DataSourceInfo( this ) );
			  this._kernelVersion = kernel.Version().ToString();
			  this._instanceId = kernel.InstanceId();
			  this._query = support.CreateMBeanQuery( _instanceId );

			  _kernelStartTime = ( DateTime.Now ).Ticks;
		 }

		 internal virtual string InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual ObjectName MBeanQuery
		 {
			 get
			 {
				  return _query;
			 }
		 }

		 public virtual DateTime KernelStartTime
		 {
			 get
			 {
				  return new DateTime( _kernelStartTime );
			 }
		 }

		 public virtual DateTime StoreCreationDate
		 {
			 get
			 {
				  return new DateTime( _storeCreationDate );
			 }
		 }

		 public virtual string StoreId
		 {
			 get
			 {
				  return _storeId.ToString( "x" );
			 }
		 }

		 public virtual long StoreLogVersion
		 {
			 get
			 {
				  return _storeLogVersion;
			 }
		 }

		 public virtual string KernelVersion
		 {
			 get
			 {
				  return _kernelVersion;
			 }
		 }

		 public virtual bool ReadOnly
		 {
			 get
			 {
				  return _isReadOnly;
			 }
		 }

		 public virtual string DatabaseName
		 {
			 get
			 {
				  return _databaseName;
			 }
		 }

		 private class DataSourceInfo : DataSourceManager.Listener
		 {
			 private readonly KernelBean _outerInstance;

			 public DataSourceInfo( KernelBean outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Registered( NeoStoreDataSource ds )
			  {
					StoreId id = ds.StoreId;
					outerInstance.storeLogVersion = ds.DependencyResolver.resolveDependency( typeof( LogVersionRepository ) ).CurrentLogVersion;
					outerInstance.storeCreationDate = id.CreationTime;
					outerInstance.isReadOnly = ds.ReadOnly;
					outerInstance.storeId = id.RandomId;
					outerInstance.databaseName = ds.DatabaseName;
			  }

			  public override void Unregistered( NeoStoreDataSource ds )
			  {
					outerInstance.storeCreationDate = -1;
					outerInstance.storeLogVersion = -1;
					outerInstance.isReadOnly = false;
					outerInstance.storeId = -1;
			  }
		 }
	}

}