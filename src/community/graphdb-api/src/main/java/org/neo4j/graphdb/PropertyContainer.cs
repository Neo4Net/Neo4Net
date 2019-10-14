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
namespace Neo4Net.Graphdb
{

	/// <summary>
	/// Defines a common API for handling properties on both <seealso cref="Node nodes"/> and
	/// <seealso cref="Relationship relationships"/>.
	/// <para>
	/// Properties are key-value pairs. The keys are always strings. Valid property
	/// value types are all the Java primitives (<code>int</code>, <code>byte</code>,
	/// <code>float</code>, etc), <code>java.lang.String</code>s, the <em>Spatial</em>
	/// and <em>Temporal</em> types and arrays of any of these.
	/// </para>
	/// <para>
	/// The complete list of currently supported property types is:
	/// <ul>
	/// <li><code>boolean</code></li>
	/// <li><code>byte</code></li>
	/// <li><code>short</code></li>
	/// <li><code>int</code></li>
	/// <li><code>long</code></li>
	/// <li><code>float</code></li>
	/// <li><code>double</code></li>
	/// <li><code>char</code></li>
	/// <li><code>java.lang.String</code></li>
	/// <li><code>org.neo4j.graphdb.spatial.Point</code></li>
	/// <li><code>java.time.LocalDate</code></li>
	/// <li><code>java.time.OffsetTime</code></li>
	/// <li><code>java.time.LocalTime</code></li>
	/// <li><code>java.time.ZonedDateTime</code><br>
	/// <div style="padding-left: 20pt;">It is also possible to use <code>java.time.OffsetDateTime</code> and it will
	/// be converted to a <code>ZonedDateTime</code> internally.</div>
	/// </li>
	/// <li><code>java.time.LocalDateTime</code></li>
	/// <li><code>java.time.temporal.TemporalAmount</code><br>
	/// <div style="padding-left: 20pt;">There are two concrete implementations of this interface, <code>java.time.Duration</code>
	/// and <code>java.time.Period</code> which will be converted to a single Neo4j <code>Duration</code>
	/// type. This means loss of type information, so properties of this type, when read back using
	/// <seealso cref="getProperty(string) getProperty"/> will be only of type <code>java.time.temporal.TemporalAmount</code>.</div>
	/// </li>
	/// <li>Arrays of any of the above types, for example <code>int[]</code>, <code>String[]</code> or <code>LocalTime[]</code></li>
	/// </ul>
	/// </para>
	/// <para>
	/// <b>Please note</b> that Neo4j does NOT accept arbitrary objects as property
	/// values. <seealso cref="setProperty(string, object) setProperty()"/> takes a
	/// <code>java.lang.Object</code> only to avoid an explosion of overloaded
	/// <code>setProperty()</code> methods.
	/// </para>
	/// </summary>
	public interface PropertyContainer
	{
		 /// <summary>
		 /// Get the <seealso cref="GraphDatabaseService"/> that this <seealso cref="Node"/> or
		 /// <seealso cref="Relationship"/> belongs to.
		 /// </summary>
		 /// <returns> The GraphDatabase this Node or Relationship belongs to. </returns>
		 GraphDatabaseService GraphDatabase { get; }

		 /// <summary>
		 /// Returns <code>true</code> if this property container has a property
		 /// accessible through the given key, <code>false</code> otherwise. If key is
		 /// <code>null</code>, this method returns <code>false</code>.
		 /// </summary>
		 /// <param name="key"> the property key </param>
		 /// <returns> <code>true</code> if this property container has a property
		 ///         accessible through the given key, <code>false</code> otherwise </returns>
		 bool HasProperty( string key );

		 /// <summary>
		 /// Returns the property value associated with the given key. The value is of
		 /// one of the valid property types, i.e. a Java primitive,
		 /// a <seealso cref="string String"/>, a <seealso cref="org.neo4j.graphdb.spatial.Point Point"/>,
		 /// a valid temporal type, or an array of any of the valid types.
		 /// See the <seealso cref="PropertyContainer the class description"/>
		 /// for a full list of known types.
		 /// <para>
		 /// If there's no property associated with <code>key</code> an unchecked
		 /// exception is raised. The idiomatic way to avoid an exception for an
		 /// unknown key and instead get <code>null</code> back is to use a default
		 /// value: {@link #getProperty(String, Object) Object valueOrNull =
		 /// nodeOrRel.getProperty( key, null )}
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="key"> the property key </param>
		 /// <returns> the property value associated with the given key </returns>
		 /// <exception cref="NotFoundException"> if there's no property associated with
		 ///             <code>key</code> </exception>
		 object GetProperty( string key );

		 /// <summary>
		 /// Returns the property value associated with the given key, or a default
		 /// value. The value is of one of the valid property types, i.e. a Java primitive,
		 /// a <seealso cref="string String"/>, a <seealso cref="org.neo4j.graphdb.spatial.Point Point"/>,
		 /// a valid temporal type, or an array of any of the valid types.
		 /// See the <seealso cref="PropertyContainer the class description"/>
		 /// for a full list of known types.
		 /// </summary>
		 /// <param name="key"> the property key </param>
		 /// <param name="defaultValue"> the default value that will be returned if no
		 ///            property value was associated with the given key </param>
		 /// <returns> the property value associated with the given key </returns>
		 object GetProperty( string key, object defaultValue );

		 /// <summary>
		 /// Sets the property value for the given key to <code>value</code>. The
		 /// property value must be one of the valid property types, i.e. a Java primitive,
		 /// a <seealso cref="string String"/>, a <seealso cref="org.neo4j.graphdb.spatial.Point Point"/>,
		 /// a valid temporal type, or an array of any of the valid types.
		 /// See the <seealso cref="PropertyContainer the class description"/>
		 /// for a full list of known types.
		 /// <para>
		 /// This means that <code>null</code> is not an accepted property value.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="key"> the key with which the new property value will be associated </param>
		 /// <param name="value"> the new property value, of one of the valid property types </param>
		 /// <exception cref="IllegalArgumentException"> if <code>value</code> is of an
		 ///             unsupported type (including <code>null</code>) </exception>
		 void SetProperty( string key, object value );

		 /// <summary>
		 /// Removes the property associated with the given key and returns the old
		 /// value. If there's no property associated with the key, <code>null</code>
		 /// will be returned.
		 /// </summary>
		 /// <param name="key"> the property key </param>
		 /// <returns> the property value that used to be associated with the given key </returns>
		 object RemoveProperty( string key );

		 /// <summary>
		 /// Returns all existing property keys, or an empty iterable if this property
		 /// container has no properties.
		 /// </summary>
		 /// <returns> all property keys on this property container </returns>
		 // TODO: figure out concurrency semantics
		 IEnumerable<string> PropertyKeys { get; }

		 /// <summary>
		 /// Returns specified existing properties. The collection is mutable,
		 /// but changing it has no impact on the graph as the data is detached.
		 /// </summary>
		 /// <param name="keys"> the property keys to return </param>
		 /// <returns> specified properties on this property container </returns>
		 /// <exception cref="NullPointerException"> if the array of keys or any key is null </exception>
		 IDictionary<string, object> GetProperties( params string[] keys );

		 /// <summary>
		 /// Returns all existing properties.
		 /// </summary>
		 /// <returns> all properties on this property container </returns>
		 IDictionary<string, object> AllProperties { get; }
	}

}