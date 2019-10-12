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
namespace Neo4Net.management.impl
{

	using ConfigurationBean = Neo4Net.Jmx.impl.ConfigurationBean;
	using KernelBean = Neo4Net.Jmx.impl.KernelBean;
	using ManagementSupport = Neo4Net.Jmx.impl.ManagementSupport;

	internal abstract class AdvancedManagementSupport : ManagementSupport
	{
		 protected internal override bool SupportsMxBeans()
		 {
			  return true;
		 }

		 protected internal override T MakeProxy<T>( KernelBean kernel, ObjectName name, Type beanInterface )
		 {
				 beanInterface = typeof( T );
			  return BeanProxy.Load( MBeanServer, beanInterface, name );
		 }

		 protected internal override string GetBeanName( Type beanInterface )
		 {
			  if ( beanInterface == typeof( DynamicMBean ) )
			  {
					return ConfigurationBean.CONFIGURATION_MBEAN_NAME;
			  }
			  return KernelProxy.BeanName( beanInterface );
		 }

		 protected internal override ObjectName CreateObjectName( string instanceId, string beanName, bool query, params string[] extraNaming )
		 {
			  return query ? KernelProxy.createObjectNameQuery( instanceId, beanName, extraNaming ) : KernelProxy.createObjectName( instanceId, beanName, extraNaming );
		 }
	}

}