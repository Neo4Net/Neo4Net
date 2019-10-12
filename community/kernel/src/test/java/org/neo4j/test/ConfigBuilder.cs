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
namespace Org.Neo4j.Test
{

	using Org.Neo4j.Graphdb.config;

	/// <summary>
	/// Convenience for building config for a test database.
	/// In particular this is class is useful when parameterizing a test with different configurations.
	/// <para>
	/// Usage:
	/// <pre><code>
	///  import static org.neo4j.test.ConfigBuilder.configure;
	/// 
	/// {@literal @}<seealso cref="org.junit.runner.RunWith RunWith"/>(<seealso cref="org.junit.runners.Parameterized Parameterized.class"/>)
	///  public class SomeTest
	///  {
	///     {@literal @}<seealso cref="org.junit.runners.Parameterized.Parameters Parameterized.Parameters"/>( name = "{0}" )
	///      public static Iterable&lt;Object[]&gt; configurations()
	///      {
	///          return Arrays.asList(
	///              // First set of configuration
	///              <seealso cref="configure(Setting, string) configure"/>( {@link
	///              org.neo4j.graphdb.factory.GraphDatabaseSettings#query_cache_size
	///              GraphDatabaseSettings.query_cache_size}, "42" ).<seealso cref="asParameters() asParameters"/>(),
	///              // Second set of configuration
	///              <seealso cref="configure(Setting, string) configure"/>( {@link
	///              org.neo4j.graphdb.factory.GraphDatabaseSettings#query_cache_size
	///              GraphDatabaseSettings.query_cache_size}, "12" )
	///                   .<seealso cref="and(Setting, string) and"/>( {@link
	///                   org.neo4j.graphdb.factory.GraphDatabaseSettings#cypher_min_replan_interval
	///                   GraphDatabaseSettings.cypher_min_replan_interval}, "5000" ).<seealso cref="asParameters() asParameters"/>()
	///          );
	///      }
	/// 
	///      public final{@literal @}Rule <seealso cref="org.neo4j.test.rule.DatabaseRule DatabaseRule"/> db;
	/// 
	///      public SomeTest( ConfigBuilder config )
	///      {
	///          this.db = new {@link org.neo4j.test.rule.ImpermanentDatabaseRule
	///          ImpermanentDatabaseRule}().{@link org.neo4j.test.rule.DatabaseRule#withConfiguration(Map)
	///          withConfiguration}( config.<seealso cref="configuration() configuration"/>() );
	///      }
	///  }
	/// </code></pre>
	/// </para>
	/// </summary>
	public sealed class ConfigBuilder
	{
		 public static ConfigBuilder Configure<T1>( Setting<T1> key, string value )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>();
			  IDictionary<Setting<object>, string> config = new Dictionary<Setting<object>, string>();
			  config[key] = value;
			  return new ConfigBuilder( config );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> config;
		 private readonly IDictionary<Setting<object>, string> _config;

		 private ConfigBuilder<T1>( IDictionary<T1> config )
		 {
			  this._config = config;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> configuration()
		 public IDictionary<Setting<object>, string> Configuration()
		 {
			  return Collections.unmodifiableMap( _config );
		 }

		 public ConfigBuilder And<T1>( Setting<T1> key, string value )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> config = new java.util.HashMap<>(this.config);
			  IDictionary<Setting<object>, string> config = new Dictionary<Setting<object>, string>( this._config );
			  config[key] = value;
			  return new ConfigBuilder( config );
		 }

		 public object[] AsParameters()
		 {
			  return new object[] { this };
		 }

		 public override string ToString()
		 {
			  return _config.ToString();
		 }
	}

}