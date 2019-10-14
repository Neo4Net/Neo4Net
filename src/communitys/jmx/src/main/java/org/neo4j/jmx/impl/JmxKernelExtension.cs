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

	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Service = Neo4Net.Helpers.Service;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Neo4Net.Kernel.@internal.KernelData;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	[Obsolete]
	public class JmxKernelExtension : LifecycleAdapter
	{
		 private readonly KernelData _kernelData;
		 private readonly DataSourceManager _dataSourceManager;
		 private readonly Log _log;
		 private IList<Neo4jMBean> _beans;
		 private MBeanServer _mbs;
		 private ManagementSupport _support;

		 public JmxKernelExtension( KernelData kernelData, DataSourceManager dataSourceManager, LogProvider logProvider )
		 {
			  this._kernelData = kernelData;
			  this._dataSourceManager = dataSourceManager;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Start()
		 {
			  _support = ManagementSupport.Load();
			  _mbs = _support.MBeanServer;
			  _beans = new LinkedList<Neo4jMBean>();
			  try
			  {
					Neo4jMBean bean = new KernelBean( _kernelData, _dataSourceManager, _support );
					_mbs.registerMBean( bean, bean.ObjectName );
					_beans.Add( bean );
			  }
			  catch ( Exception )
			  {
					_log.info( "Failed to register Kernel JMX Bean" );
			  }

			  foreach ( ManagementBeanProvider provider in Service.load( typeof( ManagementBeanProvider ) ) )
			  {
					try
					{
						 foreach ( Neo4jMBean bean in provider.LoadBeans( _kernelData, _support ) )
						 {
							  _mbs.registerMBean( bean, bean.ObjectName );
							  _beans.Add( bean );
						 }
					}
					catch ( Exception e )
					{
						 _log.info( "Failed to register JMX Bean " + provider + " (" + e + ")" );
					}
			  }
			  try
			  {
					Neo4jMBean bean = new ConfigurationBean( _kernelData, _support );
					_mbs.registerMBean( bean, bean.ObjectName );
					_beans.Add( bean );
			  }
			  catch ( Exception )
			  {
					_log.info( "Failed to register Configuration JMX Bean" );
			  }
		 }

		 public override void Stop()
		 {
			  foreach ( Neo4jMBean bean in _beans )
			  {
					try
					{
						 _mbs.unregisterMBean( bean.ObjectName );
					}
					catch ( Exception e )
					{
						 _log.warn( "Could not unregister MBean " + bean.ObjectName.ToString(), e );
					}
			  }
		 }

		 public T GetSingleManagementBean<T>( Type type )
		 {
				 type = typeof( T );
			  IEnumerator<T> beans = GetManagementBeans( type ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( beans.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					T bean = beans.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( beans.hasNext() )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new NotFoundException( "More than one management bean for " + type.FullName );
					}
					return bean;
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new NotFoundException( "No management bean found for " + type.FullName );
		 }

		 private ICollection<T> GetManagementBeans<T>( Type beanInterface )
		 {
				 beanInterface = typeof( T );
			  ICollection<T> result = null;
			  if ( _support.GetType() != typeof(ManagementSupport) && _beans.Count > 0 && _beans[0] is KernelBean )
			  {
					try
					{
						 result = _support.getProxiesFor( beanInterface, ( KernelBean ) _beans[0] );
					}
					catch ( System.NotSupportedException )
					{
						 // go to fall back
					}
			  }
			  if ( result == null )
			  {
					// Fall back: if we cannot create proxy, we can search for instances
					result = new List<T>();
					foreach ( Neo4jMBean bean in _beans )
					{
						 if ( beanInterface.IsInstanceOfType( bean ) )
						 {
							  result.Add( beanInterface.cast( bean ) );
						 }
					}
			  }
			  return result;
		 }
	}

}