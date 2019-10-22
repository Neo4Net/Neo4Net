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
namespace Neo4Net.Kernel.extension
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using EmbeddedGraphDatabase = Neo4Net.GraphDb.facade.embedded.EmbeddedGraphDatabase;
	using Service = Neo4Net.Helpers.Service;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	/// <summary>
	/// Base class for testing a <seealso cref="org.Neo4Net.kernel.extension.KernelExtensionFactory"/>. The base test cases in this
	/// class verifies that a extension upholds the <seealso cref="org.Neo4Net.kernel.extension.KernelExtensionFactory"/> contract.
	/// </summary>
	public abstract class KernelExtensionFactoryContractTest
	{
		 private readonly Type _extClass;
		 private readonly string _key;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory target = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Target = TestDirectory.testDirectory();

		 public KernelExtensionFactoryContractTest( string key, Type extClass )
		 {
			  this._extClass = extClass;
			  this._key = key;
		 }

		 protected internal virtual GraphDatabaseAPI GraphDb( int instance )
		 {
			  IDictionary<string, string> config = Configuration( instance );
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(config).newGraphDatabase();
		 }

		 /// <summary>
		 /// Override to create default configuration for the <seealso cref="org.Neo4Net.kernel.extension.KernelExtensionFactory"/>
		 /// under test.
		 /// </summary>
		 /// <param name="instance">   used for differentiating multiple instances that will run
		 ///                   simultaneously. </param>
		 /// <returns> configuration for an <seealso cref="EmbeddedGraphDatabase"/> that </returns>
		 protected internal virtual IDictionary<string, string> Configuration( int instance )
		 {
			  return MapUtil.stringMap();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extensionShouldHavePublicNoArgConstructor()
		 public virtual void ExtensionShouldHavePublicNoArgConstructor()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: KernelExtensionFactory<?> instance = null;
			  KernelExtensionFactory<object> instance = null;
			  try
			  {
					instance = NewInstance();
			  }
			  catch ( System.ArgumentException failure )
			  {
					Console.WriteLine( failure.ToString() );
					Console.Write( failure.StackTrace );
					fail( "Contract violation: extension class must have public no-arg constructor (Exception in stderr)" );
			  }
			  assertNotNull( instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoadExtensionAsAServiceProvider()
		 public virtual void ShouldBeAbleToLoadExtensionAsAServiceProvider()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: KernelExtensionFactory<?> instance = null;
			  KernelExtensionFactory<object> instance = null;
			  try
			  {
					instance = LoadInstance();
			  }
			  catch ( System.InvalidCastException failure )
			  {
					Console.WriteLine( failure.ToString() );
					Console.Write( failure.StackTrace );
					fail( "Loaded instance does not match the extension class (Exception in stderr)" );
			  }

			  assertNotNull( "Could not load the kernel extension with the provided key", instance );
			  assertSame( "Class of the loaded instance is a subclass of the extension class", instance.GetType(), _extClass );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void differentInstancesShouldHaveEqualHashCodesAndBeEqual()
		 public virtual void DifferentInstancesShouldHaveEqualHashCodesAndBeEqual()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: KernelExtensionFactory<?> one = newInstance();
			  KernelExtensionFactory<object> one = NewInstance();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: KernelExtensionFactory<?> two = newInstance();
			  KernelExtensionFactory<object> two = NewInstance();
			  assertEquals( "new instances have different hash codes", one.GetHashCode(), two.GetHashCode() );
			  assertEquals( "new instances are not equals", one, two );

			  one = LoadInstance();
			  two = LoadInstance();
			  assertEquals( "loaded instances have different hash codes", one.GetHashCode(), two.GetHashCode() );
			  assertEquals( "loaded instances are not equals", one, two );

			  one = LoadInstance();
			  two = NewInstance();
			  assertEquals( "loaded instance and new instance have different hash codes", one.GetHashCode(), two.GetHashCode() );
			  assertEquals( "loaded instance and new instance are not equals", one, two );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private KernelExtensionFactory<?> newInstance()
		 private KernelExtensionFactory<object> NewInstance()
		 {
			  try
			  {
					return System.Activator.CreateInstance( _extClass );
			  }
			  catch ( Exception cause )
			  {
					throw new System.ArgumentException( "Could not instantiate extension class", cause );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private KernelExtensionFactory<?> loadInstance()
		 private KernelExtensionFactory<object> LoadInstance()
		 {
			  return _extClass.cast( Service.load( typeof( KernelExtensionFactory ), _key ) );
		 }
	}

}