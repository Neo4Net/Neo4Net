using System;

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
namespace Neo4Net.Util
{

	/// <summary>
	/// Feature toggles are used for features that are possible to configure, but where the configuration is always fixed in
	/// a production system.
	/// 
	/// Typical use cases for feature toggles are features that are integrated into the code base for integration and
	/// testing purposes, but are not ready for inclusion in the finished product yet, or features that are always on or has
	/// a fixed configured value in the finished product, but can assume a different configuration or be turned off in some
	/// test.
	/// 
	/// Feature toggles are passed to the JVM through <seealso cref="System.getProperty(string) system properties"/>, and
	/// expected to be looked up from a static context, a {@code static final} fields of the class the toggle controls.
	/// 
	/// All methods in this class returns the default value if the system property has not been assigned, or if the value of
	/// the system property cannot be interpreted as a value of the expected type.
	/// 
	/// For features that the user is ever expected to touch, feature toggles is the wrong abstraction!
	/// </summary>
	public class FeatureToggles
	{
		 /// <summary>
		 /// Get the value of a {@code boolean} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static bool Flag( Type location, string name, bool defaultValue )
		 {
			  return BooleanProperty( name( location, name ), defaultValue );
		 }

		 /// <summary>
		 /// Get the value of a {@code boolean} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the package of the provided class and local name.
		 /// </summary>
		 /// <param name="location"> a class in the package that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static bool PackageFlag( Type location, string name, bool defaultValue )
		 {
			  return BooleanProperty( name( location.Assembly, name ), defaultValue );
		 }

		 /// <summary>
		 /// Get the value of a {@code long} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static long GetLong( Type location, string name, long defaultValue )
		 {
			  return Long.getLong( name( location, name ), defaultValue );
		 }

		 /// <summary>
		 /// Get the value of a {@code int} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static int GetInteger( Type location, string name, int defaultValue )
		 {
			  return Integer.getInteger( name( location, name ), defaultValue );
		 }

		 /// <summary>
		 /// Get the value of a {@code double} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static double GetDouble( Type location, string name, double defaultValue )
		 {
			  try
			  {
					string propertyValue = System.getProperty( name( location, name ) );
					if ( !string.ReferenceEquals( propertyValue, null ) && propertyValue.Length > 0 )
					{
						 return double.Parse( propertyValue );
					}
			  }
			  catch ( Exception )
			  {
					// ignored
			  }
			  return defaultValue;
		 }

		 /// <summary>
		 /// Get the value of a <seealso cref="string"/> system property.
		 /// <para>
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static string GetString( Type location, string name, string defaultValue )
		 {
			  string propertyValue = System.getProperty( name( location, name ) );
			  return string.ReferenceEquals( propertyValue, null ) || propertyValue.Length == 0 ? defaultValue : propertyValue;
		 }

		 /// <summary>
		 /// Get the value of a {@code enum} system property.
		 /// 
		 /// The absolute name of the system property is computed based on the provided class and local name.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="defaultValue"> the default value of the flag if the system property is not assigned. </param>
		 /// @param <E> the enum value type. </param>
		 /// <returns> the parsed value of the system property, or the default value. </returns>
		 public static E Flag<E>( Type location, string name, E defaultValue ) where E : Enum<E>
		 {
			  return EnumProperty( defaultValue.DeclaringClass, name( location, name ), defaultValue );
		 }

		 /// <summary>
		 /// Set the value of a system property.
		 /// <para>
		 /// The name of the system property is computed based on the provided class and local name.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="value"> the value to assign to the system property. </param>
		 public static void Set( Type location, string name, object value )
		 {
			  System.setProperty( name( location, name ), Objects.ToString( value ) );
		 }

		 /// <summary>
		 /// Clear the value of a system property.
		 /// <para>
		 /// The name of the system property is computed based on the provided class and local name.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 public static void Clear( Type location, string name )
		 {
			  System.clearProperty( name( location, name ) );
		 }

		 /// <summary>
		 /// Helps creating a JVM parameter for setting a feature toggle of an arbitrary type.
		 /// Given value will be converted to string using <seealso cref="Objects.toString(object)"/> method.
		 /// </summary>
		 /// <param name="location"> the class that owns the flag. </param>
		 /// <param name="name"> the local name of the flag. </param>
		 /// <param name="value"> the value to assign to the feature toggle. </param>
		 /// <returns> the parameter to pass to the command line of the forked JVM. </returns>
		 public static string Toggle( Type location, string name, object value )
		 {
			  return Toggle( name( location, name ), Objects.ToString( value ) );
		 }

		 // <implementation>

		 private static string Toggle( string key, string value )
		 {
			  return "-D" + key + "=" + value;
		 }

		 private FeatureToggles()
		 {
		 }

		 private static string Name( Type location, string name )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  return location.FullName + "." + name;
		 }

		 private static string Name( Package location, string name )
		 {
			  return location.Name + "." + name;
		 }

		 private static bool BooleanProperty( string flag, bool defaultValue )
		 {
			  return ParseBoolean( System.getProperty( flag ), defaultValue );
		 }

		 private static bool ParseBoolean( string value, bool defaultValue )
		 {
			  return defaultValue ?!"false".Equals( value, StringComparison.OrdinalIgnoreCase ) : "true".Equals( value, StringComparison.OrdinalIgnoreCase );
		 }

		 private static E EnumProperty<E>( Type enumClass, string name, E defaultValue ) where E : Enum<E>
		 {
				 enumClass = typeof( E );
			  try
			  {
					return Enum.valueOf( enumClass, System.getProperty( name, defaultValue.name() ) );
			  }
			  catch ( System.ArgumentException )
			  {
					return defaultValue;
			  }
		 }
	}

}