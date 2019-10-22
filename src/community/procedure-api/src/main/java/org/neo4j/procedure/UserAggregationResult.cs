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
namespace Neo4Net.Procedure
{

	/// <summary>
	/// Declares a method as the result method of an aggregation.
	/// <para>
	/// This method is called once when the aggregation is done (<seealso cref="UserAggregationUpdate"/>
	/// 
	/// <h2>Output declaration</h2>
	/// A function must always return a single value.
	/// </para>
	/// <para>
	/// Valid return types are as follows:
	/// 
	/// <ul>
	///     <li><seealso cref="string"/></li>
	///     <li><seealso cref="Long"/> or {@code long}</li>
	///     <li><seealso cref="Double"/> or {@code double}</li>
	///     <li><seealso cref="Number"/></li>
	///     <li><seealso cref="Boolean"/> or {@code boolean}</li>
	///     <li><seealso cref="org.Neo4Net.graphdb.Node"/></li>
	///     <li><seealso cref="org.Neo4Net.graphdb.Relationship"/></li>
	///     <li><seealso cref="org.Neo4Net.graphdb.Path"/></li>
	///     <li><seealso cref="System.Collections.IDictionary"/> with key <seealso cref="string"/> and value of any type in this list, including <seealso cref="System.Collections.IDictionary"/></li>
	///     <li><seealso cref="System.Collections.IList"/> of elements of any valid field type, including <seealso cref="System.Collections.IList"/></li>
	///     <li><seealso cref="object"/>, meaning any of the valid return types above</li>
	/// </ul>
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class UserAggregationResult : System.Attribute
	{

	}

}