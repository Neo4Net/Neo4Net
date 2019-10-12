using System;

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

	using Format = Org.Neo4j.Helpers.Format;
	using Service = Org.Neo4j.Helpers.Service;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using ClusterMemberInfo = Org.Neo4j.management.ClusterMemberInfo;
	using HighAvailability = Org.Neo4j.management.HighAvailability;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class HighAvailabilityBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	public sealed class HighAvailabilityBean : ManagementBeanProvider
	{
		 public HighAvailabilityBean() : base(typeof(HighAvailability))
		 {
		 }

		 protected internal override Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  if ( !IsHA( management ) )
			  {
					return null;
			  }
			  return new HighAvailabilityImpl( management, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  if ( !IsHA( management ) )
			  {
					return null;
			  }
			  return new HighAvailabilityImpl( management );
		 }

		 private static bool IsHA( ManagementData management )
		 {
			  return OperationalMode.ha == management.ResolveDependency( typeof( DatabaseInfo ) ).operationalMode;
		 }

		 private class HighAvailabilityImpl : Neo4jMBean, HighAvailability
		 {
			  internal readonly ManagementData ManagementData;
			  internal readonly HighlyAvailableKernelData KernelData;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: HighAvailabilityImpl(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal HighAvailabilityImpl( ManagementData management ) : base( management )
			  {
					this.ManagementData = management;
					this.KernelData = ( HighlyAvailableKernelData ) management.KernelData;
			  }

			  internal HighAvailabilityImpl( ManagementData management, bool isMXBean ) : base( management, isMXBean )
			  {
					this.ManagementData = management;
					this.KernelData = ( HighlyAvailableKernelData ) management.KernelData;
			  }

			  public virtual string InstanceId
			  {
				  get
				  {
						return KernelData.MemberInfo.InstanceId;
				  }
			  }

			  public virtual ClusterMemberInfo[] InstancesInCluster
			  {
				  get
				  {
						return KernelData.ClusterInfo;
				  }
			  }

			  public virtual string Role
			  {
				  get
				  {
						return KernelData.MemberInfo.HaRole;
				  }
			  }

			  public virtual bool Available
			  {
				  get
				  {
						return KernelData.MemberInfo.Available;
				  }
			  }

			  public virtual bool Alive
			  {
				  get
				  {
						return KernelData.MemberInfo.Alive;
				  }
			  }

			  public virtual string LastUpdateTime
			  {
				  get
				  {
						long lastUpdateTime = KernelData.MemberInfo.LastUpdateTime;
						return lastUpdateTime == 0 ? "N/A" : Format.date( lastUpdateTime );
				  }
			  }

			  public virtual long LastCommittedTxId
			  {
				  get
				  {
						return KernelData.MemberInfo.LastCommittedTxId;
				  }
			  }

			  public override string Update()
			  {
					long time = DateTimeHelper.CurrentUnixTimeMillis();
					try
					{
						 ManagementData.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
					}
					catch ( Exception e )
					{
						 return "Update failed: " + e;
					}
					time = DateTimeHelper.CurrentUnixTimeMillis() - time;
					return "Update completed in " + time + "ms";
			  }
		 }
	}

}