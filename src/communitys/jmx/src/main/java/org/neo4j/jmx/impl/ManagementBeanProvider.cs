using System;
using System.Collections.Generic;

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

	using Service = Neo4Net.Helpers.Service;
	using KernelData = Neo4Net.Kernel.@internal.KernelData;

	[Obsolete]
	public abstract class ManagementBeanProvider : Service
	{
		 internal readonly Type BeanInterface;

		 public ManagementBeanProvider( Type beanInterface ) : base( ManagementSupport.BeanName( beanInterface ) )
		 {
			  if ( beanInterface.IsAssignableFrom( typeof( DynamicMBean ) ) )
			  {
					beanInterface = typeof( DynamicMBean );
			  }
			  this.BeanInterface = beanInterface;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Iterable<? extends Neo4jMBean> createMBeans(ManagementData management) throws javax.management.NotCompliantMBeanException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal virtual IEnumerable<Neo4jMBean> CreateMBeans( ManagementData management )
		 {
			  return SingletonOrNone( CreateMBean( management ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Iterable<? extends Neo4jMBean> createMXBeans(ManagementData management) throws javax.management.NotCompliantMBeanException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal virtual IEnumerable<Neo4jMBean> CreateMXBeans( ManagementData management )
		 {
			  Type implClass;
			  try
			  {
					implClass = this.GetType().getDeclaredMethod("createMBeans", typeof(ManagementData)).DeclaringClass;
			  }
			  catch ( Exception )
			  {
					implClass = typeof( ManagementBeanProvider ); // Assume no override
			  }
			  if ( implClass != typeof( ManagementBeanProvider ) )
			  { // if createMBeans is overridden, delegate to it
					return CreateMBeans( management );
			  }
			  else
			  { // otherwise delegate to the createMXBean method and create a list
					return SingletonOrNone( CreateMXBean( management ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract Neo4jMBean createMBean(ManagementData management) throws javax.management.NotCompliantMBeanException;
		 protected internal abstract Neo4jMBean CreateMBean( ManagementData management );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4jMBean createMXBean(ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal virtual Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  return CreateMBean( management );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: final Iterable<? extends Neo4jMBean> loadBeans(org.neo4j.kernel.internal.KernelData kernel, ManagementSupport support) throws Exception
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 internal IEnumerable<Neo4jMBean> LoadBeans( KernelData kernel, ManagementSupport support )
		 {
			  if ( support.SupportsMxBeans() )
			  {
					return CreateMXBeans( new ManagementData( this, kernel, support ) );
			  }
			  else
			  {
					return CreateMBeans( new ManagementData( this, kernel, support ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Collection<? extends Neo4jMBean> singletonOrNone(Neo4jMBean mbean)
		 private static ICollection<Neo4jMBean> SingletonOrNone( Neo4jMBean mbean )
		 {
			  if ( mbean == null )
			  {
					return Collections.emptySet();
			  }
			  return Collections.singleton( mbean );
		 }
	}

}