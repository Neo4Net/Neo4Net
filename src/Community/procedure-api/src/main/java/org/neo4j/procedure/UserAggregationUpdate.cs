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
namespace Neo4Net.Procedure
{

	/// <summary>
	/// Declares a method as the update method of an aggregation.
	/// <para>
	/// The update method is called multiple times and allows the class to aggregate a result later retrieved from the
	/// method
	/// annotated with <seealso cref="UserAggregationResult"/>.
	/// 
	/// <h2>Input declaration</h2>
	/// The update method can accept input arguments, which is defined in the arguments to the
	/// annotated method. Each method argument must be a valid input type, and
	/// each must be annotated with the <seealso cref="Name"/> annotation, declaring the input name.
	/// </para>
	/// <para>
	/// Valid input types are as follows:
	/// <ul>
	/// <li><seealso cref="string"/></li>
	/// <li><seealso cref="Long"/> or {@code long}</li>
	/// <li><seealso cref="Double"/> or {@code double}</li>
	/// <li><seealso cref="Number"/></li>
	/// <li><seealso cref="Boolean"/> or {@code boolean}</li>
	/// <li><seealso cref="org.neo4j.graphdb.Node"/></li>
	/// <li><seealso cref="org.neo4j.graphdb.Relationship"/></li>
	/// <li><seealso cref="org.neo4j.graphdb.Path"/></li>
	/// <li><seealso cref="System.Collections.IDictionary"/> with key <seealso cref="string"/> and value of any type in this list, including {@link
	/// java.util.Map}</li>
	/// <li><seealso cref="System.Collections.IList"/> with element type of any type in this list, including <seealso cref="System.Collections.IList"/></li>
	/// <li><seealso cref="object"/>, meaning any of the valid input types above</li>
	/// </ul>
	/// 
	/// The update method cannot return any value and must be a void method.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class UserAggregationUpdate : System.Attribute
	{
	}

}