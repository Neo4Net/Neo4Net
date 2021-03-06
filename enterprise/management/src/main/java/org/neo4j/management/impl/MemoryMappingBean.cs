﻿using System;

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
namespace Org.Neo4j.management.impl
{

	using Service = Org.Neo4j.Helpers.Service;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Service.Implementation(ManagementBeanProvider.class) public final class MemoryMappingBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	[Obsolete]
	public sealed class MemoryMappingBean : ManagementBeanProvider
	{
		 public MemoryMappingBean() : base(typeof(MemoryMapping))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new MemoryMappingImpl( management );
		 }

		 protected internal override Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  return new MemoryMappingImpl( management, true );
		 }

		 private class MemoryMappingImpl : Neo4jMBean, MemoryMapping
		 {
			  internal readonly NeoStoreDataSource Datasource;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: MemoryMappingImpl(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
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