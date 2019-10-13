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
namespace Neo4Net.Graphdb.factory
{

	using Neo4Net.Graphdb.config;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Builder for <seealso cref="GraphDatabaseService"/>s that allows for setting and loading
	/// configuration.
	/// </summary>
	public class GraphDatabaseBuilder
	{
		 /// @deprecated This will be moved to an internal package in the future. 
		 [Obsolete("This will be moved to an internal package in the future.")]
		 public interface DatabaseCreator
		 {
			  /// <param name="config"> initial configuration for the database. </param>
			  /// <returns> an instance of <seealso cref="GraphDatabaseService"/>. </returns>
			  /// @deprecated this method will go away in 4.0. See <seealso cref="newDatabase(Config)"/> instead. 
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
[Obsolete("this method will go away in 4.0. See <seealso cref=\"newDatabase(Config)\"/> instead.")]
//			  default org.neo4j.graphdb.GraphDatabaseService newDatabase(java.util.Map<String, String> config)
	//		  {
	//				return newDatabase(Config.defaults(config));
	//		  }

			  /// <param name="config"> initial configuration for the database. </param>
			  /// <returns> an instance of <seealso cref="GraphDatabaseService"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: default org.neo4j.graphdb.GraphDatabaseService newDatabase(@Nonnull Config config)
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default org.neo4j.graphdb.GraphDatabaseService newDatabase( Config config)
	//		  {
	//				return newDatabase(config.getRaw());
	//		  }
		 }

		 protected internal DatabaseCreator Creator;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal IDictionary<string, string> ConfigConflict = new Dictionary<string, string>();

		 /// <summary>
		 /// @deprecated
		 /// </summary>
		 [Obsolete]
		 public GraphDatabaseBuilder( DatabaseCreator creator )
		 {
			  this.Creator = creator;
		 }

		 /// <summary>
		 /// Set a database setting to a particular value.
		 /// </summary>
		 /// <param name="setting"> Database setting to set </param>
		 /// <param name="value"> New value of the setting </param>
		 /// <returns> the builder </returns>
		 public virtual GraphDatabaseBuilder SetConfig<T1>( Setting<T1> setting, string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					ConfigConflict.Remove( setting.Name() );
			  }
			  else
			  {
					// Test if we can get this setting with an updated config
					IDictionary<string, string> testValue = stringMap( setting.Name(), value );
					setting.apply( key => testValue.ContainsKey( key ) ? testValue[key] : ConfigConflict[key] );

					// No exception thrown, add it to existing config
					ConfigConflict[setting.Name()] = value;
			  }
			  return this;
		 }

		 /// <summary>
		 /// Set an unvalidated configuration option.
		 /// </summary>
		 /// <param name="name"> Name of the setting </param>
		 /// <param name="value"> New value of the setting </param>
		 /// <returns> the builder </returns>
		 /// @deprecated Use setConfig with explicit <seealso cref="Setting"/> instead. 
		 [Obsolete("Use setConfig with explicit <seealso cref=\"Setting\"/> instead.")]
		 public virtual GraphDatabaseBuilder SetConfig( string name, string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					ConfigConflict.Remove( name );
			  }
			  else
			  {
					ConfigConflict[name] = value;
			  }
			  return this;
		 }

		 /// <summary>
		 /// Set a map of configuration settings into the builder. Overwrites any existing values.
		 /// </summary>
		 /// <param name="config"> Map of configuration settings </param>
		 /// <returns> the builder </returns>
		 /// @deprecated Use setConfig with explicit <seealso cref="Setting"/> instead 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated("Use setConfig with explicit <seealso cref=\"Setting\"/> instead") @SuppressWarnings("deprecation") public GraphDatabaseBuilder setConfig(java.util.Map<String,String> config)
		 [Obsolete("Use setConfig with explicit <seealso cref=\"Setting\"/> instead")]
		 public virtual GraphDatabaseBuilder SetConfig( IDictionary<string, string> config )
		 {
			  foreach ( KeyValuePair<string, string> stringStringEntry in config.SetOfKeyValuePairs() )
			  {
					setConfig( stringStringEntry.Key, stringStringEntry.Value );
			  }
			  return this;
		 }

		 /// <summary>
		 /// Load a Properties file from a given file, and add the settings to
		 /// the builder.
		 /// </summary>
		 /// <param name="fileName"> Filename of properties file to use </param>
		 /// <returns> the builder </returns>
		 /// <exception cref="IllegalArgumentException"> if the builder was unable to load from the given filename </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GraphDatabaseBuilder loadPropertiesFromFile(String fileName) throws IllegalArgumentException
		 public virtual GraphDatabaseBuilder LoadPropertiesFromFile( string fileName )
		 {
			  try
			  {
					return LoadPropertiesFromURL( ( new File( fileName ) ).toURI().toURL() );
			  }
			  catch ( MalformedURLException e )
			  {
					throw new System.ArgumentException( "Illegal filename:" + fileName, e );
			  }
		 }

		 /// <summary>
		 /// Load Properties file from a given URL, and add the settings to
		 /// the builder.
		 /// </summary>
		 /// <param name="url"> URL of properties file to use </param>
		 /// <returns> the builder </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GraphDatabaseBuilder loadPropertiesFromURL(java.net.URL url) throws IllegalArgumentException
		 public virtual GraphDatabaseBuilder LoadPropertiesFromURL( URL url )
		 {
			  Properties props = new Properties();
			  try
			  {
					using ( Stream stream = url.openStream() )
					{
						 props.load( stream );
					}
			  }
			  catch ( Exception e )
			  {
					throw new System.ArgumentException( "Unable to load " + url, e );
			  }
			  ISet<KeyValuePair<object, object>> entries = props.entrySet();
			  foreach ( KeyValuePair<object, object> entry in entries )
			  {
					string key = ( string ) entry.Key;
					string value = ( string ) entry.Value;
					SetConfig( key, value );
			  }

			  return this;
		 }

		 /// <summary>
		 /// Create a new database with the configuration registered
		 /// through the builder.
		 /// </summary>
		 /// <returns> an instance of GraphDatabaseService </returns>
		 public virtual GraphDatabaseService NewGraphDatabase()
		 {
			  return Creator.newDatabase( Config.defaults( ConfigConflict ) );
		 }

		 /// @deprecated This will be removed in the future. 
		 [Obsolete("This will be removed in the future.")]
		 public class Delegator : GraphDatabaseBuilder
		 {
			  internal readonly GraphDatabaseBuilder Actual;

			  public Delegator( GraphDatabaseBuilder actual ) : base( null )
			  {
					this.Actual = actual;
			  }

			  public override GraphDatabaseBuilder SetConfig<T1>( Setting<T1> setting, string value )
			  {
					Actual.setConfig( setting, value );
					return this;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("deprecation") public GraphDatabaseBuilder setConfig(String name, String value)
			  public override GraphDatabaseBuilder SetConfig( string name, string value )
			  {
					Actual.setConfig( name, value );
					return this;
			  }

			  public override GraphDatabaseBuilder SetConfig( IDictionary<string, string> config )
			  {
					Actual.Config = config;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GraphDatabaseBuilder loadPropertiesFromFile(String fileName) throws IllegalArgumentException
			  public override GraphDatabaseBuilder LoadPropertiesFromFile( string fileName )
			  {
					Actual.loadPropertiesFromFile( fileName );
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GraphDatabaseBuilder loadPropertiesFromURL(java.net.URL url) throws IllegalArgumentException
			  public override GraphDatabaseBuilder LoadPropertiesFromURL( URL url )
			  {
					Actual.loadPropertiesFromURL( url );
					return this;
			  }

			  public override GraphDatabaseService NewGraphDatabase()
			  {
					return Actual.newGraphDatabase();
			  }
		 }
	}

}