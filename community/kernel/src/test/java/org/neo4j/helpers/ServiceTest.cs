using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Helpers
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class ServiceTest
	{

		 private ClassLoader _contextClassLoader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _contextClassLoader = Thread.CurrentThread.ContextClassLoader;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  Thread.CurrentThread.ContextClassLoader = _contextClassLoader;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadServiceInDefaultEnvironment()
		 internal virtual void ShouldLoadServiceInDefaultEnvironment()
		 {
			  FooService fooService = Service.Load( typeof( FooService ), "foo" );
			  assertTrue( fooService is BarService );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void whenContextCallsLoaderBlocksServicesFolderShouldLoadClassFromKernelClassloader()
		 internal virtual void WhenContextCallsLoaderBlocksServicesFolderShouldLoadClassFromKernelClassloader()
		 {
			  Thread.CurrentThread.ContextClassLoader = new ServiceBlockClassLoader( _contextClassLoader );
			  FooService fooService = Service.Load( typeof( FooService ), "foo" );
			  assertTrue( fooService is BarService );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void whenContextClassLoaderOverridesServiceShouldLoadThatClass()
		 internal virtual void WhenContextClassLoaderOverridesServiceShouldLoadThatClass()
		 {
			  Thread.CurrentThread.ContextClassLoader = new ServiceRedirectClassLoader( _contextClassLoader );
			  FooService fooService = Service.Load( typeof( FooService ), "foo" );
			  assertTrue( fooService is BazService );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void whenContextClassLoaderDuplicatesServiceShouldLoadItOnce()
		 internal virtual void WhenContextClassLoaderDuplicatesServiceShouldLoadItOnce()
		 {
			  Thread.CurrentThread.ContextClassLoader = typeof( Service ).ClassLoader;
			  IEnumerable<FooService> services = Service.Load( typeof( FooService ) );
			  assertEquals( 1, Iterables.count( services ) );
		 }

		 private sealed class ServiceBlockClassLoader : ClassLoader
		 {

			  internal ServiceBlockClassLoader( ClassLoader parent ) : base( parent )
			  {
			  }

			  public override URL GetResource( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? null : base.GetResource( name );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Iterator<java.net.URL> getResources(String name) throws java.io.IOException
			  public override IEnumerator<URL> GetResources( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? Collections.enumeration( System.Linq.Enumerable.Empty<URL>() ) : base.GetResources(name);
			  }

			  public override Stream GetResourceAsStream( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? null : base.GetResourceAsStream( name );
			  }
		 }

		 private sealed class ServiceRedirectClassLoader : ClassLoader
		 {

			  internal ServiceRedirectClassLoader( ClassLoader parent ) : base( parent )
			  {
			  }

			  public override URL GetResource( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? base.GetResource( "test/" + name ) : base.GetResource( name );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Iterator<java.net.URL> getResources(String name) throws java.io.IOException
			  public override IEnumerator<URL> GetResources( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? base.GetResources( "test/" + name ) : base.GetResources( name );
			  }

			  public override Stream GetResourceAsStream( string name )
			  {
					return name.StartsWith( "META-INF/services", StringComparison.Ordinal ) ? base.GetResourceAsStream( "test/" + name ) : base.GetResourceAsStream( name );
			  }
		 }
	}

}