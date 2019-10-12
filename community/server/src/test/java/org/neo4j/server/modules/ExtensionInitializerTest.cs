using System;
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
namespace Org.Neo4j.Server.modules
{
	using Configuration = org.apache.commons.configuration.Configuration;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using Org.Neo4j.Server.plugins;
	using PluginLifecycle = Org.Neo4j.Server.plugins.PluginLifecycle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;


	public class ExtensionInitializerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPluginInitialization()
		 public virtual void TestPluginInitialization()
		 {
			  Config config = Config.defaults( ServerSettings.transaction_idle_timeout, "600" );
			  NeoServer neoServer = Mockito.mock( typeof( NeoServer ), Mockito.RETURNS_DEEP_STUBS );
			  Mockito.when( neoServer.Config ).thenReturn( config );
			  ExtensionInitializer extensionInitializer = new ExtensionInitializer( neoServer );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.server.plugins.Injectable<?>> injectableProperties = extensionInitializer.initializePackages(java.util.Collections.singletonList("org.neo4j.server.modules"));
			  ICollection<Injectable<object>> injectableProperties = extensionInitializer.InitializePackages( Collections.singletonList( "org.neo4j.server.modules" ) );

			  assertTrue( injectableProperties.Any( i => ServerSettings.transaction_idle_timeout.name().Equals(i.Value) ) );
		 }

		 private class ArgumentMatcherAnonymousInnerClass : org.mockito.ArgumentMatcher<IList<string>>
		 {
			 private readonly DBMSModuleTest outerInstance;

			 public ArgumentMatcherAnonymousInnerClass( DBMSModuleTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool matches( IList<string> argument )
			 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return argument.contains( typeof( UserService ).FullName );
			 }

			 public override string ToString()
			 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return "<List containing " + typeof( UserService ).FullName + ">";
			 }
		 }

		 public class PropertyCollectorPlugin : PluginLifecycle
		 {

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Collection<org.neo4j.server.plugins.Injectable<?>> start(org.neo4j.graphdb.GraphDatabaseService graphDatabaseService, org.apache.commons.configuration.Configuration config)
			  public override ICollection<Injectable<object>> Start( GraphDatabaseService graphDatabaseService, Configuration config )
			  {
					return Iterators.asList( Iterators.map( new StringToInjectableFunction( this ), config.Keys ) );
			  }

			  public override void Stop()
			  {

			  }

			  private class StringToInjectableFunction : System.Func<string, Injectable<JavaToDotNetGenericWildcard>>
			  {
				  private readonly ExtensionInitializerTest.PropertyCollectorPlugin _outerInstance;

				  public StringToInjectableFunction( ExtensionInitializerTest.PropertyCollectorPlugin outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }


//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.server.plugins.Injectable<String> apply(final String value)
					public override Injectable<string> Apply( string value )
					{
						 return new InjectableAnonymousInnerClass( this, value );
					}

					private class InjectableAnonymousInnerClass : Injectable<string>
					{
						private readonly StringToInjectableFunction _outerInstance;

						private string _value;

						public InjectableAnonymousInnerClass( StringToInjectableFunction outerInstance, string value )
						{
							this.outerInstance = outerInstance;
							this._value = value;
						}

						public string Value
						{
							get
							{
								 return _value;
							}
						}

						public Type<string> Type
						{
							get
							{
								 return typeof( string );
							}
						}
					}
			  }
		 }
	}

}