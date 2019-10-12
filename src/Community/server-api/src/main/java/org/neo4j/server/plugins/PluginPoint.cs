using System;

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
namespace Neo4Net.Server.plugins
{
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using Representation = Neo4Net.Server.rest.repr.Representation;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public abstract class PluginPoint
	{
		 private readonly string _name;
		 private readonly Type _extendsType;
		 private readonly string _description;

		 [Obsolete]
		 protected internal PluginPoint( Type type, string name, string description )
		 {
			  this._extendsType = type;
			  this._description = string.ReferenceEquals( description, null ) ? "" : description;
			  this._name = ServerPlugin.VerifyName( name );
		 }

		 [Obsolete]
		 protected internal PluginPoint( Type type, string name ) : this( type, name, null )
		 {
		 }

		 [Obsolete]
		 public string Name()
		 {
			  return _name;
		 }

		 [Obsolete]
		 public Type ForType()
		 {
			  return _extendsType;
		 }

		 [Obsolete]
		 public virtual string Description
		 {
			 get
			 {
				  return _description;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.server.rest.repr.Representation invoke(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object context, ParameterList params) throws org.neo4j.server.rest.repr.BadInputException, BadPluginInvocationException, PluginInvocationFailureException;
		 [Obsolete]
		 public abstract Representation Invoke( GraphDatabaseAPI graphDb, object context, ParameterList @params );

		 [Obsolete]
		 protected internal virtual void DescribeParameters( ParameterDescriptionConsumer consumer )
		 {
		 }
	}

}