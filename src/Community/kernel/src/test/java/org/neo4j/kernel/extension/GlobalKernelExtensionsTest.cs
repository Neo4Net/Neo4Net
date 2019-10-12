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
namespace Neo4Net.Kernel.extension
{
	using Test = org.junit.Test;

	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using UnsatisfiedDependencyException = Neo4Net.Kernel.impl.util.UnsatisfiedDependencyException;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.iterable;

	public class GlobalKernelExtensionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultUnsatisfiedDependencyHandlerOnMissingDependencies()
		 public virtual void ShouldConsultUnsatisfiedDependencyHandlerOnMissingDependencies()
		 {
			  // GIVEN
			  KernelContext context = mock( typeof( KernelContext ) );
			  KernelExtensionFailureStrategy handler = mock( typeof( KernelExtensionFailureStrategy ) );
			  Dependencies dependencies = new Dependencies(); // that hasn't got anything.
			  TestingExtensionFactory extensionFactory = new TestingExtensionFactory();
			  GlobalKernelExtensions extensions = new GlobalKernelExtensions( context, iterable( extensionFactory ), dependencies, handler );

			  // WHEN
			  LifeSupport life = new LifeSupport();
			  life.Add( extensions );
			  try
			  {
					life.Start();

					// THEN
					verify( handler ).handle( eq( extensionFactory ), any( typeof( UnsatisfiedDependencyException ) ) );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultUnsatisfiedDependencyHandlerOnFailingDependencyClasses()
		 public virtual void ShouldConsultUnsatisfiedDependencyHandlerOnFailingDependencyClasses()
		 {
			  // GIVEN
			  KernelContext context = mock( typeof( KernelContext ) );
			  KernelExtensionFailureStrategy handler = mock( typeof( KernelExtensionFailureStrategy ) );
			  Dependencies dependencies = new Dependencies(); // that hasn't got anything.
			  UninitializableKernelExtensionFactory extensionFactory = new UninitializableKernelExtensionFactory();
			  GlobalKernelExtensions extensions = new GlobalKernelExtensions( context, iterable( extensionFactory ), dependencies, handler );

			  // WHEN
			  LifeSupport life = new LifeSupport();
			  life.Add( extensions );
			  try
			  {
					life.Start();

					// THEN
					verify( handler ).handle( eq( extensionFactory ), any( typeof( System.ArgumentException ) ) );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private interface TestingDependencies
		 {
			  // Just some dependency
			  JobScheduler JobScheduler();
		 }

		 private class TestingExtensionFactory : KernelExtensionFactory<TestingDependencies>
		 {
			  internal TestingExtensionFactory() : base("testing")
			  {
			  }

			  public override Lifecycle NewInstance( KernelContext context, TestingDependencies dependencies )
			  {
					return new TestingExtension( dependencies.JobScheduler() );
			  }
		 }

		 private class TestingExtension : LifecycleAdapter
		 {
			  internal TestingExtension( JobScheduler jobScheduler )
			  {
					// We don't need it right now
			  }
		 }
	}

}