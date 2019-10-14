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
	/// Declares a method as a Procedure, meaning the method can be called from the
	/// cypher query language.
	/// <para>
	/// Procedures accept input, use that input to perform work, and then return a
	/// <seealso cref="java.util.stream.Stream"/> of {@code Records}. The work performed usually
	/// involves one or more resources, such as a <seealso cref="org.neo4j.graphdb.GraphDatabaseService"/>.
	/// </para>
	/// <para>
	/// A procedure is associated with one of the following modes
	///      READ    allows only reading the graph (default mode)
	///      WRITE   allows reading and writing the graph
	///      SCHEMA  allows reading the graphs and performing schema operations
	///      DBMS    allows managing the database (i.e. change password)
	/// 
	/// <h2>Input declaration</h2>
	/// A procedure can accept input arguments, which is defined in the arguments to the
	/// annotated method. Each method argument must be a valid Procedure input type, and
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
	///     <li><seealso cref="object"/>, meaning any of the valid input types above</li>
	/// </ul>
	/// 
	/// <h2>Output declaration</h2>
	/// A procedure must always return a <seealso cref="java.util.stream.Stream"/> of {@code Records}, or nothing.
	/// The record is defined per procedure, as a class with only public, non-final fields.
	/// The types, order and names of the fields in this class define the format of the returned records.
	/// </para>
	/// <para>
	/// Valid field types are as follows:
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
	/// The procedure method itself can contain arbitrary Java code - but in order to work with the underlying graph,
	/// it must have access to the graph API. This is done by declaring fields in the procedure class, and annotating
	/// them with the <seealso cref="Context"/> annotation. Fields declared this way are automatically injected with the
	/// requested resource. This is how procedures gain access to APIs to do work with.
	/// </para>
	/// <para>
	/// All fields in the class containing the procedure declaration must either be static; or it must be public, non-final
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
	/// The class that declares your procedure method may be re-instantiated before each call. Because of this,
	/// no regular state can be stored in the fields of the procedure.
	/// </para>
	/// <para>
	/// If you want to maintain state between invocations to your procedure, simply use a static field. Note that
	/// procedures may be called concurrently, meaning you need to take care to ensure the state you store in
	/// static fields can be safely accessed by multiple callers simultaneously.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class Procedure : System.Attribute
	{
		 /// <summary>
		 /// The namespace and name for the procedure, as a period-separated
		 /// string. For instance {@code myprocedures.myprocedure}.
		 /// 
		 /// If this is left empty, the name defaults to the package name of
		 /// the class the procedure is declared in, combined with the method
		 /// name. Notably, the class name is omitted.
		 /// </summary>
		 /// <returns> the namespace and procedure name. </returns>
		 internal string value;

		 /// <summary>
		 /// Synonym for <seealso cref="value()"/> </summary>
		 /// <returns> the namespace and procedure name. </returns>
		 internal string name;

		 /// <summary>
		 /// A procedure is associated with one of the following modes
		 ///      READ    allows only reading the graph (default mode)
		 ///      WRITE   allows reading and writing the graph
		 ///      SCHEMA  allows reading the graphs and performing schema operations
		 ///      DBMS    allows managing the database (i.e. change password) </summary>
		 /// <returns> the associated mode. </returns>
		 internal Mode mode;

		 /// <summary>
		 /// Cypher normally streams data lazily between operations, but
		 /// for read-write queries this can cause side effects that can only
		 /// be solved using <code>Eager</code> operators between the read and
		 /// write components. Cypher can plan this for you for pure Cypher queries,
		 /// but when you include a <code>WRITE</code> procedure into the query
		 /// the planner will not know enough about the internals of the procedure
		 /// to figure this out. If the procedure can perform updates (like deletes)
		 /// that might negatively impact preceding reads in the Cypher query, then
		 /// set this annotation attribute to <code>true</code>, so that all reads
		 /// will be completed before calling the procedure. Note that this does
		 /// not prevent the procedure from negatively interacting with itself, and
		 /// developers still need to catch and deal with relevant exceptions themselves.
		 /// </summary>
		 internal bool eager;

		 /// <summary>
		 /// When deprecating a procedure it is useful to indicate a possible
		 /// replacement procedure that clients might show in warnings. </summary>
		 /// <returns> a string representation of the replacement procedure. </returns>
		 internal string deprecatedBy;

		public Procedure( String value = "", String name = "", Mode mode = Mode.DEFAULT, boolean eager = false, String deprecatedBy = "" )
		{
			this.value = value;
			this.name = name;
			this.mode = mode;
			this.eager = eager;
			this.deprecatedBy = deprecatedBy;
		}
	}

}