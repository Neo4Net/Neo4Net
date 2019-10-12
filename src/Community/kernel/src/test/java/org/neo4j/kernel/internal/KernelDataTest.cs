using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.@internal
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.forced_kernel_id;

	public class KernelDataTest
	{
		private bool InstanceFieldsInitialized = false;

		public KernelDataTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly ICollection<Kernel> _kernels = new HashSet<Kernel>();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  IEnumerator<Kernel> kernelIterator = _kernels.GetEnumerator();
			  while ( kernelIterator.MoveNext() )
			  {
					Kernel kernel = kernelIterator.Current;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					kernelIterator.remove();
					kernel.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUniqueInstanceIdentifiers()
		 public virtual void ShouldGenerateUniqueInstanceIdentifiers()
		 {
			  // given
			  Kernel kernel1 = new Kernel( this, null );

			  // when
			  Kernel kernel2 = new Kernel( this, null );

			  // then
			  assertNotNull( kernel1.InstanceId() );
			  assertNotNull( kernel2.InstanceId() );
			  assertNotEquals( kernel1.InstanceId(), kernel2.InstanceId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseInstanceIdentifiers()
		 public virtual void ShouldReuseInstanceIdentifiers()
		 {
			  // given
			  Kernel kernel = new Kernel( this, null );
			  string instanceId = kernel.InstanceId();
			  kernel.Shutdown();

			  // when
			  kernel = new Kernel( this, null );

			  // then
			  assertEquals( instanceId, kernel.InstanceId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowConfigurationOfInstanceId()
		 public virtual void ShouldAllowConfigurationOfInstanceId()
		 {
			  // when
			  Kernel kernel = new Kernel( this, "myInstance" );

			  // then
			  assertEquals( "myInstance", kernel.InstanceId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateInstanceIdentifierWhenNullConfigured()
		 public virtual void ShouldGenerateInstanceIdentifierWhenNullConfigured()
		 {
			  // when
			  Kernel kernel = new Kernel( this, null );

			  // then
			  assertEquals( kernel.InstanceId(), kernel.InstanceId().Trim() );
			  assertTrue( kernel.InstanceId().Length > 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateInstanceIdentifierWhenEmptyStringConfigured()
		 public virtual void ShouldGenerateInstanceIdentifierWhenEmptyStringConfigured()
		 {
			  // when
			  Kernel kernel = new Kernel( this, "" );

			  // then
			  assertEquals( kernel.InstanceId(), kernel.InstanceId().Trim() );
			  assertTrue( kernel.InstanceId().Length > 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMultipleInstancesWithTheSameConfiguredInstanceId()
		 public virtual void ShouldNotAllowMultipleInstancesWithTheSameConfiguredInstanceId()
		 {
			  // given
			  new Kernel( this, "myInstance" );

			  // when
			  try
			  {
					new Kernel( this, "myInstance" );
					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( System.InvalidOperationException e )
			  {
					assertEquals( "There is already a kernel started with unsupported.dbms.kernel_id='myInstance'.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReuseOfConfiguredInstanceIdAfterShutdown()
		 public virtual void ShouldAllowReuseOfConfiguredInstanceIdAfterShutdown()
		 {
			  // given
			  ( new Kernel( this, "myInstance" ) ).Shutdown();

			  // when
			  Kernel kernel = new Kernel( this, "myInstance" );

			  // then
			  assertEquals( "myInstance", kernel.InstanceId() );
		 }

		 private class Kernel : KernelData
		 {
			 private readonly KernelDataTest _outerInstance;

			  internal Kernel( KernelDataTest outerInstance, string desiredId ) : base( outerInstance.fileSystemRule.Get(), outerInstance.pageCacheRule.GetPageCache(outerInstance.fileSystemRule.Get()), new File(GraphDatabaseSettings.DEFAULT_DATABASE_NAME), Config.defaults(forced_kernel_id, desiredId), mock(typeof(DataSourceManager)) )
			  {
				  this._outerInstance = outerInstance;
					outerInstance.kernels.Add( this );
			  }

			  public override void Shutdown()
			  {
					base.Shutdown();
					outerInstance.kernels.remove( this );
			  }
		 }
	}

}