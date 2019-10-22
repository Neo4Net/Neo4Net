using System;

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
namespace Neo4Net.management.impl
{

	using Service = Neo4Net.Helpers.Service;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Service.Implementation(ManagementBeanProvider.class) public final class MemoryMappingBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	[Obsolete]
	public sealed class MemoryMappingBean : ManagementBeanProvider
	{
		 public MemoryMappingBean() : base(typeof(MemoryMapping))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.jmx.impl.Neo4NetMBean createMBean(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new MemoryMappingImpl( management );
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
		 {
			  return new MemoryMappingImpl( management, true );
		 }

		 private class MemoryMappingImpl : Neo4NetMBean, MemoryMapping
		 {
			  internal readonly NeoStoreDataSource Datasource;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: MemoryMappingImpl(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal MemoryMappingImpl( ManagementData management ) : base( management )
			  {
					this.Datasource = NeoDataSource( management );
			  }

			  internal virtual NeoStoreDataSource NeoDataSource( ManagementData management )
			  {
					return management.KernelData.DataSourceManager.DataSource;
			  }

			  internal MemoryMappingImpl( ManagementData management, bool isMxBean ) : base( management, isMxBean )
			  {
					this.Datasource = NeoDataSource( management );
			  }

			  [Obsolete]
			  public virtual WindowPoolInfo[] MemoryPools
			  {
				  get
				  {
						return GetMemoryPoolsImpl( Datasource );
				  }
			  }

			  [Obsolete]
			  public static WindowPoolInfo[] GetMemoryPoolsImpl( NeoStoreDataSource datasource )
			  {
					return new WindowPoolInfo[0];
			  }
		 }
	}

}