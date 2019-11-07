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
namespace Neo4Net.management
{
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using Kernel = Neo4Net.Jmx.Kernel;
	using Primitives = Neo4Net.Jmx.Primitives;
	using JmxKernelExtension = Neo4Net.Jmx.impl.JmxKernelExtension;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ManagementBeansTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public static EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();
		 private static GraphDatabaseAPI _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static synchronized void startGraphDb()
		 public static void StartGraphDb()
		 {
			 lock ( typeof( ManagementBeansTest ) )
			 {
				  _graphDb = DbRule.GraphDatabaseAPI;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAccessKernelBean()
		 public virtual void CanAccessKernelBean()
		 {
			  Kernel kernel = _graphDb.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Kernel ) );
			  assertNotNull( "kernel bean is null", kernel );
			  assertNotNull( "MBeanQuery of kernel bean is null", kernel.MBeanQuery );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAccessPrimitivesBean()
		 public virtual void CanAccessPrimitivesBean()
		 {
			  Primitives primitives = _graphDb.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Primitives ) );
			  assertNotNull( "primitives bean is null", primitives );
			  primitives.NumberOfNodeIdsInUse;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canListAllBeans()
		 public virtual void CanListAllBeans()
		 {
			  Neo4NetManager manager = Manager;
			  assertTrue( "No beans returned", manager.AllBeans().Count > 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetConfigurationParameters()
		 public virtual void CanGetConfigurationParameters()
		 {
			  Neo4NetManager manager = Manager;
			  IDictionary<string, object> configuration = manager.Configuration;
			  assertTrue( "No configuration returned", configuration.Count > 0 );
		 }

		 private Neo4NetManager Manager
		 {
			 get
			 {
				  return new Neo4NetManager( _graphDb.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Kernel ) ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetLockManagerBean()
		 public virtual void CanGetLockManagerBean()
		 {
			  assertNotNull( Manager.LockManagerBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canIndexSamplingManagerBean()
		 public virtual void CanIndexSamplingManagerBean()
		 {
			  assertNotNull( Manager.IndexSamplingManagerBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetMemoryMappingBean()
		 public virtual void CanGetMemoryMappingBean()
		 {
			  assertNotNull( Manager.MemoryMappingBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetPrimitivesBean()
		 public virtual void CanGetPrimitivesBean()
		 {
			  assertNotNull( Manager.PrimitivesBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetStoreFileBean()
		 public virtual void CanGetStoreFileBean()
		 {
			  assertNotNull( Manager.StoreFileBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetTransactionManagerBean()
		 public virtual void CanGetTransactionManagerBean()
		 {
			  assertNotNull( Manager.TransactionManagerBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetPageCacheBean()
		 public virtual void CanGetPageCacheBean()
		 {
			  assertNotNull( Manager.PageCacheBean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAccessMemoryMappingCompositData()
		 public virtual void CanAccessMemoryMappingCompositData()
		 {
			  assertNotNull( "MemoryPools is null", Manager.MemoryMappingBean.MemoryPools );
		 }
	}

}