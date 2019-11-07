using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Kernel.configuration
{

	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Log = Neo4Net.Logging.Log;

	public class AnnotationBasedConfigurationMigrator : ConfigurationMigrator
	{
		 private List<ConfigurationMigrator> _migrators = new List<ConfigurationMigrator>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: AnnotationBasedConfigurationMigrator(@Nonnull Iterable<Neo4Net.configuration.LoadableConfig> settingsClasses)
		 internal AnnotationBasedConfigurationMigrator( IEnumerable<LoadableConfig> settingsClasses )
		 {
			  foreach ( LoadableConfig loadableConfig in settingsClasses )
			  {
					foreach ( ConfigurationMigrator field in GetMigratorsFromClass( loadableConfig.GetType() ) )
					{
						 _migrators.Add( field );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public java.util.Map<String,String> apply(@Nonnull Map<String,String> rawConfiguration, @Nonnull Log log)
		 public override IDictionary<string, string> Apply( IDictionary<string, string> rawConfiguration, Log log )
		 {
			  foreach ( ConfigurationMigrator migrator in _migrators )
			  {
					rawConfiguration = migrator.Apply( rawConfiguration, log );
			  }
			  return rawConfiguration;
		 }

		 /// <summary>
		 /// Find all <seealso cref="ConfigurationMigrator"/> annotated with <seealso cref="Migrator"/> from a given class.
		 /// </summary>
		 /// <param name="clazz"> The class to scan for migrators. </param>
		 /// <exception cref="AssertionError"> if a field annotated as a <seealso cref="Migrator"/> is not static or does not implement
		 /// <seealso cref="ConfigurationMigrator"/>. </exception>
		 private static IEnumerable<ConfigurationMigrator> GetMigratorsFromClass( Type clazz )
		 {
			  IList<ConfigurationMigrator> found = new List<ConfigurationMigrator>();
			  foreach ( System.Reflection.FieldInfo field in clazz.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
			  {
					if ( field.isAnnotationPresent( typeof( Migrator ) ) )
					{
						 if ( !field.Type.IsAssignableFrom( typeof( ConfigurationMigrator ) ) )
						 {
							  throw new AssertionError( "Field annotated as Migrator has to implement ConfigurationMigrator" );
						 }

						 if ( !Modifier.isStatic( field.Modifiers ) )
						 {
							  throw new AssertionError( "Field annotated as Migrator has to be static" );
						 }

						 try
						 {
							  field.Accessible = true;
							  found.Add( ( ConfigurationMigrator ) field.get( null ) );
						 }
						 catch ( IllegalAccessException ex )
						 {
							  throw new AssertionError( "Field annotated as Migrator could not be accessed", ex );
						 }
					}
			  }

			  return found;
		 }
	}

}