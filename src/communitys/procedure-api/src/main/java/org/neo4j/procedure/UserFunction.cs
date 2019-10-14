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
	/// Declares a method as a Function, meaning the method can be called from the
	/// cypher query language.
	/// <para>
	/// Functions accept input, use that input to perform work, and then return a value. The work performed usually
	/// involves one or more resources, such as a <seealso cref="org.neo4j.graphdb.GraphDatabaseService"/>. Functions are read-only, i.e
	/// can't update neither the graph nor update schema.
	/// 
	/// <h2>Input declaration</h2>
	/// A function can accept input arguments, which is defined in the arguments to the
	/// annotated method. Each method argument must be a valid Function input type, and
	/// each must be annotated with the <seealso cref="Name"/> annotation, declaring the input name.
	/// </para>
	/// <para>
	/// Valid input types are as follows:
	/// 
	/// <ul>
	///     <li><seealso cref="string"/></li>
	///     <li><seealso cref="Long"/> or {@code long}</li>
	///     <li><seealso cref="Double"/> or {@code double}</li>
	///     <li><seealso cref="Number"/></li>
	///     <li><seealso cref="Boolean"/> or {@code boolean}</li>
	///     <li><seealso cref="org.neo4j.graphdb.Node"/></li>
	///     <li><seealso cref="org.neo4j.graphdb.Relationship"/></li>
	///     <li><seealso cref="org.neo4j.graphdb.Path"/></li>
	///     <li><seealso cref="System.Collections.IDictionary"/> with key <seealso cref="string"/> and value of any type in this list, including <seealso cref="System.Collections.IDictionary"/></li>
	///     <li><seealso cref="System.Collections.IList"/> with element type of any type in this list, including <seealso cref="System.Collections.IList"/></li>
	///     <li><seealso cref="object"/>, meaning any valid input types above</li>
	/// </ul>
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
	///     <li><seealso cref="org.neo4j.graphdb.Node"/></li>
	///     <li><seealso cref="org.neo4j.graphdb.Relationship"/></li>
	///     <li><seealso cref="org.neo4j.graphdb.Path"/></li>
	///     <li><seealso cref="System.Collections.IDictionary"/> with key <seealso cref="string"/> and value of any type in this list, including <seealso cref="System.Collections.IDictionary"/></li>
	///     <li><seealso cref="System.Collections.IList"/> of elements of any valid field type, including <seealso cref="System.Collections.IList"/></li>
	///     <li><seealso cref="object"/>, meaning any of the valid field types above</li>
	/// </ul>
	/// 
	/// <h2>Resource declarations</h2>
	/// The function method itself can contain arbitrary Java code - but in order to work with the underlying graph,
	/// it must have access to the graph API. This is done by declaring fields in the function class, and annotating
	/// them with the <seealso cref="Context"/> annotation. Fields declared this way are automatically injected with the
	/// requested resource. This is how functions gain access to APIs to do work with.
	/// </para>
	/// <para>
	/// All fields in the class containing the function declaration must either be static; or it must be public, non-final
	/// and annotated with <seealso cref="Context"/>.
	/// </para>
	/// <para>
	/// Resources supported by default are as follows:
	/// <ul>
	///     <li><seealso cref="org.neo4j.graphdb.GraphDatabaseService"/></li>
	///     <li><seealso cref="org.neo4j.logging.Log"/></li>
	///     <li><seealso cref="org.neo4j.procedure.TerminationGuard"/></li>
	/// </ul>
	/// 
	/// <h2>Lifecycle and state</h2>
	/// The class that declares your function method may be re-instantiated before each call. Because of this,
	/// no regular state can be stored in the fields of the function.
	/// </para>
	/// <para>
	/// If you want to maintain state between invocations to your procedure, simply use a static field. Note that
	/// procedures may be called concurrently, meaning you need to take care to ensure the state you store in
	/// static fields can be safely accessed by multiple callers simultaneously.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class UserFunction : System.Attribute
	{
		 /// <summary>
		 /// The namespace and name for the function, as a period-separated
		 /// string. For instance {@code myfunctions.myfunction}.
		 /// 
		 /// If this is left empty, the name defaults to the package name of
		 /// the class the procedure is declared in, combined with the method
		 /// name. Notably, the class name is omitted.
		 /// </summary>
		 /// <returns> the namespace and procedure name </returns>
		 internal string value;

		 /// <summary>
		 /// Synonym for <seealso cref="value()"/> </summary>
		 /// <returns> the namespace and procedure name. </returns>
		 internal string name;

		 /// <summary>
		 /// When deprecating a function it is useful to indicate a possible
		 /// replacement procedure that clients might show in warnings. </summary>
		 /// <returns> a string representation of the replacement procedure. </returns>
		 internal string deprecatedBy;

		public UserFunction( String value = "", String name = "", String deprecatedBy = "" )
		{
			this.value = value;
			this.name = name;
			this.deprecatedBy = deprecatedBy;
		}
	}

}