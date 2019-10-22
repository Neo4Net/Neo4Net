using System;
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
namespace Neo4Net.management.impl
{

	using Service = Neo4Net.Helpers.Service;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LockInfo = Neo4Net.Kernel.info.LockInfo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class LockManagerBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	public sealed class LockManagerBean : ManagementBeanProvider
	{
		 public LockManagerBean() : base(typeof(LockManager))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.jmx.impl.Neo4NetMBean createMBean(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new LockManagerImpl( management );
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
		 {
			  return new LockManagerImpl( management, true );
		 }

		 private class LockManagerImpl : Neo4NetMBean, LockManager
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Locks LockManagerConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: LockManagerImpl(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal LockManagerImpl( ManagementData management ) : base( management )
			  {
					this.LockManagerConflict = LockManager( management );
			  }

			  internal virtual Locks LockManager( ManagementData management )
			  {
					try
					{
						 return management.ResolveDependency( typeof( Locks ) );
					}
					catch ( Exception e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
						 return null;
					}
			  }

			  internal LockManagerImpl( ManagementData management, bool mxBean ) : base( management, mxBean )
			  {
					this.LockManagerConflict = LockManager( management );
			  }

			  public virtual long NumberOfAvertedDeadlocks
			  {
				  get
				  {
						return -1L;
				  }
			  }

			  public virtual IList<LockInfo> Locks
			  {
				  get
				  {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final java.util.List<org.Neo4Net.kernel.info.LockInfo> locks = new java.util.ArrayList<>();
						IList<LockInfo> locks = new List<LockInfo>();
						LockManagerConflict.accept( ( resourceType, resourceId, description, waitTime, lockIdentityHashCode ) => locks.Add( new LockInfo( resourceType.ToString(), resourceId.ToString(), description ) ) );
						return locks;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.List<org.Neo4Net.kernel.info.LockInfo> getContendedLocks(final long minWaitTime)
			  public override IList<LockInfo> GetContendedLocks( long minWaitTime )
			  {
					// Contended locks can no longer be found by the new lock manager, since that knowledge is not centralized.
					return Locks;
			  }
		 }
	}

}