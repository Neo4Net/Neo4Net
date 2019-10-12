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
namespace Neo4Net.Udc
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.singleton;

	/// <summary>
	/// A lookup key to publish or retrieve data in <seealso cref="UsageData"/>. </summary>
	/// @param <Type> The type of the data </param>
	public class UsageDataKey<Type>
	{
		 private readonly string _name;

		 /// <summary>
		 /// When key is requested and no value exists, a default value is generated and inserted using this </summary>
		 private readonly System.Func<Type> _defaultVal;

		 public static UsageDataKey<T> Key<T>( string name )
		 {
			  return Key( name, null );
		 }

		 public static UsageDataKey<T> Key<T>( string name, T defaultVal )
		 {
			  return new UsageDataKey<T>( name, singleton( defaultVal ) );
		 }

		 public static UsageDataKey<T> Key<T>( string name, System.Func<T> defaultVal )
		 {
			  return new UsageDataKey<T>( name, defaultVal );
		 }

		 public UsageDataKey( string name, System.Func<Type> defaultValue )
		 {
			  this._name = name;
			  this._defaultVal = defaultValue;
		 }

		 internal virtual string Name()
		 {
			  return _name;
		 }

		 internal virtual Type GenerateDefaultValue()
		 {
			  return _defaultVal == null ? default( Type ) : _defaultVal.get();
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: UsageDataKey<?> key = (UsageDataKey<?>) o;
			  UsageDataKey<object> key = ( UsageDataKey<object> ) o;

			  return _name.Equals( key._name );

		 }

		 public override int GetHashCode()
		 {
			  return _name.GetHashCode();
		 }
	}

}